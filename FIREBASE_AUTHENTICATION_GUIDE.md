# دليل المصادقة وجلب التوكن من Firebase

هذا الملف يشرح مسار تسجيل الدخول في تطبيق LoukUPmOVE من أول ضغطة على زر Google حتى حفظ التوكن واستعماله في طلبات الـ API.

في التطبيق يوجد توكنان مختلفان. لا تستعمل أحدهما مكان الآخر.

| التوكن | من وين بيجي | وين بنخزنه | شو بنعمل فيه |
| --- | --- | --- | --- |
| Firebase ID Token | Firebase بعد تسجيل Google | ما بنخزنه. بنبعثه مرة واحدة للسيرفر | إثبات إن حساب Google صحيح |
| Access Token | سيرفر `lookupfriseur.com` | `SecureStorage` بمفتاح `auth_token` | هيدر `Authorization` لكل طلب API |
| Refresh Token | نفس السيرفر | `SecureStorage` بمفتاح `refresh_token` | طلب توكن وصول جديد لما ينتهي الحالي |

تسجيل الإيميل وكلمة السر ما بيمر على Firebase. بيروح مباشرة على `POST /api/auth/login` والسيرفر هو اللي بيرجع الـ Access Token.

---

## 1. تجهيز Firebase عند تشغيل التطبيق

الإعدادات بتقرأ من `loukupm/appsettings.json` داخل قسم `Firebase`:

- `ApiKey`: مفتاح مشروع Firebase
- `AuthDomain`: دومين المصادقة، حالياً `app-test-2c25c.firebaseapp.com`

في `loukupm/MauiProgram.cs` بينبني العميل مرة واحدة ويتحفظ بمتغيرين ثابتين:

- `MauiProgram.firebaseconfig`
- `MauiProgram.firebaseclient`

المزوّدون المسجّلين:

- `GoogleProvider` مع scope اسمه `email`
- `EmailProvider` (مسجّل بالإعداد، ومسار جلب التوكن الفعلي المستخدم بالكود هو Google)

المكتبة المستخدمة: `FirebaseAuthentication.net` الإصدار `4.1.0`.

---

## 2. من وين بتبلش عملية Google

صفحتا الدخول والتسجيل بتستدعوا نفس الدالة:

```csharp
var result = await GoogleAuthSession.SignInAndConfirmAsync();
```

الملف: `loukupm/services/GoogleAuthSession.cs`

الدالة فيها قفل `SemaphoreSlim` حتى ما يصير تسجيلين Google بنفس الوقت. إذا في عملية شغالة بترجع:

`Google sign-in is already in progress.`

---

## 3. خطوات جلب الـ ID Token من Firebase

### 3.1 التأكد إن الإعداد موجود

قبل أي طلب، الكود بيتأكد من:

- `firebaseclient` مش `null`
- `firebaseconfig` مش `null`
- `AuthDomain` فيه قيمة
- قائمة `Providers` فيها مزوّد نوعه `FirebaseProviderType.Google`

إذا أي شرط ناقص بترجع: `Google sign-in is not configured.`

### 3.2 فتح صفحة Google وأخذ رابط الرجوع

```csharp
credential = await client.SignInWithRedirectAsync(
    FirebaseProviderType.Google,
    async uri =>
    {
        redirectResult = await GoogleAuthPage.CaptureAsync(uri, config.RedirectUri);
        return redirectResult ?? string.Empty;
    });
```

`SignInWithRedirectAsync` بتبني رابط تسجيل Google وتعطيه لدالة الـ callback.

`GoogleAuthPage.CaptureAsync` بتفتح WebView مودال، المستخدم بيسجّل دخوله، والصفحة بترجع الرابط النهائي (الـ redirect) كنص.

إذا المستخدم سكّر الصفحة أو ألغى، الاستثناء `OperationCanceledException` بيتحوّل لنتيجة `Cancelled`.

### 3.3 قراءة التوكن من ثلاث مصادر بالترتيب

بعد ما Firebase يرجع `UserCredential`، التوكن بيتجمع بهالترتيب. أول قيمة غير فاضية هي اللي بتنعتمد:

