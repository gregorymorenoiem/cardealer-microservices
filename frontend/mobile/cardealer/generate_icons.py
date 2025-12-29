#!/usr/bin/env python3
"""
CarDealer App Icon Generator
Generates app icons for iOS and Android with brand colors
"""

import os
from pathlib import Path

try:
    from PIL import Image, ImageDraw, ImageFont
    PIL_AVAILABLE = True
except ImportError:
    PIL_AVAILABLE = False
    print("⚠️  Pillow not installed. Install with: pip install Pillow")

# Brand Colors
DEEP_BLUE = "#001F54"
VIBRANT_ORANGE = "#FF6B35"
GOLD = "#FFD700"
WHITE = "#FFFFFF"

def hex_to_rgb(hex_color):
    """Convert hex color to RGB tuple"""
    hex_color = hex_color.lstrip('#')
    return tuple(int(hex_color[i:i+2], 16) for i in (0, 2, 4))

def create_app_icon(size=1024, output_path="app_icon.png"):
    """
    Create CarDealer app icon with car badge design
    
    Design: Circular badge with deep blue gradient background,
    stylized car silhouette in white/gold, orange accent ring
    """
    if not PIL_AVAILABLE:
        print("❌ Cannot generate icon: Pillow library not available")
        return False
    
    # Create image with transparency
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Colors
    bg_color = hex_to_rgb(DEEP_BLUE)
    accent_color = hex_to_rgb(VIBRANT_ORANGE)
    gold_color = hex_to_rgb(GOLD)
    white_color = hex_to_rgb(WHITE)
    
    # Draw background circle (solid deep blue)
    margin = size * 0.05
    draw.ellipse(
        [margin, margin, size - margin, size - margin],
        fill=bg_color,
        outline=accent_color,
        width=int(size * 0.02)
    )
    
    # Draw stylized car silhouette
    car_margin = size * 0.25
    car_top = size * 0.4
    car_bottom = size * 0.65
    
    # Car body (simplified rectangle with rounded corners)
    car_width = size * 0.5
    car_x_start = (size - car_width) / 2
    car_x_end = car_x_start + car_width
    
    # Main car body
    draw.rounded_rectangle(
        [car_x_start, car_top, car_x_end, car_bottom],
        radius=int(size * 0.05),
        fill=white_color
    )
    
    # Car roof (smaller rectangle on top)
    roof_width = car_width * 0.6
    roof_x_start = (size - roof_width) / 2
    roof_x_end = roof_x_start + roof_width
    roof_top = car_top - size * 0.1
    
    draw.rounded_rectangle(
        [roof_x_start, roof_top, roof_x_end, car_top],
        radius=int(size * 0.03),
        fill=white_color
    )
    
    # Wheels (two gold circles)
    wheel_radius = size * 0.08
    wheel_y = car_bottom - wheel_radius * 0.5
    wheel_left_x = car_x_start + wheel_radius * 1.5
    wheel_right_x = car_x_end - wheel_radius * 1.5
    
    # Left wheel
    draw.ellipse(
        [wheel_left_x - wheel_radius, wheel_y - wheel_radius,
         wheel_left_x + wheel_radius, wheel_y + wheel_radius],
        fill=gold_color
    )
    
    # Right wheel
    draw.ellipse(
        [wheel_right_x - wheel_radius, wheel_y - wheel_radius,
         wheel_right_x + wheel_radius, wheel_y + wheel_radius],
        fill=gold_color
    )
    
    # Add headlight accent (orange dot)
    headlight_radius = size * 0.03
    headlight_x = car_x_start + headlight_radius * 2
    headlight_y = car_top + (car_bottom - car_top) * 0.3
    
    draw.ellipse(
        [headlight_x - headlight_radius, headlight_y - headlight_radius,
         headlight_x + headlight_radius, headlight_y + headlight_radius],
        fill=accent_color
    )
    
    # Save image
    # For iOS: Convert to RGB (no alpha)
    if "ios" in output_path.lower() or output_path.endswith("app_icon.png"):
        rgb_img = Image.new('RGB', img.size, bg_color)
        rgb_img.paste(img, mask=img.split()[3] if img.mode == 'RGBA' else None)
        rgb_img.save(output_path, 'PNG', quality=100)
        print(f"✅ Created iOS icon: {output_path}")
    else:
        img.save(output_path, 'PNG', quality=100)
        print(f"✅ Created Android icon: {output_path}")
    
    return True

