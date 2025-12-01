#!/bin/bash
# Icon Generator Script for TimePE PWA
# This script creates placeholder icons. Replace with actual branded icons.

cd "$(dirname "$0")"
ICONS_DIR="../src/TimePE.WebApp/wwwroot/icons"
SPLASH_DIR="../src/TimePE.WebApp/wwwroot/splash"

echo "TimePE Icon Generator"
echo "===================="
echo ""

# Check if ImageMagick is installed
if ! command -v convert &> /dev/null; then
    echo "❌ ImageMagick not found. Installing..."
    echo "   Ubuntu/Debian: sudo apt install imagemagick"
    echo "   macOS: brew install imagemagick"
    echo "   Or manually from: https://imagemagick.org/script/download.php"
    exit 1
fi

echo "✅ ImageMagick found"
echo ""

# Create a base SVG icon (simple TimePE logo)
cat > /tmp/timepe-icon.svg << 'EOF'
<?xml version="1.0" encoding="UTF-8"?>
<svg width="512" height="512" xmlns="http://www.w3.org/2000/svg">
  <!-- Background -->
  <rect width="512" height="512" fill="#0a0e17" rx="64"/>
  
  <!-- Clock circle -->
  <circle cx="256" cy="256" r="160" fill="none" stroke="#00d4ff" stroke-width="16"/>
  
  <!-- Clock hands -->
  <line x1="256" y1="256" x2="256" y2="140" stroke="#00d4ff" stroke-width="12" stroke-linecap="round"/>
  <line x1="256" y1="256" x2="340" y2="256" stroke="#00d4ff" stroke-width="8" stroke-linecap="round"/>
  
  <!-- Center dot -->
  <circle cx="256" cy="256" r="12" fill="#00d4ff"/>
  
  <!-- Text -->
  <text x="256" y="440" font-family="Arial, sans-serif" font-size="64" font-weight="bold" 
        fill="#00d4ff" text-anchor="middle">TimePE</text>
</svg>
EOF

echo "📝 Created base SVG icon"

# Generate app icons
echo "🎨 Generating app icons..."
convert /tmp/timepe-icon.svg -resize 72x72 "$ICONS_DIR/icon-72x72.png"
convert /tmp/timepe-icon.svg -resize 96x96 "$ICONS_DIR/icon-96x96.png"
convert /tmp/timepe-icon.svg -resize 128x128 "$ICONS_DIR/icon-128x128.png"
convert /tmp/timepe-icon.svg -resize 144x144 "$ICONS_DIR/icon-144x144.png"
convert /tmp/timepe-icon.svg -resize 152x152 "$ICONS_DIR/icon-152x152.png"
convert /tmp/timepe-icon.svg -resize 192x192 "$ICONS_DIR/icon-192x192.png"
convert /tmp/timepe-icon.svg -resize 384x384 "$ICONS_DIR/icon-384x384.png"
convert /tmp/timepe-icon.svg -resize 512x512 "$ICONS_DIR/icon-512x512.png"

# Generate favicons
convert /tmp/timepe-icon.svg -resize 16x16 "$ICONS_DIR/favicon-16x16.png"
convert /tmp/timepe-icon.svg -resize 32x32 "$ICONS_DIR/favicon-32x32.png"

# Generate Apple touch icon
convert /tmp/timepe-icon.svg -resize 180x180 "$ICONS_DIR/apple-touch-icon.png"

# Generate Microsoft tiles
convert /tmp/timepe-icon.svg -resize 70x70 "$ICONS_DIR/ms-icon-70x70.png"
convert /tmp/timepe-icon.svg -resize 150x150 "$ICONS_DIR/ms-icon-150x150.png"
convert /tmp/timepe-icon.svg -resize 310x310 "$ICONS_DIR/ms-icon-310x310.png"

echo "✅ App icons generated (13 files)"

# Generate iOS splash screens
echo "🎨 Generating iOS splash screens..."

# Create splash screen template
cat > /tmp/splash-template.svg << 'EOF'
<?xml version="1.0" encoding="UTF-8"?>
<svg width="WIDTH" height="HEIGHT" xmlns="http://www.w3.org/2000/svg">
  <rect width="WIDTH" height="HEIGHT" fill="#0a0e17"/>
  <g transform="translate(CX, CY)">
    <circle cx="0" cy="-60" r="80" fill="none" stroke="#00d4ff" stroke-width="8"/>
    <line x1="0" y1="-60" x2="0" y2="-100" stroke="#00d4ff" stroke-width="6" stroke-linecap="round"/>
    <line x1="0" y1="-60" x2="40" y2="-60" stroke="#00d4ff" stroke-width="4" stroke-linecap="round"/>
    <circle cx="0" cy="-60" r="6" fill="#00d4ff"/>
    <text x="0" y="60" font-family="Arial, sans-serif" font-size="48" font-weight="bold" 
          fill="#00d4ff" text-anchor="middle">TimePE</text>
  </g>
</svg>
EOF

# Function to generate splash screen
generate_splash() {
    local width=$1
    local height=$2
    local output=$3
    local cx=$((width / 2))
    local cy=$((height / 2))
    
    sed "s/WIDTH/$width/g; s/HEIGHT/$height/g; s/CX/$cx/g; s/CY/$cy/g" /tmp/splash-template.svg > /tmp/splash-temp.svg
    convert /tmp/splash-temp.svg "$output"
}

# iPhone 5/SE
generate_splash 640 1136 "$SPLASH_DIR/iphone5_splash.png"

# iPhone 6/7/8
generate_splash 750 1334 "$SPLASH_DIR/iphone6_splash.png"

# iPhone 6+/7+/8+
generate_splash 1242 2208 "$SPLASH_DIR/iphoneplus_splash.png"

# iPhone X/XS
generate_splash 1125 2436 "$SPLASH_DIR/iphonex_splash.png"

# iPhone XR
generate_splash 828 1792 "$SPLASH_DIR/iphonexr_splash.png"

# iPhone XS Max
generate_splash 1242 2688 "$SPLASH_DIR/iphonexsmax_splash.png"

# iPad
generate_splash 1536 2048 "$SPLASH_DIR/ipad_splash.png"

# iPad Pro 10.5"
generate_splash 1668 2224 "$SPLASH_DIR/ipadpro1_splash.png"

# iPad Pro 11"
generate_splash 1668 2388 "$SPLASH_DIR/ipadpro3_splash.png"

# iPad Pro 12.9"
generate_splash 2048 2732 "$SPLASH_DIR/ipadpro2_splash.png"

echo "✅ iOS splash screens generated (10 files)"

# Cleanup
rm -f /tmp/timepe-icon.svg /tmp/splash-template.svg /tmp/splash-temp.svg

echo ""
echo "✅ Complete! Generated:"
echo "   - 13 app icons in $ICONS_DIR"
echo "   - 10 splash screens in $SPLASH_DIR"
echo ""
echo "📌 These are placeholder icons. For production:"
echo "   1. Design a proper logo/icon (512x512 PNG or SVG)"
echo "   2. Replace the SVG template in this script"
echo "   3. Re-run: ./docs/generate-icons.sh"
echo ""
echo "🔗 Useful tools:"
echo "   - Favicon Generator: https://realfavicongenerator.net/"
echo "   - PWA Builder: https://www.pwabuilder.com/"
echo "   - Maskable Icon: https://maskable.app/"