1. باراميتر `id_token` من رابط الرجوع (`Fragment` أو `Query`)
2. خاصية `Token` داخل `credential.AuthCredential` بشرط `TokenType == "IdToken"`
3. `await credential.User.GetIdTokenAsync()`

```csharp
var idToken = FirstNonEmpty(
    ExtractParameter(redirectResult, "id_token"),
    ReadGoogleIdToken(credential.AuthCredential),
    await credential.User.GetIdTokenAsync());
```

`GetIdTokenAsync()` هي الدالة الرسمية من Firebase اللي بترجع ID Token لحساب المستخدم الحالي. المصدرين الأول والثاني احتياط إذا التوكن وصل مع رابط Google قبل ما Firebase يغلّفه.

إذا التلاتة فاضيين:

- بينعمل `SignOut` من Firebase
- بترجع الرسالة: `Google did not return a valid account.`

مع التوكن كمان بنقرأ:

- `credential.User.Uid` وهو معرّف Firebase
- `credential.User.Info.Email`
- `credential.User.Info.DisplayName`

---

## 4. تحويل توكن Firebase إلى توكن التطبيق

التوكن اللي رجع من Firebase ما بنستخدمه كـ Bearer للـ API. بنبعثه للباكند.

الاستدعاء من `GoogleAuthSession`:

```csharp
var (success, accessToken, _, _, _, error) =
    await authService.LoginWithGoogleAsync(
        idToken,
        credential.User.Uid,
        email,
        displayName);
```

`LoginWithGoogleAsync` موجودة في `loukupm/services/AuthenticationService.cs`.

الطلب:

- الطريقة: `POST`
- العنوان: `https://lookupfriseur.com/api/auth/google`
- الجسم JSON وفيه نفس الـ ID Token تحت مفتاحين حتى يقبله السيرفر بأي اسم:

```json
{
  "token": "<firebase-id-token>",
  "id_token": "<firebase-id-token>",
  "user_id": "<firebase-uid>",
  "email": "user@gmail.com",
  "display_name": "User Name"
}
```

الرد الناجح بيتقرأ أولاً كـ `LoginResponse`:

- `access_token`
- `refresh_token`
- `access_expires_at`
- `refresh_expires_at`
- `user`

إذا `access_token` فاضي، بنفس الرد بينعمل fallback على شكل `RegisterResponse` لأن أول دخول Google أحياناً بينشئ الحساب ويرجع نفس الحقول بأسماء الخصائص الداخلية `AccessToken` و `RefreshToken`.

بعدها:

```csharp
await _tokenManager.SaveTokensAsync(
    accessToken,
    refreshToken,
    accessExpires,
    refreshExpires);
```

الجلسة ما بتعتبر ناجحة إلا إذا:

- السيرفر رجع `success`
- `accessToken` مش فاضي
- القيمة المحفوظة بـ `TokenManager.GetAccessTokenAsync()` تساوي نفس التوكن اللي رجع من السيرفر

إذا الحفظ فشل أو السيرفر رفض الحساب، بينعمل `firebaseclient.SignOut()` حتى ما تضل جلسة Firebase معلّقة بدون جلسة سيرفر.

---

## 5. وين بينحفظ التوكن

الملف: `loukupm/services/TokenManager.cs`

التخزين عبر `SecureStorage`:

| المفتاح | المحتوى |
| --- | --- |
| `auth_token` | Access Token |
| `refresh_token` | Refresh Token |
| `access_expires_at` | وقت انتهاء الوصول بصيغة UTC |
| `refresh_expires_at` | وقت انتهاء التحديث بصيغة UTC |

إذا السيرفر ما بعت تاريخ انتهاء، `TokenManager` بيحاول يقرأ claim اسمه `exp` من داخل الـ JWT.

القراءة لاحقاً:

```csharp
var tokenManager = AppServices.Get<TokenManager>();
string accessToken = await tokenManager.GetAccessTokenAsync();
```

`AuthenticatedHttpClientHandler` بياخد هذا التوكن ويحطه على طلبات عميل `ApiClient`.

