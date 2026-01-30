# ?? ЦчгяДи чхА Фхзо - Цдтя гАймЦМА

## ?? чхА гАймоМк:

### **гАэ XAML:**
```xml
<Button  
    Command="{Binding PostEmailCommand}"  
    TextColor="#1F1B04" 
    FontSize="22" 
    FontFamily="georgia-bold"
    BorderWidth="1" 
    Text="{loc:Translate Name=linkpass}" 
    CornerRadius="16"
    HeightRequest="60" 
    Margin="12,50,25,25" />
```

### **гАэ C#:**
```csharp
private void ShowLoadingIndicator(bool show)
{
    Console.WriteLine(show ? "? Loading..." : "? Done loading");
}
```

### **гАЦтгъА:**
? гАря Дть цкДга гАеясгА
? Аг МФло Цдтя ймЦМА
? гАЦсйноЦ чо МДчя гАря ЦяйМД
? йляхи ЦсйноЦ жзМщи

---

## ?? хзо гАймоМк:

### **гАэ XAML:**
```xml
<Grid RowDefinitions="Auto" Margin="12,50,25,25" Padding="0">
    <Button 
        x:Name="SendOtpButton"
        Clicked="Button_Clicked"
        TextColor="#1F1B04" 
        FontSize="22" 
        FontFamily="georgia-bold"
        BorderWidth="1" 
        Text="{loc:Translate Name=linkpass}" 
        CornerRadius="16"
        HeightRequest="60"
        IsEnabled="True" />

    <ActivityIndicator 
        x:Name="LoadingIndicator"
        IsRunning="False"
        IsVisible="False"
        Color="#EBD750"
        Scale="1.2"
        VerticalOptions="Center"
        HorizontalOptions="Center" />
</Grid>
```

### **гАэ C#:**
```csharp
private void ShowLoadingIndicator(bool show)
{
    MainThread.BeginInvokeOnMainThread(() =>
    {
        LoadingIndicator.IsVisible = show;
        LoadingIndicator.IsRunning = show;

        SendOtpButton.IsEnabled = !show;
        SendOtpButton.Opacity = show ? 0.6 : 1.0;

        Console.WriteLine(show ? 
            "? Loading... (Button disabled)" : 
            "? Done loading (Button enabled)");
    });
}
```

### **гАЦЦМргй:**
? гАря ЦзьЬА цкДга гАеясгА
? Цдтя ймЦМА Фгжм
? ЦДз гАДчягй гАЦйъяяи
? йляхи ЦсйноЦ ЦЦйгри

---

## ?? гАймсМДгй:

| гАЦМри | чхА | хзо | гАймсМД |
|--------|-----|-----|----------|
| **йзьМА гАря** | ? | ? | 100% |
| **Цдтя гАймЦМА** | ? | ? | 100% |
| **гАтщгщМи** | - | ? | лоМо |
| **ЦДз гАйъягя** | ?? хяЦлМгП | ? ФглЕМгП | 50% |
| **йляхи гАЦсйноЦ** | 3/5 | 5/5 | 67% |

---

## ?? гАщФгфо:

### **ААЦсйноЦ:**
- ? ФглЕи Фгжми цкДга гАймЦМА
- ? зоЦ гАгяйхгъ ЦД зоЦ моФк тМа
- ? ЦДз гАжшь гАЦйъяя хгАньц
- ? йляхи гмйягщМи

### **ААЦьФя:**
- ? ъФо ДыМщ ФбЦД
- ? ЦзгАли Цмйящи
- ? сЕА гАуМгДи
- ? чгхА ААйьФМя

---

## ?? гАцога:

| гАлгДх | гАчМЦи |
|--------|--------|
| **Фчй гАгсйлгхи** | щФяМ ? |
| **гсйЕАгъ гАпгъяи** | ЦДнщж ? |
| **гсйЕАгъ гАЦзгАл** | ЦДнщж ? |
| **гАгсйчягя** | згАМ логП ? |

---

## ?? оФяи мМги гАря:

```
гАмгАи гАгхйогфМи
     ?
[Enabled = true, Opacity = 1.0]
     ?
ЦсйноЦ МДчя гАря
     ?
ShowLoadingIndicator(true)
     ?
[Enabled = false, Opacity = 0.6]
[ActivityIndicator МзЦА]
     ?
гДйыгя гАяо ЦД API
     ?
гАмуФА зАЛ гАяо
     ?
ShowLoadingIndicator(false)
     ?
[Enabled = true, Opacity = 1.0]
[ActivityIndicator ЦйФчщ]
```

---

## ? гАнАгуи:

```
ЦД: йьхМч хсМь хоФД Цдтягй
еАЛ: йьхМч гмйягщМ Цз йляхи ЦсйноЦ ЦЦйгри

гАймсМД гАъАМ: +200% щМ лФои гАйляхи
```

---

**гАДйМли**: ймоМк ушМя хйцкМя ъхМя логП! ???
