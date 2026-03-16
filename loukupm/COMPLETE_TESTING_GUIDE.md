# ?? Ïáíá ÇáÇÎÊÈÇÑ ÇáÔÇãá - Logout/Login/ProfilePage

## ŞÈá ÇáÈÏÁ ?

```
? Şã ÈÈäÇÁ ÇáãÔÑæÚ: Ctrl+Shift+B
? ÃÛáŞ ÇáÊØÈíŞ ÊãÇãÇğ ãä ÇáÌåÇÒ (Åä ßÇä ãÔÛøáÇğ)
? ÃÚÏ ÊÔÛíá ÇáÊØÈíŞ
? ÇİÊÍ Debug Output: Debug ? Windows ? Output
```

---

## ÇáÇÎÊÈÇÑ 1: ÇáÏÎæá ÇáÃæá ??

### ÇáÎØæÇÊ:
```
1. ?? ÔÛøá ÇáÊØÈíŞ
2. ? ÇäÊÙÑ 2-3 ËæÇä áÊÍãíá ÇáæÇÌåÉ
3. ?? íÌÈ Ãä ÊÑì LoginPage
```

### ãÇ ÊÊæŞÚ ÑÄíÊå İí Console:
```
?? [CheckAuthentication START] _appJustStarted=True
?? Token from SecureStorage: EMPTY
?? No token found ? Navigating to LoginPage
? Navigated to LoginPage
```

### ÇáäÊíÌÉ ÇáãÊæŞÚÉ: ?
- ÇáÊØÈíŞ íÚÑÖ LoginPage
- áÇ ÊæÌÏ ÑÓÇÆá ÎØÃ İí Console

---

## ÇáÇÎÊÈÇÑ 2: ÊÓÌíá ÇáÏÎæá ?

### ÇáÎØæÇÊ:
```
1. ?? ÃÏÎá ÈÑíÏ ÅáßÊÑæäí ÕÍíÍ
2. ?? ÃÏÎá ßáãÉ ãÑæÑ ÕÍíÍÉ
3. ?? ÇÖÛØ "Log In"
4. ? ÇäÊÙÑ 3-5 ËæÇä
```

### ãÇ ÊÊæŞÚ ÑÄíÊå İí Console:
```
?? [LoginSuccess] Deserializing response
?? [LoginSuccess] Saving tokens
?? [LoginSuccess] Showing success popup
?? [LoginSuccess] Navigating to HomePage
? [LoginSuccess] Navigation to HomePage completed
```

### ÇáäÊíÌÉ ÇáãÊæŞÚÉ: ?
- popup "Êã ÊÓÌíá ÇáÏÎæá ÈäÌÇÍ"
- ÈÚÏ ÅÛáÇŞ ÇáÜ popup¡ ÊÕá Åáì HomePage
- ÊÑì ÇáÊÇÈÇÊ (Home, Services, Booking, Profile)

---

## ÇáÇÎÊÈÇÑ 3: ÇáÇäÊŞÇá Åáì ProfilePage ??

### ÇáÎØæÇÊ:
```
1. ?? ÊÃßÏ ãä Ãäß İí HomePage
2. ?? ÇÖÛØ Úáì "Profile" tab (ÂÎÑ tab)
3. ? ÇäÊÙÑ ËÇäíÉ æÇÍÏÉ
```

### ÇáäÊíÌÉ ÇáãÊæŞÚÉ: ?
- ÊÕá Åáì ProfilePage ãÈÇÔÑÉ
- ÊÑì ãÚáæãÇÊ Çáãáİ ÇáÔÎÕí
- ÊÑì ÃÒÑÇÑ (ÊÚÏíá ÇáÍÓÇÈ¡ ÊÛííÑ ßáãÉ ÇáãÑæÑ¡ ÅáÎ)
- **áíÓ LoginPage!** ? ? ?

---

## ÇáÇÎÊÈÇÑ 4: ÊÓÌíá ÇáÎÑæÌ ??

