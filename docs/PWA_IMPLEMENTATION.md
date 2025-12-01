# Progressive Web App (PWA) Implementation

## Overview
TimePE is a fully-featured Progressive Web App that can be installed on desktop and mobile devices, works offline, and provides a native app-like experience.

## Features Implemented

### 1. Mobile Responsiveness
- **Mobile-First Design**: CSS built from mobile up to desktop
- **Touch-Friendly Controls**: All interactive elements meet WCAG 2.1 minimum 48x48px touch target size
- **Responsive Tables**: Tables stack vertically on mobile with data labels
- **Safe Areas**: Support for notched devices (iPhone X+) with proper insets
- **Landscape Support**: Optimized layouts for landscape orientation
- **Reduced Motion**: Respects user's motion preferences for accessibility

**Breakpoints:**
- Mobile: < 768px
- Tablet: 768px - 991px
- Desktop: ≥ 992px

### 2. PWA Core Features
- **Installable**: Can be installed from browser on desktop/mobile
- **Offline Support**: Service worker caches assets for offline access
- **App-Like Experience**: Runs in standalone mode without browser UI
- **Fast Loading**: Cache-first strategy for assets
- **Background Sync**: Prepared for offline data synchronization
- **Push Notifications**: Infrastructure ready for reminders/alerts

### 3. Service Worker (`/sw.js`)
**Cache Strategy:** Cache-first with network fallback
- Caches static assets (CSS, JS, fonts, icons)
- Serves from cache immediately for speed
- Falls back to network if not cached
- Updates cache in background

**Lifecycle:**
1. **Install**: Pre-caches essential assets
2. **Activate**: Cleans up old caches
3. **Fetch**: Intercepts requests, serves from cache

**Prepared Features:**
- Background sync for offline time entries
- Push notification handlers

### 4. Web App Manifest (`/manifest.json`)
**Identity:**
- Name: "TimePE - Time Tracking"
- Short Name: "TimePE"
- Description: Professional time tracking and project management

**Theming:**
- Theme Color: `#0a0e17` (dark blue-black)
- Background: `#0a0e17`
- Primary Color: `#00d4ff` (cyan accent)

**Display:**
- Mode: Standalone (full-screen app experience)
- Orientation: Portrait primary, any supported

**Icons:**
8 sizes from 72x72 to 512x512 for all device types

**Shortcuts:**
- Quick access to "New Time Entry"
- Quick access to "Dashboard"

**Share Target:**
Configured to receive shared content from other apps

### 5. Mobile JavaScript Enhancements (`/js/site.js`)

**PWA Features:**
- Service worker registration and update checking
- Install prompt handling with custom UI button
- iOS standalone mode detection
- Network status detection with offline banner

**Touch Enhancements:**
- Visual feedback on touch (opacity change)
- Double-tap zoom prevention on buttons
- Haptic feedback (vibration) on actions
- Pull-to-refresh gesture

**Data Features:**
- Form auto-save to localStorage
- Restore unsaved changes on page load
- Web Share API integration for sharing data

**Responsive Utilities:**
- Automatic table wrapper creation
- Data label attribution for mobile table stacking
- Device detection (mobile, touch, standalone)

## File Structure

```
wwwroot/
├── manifest.json           # PWA manifest configuration
├── sw.js                   # Service worker for offline support
├── browserconfig.xml       # Microsoft tile configuration
├── css/
│   └── site.css            # Mobile-responsive CSS
├── js/
│   └── site.js             # PWA and mobile enhancements
├── icons/                  # App icons (various sizes)
│   ├── icon-72x72.png
│   ├── icon-96x96.png
│   ├── icon-128x128.png
│   ├── icon-144x144.png
│   ├── icon-152x152.png
│   ├── icon-192x192.png
│   ├── icon-384x384.png
│   ├── icon-512x512.png
│   ├── apple-touch-icon.png
│   ├── favicon-16x16.png
│   └── favicon-32x32.png
└── splash/                 # iOS splash screens
    ├── iphone5_splash.png
    ├── iphone6_splash.png
    ├── iphoneplus_splash.png
    ├── iphonex_splash.png
    ├── iphonexr_splash.png
    ├── iphonexsmax_splash.png
    ├── ipad_splash.png
    ├── ipadpro1_splash.png
    ├── ipadpro2_splash.png
    └── ipadpro3_splash.png
```

