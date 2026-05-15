# Shell.ItemTemplate - Copy-Paste Code Snippets

## 📋 Your Current Implementation

```xaml
<Shell.ItemTemplate>
    <DataTemplate>
        <Grid ColumnDefinitions="Auto,*" 
              ColumnSpacing="15" 
              Padding="20,12" 
              BackgroundColor="Transparent"
              VerticalOptions="Center">

            <Image Grid.Column="0"
                   Source="{Binding Icon}"
                   WidthRequest="24"
                   HeightRequest="24"
                   Aspect="AspectFit"
                   Opacity="0.9"
                   VerticalOptions="Center" />

            <Label Grid.Column="1"
                   Text="{Binding Title}"
                   FontSize="14"
                   FontFamily="Oswald"
                   TextColor="White"
                   VerticalOptions="Center"
                   LineBreakMode="TailTruncation"
                   MaxLines="1" />
        </Grid>
    </DataTemplate>
</Shell.ItemTemplate>
```

---

## 🎨 Preset Templates

### Preset 1: Compact Mobile

```xaml
<Shell.ItemTemplate>
    <DataTemplate>
        <Grid ColumnDefinitions="Auto,*" 
              ColumnSpacing="12" 
              Padding="15,10" 
              VerticalOptions="Center">
            <Image Grid.Column="0"
                   Source="{Binding Icon}"
                   WidthRequest="20"
                   HeightRequest="20"
                   Aspect="AspectFit"
                   VerticalOptions="Center" />
            <Label Grid.Column="1"
                   Text="{Binding Title}"
                   FontSize="13"
                   FontFamily="Oswald"
                   TextColor="White"
                   VerticalOptions="Center" />
        </Grid>
    </DataTemplate>
</Shell.ItemTemplate>
```

---

### Preset 2: Spacious Tablet

```xaml
<Shell.ItemTemplate>
    <DataTemplate>
        <Grid ColumnDefinitions="Auto,*" 
              ColumnSpacing="20" 
              Padding="25,15" 
              VerticalOptions="Center">
            <Image Grid.Column="0"
                   Source="{Binding Icon}"
                   WidthRequest="32"
                   HeightRequest="32"
                   Aspect="AspectFit"
                   VerticalOptions="Center" />
            <Label Grid.Column="1"
                   Text="{Binding Title}"
                   FontSize="16"
                   FontFamily="Oswald"
                   TextColor="White"
                   VerticalOptions="Center" />
        </Grid>
    </DataTemplate>
</Shell.ItemTemplate>
```

---

### Preset 3: Dark Theme with Accent

```xaml
<Shell.ItemTemplate>
    <DataTemplate>
        <Grid ColumnDefinitions="Auto,*" 
              ColumnSpacing="15" 
              Padding="20,12"
              BackgroundColor="#1a1a1a"
              VerticalOptions="Center">
            <Image Grid.Column="0"
                   Source="{Binding Icon}"
                   WidthRequest="24"
                   HeightRequest="24"
                   Aspect="AspectFit"
                   Opacity="0.8"
                   VerticalOptions="Center" />
            <Label Grid.Column="1"
                   Text="{Binding Title}"
                   FontSize="14"
                   FontFamily="Oswald"
                   TextColor="#A8883C"
                   FontAttributes="Bold"
                   VerticalOptions="Center" />
        </Grid>
    </DataTemplate>
</Shell.ItemTemplate>
```

---

### Preset 4: With Right Indicator

```xaml
<Shell.ItemTemplate>
    <DataTemplate>
        <Grid ColumnDefinitions="Auto,*,Auto" 
              ColumnSpacing="15" 
              Padding="20,12" 
              VerticalOptions="Center">
            <Image Grid.Column="0"
                   Source="{Binding Icon}"
                   WidthRequest="24"
                   HeightRequest="24"
                   Aspect="AspectFit"
                   VerticalOptions="Center" />
            <Label Grid.Column="1"
                   Text="{Binding Title}"
                   FontSize="14"
                   FontFamily="Oswald"
                   TextColor="White"
                   VerticalOptions="Center" />
            <!-- Right indicator -->
            <Label Grid.Column="2"
                   Text="›"
                   FontSize="16"
                   TextColor="#A8883C"
                   VerticalOptions="Center"
                   Opacity="0.5" />
        </Grid>
    </DataTemplate>
</Shell.ItemTemplate>
```

---

### Preset 5: Colorful Icons with Background

```xaml
<Shell.ItemTemplate>
    <DataTemplate>
        <Grid ColumnDefinitions="Auto,*" 
              ColumnSpacing="15" 
              Padding="20,12"
              BackgroundColor="#1e1e2e"
              VerticalOptions="Center">
            <!-- Icon in colored frame -->
            <Frame Grid.Column="0"
                   CornerRadius="6"
                   Padding="6"
                   BackgroundColor="#A8883C"
                   BorderColor="Transparent"
                   HasShadow="False"
                   VerticalOptions="Center">
                <Image Source="{Binding Icon}"
                       WidthRequest="20"
                       HeightRequest="20"
                       Aspect="AspectFit" />
            </Frame>
            <Label Grid.Column="1"
                   Text="{Binding Title}"
                   FontSize="14"
                   FontFamily="Oswald"
                   TextColor="White"
                   VerticalOptions="Center" />
        </Grid>
    </DataTemplate>
</Shell.ItemTemplate>
```

