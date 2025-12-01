// TimePE - Progressive Web App & Mobile Enhancements

// ============================================
// PWA Service Worker Registration
// ============================================
if ('serviceWorker' in navigator) {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('/sw.js')
      .then(registration => {
        console.log('✅ Service Worker registered:', registration.scope);
        
        // Check for updates periodically
        setInterval(() => {
          registration.update();
        }, 60000); // Check every minute
      })
      .catch(err => {
        console.error('❌ Service Worker registration failed:', err);
      });
  });
}

// ============================================
// PWA Install Prompt
// ============================================
let deferredPrompt;
const installButton = document.createElement('button');
installButton.className = 'btn btn-primary position-fixed bottom-0 end-0 m-3 d-none';
installButton.innerHTML = '<i class="bi bi-download"></i> Install App';
installButton.style.zIndex = '1050';
document.body.appendChild(installButton);

window.addEventListener('beforeinstallprompt', (e) => {
  e.preventDefault();
  deferredPrompt = e;
  installButton.classList.remove('d-none');
});

installButton.addEventListener('click', async () => {
  if (!deferredPrompt) return;
  
  deferredPrompt.prompt();
  const { outcome } = await deferredPrompt.userChoice;
  
  console.log(`User ${outcome === 'accepted' ? 'accepted' : 'dismissed'} the install prompt`);
  deferredPrompt = null;
  installButton.classList.add('d-none');
});

window.addEventListener('appinstalled', () => {
  console.log('✅ TimePE installed successfully');
  installButton.classList.add('d-none');
  deferredPrompt = null;
});

// ============================================
// Mobile Touch Enhancements
// ============================================

// Add touch feedback to buttons
document.addEventListener('DOMContentLoaded', () => {
  // Touch feedback for buttons and links
  const touchElements = document.querySelectorAll('.btn, .nav-link, .card, a');
  
  touchElements.forEach(element => {
    element.addEventListener('touchstart', function() {
      this.style.opacity = '0.7';
    }, { passive: true });
    
    element.addEventListener('touchend', function() {
      this.style.opacity = '1';
    }, { passive: true });
  });

  // Prevent double-tap zoom on buttons
  const buttons = document.querySelectorAll('button, .btn');
  let lastTouchEnd = 0;
  
  buttons.forEach(button => {
    button.addEventListener('touchend', (e) => {
      const now = Date.now();
      if (now - lastTouchEnd <= 300) {
        e.preventDefault();
      }
      lastTouchEnd = now;
    }, { passive: false });
  });

  // Make tables scrollable with touch
  const tables = document.querySelectorAll('table');
  tables.forEach(table => {
    if (!table.parentElement.classList.contains('table-responsive')) {
      const wrapper = document.createElement('div');
      wrapper.className = 'table-responsive';
      table.parentNode.insertBefore(wrapper, table);
      wrapper.appendChild(table);
    }
  });

  // Add data labels for responsive stacking on mobile
  addTableDataLabels();
  
  // Smooth scroll for anchor links
  document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
      const href = this.getAttribute('href');
      if (href !== '#' && href !== '#!') {
        const target = document.querySelector(href);
        if (target) {
          e.preventDefault();
          target.scrollIntoView({
            behavior: 'smooth',
            block: 'start'
          });
        }
      }
    });
  });
});

// ============================================
// Responsive Table Helper
// ============================================
function addTableDataLabels() {
  const tables = document.querySelectorAll('.table');
  
  tables.forEach(table => {
    const headers = [];
    const headerCells = table.querySelectorAll('thead th');
    
    headerCells.forEach(header => {
      headers.push(header.textContent.trim());
    });
    
    const rows = table.querySelectorAll('tbody tr');
    rows.forEach(row => {
      const cells = row.querySelectorAll('td');
      cells.forEach((cell, index) => {
        if (headers[index]) {
          cell.setAttribute('data-label', headers[index]);
        }
      });
    });
  });
}

