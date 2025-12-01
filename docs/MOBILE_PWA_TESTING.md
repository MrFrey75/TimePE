# Mobile & PWA Testing Checklist

## Pre-Testing Setup
- [ ] Application running on http://localhost:5176
- [ ] All icons generated in `/wwwroot/icons/` (13 files)
- [ ] All splash screens generated in `/wwwroot/splash/` (10 files)
- [ ] Service worker at `/wwwroot/sw.js`
- [ ] Manifest at `/wwwroot/manifest.json`
- [ ] Browser DevTools available

## Desktop Testing (Chrome/Edge)

### Service Worker
- [ ] Open DevTools → Application → Service Workers
- [ ] Verify service worker status: "activated and is running"
- [ ] Check Console for: "SW registered: ServiceWorkerRegistration"
- [ ] No service worker errors in Console

### Manifest
- [ ] DevTools → Application → Manifest
- [ ] All fields populated correctly:
  - [ ] Name: "TimePE - Time Tracking"
  - [ ] Short name: "TimePE"
  - [ ] Start URL: "/"
  - [ ] Theme color: #0a0e17
  - [ ] Background color: #0a0e17
- [ ] All 8 icons show "Installable" checkmark
- [ ] No manifest errors/warnings

### Cache Storage
- [ ] DevTools → Application → Cache Storage
- [ ] Cache named "timepe-v1" exists
- [ ] Contains files:
  - [ ] /css/site.css
  - [ ] /js/site.js
  - [ ] /lib/bootstrap/dist/css/bootstrap.min.css
  - [ ] All icon files

### Offline Functionality
- [ ] Browse several pages while online
- [ ] DevTools → Network → Throttling → "Offline"
- [ ] Reload page - loads successfully
- [ ] Navigate between pages - static assets load
- [ ] CSS/JS/Bootstrap load from cache
- [ ] Icons display correctly

### Install Prompt
- [ ] Install icon appears in address bar
- [ ] Click install → "Install TimePE" dialog appears
- [ ] Click "Install" → App opens in standalone window
- [ ] No browser UI (address bar, tabs) visible
- [ ] App appears in OS applications list
- [ ] Can launch from Start Menu/Dock
- [ ] Can uninstall from chrome://apps

## Mobile Testing (Real Device)

