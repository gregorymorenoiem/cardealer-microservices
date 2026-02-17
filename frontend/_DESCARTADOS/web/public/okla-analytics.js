/**
 * OKLA Analytics SDK
 * JavaScript SDK for tracking user behavior and events
 * Version: 1.0.0
 *
 * Usage:
 * <script src="/okla-analytics.js"></script>
 * <script>
 *   OklaAnalytics.init({ apiUrl: 'https://api.okla.com.do' });
 * </script>
 */

(function (window) {
  'use strict';

  // ============================================
  // Configuration & State
  // ============================================

  const VERSION = '1.0.0';
  let config = {
    apiUrl: 'http://localhost:8080',
    trackEndpoint: '/api/events/track',
    batchEndpoint: '/api/events/track/batch',
    batchSize: 10,
    flushIntervalMs: 5000,
    autoTrack: true,
    debug: false,
    enabled: true, // EventTrackingService is now deployed
  };

  let sessionId = null;
  let userId = null;
  let eventQueue = [];
  let flushTimer = null;
  let isInitialized = false;

  // ============================================
  // Utility Functions
  // ============================================

  function log(...args) {
    if (config.debug) {
      console.log('[OKLA Analytics]', ...args);
    }
  }

  function generateId() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
      const r = (Math.random() * 16) | 0;
      const v = c === 'x' ? r : (r & 0x3) | 0x8;
      return v.toString(16);
    });
  }

  function getSessionId() {
    if (sessionId) return sessionId;

    // Try to get from sessionStorage
    try {
      sessionId = sessionStorage.getItem('okla_session_id');
      if (!sessionId) {
        sessionId = generateId();
        sessionStorage.setItem('okla_session_id', sessionId);
      }
    } catch (e) {
      // Fallback if sessionStorage is not available
      sessionId = generateId();
    }

    return sessionId;
  }

  function getUserId() {
    if (userId) return userId;

    // Try to get from localStorage (persisted across sessions)
    try {
      userId = localStorage.getItem('okla_user_id');
      if (!userId) {
        userId = null; // Will be set when user logs in
      }
    } catch (e) {
      userId = null;
    }

    return userId;
  }

  function getDeviceType() {
    const ua = navigator.userAgent || navigator.vendor || window.opera;

    if (/(tablet|ipad|playbook|silk)|(android(?!.*mobi))/i.test(ua)) {
      return 'Tablet';
    }
    if (
      /Mobile|Android|iP(hone|od)|IEMobile|BlackBerry|Kindle|Silk-Accelerated|(hpw|web)OS|Opera M(obi|ini)/.test(
        ua
      )
    ) {
      return 'Mobile';
    }
    return 'Desktop';
  }

  function getBrowser() {
    const ua = navigator.userAgent;

    if (ua.includes('Edg/')) return 'Edge';
    if (ua.includes('Chrome/')) return 'Chrome';
    if (ua.includes('Safari/') && !ua.includes('Chrome')) return 'Safari';
    if (ua.includes('Firefox/')) return 'Firefox';
    if (ua.includes('MSIE') || ua.includes('Trident/')) return 'IE';

    return 'Unknown';
  }

  function getOS() {
    const ua = navigator.userAgent;

    if (ua.includes('Windows')) return 'Windows';
    if (ua.includes('Mac OS')) return 'macOS';
    if (ua.includes('Linux')) return 'Linux';
    if (ua.includes('Android')) return 'Android';
    if (ua.includes('iOS') || ua.includes('iPhone') || ua.includes('iPad')) return 'iOS';

    return 'Unknown';
  }

  function getLocation() {
    // In production, this could be enhanced with GeoIP API
    return {
      country: 'DO', // Default to Dominican Republic
      city: 'Unknown',
    };
  }

  // ============================================
  // Event Building
  // ============================================

  function buildBaseEvent(eventType) {
    const location = getLocation();

    return {
      eventType: eventType,
      userId: getUserId(),
      sessionId: getSessionId(),
      ipAddress: null, // Will be captured by backend
      userAgent: navigator.userAgent,
      referrer: document.referrer || null,
      currentUrl: window.location.href,
      deviceType: getDeviceType(),
      browser: getBrowser(),
      operatingSystem: getOS(),
      country: location.country,
      city: location.city,
      eventData: null,
      source: null,
      campaign: null,
      medium: null,
      content: null,
    };
  }

  // ============================================
  // Event Sending
  // ============================================

  function sendEvent(event) {
    // Skip if EventTrackingService is not deployed yet
    if (!config.enabled) {
      log('Analytics disabled - EventTrackingService not deployed');
      return;
    }

    const url = config.apiUrl + config.trackEndpoint;

    log('Sending event:', event);

    // Use sendBeacon if available (non-blocking)
    if (navigator.sendBeacon) {
      const blob = new Blob([JSON.stringify({ event: event })], {
        type: 'application/json',
      });
      navigator.sendBeacon(url, blob);
    } else {
      // Fallback to fetch
      fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ event: event }),
        keepalive: true,
      }).catch(() => {
        // Silently fail - EventTrackingService may not be deployed
      });
    }
  }

  function sendBatch(events) {
    if (events.length === 0) return;

    // Skip if EventTrackingService is not deployed yet
    if (!config.enabled) {
      log('Analytics disabled - EventTrackingService not deployed');
      return;
    }

    const url = config.apiUrl + config.batchEndpoint;

    log('Sending batch of', events.length, 'events');

    fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ events: events }),
      keepalive: true,
    }).catch(() => {
      // Silently fail - EventTrackingService may not be deployed
    });
  }

  function queueEvent(event) {
    eventQueue.push(event);

    // Flush immediately if batch size reached
    if (eventQueue.length >= config.batchSize) {
      flushQueue();
    }
  }

  function flushQueue() {
    if (eventQueue.length === 0) return;

    const eventsToSend = eventQueue.splice(0, eventQueue.length);
    sendBatch(eventsToSend);

    // Reset flush timer
    if (flushTimer) {
      clearTimeout(flushTimer);
      flushTimer = null;
    }
  }

  function startFlushTimer() {
    if (flushTimer) return;

    flushTimer = setInterval(() => {
      flushQueue();
    }, config.flushIntervalMs);
  }

  // ============================================
  // Auto-Tracking Features
  // ============================================

  let pageViewStartTime = Date.now();
  let scrollDepth = 0;

  function trackPageView() {
    const event = buildBaseEvent('PageView');
    event.pageUrl = window.location.pathname;
    event.pageTitle = document.title;
    event.previousUrl = document.referrer || null;
    event.scrollDepth = 0;
    event.timeOnPage = 0;
    event.isExit = false;
    event.isBounce = false;

    queueEvent(event);

    pageViewStartTime = Date.now();
    scrollDepth = 0;

    log('Page view tracked:', event.pageUrl);
  }

  function trackScroll() {
    const windowHeight = window.innerHeight;
    const documentHeight = document.documentElement.scrollHeight;
    const scrollTop = window.scrollY || window.pageYOffset;

    const currentScrollDepth = Math.round(((scrollTop + windowHeight) / documentHeight) * 100);

    scrollDepth = Math.max(scrollDepth, currentScrollDepth);
  }

  function trackPageExit() {
    const timeOnPage = Math.round((Date.now() - pageViewStartTime) / 1000);

    const event = buildBaseEvent('PageView');
    event.pageUrl = window.location.pathname;
    event.pageTitle = document.title;
    event.scrollDepth = scrollDepth;
    event.timeOnPage = timeOnPage;
    event.isExit = true;
    event.isBounce = timeOnPage < 10;

    sendEvent(event); // Send immediately (not queued)

    log('Page exit tracked:', { timeOnPage, scrollDepth });
  }

  function setupAutoTracking() {
    if (!config.autoTrack) return;

    // Track initial page view
    trackPageView();

    // Track scroll depth
    let scrollTimeout;
    window.addEventListener(
      'scroll',
      () => {
        clearTimeout(scrollTimeout);
        scrollTimeout = setTimeout(trackScroll, 100);
      },
      { passive: true }
    );

    // Track page exit
    window.addEventListener('beforeunload', trackPageExit);

    // Track page visibility changes (user switches tabs)
    document.addEventListener('visibilitychange', () => {
      if (document.hidden) {
        trackPageExit();
      } else {
        trackPageView();
      }
    });

    // Track history changes (SPA navigation)
    if (window.history && history.pushState) {
      const originalPushState = history.pushState;
      history.pushState = function () {
        trackPageExit();
        originalPushState.apply(history, arguments);
        setTimeout(trackPageView, 100); // Slight delay for new page to load
      };
    }

    log('Auto-tracking enabled');
  }

  // ============================================
  // Public API
  // ============================================

  const OklaAnalytics = {
    version: VERSION,

    /**
     * Initialize the SDK
     * @param {Object} options - Configuration options
     */
    init: function (options = {}) {
      if (isInitialized) {
        log('Already initialized');
        return;
      }

      // Merge options with defaults
      config = Object.assign({}, config, options);

      log('Initializing SDK v' + VERSION, config);

      // Start flush timer
      startFlushTimer();

      // Setup auto-tracking
      if (config.autoTrack) {
        if (document.readyState === 'complete') {
          setupAutoTracking();
        } else {
          window.addEventListener('load', setupAutoTracking);
        }
      }

      isInitialized = true;
      log('SDK initialized');
    },

    /**
     * Set the current user ID
     * @param {string} id - User ID (GUID)
     */
    setUserId: function (id) {
      userId = id;
      try {
        localStorage.setItem('okla_user_id', id);
      } catch (e) {
        log('Failed to store user ID');
      }
      log('User ID set:', id);
    },

    /**
     * Clear user ID (logout)
     */
    clearUserId: function () {
      userId = null;
      try {
        localStorage.removeItem('okla_user_id');
      } catch (e) {
        log('Failed to remove user ID');
      }
      log('User ID cleared');
    },

    /**
     * Track a search event
     * @param {Object} params - Search parameters
     */
    trackSearch: function (params) {
      const event = buildBaseEvent('Search');
      event.searchQuery = params.query;
      event.resultsCount = params.resultsCount || 0;
      event.searchType = params.searchType || 'General';
      event.appliedFilters = params.filters ? JSON.stringify(params.filters) : null;
      event.sortBy = params.sortBy || null;
      event.clickedPosition = params.clickedPosition || null;
      event.clickedVehicleId = params.clickedVehicleId || null;

      queueEvent(event);
      log('Search tracked:', event.searchQuery);
    },

    /**
     * Track a vehicle view event
     * @param {Object} params - Vehicle parameters
     */
    trackVehicleView: function (params) {
      const event = buildBaseEvent('VehicleView');
      event.vehicleId = params.vehicleId;
      event.vehicleTitle = params.title;
      event.vehiclePrice = params.price;
      event.make = params.make || null;
      event.model = params.model || null;
      event.year = params.year || null;
      event.timeSpentSeconds = params.timeSpent || 0;
      event.viewedImages = params.viewedImages || false;
      event.viewedSpecs = params.viewedSpecs || false;
      event.clickedContact = params.clickedContact || false;
      event.addedToFavorites = params.addedToFavorites || false;
      event.sharedVehicle = params.sharedVehicle || false;
      event.viewSource = params.viewSource || 'Direct';

      queueEvent(event);
      log('Vehicle view tracked:', event.vehicleTitle);
    },

    /**
     * Track a filter event
     * @param {Object} params - Filter parameters
     */
    trackFilter: function (params) {
      const event = buildBaseEvent('Filter');
      event.filterType = params.filterType;
      event.filterValue = params.filterValue;
      event.filterOperator = params.filterOperator || 'equals';
      event.resultsAfterFilter = params.resultsAfterFilter || 0;
      event.pageContext = params.pageContext || 'Search';

      queueEvent(event);
      log('Filter tracked:', event.filterType, '=', event.filterValue);
    },

    /**
     * Track a custom event
     * @param {string} eventType - Event type name
     * @param {Object} data - Event data
     */
    trackCustom: function (eventType, data = {}) {
      const event = buildBaseEvent(eventType);
      event.eventData = JSON.stringify(data);

      queueEvent(event);
      log('Custom event tracked:', eventType);
    },

    /**
     * Manually flush the event queue
     */
    flush: function () {
      flushQueue();
    },

    /**
     * Get the current session ID
     */
    getSessionId: function () {
      return getSessionId();
    },

    /**
     * Get the current user ID
     */
    getUserId: function () {
      return getUserId();
    },

    /**
     * Enable debug mode
     */
    enableDebug: function () {
      config.debug = true;
      log('Debug mode enabled');
    },

    /**
     * Disable debug mode
     */
    disableDebug: function () {
      config.debug = false;
    },
  };

  // Export to global scope
  window.OklaAnalytics = OklaAnalytics;

  log('SDK loaded v' + VERSION);
})(window);