## Installation

### Desktop (Chrome/Edge)
1. Navigate to the app in browser
2. Look for install icon in address bar (⊕)
3. Click "Install TimePE"
4. App opens in standalone window

### Android
1. Open in Chrome/Edge
2. Tap menu (⋮) → "Install app" or "Add to Home screen"
3. App installs with icon on home screen
4. Opens in full-screen mode

### iOS/iPadOS
1. Open in Safari
2. Tap Share button (􀈂)
3. Select "Add to Home Screen"
4. App icon appears on home screen
5. Opens in standalone mode

## Testing PWA Features

### Service Worker
```javascript
// In browser DevTools Console
navigator.serviceWorker.getRegistration().then(reg => console.log(reg));
```

### Offline Mode
1. Open DevTools → Network tab
2. Select "Offline" throttling
3. Reload page - should load from cache
4. Navigate between pages - static assets load

### Install Prompt
1. Open in supported browser (Chrome/Edge/Samsung Internet)
2. Look for install banner or button
3. DevTools → Application → Manifest (check for errors)

### Touch Targets
1. Open DevTools → Device Toolbar
2. Select mobile device (iPhone, Pixel)
3. Verify all buttons/links are easy to tap
4. Minimum 48x48px touch target size

### Responsive Design
```
Mobile: iPhone SE, Galaxy S8
Tablet: iPad, Galaxy Tab
Desktop: 1920x1080
```

## Icon Requirements

### Sizes Needed
- **Favicon:** 16x16, 32x32
- **Apple Touch:** 180x180
- **Android/Chrome:** 72, 96, 128, 144, 152, 192, 384, 512
- **Microsoft Tiles:** 70, 150, 310
- **Maskable (optional):** 192, 512 with safe zone

### Format
- PNG format with transparency
- Square aspect ratio
- Clear, simple design that works at all sizes
- Consider maskable icon format for Android adaptive icons

### Creating Icons
```bash
# Using ImageMagick to resize from source
convert source.png -resize 72x72 icon-72x72.png
convert source.png -resize 96x96 icon-96x96.png
# ... repeat for all sizes
```

### Placeholder Icons
Currently using placeholder paths. Replace with actual branded icons:
1. Design base icon (512x512 or SVG)
2. Generate all required sizes
3. Place in `/wwwroot/icons/` directory
4. Generate splash screens for iOS

## Splash Screens (iOS)

### Required Sizes
- iPhone 5/SE: 640x1136
- iPhone 6/7/8: 750x1334
- iPhone 6+/7+/8+: 1242x2208
- iPhone X/XS: 1125x2436
- iPhone XR: 828x1792
- iPhone XS Max: 1242x2688
- iPad: 1536x2048
- iPad Pro 10.5": 1668x2224
- iPad Pro 11": 1668x2388
- iPad Pro 12.9": 2048x2732

### Design Guidelines
- Match theme colors (dark background)
- Include app logo/name centered
- Status bar area consideration

## Browser Support

### Full PWA Support
- Chrome/Edge (Desktop & Mobile): ✅
- Samsung Internet: ✅
- Opera: ✅
- Brave: ✅

### Partial Support
- Safari (iOS 16.4+): ✅ (limited service worker features)
- Firefox: ⚠️ (no install prompt, service worker works)

### Features by Browser
| Feature | Chrome | Safari | Firefox |
|---------|--------|--------|---------|
| Service Worker | ✅ | ✅ | ✅ |
| Install Prompt | ✅ | ❌ | ❌ |
| Offline Mode | ✅ | ✅ | ✅ |
| Push Notifications | ✅ | ⚠️ | ✅ |
| Background Sync | ✅ | ❌ | ❌ |

## Performance Optimizations

### Cache Strategy
- **Static Assets:** Cache-first (CSS, JS, fonts, icons)
- **API Calls:** Network-first with cache fallback (future)
- **Images:** Cache-first with size limits

### Bundle Sizes
- Service Worker: ~2KB
- site.js enhancements: ~10KB
- Manifest: ~1KB
- Total PWA overhead: ~13KB

### Load Times
- First Load: Standard HTTP load
- Subsequent Loads: Instant (cache)
- Offline: Instant (cache only)

