# شرح عقد الباك إند الذي يستدعيه التطبيق

هذا المستودع تطبيق MAUI. كود السيرفر (Controllers وقاعدة البيانات) غير موجود هنا. الباك إند الحي هو:

`https://lookupfriseur.com`

العنوان الافتراضي مخزّن في `Preferences` تحت المفتاح `ApiBaseUrl`، ويُقرأ من `ApiServices.GetBaseUrl()`. إذا المفتاح فاضي، التطبيق يرجع لنفس العنوان.

كل المسارات تحت `/api`. الطلبات JSON ما عدا تحديث الصورة، والهيدر الثابت:

```http
Accept: application/json
User-Agent: LoukUPmOVE-MAUI/1.0
Accept-Language: ar | en | de
Authorization: Bearer <access_token>
```

`Authorization` يُضاف تلقائياً من `AuthenticatedHttpClientHandler` على عميل `ApiClient`. المسارات العامة لا تأخذ هذا الهيدر:

- `/api/auth/login`
- `/api/auth/register`
- `/api/auth/google`
- `/api/auth/refresh`
- `/api/auth/verify-otp`
- `/api/auth/resend-otp`
- `/api/pages/`

إذا رجع السيرفر `401`، الهاندلر يستدعي `POST /api/auth/refresh` مرة واحدة ويعيد الطلب بنفس الجسم.

أخطاء التحقق عند التطبيق تُقرأ من شكل Laravel الشائع:

```json
{
  "message": "The given data was invalid.",
  "errors": {
    "email": ["The email field is required."]
  }
}
```

`429` يُقرأ من هيدر `Retry-After` بالثواني.

---

## التوكنات التي يتوقعها التطبيق

| الحقل في الرد | التخزين على الجهاز | الاستعمال |
| --- | --- | --- |
| `access_token` | `SecureStorage` / `auth_token` | هيدر Bearer |
| `refresh_token` | `refresh_token` | تجديد الجلسة |
| `access_expires_at` | `access_expires_at` | تجديد قبل الانتهاء بـ 60 ثانية |
| `refresh_expires_at` | `refresh_expires_at` | إذا انتهى يُمسح التخزين |

تسجيل Google يرسل Firebase ID Token مرة واحدة. بعد نجاح `/api/auth/google` التطبيق يحتفظ فقط بتوكن السيرفر.

شكل المستخدم داخل الردود:

```json
{
  "id": 1,
  "name": "Name",
  "email": "user@example.com"
}
```

---

## المصادقة

الملف العميل: `loukupm/services/AuthenticationService.cs`  
النماذج: `loukupm/Model/Auth.cs`

### POST `/api/auth/register`

```json
{
  "first_name": "Ahmad",
  "last_name": "Ali",
  "email": "user@example.com",
  "phone": "+49...",
  "password": "secret",
  "password_confirmation": "secret",
  "registration_method": "email"
}
```

نجاح مع توكن: التطبيق يحفظ `access_token` و `refresh_token`.  
نجاح بدون توكن ومع حقل `otp`: الحساب يحتاج تأكيد الرمز قبل اعتبار الدخول مكتملاً. الرد قد يحتوي `masked_destination`.

`429` يعني محاولات تسجيل كثيرة.

### POST `/api/auth/login`

```json
{
  "email": "user@example.com",
  "password": "secret",
  "registration_method": "email"
}
```

نجاح `2xx` مع `access_token`.  
`401` عند التطبيق يعني إيميل أو كلمة سر غير صحيحة.

### POST `/api/auth/google`

جسم الطلب يكرر نفس Firebase ID Token باسمين حتى يقبل السيرفر أياً منهما:

```json
{
  "token": "<firebase-id-token>",
  "id_token": "<firebase-id-token>",
  "user_id": "<firebase-uid>",
  "email": "user@gmail.com",
  "display_name": "Ahmad"
}
```