def create_adaptive_foreground(size=1024, output_path="app_icon_foreground.png"):
    """
    Create Android adaptive icon foreground
    (same as main icon but with transparency)
    """
    if not PIL_AVAILABLE:
        return False
    
    # Create with transparency
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Colors
    accent_color = hex_to_rgb(VIBRANT_ORANGE)
    gold_color = hex_to_rgb(GOLD)
    white_color = hex_to_rgb(WHITE)
    
    # Draw only the car elements (no background circle)
    # This will be composed with the solid blue background
    
    # Car body
    car_margin = size * 0.25
    car_top = size * 0.4
    car_bottom = size * 0.65
    car_width = size * 0.5
    car_x_start = (size - car_width) / 2
    car_x_end = car_x_start + car_width
    
    # Main car body
    draw.rounded_rectangle(
        [car_x_start, car_top, car_x_end, car_bottom],
        radius=int(size * 0.05),
        fill=white_color
    )
    
    # Car roof
    roof_width = car_width * 0.6
    roof_x_start = (size - roof_width) / 2
    roof_x_end = roof_x_start + roof_width
    roof_top = car_top - size * 0.1
    
    draw.rounded_rectangle(
        [roof_x_start, roof_top, roof_x_end, car_top],
        radius=int(size * 0.03),
        fill=white_color
    )
    
    # Wheels
    wheel_radius = size * 0.08
    wheel_y = car_bottom - wheel_radius * 0.5
    wheel_left_x = car_x_start + wheel_radius * 1.5
    wheel_right_x = car_x_end - wheel_radius * 1.5
    
    draw.ellipse(
        [wheel_left_x - wheel_radius, wheel_y - wheel_radius,
         wheel_left_x + wheel_radius, wheel_y + wheel_radius],
        fill=gold_color
    )
    
    draw.ellipse(
        [wheel_right_x - wheel_radius, wheel_y - wheel_radius,
         wheel_right_x + wheel_radius, wheel_y + wheel_radius],
        fill=gold_color
    )
    
    # Headlight accent
    headlight_radius = size * 0.03
    headlight_x = car_x_start + headlight_radius * 2
    headlight_y = car_top + (car_bottom - car_top) * 0.3
    
    draw.ellipse(
        [headlight_x - headlight_radius, headlight_y - headlight_radius,
         headlight_x + headlight_radius, headlight_y + headlight_radius],
        fill=accent_color
    )
    
    # Add orange accent ring
    margin = size * 0.05
    draw.ellipse(
        [margin, margin, size - margin, size - margin],
        fill=None,
        outline=accent_color,
        width=int(size * 0.02)
    )
    
    img.save(output_path, 'PNG', quality=100)
    print(f"✅ Created Android adaptive foreground: {output_path}")
    return True

def main():
    """Generate all required app icons"""
    script_dir = Path(__file__).parent
    icons_dir = script_dir / "icons"
    icons_dir.mkdir(exist_ok=True)
    
    print("🎨 CarDealer App Icon Generator")
    print("=" * 50)
    
    if not PIL_AVAILABLE:
        print("\n❌ Pillow library is required to generate icons")
        print("📦 Install with: pip install Pillow")
        print("\nAlternatively, you can:")
        print("1. Design the icon in Figma/Adobe XD")
        print("2. Export as 1024x1024 PNG")
        print("3. Save to: frontend/mobile/cardealer/assets/icons/")
        return
    
    # Generate main app icon (1024x1024 for iOS)
    icon_path = icons_dir / "app_icon.png"
    success_main = create_app_icon(1024, str(icon_path))
    
    # Generate adaptive foreground (1024x1024 for Android)
    foreground_path = icons_dir / "app_icon_foreground.png"
    success_adaptive = create_adaptive_foreground(1024, str(foreground_path))
    
    if success_main and success_adaptive:
        print("\n" + "=" * 50)
        print("✨ Icon generation complete!")
        print("\n📁 Generated files:")
        print(f"   - {icon_path}")
        print(f"   - {foreground_path}")
        print("\n🚀 Next steps:")
        print("   1. Review the generated icons")
        print("   2. Run: flutter pub get")
        print("   3. Run: flutter pub run flutter_launcher_icons")
        print("   4. Icons will be generated for all platforms")
    else:
        print("\n⚠️  Some icons could not be generated")

if __name__ == "__main__":
    main()