### ÇáÎØæÇÊ:
```
1. ?? ÊÃßÏ ãä Ãäß İí ProfilePage
2. ?? ãÑÑ äÍæ ÇáÃÓİá İí ÇáÕİÍÉ
3. ?? ÇÖÛØ Úáì "Log Out"
4. ? ÇäÊÙÑ ËÇäíÉ æÇÍÏÉ
```

### ãÇ ÊÊæŞÚ ÑÄíÊå İí Console:
```
?? [LogoutButton START]
1?? Calling OneSignalService.Logout()
2?? Removing auth_token
3?? Removing refresh_token
4?? Clearing PageSourceMap
5?? Resetting authentication check
?? Authentication check reset
6?? Showing logout popup
```

### ãÇ ÊÊæŞÚ ÑÄíÊå İí ÇáÊØÈíŞ:
- popup íÙåÑ ãÚ ÎíÇÑÇÊ (ÊÃßíÏ ÇáÎÑæÌ Ãæ ÅáÛÇÁ)

---

## ÇáÇÎÊÈÇÑ 5: ÊÃßíÏ ÇáÎÑæÌ ??

### ÇáÎØæÇÊ:
```
1. ?? ÇÖÛØ "ÊÃßíÏ ÇáÎÑæÌ"
2. ? ÇäÊÙÑ ËÇäíÉ æÇÍÏÉ
```

### ãÇ ÊÊæŞÚ ÑÄíÊå İí Console:
```
?? [LogoutFlow START]
?? Step 1: Close popup
?? Step 2: Wait 300ms
?? Step 3: Navigate to LoginPage
? Successfully navigated to LoginPage
?? [LogoutFlow END]
```

### ÇáäÊíÌÉ ÇáãÊæŞÚÉ: ?
- popup ÊÛáŞ
- ÊÕá Åáì LoginPage
- **ÇáÂä íÌÈ Ãä Êßæä İí LoginPage** ?

---

## ÇáÇÎÊÈÇÑ 6: ÇáÏÎæá ãÑÉ ÃÎÑì (ÇáÇÎÊÈÇÑ ÇáÃÓÇÓí!) ??

### ÇáÎØæÇÊ:
```
1. ?? ÃÏÎá äİÓ ÇáÈíÇäÇÊ ãÑÉ ÃÎÑì
2. ?? ÇÖÛØ "Log In"
3. ? ÇäÊÙÑ 3-5 ËæÇä
```

### ãÇ ÊÊæŞÚ ÑÄíÊå İí Console:
```
?? [CheckAuthentication START] _appJustStarted=False
?? Skipping auth check - app already initialized
?? [LoginSuccess] Deserializing response
?? [LoginSuccess] Saving tokens
?? [LoginSuccess] Showing success popup
?? [LoginSuccess] Navigating to HomePage
? [LoginSuccess] Navigation to HomePage completed
```

**áÇÍÙ:** `_appJustStarted=False` ? - åĞÇ ÕÍíÍ!

### ÇáäÊíÌÉ ÇáãÊæŞÚÉ: ?
- popup "Êã ÊÓÌíá ÇáÏÎæá ÈäÌÇÍ"
- ÈÚÏ ÇáÅÛáÇŞ¡ ÊÕá Åáì HomePage ãÈÇÔÑÉ
- **áíÓ LoginPage!** ?

---

## ÇáÇÎÊÈÇÑ 7: ProfilePage ãÑÉ ÃÎÑì (ÇáÇÎÊÈÇÑ ÇáÃåã!) ??

### ÇáÎØæÇÊ:
```
1. ?? ÊÃßÏ ãä Ãäß İí HomePage
2. ?? ÇÖÛØ Úáì "Profile" tab
3. ? ÇäÊÙÑ ËÇäíÉ æÇÍÏÉ
```