الرد يُقرأ أولاً كـ `LoginResponse` (`access_token`). إذا الحقل فاضي يُقرأ كـ `RegisterResponse` (`access_token` أيضاً). غياب التوكن بعد `2xx` يُعتبر فشلاً ويُسجَّل خروج من Firebase.

### POST `/api/auth/refresh`

```json
{ "refresh_token": "<refresh>" }
```

إذا التوكن فاضي أو `refresh_expires_at` انتهى، التطبيق يمسح الجلسة ولا يرسل الطلب. الرد الناجح يحفظ توكن الوصول الجديد.

### POST `/api/auth/verify-otp`

حقل الرمز اسمه `otp` وطوله 6. ليس `code`.

```json
{
  "email": "user@example.com",
  "phone": null,
  "otp": "123456",
  "registration_method": "email"
}
```

`registration_method` هو `email` أو `phone`، والسجل المطابق هو الحقل المطلوب.  
`422` تحقق خاطئ. `403` حساب غير مؤكد أو رمز منتهٍ.  
عند وجود `access_token` يُحفظ مثل تسجيل الدخول.

### POST `/api/auth/resend-verification-otp`

```json
{
  "email": "user@example.com",
  "phone": null,
  "registration_method": "email"
}
```

الرد المتوقع: `message` و `resend_after`. `429` يعني الانتظار قبل إعادة الإرسال.

### POST `/api/auth/forgot-password`

من `RestPassword.xaml.cs`. بدون Bearer.

```json
{
  "email": "user@example.com",
  "registration_method": "email"
}
```

`404` أو نص `USER_NOT_FOUND`: الإيميل غير موجود.  
`400` أو `422`: الإيميل غير صالح.  
`429`: كثرة الطلبات.  
التطبيق لا يملأ رمز OTP من الرد. المستخدم يدخله من الإيميل.

### POST `/api/auth/reset-password`

من `EditPasswordVerification.xaml.cs`. بدون Bearer.

```json
{
  "registration_method": "email",
  "email": "user@example.com",
  "otp": "123456",
  "password": "new-secret",
  "password_confirmation": "new-secret"
}
```

`404` / `USER_NOT_FOUND`: المستخدم غير موجود.  
رد يصف OTP غير صالح أو منتهٍ: الرمز مرفوض.  
`429`: الانتظار.

---

## الملف الشخصي

الملف: `loukupm/services/ApiServices.cs`  
كل هذه المسارات تحتاج Bearer.

### GET `/api/profile`

يرجع كائناً فيه `data` لبيانات المستخدم الحالي.

### POST `/api/profile`

`multipart/form-data` وليس JSON.

| الحقل | متى يُرسل |
| --- | --- |
| `first_name` | إذا الاسم غير فاضي |
| `phone` | إذا الرقم غير فاضي |
| `image` | ملف الصورة إذا المسار موجود. الأنواع: jpeg, png, gif, bmp, webp |

### DELETE `/api/profile`

```json
{
  "current_password": "secret",
  "confirmation": true
}
```

`422` عند التطبيق يعني كلمة السر الحالية غلط. `401` جلسة غير صالحة.

### POST `/api/profile/change-password`

```json
{
  "current_password": "old",
  "password": "new",
  "password_confirmation": "new"
}
```

### POST `/api/profile/phone/send-otp`

```json
{ "phone": "+49..." }
```

### POST `/api/profile/phone/verify-otp`

```json
{
  "phone": "+49...",
  "otp": "123456"
}
```

`400` و `422` أخطاء تحقق. `403` فشل التحقق. `429` مع `Retry-After`. الحجز في التطبيق لا يكتمل قبل ما يعتبر رقم الهاتف مؤكداً.

---

## الحجز والمواعيد

### GET `/api/services`

```text
/api/services?category_id=&featured=&search=&sort_by=sort_order&sort_direction=asc&per_page=15&lang={ar|en|de}
```

### GET `/api/providers`

```text
/api/providers?search=&branch_id=1&service_id=&sort_by=first_name&sort_direction=asc&per_page=15
```

قائمة الفريق. الفرع المستخدم في هذا الاستدعاء ثابت: `branch_id=1`.