### Android (Chrome/Edge/Samsung Internet)
- [ ] Access app via local network (http://192.168.x.x:5176)
- [ ] Install banner appears automatically OR
- [ ] Menu (⋮) → "Install app" option available
- [ ] Install → App icon appears on home screen
- [ ] Launch from home screen → Opens in full-screen
- [ ] Status bar shows app theme color (#0a0e17)
- [ ] No browser chrome visible
- [ ] Back button behavior works correctly
- [ ] Can uninstall: Long-press icon → Uninstall

### iOS/iPadOS (Safari)
- [ ] Access app via local network
- [ ] Share button (􀈂) → "Add to Home Screen" available
- [ ] Add → Custom icon appears on home screen
- [ ] Launch → Opens in standalone mode
- [ ] Status bar styling applied
- [ ] Splash screen displays on launch
- [ ] No Safari UI visible
- [ ] Can delete: Long-press icon → Remove App

## Responsive Design Testing

### Mobile (<768px)
- [ ] Chrome DevTools → Device Toolbar
- [ ] Test devices: iPhone SE, Pixel 5
- [ ] All buttons minimum 48x48px touch targets
- [ ] Tables stack vertically with data labels
- [ ] Forms are easy to fill on mobile
- [ ] Navigation menu accessible
- [ ] No horizontal scrolling
- [ ] Text readable without zooming

### Tablet (768px-991px)
- [ ] Test devices: iPad, Galaxy Tab
- [ ] Layout optimized for tablet size
- [ ] Touch targets still 48x48px minimum
- [ ] Good use of screen space
- [ ] Navigation appropriate for tablet

### Landscape Orientation
- [ ] Rotate to landscape on mobile
- [ ] Layout adjusts appropriately
- [ ] No content cut off
- [ ] Navigation still accessible
- [ ] Forms usable in landscape

## Touch Interactions

### Touch Feedback
- [ ] Tap any button → Opacity changes briefly
- [ ] Visual feedback on all interactive elements
- [ ] No delay in response (tap delay removed)

### Double-Tap Zoom Prevention
- [ ] Double-tap buttons → No zoom occurs
- [ ] Double-tap other areas → Zoom works normally

### Haptic Feedback (Supported Devices)
- [ ] Tap button → Brief vibration felt
- [ ] Submit form → Vibration on success
- [ ] Delete action → Vibration confirmation

### Pull-to-Refresh
- [ ] Pull down from top of page
- [ ] Page refreshes when released
- [ ] Visual indicator during pull

## PWA Features

### Network Status Detection
- [ ] Disconnect network/WiFi
- [ ] Banner appears: "You are offline"
- [ ] Banner styled correctly
- [ ] Reconnect → Banner disappears

### Auto-Save (Forms)
- [ ] Start filling a form
- [ ] Navigate away or close browser
- [ ] Return to form
- [ ] Data restored from localStorage
- [ ] Can clear saved data

### Install Prompt Button
- [ ] If not installed, button appears in UI
- [ ] Click → Browser install dialog opens
- [ ] After install → Button disappears

### Web Share API (Supported Browsers)
- [ ] Share functionality available
- [ ] Click share → Native share dialog opens
- [ ] Can share to other apps

## Performance Testing

### Load Times
- [ ] First load: < 3 seconds
- [ ] Subsequent loads: < 1 second (cached)
- [ ] Offline loads: Instant

### Bundle Sizes
- [ ] Check Network tab → Disable cache
- [ ] site.css: Reasonable size
- [ ] site.js: ~10-15KB
- [ ] Service worker: ~2KB
- [ ] Manifest: ~1KB

### Lighthouse Audit
- [ ] DevTools → Lighthouse → Generate report
- [ ] Performance: > 90
- [ ] Accessibility: > 90
- [ ] Best Practices: > 90
- [ ] **PWA: 100** (all checks green)
- [ ] SEO: > 80

## Accessibility Testing

### Touch Targets
- [ ] All buttons: 48x48px minimum
- [ ] Links: 48x48px minimum
- [ ] Form inputs: 48px height minimum

### Color Contrast
- [ ] Text on background: 7:1 ratio minimum
- [ ] Cyan (#00d4ff) on dark: Passes contrast
- [ ] All text readable

### Reduced Motion
- [ ] System Settings → Enable "Reduce Motion"
- [ ] Reload app
- [ ] Animations disabled/reduced
- [ ] Transitions simplified

### Screen Reader
- [ ] Enable VoiceOver (iOS) or TalkBack (Android)
- [ ] Navigate through app
- [ ] All interactive elements announced
- [ ] Proper labels on buttons/inputs

## Cross-Browser Testing

### Chrome (Desktop & Mobile)
- [ ] Full PWA support
- [ ] Install prompt works
- [ ] Service worker active
- [ ] Offline mode works

### Edge (Desktop & Mobile)
- [ ] Full PWA support
- [ ] Install works
- [ ] Same as Chrome behavior

### Safari (Desktop & iOS)
- [ ] Service worker works
- [ ] Manifest recognized
- [ ] Add to Home Screen works
- [ ] Limited PWA features noted

### Firefox
- [ ] Service worker works
- [ ] Offline functionality works
- [ ] No install prompt (expected)
- [ ] Manual "Install" if available

## Known Issues & Limitations

### HTTPS Requirement
- [ ] Service worker works on localhost HTTP
- [ ] Production requires HTTPS for PWA features
- [ ] Self-signed cert warning on iOS

### Safari Limitations
- [ ] No install prompt (use Share → Add to Home Screen)
- [ ] Limited background sync support
- [ ] Push notifications restricted

### Firefox
- [ ] No standard install prompt
- [ ] about:config may enable experimental features

## Bug Reporting Template

If you find issues during testing, report with:

```markdown
**Environment:**
- Device: [iPhone 14, Galaxy S23, Desktop, etc.]
- Browser: [Chrome 119, Safari 17, etc.]
- OS: [iOS 17, Android 14, Windows 11, etc.]

**Issue:**
[Clear description]

**Steps to Reproduce:**
1. Open app
2. Navigate to...
3. Observe...

**Expected Behavior:**
[What should happen]

**Actual Behavior:**
[What actually happens]

**Screenshots:**
[If applicable]

**Console Errors:**
[From DevTools Console tab]
```

## Testing on Local Network

To test on mobile devices:

```bash
# Find your local IP
ip addr | grep "inet 192"

# Run app on network
cd src/TimePE.WebApp
dotnet run --urls=http://0.0.0.0:5176

# Access from mobile device
http://192.168.x.x:5176
```

**For iOS (requires HTTPS):**
```bash
# Create self-signed cert (one-time)
dotnet dev-certs https --trust

# Run with HTTPS
dotnet run --urls=https://0.0.0.0:5176
```

## Automated Testing Commands

```bash
# Check icon files exist
ls -la src/TimePE.WebApp/wwwroot/icons/ | wc -l
# Should show 13 files

# Check splash screens exist
ls -la src/TimePE.WebApp/wwwroot/splash/ | wc -l
# Should show 10 files

# Verify manifest validity
curl http://localhost:5176/manifest.json | jq .
# Should show valid JSON

# Check service worker
curl http://localhost:5176/sw.js | head -5
# Should show service worker code
```

## Post-Testing Actions

After successful testing:
- [ ] Document any issues found
- [ ] Update icons with production designs
- [ ] Consider adding to web app stores:
  - [ ] Microsoft Store (Windows PWA)
  - [ ] Google Play (TWA - Trusted Web Activity)
  - [ ] iOS App Store (via wrapper, if needed)
- [ ] Set up analytics for PWA install rates
- [ ] Monitor service worker errors in production

## Resources

- **Chrome DevTools:** chrome://inspect
- **Edge DevTools:** edge://inspect
- **Safari DevTools:** Safari → Develop → Show Web Inspector
- **Lighthouse CLI:** `npm install -g lighthouse && lighthouse http://localhost:5176 --view`
- **PWA Builder:** https://www.pwabuilder.com/ (validates manifest)
- **Can I Use:** https://caniuse.com/?search=pwa (browser support)

---

**Testing Status:** ⬜ Not Started | ⏳ In Progress | ✅ Completed | ❌ Failed

Last Updated: December 1, 2024