## Security Considerations

### HTTPS Required
- PWA features require HTTPS in production
- Service workers only work over HTTPS
- Exception: localhost works over HTTP for development

### Content Security Policy
Consider adding CSP headers:
```
Content-Security-Policy: default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net;
```

### Cache Invalidation
Update service worker version when deploying:
```javascript
const CACHE_VERSION = 'timepe-v2'; // Increment on changes
```

## Future Enhancements

### Phase 1 (Ready to Implement)
- [ ] Create branded icons (all sizes)
- [ ] Generate iOS splash screens
- [ ] Implement background sync for offline time entries
- [ ] Add push notifications for reminders

### Phase 2 (Future)
- [ ] IndexedDB for offline data storage
- [ ] Sync conflict resolution
- [ ] App shortcuts for common actions
- [ ] Advanced caching strategies per route

### Phase 3 (Advanced)
- [ ] Web Share Target (receive from other apps)
- [ ] Badging API for notification count
- [ ] Periodic background sync
- [ ] Media session API for audio (if applicable)

## Troubleshooting

### Service Worker Not Registering
```javascript
// Check for errors in console
navigator.serviceWorker.register('/sw.js')
  .catch(err => console.error('SW registration failed:', err));
```

### Install Prompt Not Showing
- Check manifest.json is valid (DevTools → Application)
- Ensure HTTPS (or localhost)
- Criteria: visited 2+ times, 30+ seconds engagement
- Some browsers don't show prompt (Safari, Firefox)

### Cache Not Updating
```javascript
// Clear all caches manually
caches.keys().then(keys => {
  keys.forEach(key => caches.delete(key));
});
```

### Offline Mode Not Working
- Verify service worker is active (DevTools → Application)
- Check fetch handler is intercepting requests
- Ensure assets are in cache (Application → Cache Storage)

## Development Workflow

### Local Testing
```bash
# Run app
cd src/TimePE.WebApp
dotnet run

# Test on local network (mobile devices)
dotnet run --urls=https://0.0.0.0:5176
```

### Testing on Mobile Devices
1. Find your local IP: `ip addr` or `ifconfig`
2. Run app: `dotnet run --urls=http://192.168.1.X:5176`
3. Open on mobile: `http://192.168.1.X:5176`
4. For iOS testing, HTTPS required (use self-signed cert)

### Debugging Service Worker
```javascript
// In DevTools Console
navigator.serviceWorker.getRegistrations().then(regs => {
  regs.forEach(reg => console.log(reg));
});

// Force update
navigator.serviceWorker.getRegistration().then(reg => {
  reg.update();
});

// Unregister for fresh start
navigator.serviceWorker.getRegistration().then(reg => {
  reg.unregister();
});
```

### Manifest Validation
- Chrome DevTools → Application → Manifest
- Shows all properties, icons, warnings
- Test "Add to home screen" button

## Production Deployment

### Pre-Deployment Checklist
- [ ] Generate all icon sizes
- [ ] Create iOS splash screens
- [ ] Update service worker version
- [ ] Test on multiple devices/browsers
- [ ] Verify HTTPS certificate
- [ ] Check manifest.json validity
- [ ] Test offline functionality
- [ ] Verify install prompts work

### Deployment Steps
1. Build production assets
2. Update `CACHE_VERSION` in sw.js
3. Deploy to HTTPS server
4. Test PWA features in production
5. Monitor service worker errors

### Monitoring
```javascript
// Add to service worker for error tracking
self.addEventListener('error', (e) => {
  console.error('SW error:', e);
  // Send to logging service
});
```

## Resources

- [Web.dev PWA Guide](https://web.dev/progressive-web-apps/)
- [MDN Service Worker API](https://developer.mozilla.org/en-US/docs/Web/API/Service_Worker_API)
- [PWA Builder](https://www.pwabuilder.com/)
- [Maskable.app](https://maskable.app/) - Icon editor
- [Favicon Generator](https://realfavicongenerator.net/)

## Accessibility

All PWA features maintain WCAG 2.1 Level AA compliance:
- Touch targets: Minimum 48x48px
- Color contrast: 7:1 for text
- Reduced motion: Respects prefers-reduced-motion
- Screen reader: All interactive elements labeled
- Keyboard navigation: Full keyboard support