### GET `/api/providers/{providerId}/services`

خدمات مزوّد واحد.

### GET `/api/availability/provider`

```text
/api/availability/provider?service_id={id}&provider_id={id}&date=yyyy-MM-dd&branch_id={id}
```

`branch_id` اختياري في `GetProviderAvailabilityAsync`. استدعاء آخر في `AppViweModel` يرسل `branch_id=1` دائماً. التاريخ بصيغة `yyyy-MM-dd`.

### GET `/api/appointments`

```text
/api/appointments?user_id={id}&status={status}
```

### POST `/api/bookings`

الجسم من `Booking.ToCreateBookingPayload`:

```json
{
  "date": "2026-10-17",
  "payment_method": "cash",
  "notes": "optional",
  "reminder_offset_hours": 24,
  "services": [
    {
      "service_id": 3,
      "provider_id": 8,
      "start_time": "14:30"
    }
  ]
}
```

`notes` يُحذف إذا فاضي. `reminder_offset_hours` يُرسل فقط إذا القيمة أكبر من صفر. كل الخدمات في الحجز لازم تكون بنفس اليوم. مدة الخدمة على العميل بين 30 و 480 دقيقة إذا القيمة القادمة من السيرفر خارج هذا المدى.

`201` نجاح. `422` رسالة من `message` و `errors`. `401` غير مصرح. `403` رسالة `message`. `429` مع `Retry-After`.

### POST `/api/bookings/{bookingId}/cancel`

```json
{ "cancellation_reason": "reason text" }
```

---

## التذكيرات

المسار الرسمي في `ApiServices`:

### GET `/api/appointments/reminders/options?lang={code}`

الهيدر `Accept-Language` بنفس اللغة (`ar` أو `en` أو `de`). الرد المتوقع:

```json
{
  "success": true,
  "data": {
    "options": [{ "offset_hours": 24, "label": "قبل يوم" }],
    "default_offset_hours": 24,
    "has_active_channel": true,
    "channels": [
      { "type": "push", "enabled": true, "deliverable": true }
    ],
    "texts": {}
  }
}
```

القنوات المعروفة في النموذج: `push` و `email` و `sms`. التطبيق لا يثبّت خيارات التذكير محلياً. يقرأها من هذا الرد.

### POST `/api/appointments/reminders`

عملية استبدال: الطلب الجديد يحل مكان التذكير الحالي. لا يُرسل تاريخ مطلق.

```json
{
  "appointment_id": 15,
  "offset_hours": 2
}
```

الرد: `success` و `data` و `message` و `errors` و `error_type` و `channels`. عنصر التذكير يحتوي `id` و `appointment_id` و `remind_at` و `offset_hours` و `status` و `is_active` و `sent_at` و `cancelled_at`.

### GET `/api/appointments/{appointmentId}/reminders`

`200` مع `data: null` يعني لا يوجد تذكير. `404` يُعامل أيضاً كعدم وجود تذكير.

### DELETE `/api/appointments/{appointmentId}/reminders`

`200` و `404` كلاهما نجاح عند التطبيق.

يوجد استدعاء أقدم في `ReminderService` إلى نفس `POST /api/appointments/reminders` بجسم مختلف (`provider_id`, `user_id`, `reminder_time`, `appointment_date`, `minutes_until_appointment`, `status`). المسار الذي تبنيه صفحة الحجز هو `appointment_id` مع `offset_hours`.

---

## الإشعارات والجهاز

### GET `/api/notifications`

```text
/api/notifications?per_page={n}&cursor={cursor}&status=all
```

### GET `/api/notifications/unread`

عدد غير المقروء. النموذج يذكر أيضاً شكل رد `unread-count`.

### POST `/api/notifications/{id}/read`

بدون جسم. يعلّم الإشعار مقروءاً.

### POST `/api/register-device`

### POST `/api/deregister-device`

```json
{
  "player_id": "<onesignal-subscription-id>",
  "device_type": "android"
}
```