### ÇáäÊíÌÉ ÇáãÊæŞÚÉ: ???
- **ÊÕá Åáì ProfilePage ãÈÇÔÑÉ**
- ÊÑì ãÚáæãÇÊ Çáãáİ ÇáÔÎÕí
- **áíÓ LoginPage!**
- **áíÓ popup ÎØÃ!**
- **áÇ ÊæÌÏ ÑÓÇÆá ÎØÃ!**

**ÅĞÇ æÕáÊ åäÇ = ÇáãÔßáÉ Êã ÍáåÇ ÊãÇãÇğ!** ??

---

## ÇáÇÎÊÈÇÑ 8: ÇáÊäŞá Èíä ÇáÊÇÈÇÊ ??

### ÇáÎØæÇÊ:
```
1. ?? ÇÖÛØ Úáì "Home" tab
2. ?? ÇÖÛØ Úáì "Services" tab
3. ?? ÇÖÛØ Úáì "Booking" tab
4. ?? ÇÖÛØ Úáì "Profile" tab
```

### ÇáäÊíÌÉ ÇáãÊæŞÚÉ: ?
- ÌãíÚ ÇáÊÇÈÇÊ ÊÚãá ÈÔßá ÓáÓ
- áÇ ÊæÌÏ ÑÓÇÆá ÎØÃ
- áÇ ÊÚæÏ Åáì LoginPage

---

## ÇáÇÎÊÈÇÑ 9: ÅÛáÇŞ æÅÚÇÏÉ İÊÍ ÇáÊØÈíŞ ??

### ÇáÎØæÇÊ:
```
1. ?? ÇÖÛØ Úáì ÒÑ ÇáÑÌæÚ Ãæ ÃÛáŞ ÇáÊØÈíŞ
2. ? ÇäÊÙÑ 3 ËæÇä
3. ?? ÃÚÏ ÊÔÛíá ÇáÊØÈíŞ
```

### ãÇ ÊÊæŞÚ ÑÄíÊå İí Console:
```
?? [CheckAuthentication START] _appJustStarted=True
?? Token from SecureStorage: EXISTS
? Token found ? Navigating to HomePage
```

**áÇÍÙ:** `_appJustStarted=True` ? - ŞÇã ÈÇáÊÍŞŞ ãÑÉ ÃÎÑì

### ÇáäÊíÌÉ ÇáãÊæŞÚÉ: ?
- ÇáÊØÈíŞ íİÊÍ ãÈÇÔÑÉ İí HomePage (áÃä Token ãÍİæÙ)
- áÇ ÊĞåÈ Åáì LoginPage
- ÌãíÚ ÇáÊÇÈÇÊ ÊÚãá

---

## ãáÎÕ äÊÇÆÌ ÇáÇÎÊÈÇÑ ?

```
???????????????????????????????????????????????????????
? ÇáÇÎÊÈÇÑ                  ? ÇáäÊíÌÉ       ? ÇáäÕ    ?
???????????????????????????????????????????????????????
? 1. ÇáÏÎæá ÇáÃæá           ? ? LoginPage  ? ÚÇÏí   ?
? 2. ÊÓÌíá ÇáÏÎæá           ? ? HomePage   ? ÚÇÏí   ?
? 3. ProfilePage             ? ? Profile    ? ???   ?
? 4. ÇáÎÑæÌ                  ? ? LoginPage  ? ÚÇÏí   ?
? 5. ÊÃßíÏ ÇáÎÑæÌ            ? ? LoginPage  ? ÚÇÏí   ?
? 6. ÇáÏÎæá ãÑÉ ÃÎÑì        ? ? HomePage   ? ???   ?
? 7. ProfilePage ãÑÉ ÃÎÑì    ? ? Profile    ? ???   ?
? 8. ÇáÊäŞá Èíä ÇáÊÇÈÇÊ      ? ? ÓáÓ       ? ???   ?
? 9. ÅÛáÇŞ/ÅÚÇÏÉ İÊÍ         ? ? HomePage   ? ÚÇÏí   ?
???????????????????????????????????????????????????????

? ÌãíÚ ÇáÇÎÊÈÇÑÇÊ äÌÍÊ = ÇáãÔßáÉ Êã ÍáåÇ! ??
```

