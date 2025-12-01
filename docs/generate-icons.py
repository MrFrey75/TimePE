#!/usr/bin/env python3
"""
TimePE Icon Generator - Python Version
Creates placeholder icons for PWA without requiring ImageMagick
"""

import os
from pathlib import Path

try:
    from PIL import Image, ImageDraw, ImageFont
except ImportError:
    print("❌ PIL/Pillow not found. Installing...")
    print("   Run: pip3 install pillow")
    print("   Or: python3 -m pip install pillow")
    exit(1)

# Directories
script_dir = Path(__file__).parent
icons_dir = script_dir.parent / "src" / "TimePE.WebApp" / "wwwroot" / "icons"
splash_dir = script_dir.parent / "src" / "TimePE.WebApp" / "wwwroot" / "splash"

# Create directories
icons_dir.mkdir(parents=True, exist_ok=True)
splash_dir.mkdir(parents=True, exist_ok=True)

# Colors
BG_COLOR = "#0a0e17"
FG_COLOR = "#00d4ff"

def hex_to_rgb(hex_color):
    """Convert hex color to RGB tuple"""
    hex_color = hex_color.lstrip('#')
    return tuple(int(hex_color[i:i+2], 16) for i in (0, 2, 4))

def create_icon(size, output_path):
    """Create a simple clock icon"""
    img = Image.new('RGB', (size, size), hex_to_rgb(BG_COLOR))
    draw = ImageDraw.Draw(img)
    
    # Clock circle
    margin = size // 8
    draw.ellipse([margin, margin, size - margin, size - margin], 
                 outline=hex_to_rgb(FG_COLOR), width=max(2, size // 32))
    
    # Clock hands
    center = size // 2
    hand_len = size // 4
    # Hour hand (pointing up)
    draw.line([(center, center), (center, center - hand_len)], 
              fill=hex_to_rgb(FG_COLOR), width=max(2, size // 64))
    # Minute hand (pointing right)
    draw.line([(center, center), (center + hand_len, center)], 
              fill=hex_to_rgb(FG_COLOR), width=max(1, size // 96))
    
    # Center dot
    dot_size = max(2, size // 42)
    draw.ellipse([center - dot_size, center - dot_size, 
                  center + dot_size, center + dot_size], 
                 fill=hex_to_rgb(FG_COLOR))
    
    # Save
    img.save(output_path, 'PNG')
    return True

def create_splash(width, height, output_path):
    """Create a splash screen with logo and text"""
    img = Image.new('RGB', (width, height), hex_to_rgb(BG_COLOR))
    draw = ImageDraw.Draw(img)
    
    # Center position
    cx, cy = width // 2, height // 2
    
    # Clock icon (smaller)
    radius = min(width, height) // 8
    draw.ellipse([cx - radius, cy - radius - 60, cx + radius, cy + radius - 60], 
                 outline=hex_to_rgb(FG_COLOR), width=max(4, radius // 20))
    
    # Clock hands
    hand_len = radius * 2 // 3
    draw.line([(cx, cy - 60), (cx, cy - 60 - hand_len)], 
              fill=hex_to_rgb(FG_COLOR), width=max(3, radius // 20))
    draw.line([(cx, cy - 60), (cx + hand_len // 2, cy - 60)], 
              fill=hex_to_rgb(FG_COLOR), width=max(2, radius // 30))
    
    # Center dot
    dot_size = max(3, radius // 13)
    draw.ellipse([cx - dot_size, cy - 60 - dot_size, 
                  cx + dot_size, cy - 60 + dot_size], 
                 fill=hex_to_rgb(FG_COLOR))
    
    # Text (using default font since custom fonts may not be available)
    try:
        font_size = min(width, height) // 15
        font = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf", font_size)
    except:
        # Fallback to default font
        font = ImageFont.load_default()
    
    text = "TimePE"
    # Get text bounding box
    bbox = draw.textbbox((0, 0), text, font=font)
    text_width = bbox[2] - bbox[0]
    text_height = bbox[3] - bbox[1]
    
    # Draw text centered below icon
    text_x = cx - text_width // 2
    text_y = cy + 40
    draw.text((text_x, text_y), text, fill=hex_to_rgb(FG_COLOR), font=font)
    
    # Save
    img.save(output_path, 'PNG')
    return True

print("TimePE Icon Generator (Python)")
print("================================")
print("")

# Generate app icons
print("🎨 Generating app icons...")
icon_sizes = [72, 96, 128, 144, 152, 192, 384, 512]
for size in icon_sizes:
    create_icon(size, icons_dir / f"icon-{size}x{size}.png")

# Generate favicons
create_icon(16, icons_dir / "favicon-16x16.png")
create_icon(32, icons_dir / "favicon-32x32.png")

# Generate Apple touch icon
create_icon(180, icons_dir / "apple-touch-icon.png")

# Generate Microsoft tiles
create_icon(70, icons_dir / "ms-icon-70x70.png")
create_icon(150, icons_dir / "ms-icon-150x150.png")
create_icon(310, icons_dir / "ms-icon-310x310.png")

print(f"✅ App icons generated (13 files) in {icons_dir}")

# Generate iOS splash screens
print("🎨 Generating iOS splash screens...")
splash_configs = [
    (640, 1136, "iphone5_splash.png"),
    (750, 1334, "iphone6_splash.png"),
    (1242, 2208, "iphoneplus_splash.png"),
    (1125, 2436, "iphonex_splash.png"),
    (828, 1792, "iphonexr_splash.png"),
    (1242, 2688, "iphonexsmax_splash.png"),
    (1536, 2048, "ipad_splash.png"),
    (1668, 2224, "ipadpro1_splash.png"),
    (1668, 2388, "ipadpro3_splash.png"),
    (2048, 2732, "ipadpro2_splash.png"),
]

for width, height, filename in splash_configs:
    create_splash(width, height, splash_dir / filename)

print(f"✅ iOS splash screens generated (10 files) in {splash_dir}")
print("")
print("✅ Complete! All icons and splash screens generated.")
print("")
print("📌 These are placeholder icons. For production:")
print("   1. Design a proper logo/icon")
print("   2. Use a tool like Figma/Illustrator to create professional assets")
print("   3. Or use online generators:")
print("      - https://realfavicongenerator.net/")
print("      - https://www.pwabuilder.com/")
print("      - https://maskable.app/")