`device_type` هو `ios` أو `android`. إذا رجع `422`، التطبيق يعيد الإرسال ويضيف الحقول التي طلبها السيرفر في `errors`، مع الإبقاء على `player_id` و `device_type`.

معرّف OneSignal في التطبيق موجود في `appsettings.json` تحت `OneSignal:AppId`.

---

## الإعدادات والمحتوى

### GET `/api/settings`

وأيضاً:

```text
/api/settings?lang={code}
```

```json
{
  "success": true,
  "data": [
    {
      "key": "notifications_enabled",
      "label": "Notifications",
      "description": "",
      "type": "boolean",
      "group": "notifications",
      "value": true,
      "is_default": false
    }
  ]
}
```

المفاتيح تأتي من السيرفر. الواجهة لا تثبّتها مسبقاً.

### PATCH `/api/settings/{key}`

```json
{ "value": true }
```

الرد: `success` و `message` و `data.key` و `data.value`. `401` يطلق تجديد التوكن ثم إعادة المحاولة.

### صفحات عامة

| الطريقة والمسار | الاستعلام |
| --- | --- |
| GET `/api/sliders/home` | `locale` |
| GET `/api/about-us` | `locale` |
| GET `/api/pages/terms-conditions` | `lang` |
| GET `/api/pages/privacy-policy` | `lang` |
| GET `/api/pages/impressum` | `lang` |

اللغات المستخدمة: `ar` و `en` و `de`. رد سياسة الخصوصية أحياناً يصل كنص JSON داخل نص (double-encoded)، والتطبيق يفك الطبقة الزائدة قبل التحويل.

---

## مسارات غير مربوطة بالسيرفر الحي

هذه الاستدعاءات ما زالت بعناوين تجريبية ولا تمثل باك إند `lookupfriseur.com`:

| المكان | العنوان الحالي |
| --- | --- |
| `AppViweModel` إعادة تعيين كلمة سر قديمة | `https://example.com/api/reset-password` |
| بحث منتجات | `https://center-yazan.com/api/sandbox/products` |
| إنشاء Payment Intent | `https://your-backend.example.com/api/stripe/create-payment-intent` |
| Stripe Checkout | رابط `donate.stripe.com` تجريبي |

مسار كلمة السر الفعلي هو `/api/auth/forgot-password` ثم `/api/auth/reset-password`.

---

## تسلسل الجلسة

```text
register أو login أو google أو verify-otp
        |
        v
السيرفر يرجع access_token + refresh_token + تواريخ الانتهاء
        |
        v
TokenManager يحفظها في SecureStorage
        |
        v
أي طلب محمي: Authorization Bearer
        |
        +-- 401 --> POST /api/auth/refresh --> إعادة الطلب مرة واحدة
        |
        +-- بقاء أقل من 60 ثانية على access --> refresh قبل الإرسال
```

الملفات التي تبني هذه الطلبات:

| الملف | ماذا يغطي |
| --- | --- |
| `loukupm/services/AuthenticationService.cs` | تسجيل، دخول، Google، تجديد |
| `loukupm/services/ApiServices.cs` | OTP، بروفايل، حجز، صفحات، هاتف، تذكير |
| `loukupm/services/AuthenticatedHttpClientHandler.cs` | Bearer وإعادة المحاولة بعد 401 |
| `loukupm/services/TokenManager.cs` | تخزين التوكن والانتهاء |
| `loukupm/services/NotificationService.cs` | قائمة الإشعارات والقراءة |
| `loukupm/services/SettingsService.cs` | قراءة وتعديل الإعدادات |
| `loukupm/services/ReminderService.cs` | جسم التذكير القديم |
| `loukupm/View/RestPassword.xaml.cs` | نسيان كلمة السر |
| `loukupm/View/EditPasswordVerification.xaml.cs` | تعيين كلمة السر بالرمز |
| `loukupm/Model/Auth.cs` | أشكال المصادقة |
| `loukupm/Model/Booking.cs` | جسم إنشاء الحجز |