---

## 6. تجديد التوكن

`AuthenticationService.RefreshTokenAsync()`:

1. بتجيب `refresh_token` من `TokenManager`
2. إذا فاضي أو منتهي بتمسح التوكنات وبترجع `false`
3. بتبعت `POST https://lookupfriseur.com/api/auth/refresh` مع `{ "refresh_token": "..." }`
4. بتحفظ الـ Access Token الجديد

في قفل حتى ما ينبعت أكثر من طلب تجديد واحد بنفس الوقت. التجديد الاستباقي بيصير قبل الانتهاء بـ 60 ثانية (`ProactiveRefreshBufferSeconds`).

---

## 7. تسلسل كامل من الزر حتى الـ API

```text
LoginPage / SinginPage
        |
        v
GoogleAuthSession.SignInAndConfirmAsync()
        |
        v
FirebaseAuthClient.SignInWithRedirectAsync(Google)
        |
        v
GoogleAuthPage WebView  -->  المستخدم يوافق
        |
        v
id_token من الرابط  أو  AuthCredential.Token  أو  User.GetIdTokenAsync()
        |
        v
POST /api/auth/google   body: id_token + uid + email + name
        |
        v
السيرفر يرجع access_token + refresh_token
        |
        v
TokenManager.SaveTokensAsync()  -->  SecureStorage
        |
        v
طلبات ApiClient تحمل Authorization: Bearer <access_token>
```

---

## 8. أخطاء متوقعة وسببها

| الرسالة | السبب |
| --- | --- |
| `Google sign-in is already in progress.` | في محاولة ثانية قبل ما الأولى تخلص |
| `Google sign-in is not configured.` | `ApiKey` أو `AuthDomain` أو مزوّد Google ناقص |
| `Google did not return a valid account.` | ما رجع ID Token من أي مصدر من الثلاثة |
| `Google token is required` | `LoginWithGoogleAsync` انستدعيت بتوكن فاضي |
| `The server did not confirm this Google account.` | السيرفر ما رجّع Access Token أو الحفظ ما طابق التوكن |
| `Backend did not return an access token.` | الرد 2xx بدون `access_token` |
| `Authentication service is not available.` | `AuthenticationService` أو `TokenManager` مش مسجّلين في DI |

التسجيل صار في `MauiProgram.CreateMauiApp`:

- `TokenManager` كـ Singleton
- `AuthenticationService` مربوط بعميل HTTP اسمه `AuthClient` وعنوانه `https://lookupfriseur.com/`

---

## 9. الملفات المسؤولة

| الملف | الدور |
| --- | --- |
| `loukupm/appsettings.json` | مفتاح Firebase ودومين المصادقة |
| `loukupm/MauiProgram.cs` | بناء `FirebaseAuthClient` وتسجيل الخدمات |
| `loukupm/services/GoogleAuthSession.cs` | جلب ID Token من Firebase |
| `loukupm/View/GoogleAuthPage.xaml.cs` | WebView اللي بيلتقط رابط الرجوع |
| `loukupm/services/AuthenticationService.cs` | إرسال التوكن للسيرفر وتجديده |
| `loukupm/services/TokenManager.cs` | حفظ وقراءة ومسح التوكنات |
| `loukupm/services/AuthenticatedHttpClientHandler.cs` | إرفاق Access Token بطلبات الـ API |
| `loukupm/Model/Auth.cs` | أشكال طلب ورد تسجيل الدخول |

---

## 10. مثال استعمال بعد نجاح الدخول

```csharp
var result = await GoogleAuthSession.SignInAndConfirmAsync();

if (result.Cancelled)
    return;

if (!result.Success)
{
    // result.Error فيها سبب الفشل
    return;
}

// الجلسة صارت محفوظة. أي طلب على ApiClient بياخد التوكن لحاله.
var token = await AppServices.Get<TokenManager>().GetAccessTokenAsync();
```

`result.UserId` هو UID تبع Firebase. توكن طلبات التطبيق مش هذا الحقل، هو القيمة المحفوظة تحت `auth_token`.
