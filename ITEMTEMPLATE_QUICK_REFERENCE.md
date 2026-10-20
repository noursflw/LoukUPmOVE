# Shell.ItemTemplate - Quick Reference

## Your Current Configuration

### 📐 Grid Layout
```xaml
<Grid ColumnDefinitions="Auto,*" 
      ColumnSpacing="15" 
      Padding="20,12" 
      VerticalOptions="Center">
```

| Property | Value | Purpose |
|----------|-------|---------|
| `ColumnDefinitions` | `Auto,*` | Col 0: fit icon, Col 1: fill remaining |
| `ColumnSpacing` | `15` | Gap between icon and text (pixels) |
| `Padding` | `20,12` | Horizontal=20, Vertical=12 (pixels) |
| `VerticalOptions` | `Center` | Vertically align all content |

---

## 🖼️ Icon Configuration

```xaml
<Image Grid.Column="0"
       Source="{Binding Icon}"
       WidthRequest="24"
       HeightRequest="24"
       Aspect="AspectFit"
       Opacity="0.9"
       VerticalOptions="Center" />
```

| Property | Value | Customizable | Effect |
|----------|-------|--------------|--------|
| `Source` | `{Binding Icon}` | ✅ Auto-binds | Gets icon from FlyoutItem |
| `WidthRequest` | `24` | ✅ Change to 20-40 | Icon width in pixels |
| `HeightRequest` | `24` | ✅ Match width | Icon height in pixels |
| `Aspect` | `AspectFit` | ⚠️ Keep as-is | Maintains aspect ratio |
| `Opacity` | `0.9` | ✅ Change to 0.7-1.0 | Transparency level |
| `VerticalOptions` | `Center` | ⚠️ Keep as-is | Vertical alignment |

---

## 📝 Label Configuration

```xaml
<Label Grid.Column="1"
       Text="{Binding Title}"
       FontSize="14"
       FontFamily="Oswald"
       TextColor="White"
       VerticalOptions="Center"
       LineBreakMode="TailTruncation"
       MaxLines="1" />
```

| Property | Value | Customizable | Effect |
|----------|-------|--------------|--------|
| `Text` | `{Binding Title}` | ✅ Auto-binds | Gets title from FlyoutItem |
| `FontSize` | `14` | ✅ Change to 12-18 | Text size in points |
| `FontFamily` | `Oswald` | ✅ Use your font | Font style |
| `TextColor` | `White` | ✅ Change to #color | Text color (hex) |
| `VerticalOptions` | `Center` | ⚠️ Keep as-is | Vertical alignment |
| `LineBreakMode` | `TailTruncation` | ⚠️ Keep as-is | Truncates long text with… |
| `MaxLines` | `1` | ⚠️ Keep as-is | Prevents text wrapping |

---

## 🎨 Customization Quick Presets

### Compact Style (Mobile)
```xaml
ColumnSpacing="12" 
Padding="15,10"
<!-- Icon: 20x20, FontSize: 13 -->
```

### Default Style (Balanced)
```xaml
ColumnSpacing="15" 
Padding="20,12"
<!-- Icon: 24x24, FontSize: 14 -->
```

### Spacious Style (Tablet)
```xaml
ColumnSpacing="18" 
Padding="25,15"
<!-- Icon: 28x28, FontSize: 15 -->
```

### Large Icons Style
```xaml
ColumnSpacing="20" 
Padding="25,15"
<!-- Icon: 32x32, FontSize: 16 -->
```

---

## 🔄 Binding Properties Available

From `FlyoutItem`, you can bind to:

```xaml
Text="{Binding Title}"              <!-- FlyoutItem Title -->
Source="{Binding Icon}"             <!-- FlyoutItem Icon -->
IsVisible="{Binding IsVisible}"     <!-- FlyoutItem visibility -->
```

**Example FlyoutItem:**
```xaml
<FlyoutItem Title="About Us" Icon="info.png">
    <!-- These values bind to {Binding Title} and {Binding Icon} -->
</FlyoutItem>
```

---

## ⚡ Common Modifications

### Change Icon Size
```xaml
WidthRequest="32"    <!-- Old: 24 -->
HeightRequest="32"   <!-- Old: 24 -->
```

### Change Text Font Size
```xaml
FontSize="16"        <!-- Old: 14 -->
```

### Increase Spacing
```xaml
ColumnSpacing="20"   <!-- Old: 15 -->
```

### Add Background to Items
```xaml
<Grid BackgroundColor="#222222">    <!-- Old: Transparent -->
```

### Change Text Color
```xaml
TextColor="#A8883C"  <!-- Old: White (your accent color) -->
```

### Add Text Styling
```xaml
<Label FontAttributes="Bold"        <!-- Add this -->
       Text="{Binding Title}" />
```

---

## ✅ Production Checklist

- [ ] Icons display correctly and are properly sized
- [ ] Text aligns vertically with icons
- [ ] Spacing feels balanced
- [ ] No text gets cut off
- [ ] Colors match your design system
- [ ] Tap/navigation still works
- [ ] Tested on phone and tablet
- [ ] Works with light and dark themes

---

## 🚀 You're All Set!

Your Shell.ItemTemplate is now:
✅ Professionally styled
✅ Fully customizable
✅ Production-ready
✅ Consistent across all Flyout items
✅ Easy to maintain
