# CarDealer App Icon Design Specification

## Design Concept
Modern, professional car marketplace icon with brand colors

## Color Palette
- **Primary Background**: #001F54 (Deep Blue)
- **Accent**: #FF6B35 (Vibrant Orange)
- **Premium**: #FFD700 (Gold)
- **White**: #FFFFFF

## Icon Elements

### Main Design (1024x1024)
1. **Background**: Deep blue gradient (#001F54 → #002A75)
2. **Main Element**: Stylized car silhouette in white/gold
3. **Accent**: Orange speed lines or location pin
4. **Style**: Minimalist, modern, recognizable at small sizes

### Design Variations

#### Option 1: Car Badge
```
- Circular badge with deep blue background
- White/gold car icon in center
- Orange ring or border accent
```

#### Option 2: Location + Car
```
- Location pin with car inside
- Deep blue pin outline
- Orange fill with white car silhouette
```

#### Option 3: Abstract "C" + Car
```
- Stylized "C" letter forming a car shape
- Gradient from blue to orange
- Gold highlight details
```

## Technical Requirements

### iOS
- **App Icon**: 1024x1024px (PNG, no alpha)
- **Sizes**: Generated automatically by flutter_launcher_icons
  - 180x180 (@3x)
  - 120x120 (@2x)
  - 60x60 (@1x)

### Android
- **Legacy Icon**: 512x512px (PNG with transparency)
- **Adaptive Icon**: 
  - Foreground: 1024x1024px (transparent PNG)
  - Background: Solid color #001F54
- **Sizes**: Generated for all densities (mdpi to xxxhdpi)

## Export Settings
- **Format**: PNG
- **Color Space**: sRGB
- **Bit Depth**: 8-bit
- **Compression**: Optimized for size
- **No alpha channel for iOS** (requirement)

## File Naming
- `app_icon.png` - Main 1024x1024 icon for iOS
- `app_icon_foreground.png` - Foreground for Android adaptive icon
- `app_icon_legacy.png` - Legacy Android icon (optional)

## Brand Consistency
Ensure icon aligns with:
- Splash screen colors
- App theme colors
- Marketing materials
- Professional automotive industry standards