---

## ÅĞÇ İÔá Ãí ÇÎÊÈÇÑ ?

### ÇáÎØæÉ 1: ÇŞÑÃ ÑÓÇáÉ ÇáÎØÃ
```
åá ÊÑì:
? [LogoutFlow ERROR]
? [LoginSuccess ERROR]
? [LogoutButton ERROR]
[SecureStorage ERROR]
[App CheckAuthentication ERROR]
```

### ÇáÎØæÉ 2: ÊÍŞŞ ãä ÇáÜ Flags
```
ÃÖİ åĞÇ ÇáßæÏ İí CheckAuthentication():
Console.WriteLine($"FLAGS: _appJustStarted={_appJustStarted}, _authenticationChecked={_authenticationChecked}");
```

### ÇáÎØæÉ 3: ÊÍŞŞ ãä ÇáãáİÇÊ
```
[ ] App.xaml.cs íÍÊæí Úáì _appJustStarted = true
[ ] App.xaml.cs íÍÊæí Úáì _appJustStarted = false
[ ] MassegBoxLogout.xaml.cs íÍÊæí Úáì await Task.Delay(300)
[ ] ProfilePage.xaml.cs íÍÊæí Úáì App.ResetAuthenticationCheck()
```

### ÇáÎØæÉ 4: ÃÚÏ ÇáÈäÇÁ
```
Ctrl+Shift+B (ÈäÇÁ ßÇãá¡ áíÓ Hot Reload)
```

### ÇáÎØæÉ 5: ÇØáÈ ÇáÏÚã
```
ÔÇÑß:
1. ÑÓÇáÉ ÇáÎØÃ ÇáÏŞíŞÉ ãä Console
2. ÑŞã ÇáÇÎÊÈÇÑ ÇáĞí İÔá
3. ÇáÎØæÉ ÈÇáÖÈØ ÇáÊí ÍÏËÊ İíåÇ ÇáãÔßáÉ
```

---

## äÕÇÆÍ ãåãÉ ??

1. **ÇÓÊÎÏã ÌåÇÒ ÍŞíŞí Ãæ Emulator ÍÏíË**
   - ÇáÃÌåÒÉ ÇáŞÏíãÉ ŞÏ Êßæä ÈØíÆÉ

2. **áÇ ÊÓÊÎÏã Hot Reload ÚäÏ ÇáÇÎÊÈÇÑ**
   - ÇÓÊÎÏã Rebuild ßÇãá

3. **ÊÃßÏ ãä Permissions**
   - ÊÃßÏ ãä Ãä ÇáÊØÈíŞ áÏíå ÕáÇÍíÉ SecureStorage

4. **ÍİÙ ÇáÈíÇäÇÊ**
   - ÈÚÏ ÇáÏÎæá ÇáÃæá¡ íÌÈ ÍİÙ Token İí SecureStorage

5. **ÇáÊÓÌíáÇÊ ãåãÉ**
   - ßá ÑÓÇáÉ İí Console ÊÎÈÑß ÈãÇ íÍÏË

---

## ÇáÎØ ÇáäåÇÆí ??

ÅĞÇ äÌÍÊ ÌãíÚ ÇáÇÎÊÈÇÑÇÊ ÃÚáÇå **ÈÇáÊÑÊíÈ**¡ İÇáãÔßáÉ Êã ÍáåÇ ÈäÓÈÉ 100%! 

**ÇáãÔßáÉ ÇáÃÕáíÉ:**
- ÈÚÏ Logout æLogin¡ ProfilePage áÇ íÚãá

**ÇáÍá:**
- ÅÖÇİÉ `_appJustStarted` flag
- ÌÚá `CheckAuthentication()` ÊÚãá ãÑÉ æÇÍÏÉ İŞØ
- ÅÚÇÏÉ ÊÚííä ÇáÜ flag ÚäÏ Logout
- ÊÍÓíä Race Condition İí MassegBoxLogout
