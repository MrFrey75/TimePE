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
**Modern ES2024+ Implementation**

**Version:** 2.0.0

**Cache Strategy:** Intelligent multi-strategy caching
- **Static Assets (CSS, JS, Fonts):** Cache-first strategy for maximum performance
- **Images:** Separate image cache with size limits
- **HTML Pages:** Network-first with cache fallback for fresh content
- **API Calls:** Network-first strategy (when implemented)

**Cache Management:**
- **Static Cache:** Core application files
- **Dynamic Cache:** HTML pages and API responses (max 50 items)
- **Image Cache:** Cached images (max 30 items)
- **Auto-cleanup:** Old caches deleted on activation
- **Size Limits:** Prevents unlimited cache growth

**Modern Features:**
- **Async/await:** Clean, readable asynchronous code throughout
- **Intelligent Routing:** Different strategies per content type
- **Cache Limiting:** Automatic pruning of old cached items
- **Error Handling:** Comprehensive try/catch blocks with logging
- **Message API:** Two-way communication with web app
- **Periodic Sync:** Background data refresh (when supported)

**Lifecycle Events:**
1. **Install:** Pre-caches static assets with async/await
2. **Activate:** Cleans up old cache versions intelligently
3. **Fetch:** Routes requests to appropriate caching strategy
4. **Sync:** Background synchronization support
5. **Push:** Enhanced push notification handling
6. **Message:** Communication channel with client pages
7. **Periodic Sync:** Automatic cache refresh

**Advanced Features:**
- **Background Sync:** Queues offline actions for later sync
- **Push Notifications:** Rich notifications with actions
- **Smart Window Management:** Focus existing tabs vs. opening new
- **Version Management:** Query and manage cache versions
- **Manual Cache Control:** Clear cache on demand via messages

**Communication API:**
```javascript
// Get service worker version
navigator.serviceWorker.controller.postMessage({ type: 'GET_VERSION' });

// Force service worker update
navigator.serviceWorker.controller.postMessage({ type: 'SKIP_WAITING' });

// Clear all caches
navigator.serviceWorker.controller.postMessage({ type: 'CLEAR_CACHE' });
```

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
- Service Worker: ~6KB (modern implementation)
- site.js enhancements: ~10KB
- Manifest: ~1KB
- Total PWA overhead: ~17KB

### Load Times
- First Load: Standard HTTP load
- Subsequent Loads: Instant (from cache)
- Offline: Instant (cache-only mode)
- Cache Strategy: Smart routing per content type

### Caching Efficiency
- **Static Assets:** Cached indefinitely, cache-first
- **Dynamic Content:** Max 50 items, network-first
- **Images:** Max 30 items, cache-first
- **Auto-cleanup:** Prevents cache bloat

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
const CACHE_VERSION = '2.0.0'; // Update this on changes
const CACHE_NAME = `timepe-v${CACHE_VERSION}`;
const STATIC_CACHE = `${CACHE_NAME}-static`;
const DYNAMIC_CACHE = `${CACHE_NAME}-dynamic`;
const IMAGE_CACHE = `${CACHE_NAME}-images`;
```

**Best Practices:**
- Increment version on any service worker changes
- Use semantic versioning (MAJOR.MINOR.PATCH)
- Old caches automatically cleaned on activation
- Clients get new version on next page load

## Future Enhancements

### Phase 1 (Implemented ✅)
- [x] Modern ES2024+ service worker with async/await
- [x] Intelligent multi-strategy caching
- [x] Cache size limiting and auto-cleanup
- [x] Message API for version management
- [x] Enhanced push notifications with actions
- [x] Background sync infrastructure
- [x] Periodic sync support

### Phase 2 (Ready to Implement)
- [ ] Create branded icons (all sizes)
- [ ] Generate iOS splash screens
- [ ] Implement IndexedDB for offline data storage
- [ ] Complete background sync for time entries
- [ ] Add push notification subscription UI

### Phase 3 (Future)
- [ ] Sync conflict resolution
- [ ] App shortcuts for common actions
- [ ] Web Share Target (receive from other apps)
- [ ] Badging API for notification count
- [ ] Contact Picker API integration

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

// Check registration status
navigator.serviceWorker.getRegistration().then(reg => {
  console.log('Registration:', reg);
  console.log('Active:', reg.active);
  console.log('Installing:', reg.installing);
  console.log('Waiting:', reg.waiting);
});

// Get service worker version
navigator.serviceWorker.controller?.postMessage({ type: 'GET_VERSION' });

// Listen for version response
navigator.serviceWorker.addEventListener('message', event => {
  if (event.data.version) {
    console.log('Service Worker Version:', event.data.version);
  }
});

// Force update
navigator.serviceWorker.getRegistration().then(reg => {
  reg.update();
});

// Skip waiting (activate new SW immediately)
navigator.serviceWorker.controller?.postMessage({ type: 'SKIP_WAITING' });

// Clear all caches
navigator.serviceWorker.controller?.postMessage({ type: 'CLEAR_CACHE' });

// Unregister for fresh start
navigator.serviceWorker.getRegistration().then(reg => {
  reg.unregister().then(() => {
    console.log('Service worker unregistered');
    location.reload();
  });
});

// Check all caches
caches.keys().then(keys => {
  console.log('Cache names:', keys);
  keys.forEach(key => {
    caches.open(key).then(cache => {
      cache.keys().then(requests => {
        console.log(`${key} has ${requests.length} items`);
      });
    });
  });
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
- [x] Update service worker version (v2.0.0)
- [x] Test cache strategies (static, dynamic, image)
- [ ] Test on multiple devices/browsers
- [ ] Verify HTTPS certificate
- [ ] Check manifest.json validity
- [x] Test offline functionality with cache limits
- [ ] Verify install prompts work
- [x] Test message API communication
- [x] Verify cache cleanup on activation

### Deployment Steps
1. Build production assets
2. Update `CACHE_VERSION` in sw.js (current: 2.0.0)
3. Deploy to HTTPS server
4. Test PWA features in production
5. Monitor service worker errors
6. Verify cache strategies working correctly
7. Check DevTools → Application → Service Workers

### Monitoring
```javascript
// Add to service worker for error tracking (already implemented)
self.addEventListener('error', (e) => {
  console.error('[Service Worker] Error:', e);
  // Send to logging service in production
});

// Monitor cache sizes
self.addEventListener('activate', async (event) => {
  const caches = await caches.keys();
  console.log('[Service Worker] Active caches:', caches);
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