---

## 🔄 Complete Shell Examples

### Full Example: Professional App

```xaml
<Shell x:Class="loukupm.AppShell"
       xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       Shell.FlyoutBehavior="Flyout"
       Shell.FlyoutBackgroundColor="#121416">

    <!-- Custom Item Template -->
    <Shell.ItemTemplate>
        <DataTemplate>
            <Grid ColumnDefinitions="Auto,*" 
                  ColumnSpacing="15" 
                  Padding="20,12" 
                  VerticalOptions="Center">
                <Image Grid.Column="0"
                       Source="{Binding Icon}"
                       WidthRequest="24"
                       HeightRequest="24"
                       Aspect="AspectFit"
                       Opacity="0.9"
                       VerticalOptions="Center" />
                <Label Grid.Column="1"
                       Text="{Binding Title}"
                       FontSize="14"
                       FontFamily="Oswald"
                       TextColor="White"
                       VerticalOptions="Center" />
            </Grid>
        </DataTemplate>
    </Shell.ItemTemplate>

    <!-- Flyout Header -->
    <Shell.FlyoutHeader>
        <Grid Padding="20">
            <Label Text="My App" FontSize="24" TextColor="White" FontAttributes="Bold" />
        </Grid>
    </Shell.FlyoutHeader>

    <!-- Main Navigation -->
    <TabBar>
        <ShellContent Title="Home" Icon="home.png" ContentTemplate="{DataTemplate ...}" />
        <ShellContent Title="Settings" Icon="settings.png" ContentTemplate="{DataTemplate ...}" />
    </TabBar>

    <!-- Flyout Items -->
    <FlyoutItem Title="About" Icon="info.png">
        <ShellContent ContentTemplate="{DataTemplate ...}" />
    </FlyoutItem>

    <MenuFlyoutItem Text="────────────" IsEnabled="False" />

    <FlyoutItem Title="Privacy" Icon="lock.png">
        <ShellContent ContentTemplate="{DataTemplate ...}" />
    </FlyoutItem>

</Shell>
```

---

## 🛠️ Customization Snippets

### Change Icon Size

Replace:
```xaml
WidthRequest="24"
HeightRequest="24"
```

With:
```xaml
WidthRequest="32"        <!-- Larger -->
HeightRequest="32"
```

---

### Change Text Size

Replace:
```xaml
FontSize="14"
```

With:
```xaml
FontSize="16"            <!-- Larger -->
```

---

### Change Font

Replace:
```xaml
FontFamily="Oswald"
```

With:
```xaml
FontFamily="Roboto"      <!-- Or any font in your project -->
```

---

### Change Text Color

Replace:
```xaml
TextColor="White"
```

With:
```xaml
TextColor="#A8883C"      <!-- Your accent color -->
```

---

### Increase Spacing

Replace:
```xaml
ColumnSpacing="15"
Padding="20,12"
```

With:
```xaml
ColumnSpacing="20"       <!-- Wider gap -->
Padding="25,15"          <!-- More padding -->
```

---

### Add Bold Text

Replace:
```xaml
<Label Text="{Binding Title}"
       FontSize="14"
       TextColor="White" />
```

With:
```xaml
<Label Text="{Binding Title}"
       FontSize="14"
       TextColor="White"
       FontAttributes="Bold" />    <!-- Add this -->
```

---

### Add Item Background

Replace:
```xaml
<Grid ColumnDefinitions="Auto,*" ...>
```

With:
```xaml
<Grid ColumnDefinitions="Auto,*" 
      BackgroundColor="#1a1a1a">    <!-- Add this -->
```

---

## 📱 Platform-Specific Styling

### iOS Style
```xaml
<Shell.ItemTemplate>
    <DataTemplate>
        <Grid ColumnDefinitions="Auto,*" 
              ColumnSpacing="16" 
              Padding="16,12">
            <Image Grid.Column="0"
                   Source="{Binding Icon}"
                   WidthRequest="28"
                   HeightRequest="28" />
            <Label Grid.Column="1"
                   Text="{Binding Title}"
                   FontSize="16"
                   FontFamily="SFProDisplay" />
        </Grid>
    </DataTemplate>
</Shell.ItemTemplate>
```

---

### Android Style
```xaml
<Shell.ItemTemplate>
    <DataTemplate>
        <Grid ColumnDefinitions="Auto,*" 
              ColumnSpacing="12" 
              Padding="20,16">
            <Image Grid.Column="0"
                   Source="{Binding Icon}"
                   WidthRequest="24"
                   HeightRequest="24" />
            <Label Grid.Column="1"
                   Text="{Binding Title}"
                   FontSize="14"
                   FontFamily="Roboto" />
        </Grid>
    </DataTemplate>
</Shell.ItemTemplate>
```

---

## 🎯 Testing Checklist

```csharp
// Test in your code-behind or XAML
private void TestItemTemplate()
{
    // Add test FlyoutItem
    var testItem = new FlyoutItem 
    { 
        Title = "Test Item", 
        Icon = "test_icon.png" 
    };

    // Verify template applies
    // Verify icon displays
    // Verify text displays
    // Verify spacing
    // Verify navigation works
}
```

---

## 🚀 Ready to Deploy

Your template is:
✅ Production-ready
✅ Fully customizable
✅ Easy to maintain
✅ Professional appearance
✅ Consistent across all items

Pick a preset, customize as needed, and you're done! 🎉