// ============================================
// Network Status Detection
// ============================================
function updateOnlineStatus() {
  const statusBanner = document.getElementById('network-status');
  
  if (!navigator.onLine) {
    if (!statusBanner) {
      const banner = document.createElement('div');
      banner.id = 'network-status';
      banner.className = 'alert alert-warning position-fixed top-0 start-0 end-0 m-0 rounded-0 text-center';
      banner.style.zIndex = '2000';
      banner.innerHTML = '<i class="bi bi-wifi-off"></i> You are offline. Changes will be synced when connection is restored.';
      document.body.insertBefore(banner, document.body.firstChild);
    }
  } else {
    if (statusBanner) {
      statusBanner.remove();
    }
  }
}

window.addEventListener('online', updateOnlineStatus);
window.addEventListener('offline', updateOnlineStatus);

// Check status on load
document.addEventListener('DOMContentLoaded', updateOnlineStatus);

// ============================================
// Form Auto-save (Local Storage)
// ============================================
function enableAutoSave(formId, storageKey) {
  const form = document.getElementById(formId);
  if (!form) return;
  
  // Load saved data
  const savedData = localStorage.getItem(storageKey);
  if (savedData) {
    const data = JSON.parse(savedData);
    Object.keys(data).forEach(key => {
      const input = form.querySelector(`[name="${key}"]`);
      if (input) {
        input.value = data[key];
      }
    });
  }
  
  // Save on input
  form.addEventListener('input', () => {
    const formData = new FormData(form);
    const data = {};
    formData.forEach((value, key) => {
      data[key] = value;
    });
    localStorage.setItem(storageKey, JSON.stringify(data));
  });
  
  // Clear on successful submit
  form.addEventListener('submit', () => {
    localStorage.removeItem(storageKey);
  });
}

// ============================================
// Haptic Feedback (for supported devices)
// ============================================
function vibrate(pattern = [50]) {
  if ('vibrate' in navigator) {
    navigator.vibrate(pattern);
  }
}

// Add haptic feedback to important actions
document.addEventListener('DOMContentLoaded', () => {
  // Vibrate on form submission
  const forms = document.querySelectorAll('form');
  forms.forEach(form => {
    form.addEventListener('submit', () => vibrate([50]));
  });
  
  // Vibrate on delete actions
  const deleteButtons = document.querySelectorAll('.btn-danger');
  deleteButtons.forEach(button => {
    button.addEventListener('click', () => vibrate([30, 50, 30]));
  });
});

// ============================================
// Pull-to-Refresh (Optional)
// ============================================
let startY = 0;
let currentY = 0;
let pulling = false;

document.addEventListener('touchstart', (e) => {
  if (window.scrollY === 0) {
    startY = e.touches[0].clientY;
    pulling = true;
  }
}, { passive: true });

document.addEventListener('touchmove', (e) => {
  if (!pulling) return;
  currentY = e.touches[0].clientY;
  
  if (currentY - startY > 100) {
    // Trigger refresh
    pulling = false;
    location.reload();
  }
}, { passive: true });

document.addEventListener('touchend', () => {
  pulling = false;
  startY = 0;
  currentY = 0;
}, { passive: true });

// ============================================
// iOS Standalone Mode Detection
// ============================================
if (window.navigator.standalone === true) {
  console.log('Running in iOS standalone mode');
  document.body.classList.add('ios-standalone');
}

// ============================================
// Share API Support
// ============================================
async function shareContent(title, text, url) {
  if (navigator.share) {
    try {
      await navigator.share({
        title: title,
        text: text,
        url: url
      });
      console.log('✅ Content shared successfully');
    } catch (err) {
      console.log('❌ Share cancelled or failed:', err);
    }
  } else {
    console.log('Web Share API not supported');
  }
}

// Make shareContent globally available
window.shareContent = shareContent;

// ============================================
// Utilities
// ============================================

// Detect if device is mobile
function isMobile() {
  return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
}

// Detect if device has touch capability
function isTouchDevice() {
  return ('ontouchstart' in window) || (navigator.maxTouchPoints > 0);
}

// Add classes to body for device detection
document.addEventListener('DOMContentLoaded', () => {
  if (isMobile()) {
    document.body.classList.add('is-mobile');
  }
  if (isTouchDevice()) {
    document.body.classList.add('is-touch');
  }
});

console.log('✅ TimePE PWA initialized');

