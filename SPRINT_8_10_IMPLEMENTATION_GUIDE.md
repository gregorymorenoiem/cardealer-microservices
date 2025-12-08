# Sprints 8-10 Implementation Guide

## Sprint 8: Dealer Panel Mobile (PARTIAL - Foundation Created)

### ✅ Completed
- DealerStats entity with full statistics structure
- TopPerformingVehicle entity for analytics

### 📋 Remaining Tasks

#### Domain Layer
- [ ] Create DealerListing entity (vehicle listing with dealer-specific fields)
- [ ] Create Lead entity (customer lead with status pipeline)
- [ ] Create DealerRepository interface with methods:
  * getDealerStats, getDealerListings, createListing, updateListing
  * deleteListing, getLeads, updateLeadStatus, getAnalytics
- [ ] Implement GetDealerStats use case
- [ ] Implement ManageListing use case (CRUD operations)
- [ ] Implement GetLeads use case
- [ ] Implement UpdateLeadStatus use case

#### Data Layer
- [ ] Create DealerStatsModel with JSON serialization
- [ ] Create DealerListingModel with JSON serialization
- [ ] Create LeadModel with JSON serialization
- [ ] Implement MockDealerRepository with mock data
- [ ] Mock dashboard statistics (views, leads, revenue charts)
- [ ] Mock 10-15 dealer listings with various statuses

#### Presentation Layer
- [ ] Create DealerBloc with states (Loading, Loaded, Creating, Updated, Error)
- [ ] Implement events (LoadStats, LoadListings, CreateListing, UpdateListing, DeleteListing)
- [ ] Create LeadsBloc for CRM functionality
- [ ] Implement pipeline status updates

#### UI Layer - Dashboard
- [ ] DealerDashboardPage with stats cards:
  * Total listings (active/pending/sold)
  * Views this month with chart
  * Leads this month with chart
  * Conversion rate card
  * Revenue card with monthly breakdown
- [ ] Quick actions grid (Create Listing, View Leads, Analytics, Messages)
- [ ] Recent activity feed widget
- [ ] Performance chart using fl_chart package

#### UI Layer - Listings Management
- [ ] DealerListingsPage with filterable list (All, Active, Pending, Sold)
- [ ] Swipe actions for quick edit/delete
- [ ] Bulk select mode
- [ ] CreateListingPage with multi-step form:
  * Step 1: Basic info (make, model, year, price)
  * Step 2: Detailed specs (mileage, fuel, transmission, etc.)
  * Step 3: Images upload (multi-select with image_picker)
  * Step 4: Description and features
  * Step 5: Preview before publish
- [ ] EditListingPage reusing form components
- [ ] Draft system with local storage

#### UI Layer - Mobile CRM
- [ ] LeadsPage with Kanban-style pipeline view
- [ ] Lead detail page with contact history
- [ ] Quick action buttons (Call, Message, Schedule)
- [ ] Notes section for lead
- [ ] Status update dropdown
- [ ] Lead filtering and search

#### UI Layer - Analytics
- [ ] SimplifiedAnalyticsPage for mobile
- [ ] Key metrics cards with trends
- [ ] Performance charts (views, leads, conversions)
- [ ] Date range picker
- [ ] Export option (CSV/PDF)

#### DI Registration
- [ ] Register DealerRepository (mock)
- [ ] Register 4+ dealer use cases
- [ ] Register DealerBloc and LeadsBloc

---

## Sprint 9: Maps & Geolocation (NOT STARTED)

### Required Dependencies
```yaml
dependencies:
  google_maps_flutter: ^2.5.0
  geolocator: ^10.1.0
  geocoding: ^2.1.1
```

### Tasks

#### Maps Integration
- [ ] Setup Google Maps API keys (Android: AndroidManifest.xml, iOS: AppDelegate.swift)
- [ ] Create MapViewPage with GoogleMap widget
- [ ] Implement custom VehicleMapMarker widget
- [ ] Setup marker clustering for performance (google_maps_cluster_manager)
- [ ] Custom info windows for vehicle preview

#### Geolocation Services
- [ ] Create LocationService class:
  * requestPermission()
  * getCurrentLocation()
  * watchPosition() for real-time updates
- [ ] Implement LocationPicker widget (draggable marker)
- [ ] Address autocomplete using geocoding package
- [ ] Reverse geocoding (lat/lng to address)

#### Map Features
- [ ] Filter vehicles by radius (circle overlay)
- [ ] Draw search area (polygon tool - optional)
- [ ] Show user's current location with blue dot
- [ ] Directions to seller (launch Google Maps/Apple Maps)
- [ ] Street View integration (if available)
- [ ] Map/List toggle button

#### Domain Layer
- [ ] Location entity (latitude, longitude, address)
- [ ] LocationRepository interface
- [ ] GetNearbyVehicles use case (with radius parameter)
- [ ] GetDirections use case

#### Presentation Layer
- [ ] MapBloc with states (Loading, Loaded, Error, LocationPermissionDenied)
- [ ] Events (LoadNearbyVehicles, UpdateMapBounds, SelectMarker, GetDirections)
- [ ] Handle location permission states

#### UI Components
- [ ] MapPage with vehicle markers
- [ ] BottomSheet for selected vehicle preview
- [ ] Radius filter slider (1km, 5km, 10km, 25km, 50km)
- [ ] Location permission request dialog
- [ ] Floating action buttons (Current Location, Map Style, Filter)

---

## Sprint 10: Offline Support & Sync (NOT STARTED)

### Required Dependencies
```yaml
dependencies:
  connectivity_plus: ^5.0.2
  hive: ^2.2.3
  hive_flutter: ^1.1.0
```

### Tasks

#### Offline Architecture
- [ ] Implement ConnectivityService using connectivity_plus
  * Listen to connectivity changes (wifi, mobile, none)
  * Emit stream of ConnectivityStatus
  * Check internet reachability (not just connectivity)
- [ ] Create OfflineIndicator widget (banner at top when offline)
- [ ] Setup Hive database for local caching

#### Cache Strategy
- [ ] Implement CacheService with Hive:
  * Cache vehicles viewed (last 100)
  * Cache search results (with expiry time)
  * Cache user profile
  * Cache favorites offline
  * Cache conversation messages
- [ ] Define TTL (Time To Live) for each cache type
- [ ] Implement cache invalidation logic

#### Data Sync
- [ ] Create SyncQueue for pending operations:
  * Queue favorite toggle operations
  * Queue message sends
  * Queue profile updates
  * Queue search history additions
- [ ] Implement SyncService:
  * processSyncQueue() when online
  * retryFailedOperations() with exponential backoff
  * clearSyncQueue() after successful sync
- [ ] Add sync status indicators in UI

#### Conflict Resolution
- [ ] Implement Last-Write-Wins strategy for simple conflicts
- [ ] Version-based conflict resolution for complex entities
- [ ] User prompt for unresolvable conflicts

#### Offline Features
- [ ] View cached vehicles without internet
- [ ] Browse favorites offline
- [ ] View cached conversations
- [ ] Draft messages (send when online)
- [ ] Search history works offline
- [ ] Profile viewing offline

#### UI Components
- [ ] OfflineBanner widget (appears when no connection)
- [ ] SyncStatus indicator (syncing, synced, failed)
- [ ] Manual sync trigger button
- [ ] Offline mode toggle in settings (force offline for testing)
- [ ] Cache management page (view cache size, clear cache)

#### Repository Updates
- [ ] Update VehicleRepository to check cache first
- [ ] Update MessagingRepository to queue messages offline
- [ ] Update AuthRepository to work with cached profile
- [ ] Implement stale-while-revalidate pattern

#### Background Sync
- [ ] Setup WorkManager for background sync (Android)
- [ ] Setup Background Fetch for periodic sync (iOS)
- [ ] Sync on app resume if offline data changed

---

## Implementation Priority

### High Priority (Must Have)
1. **Sprint 8**: Dealer Dashboard + Listings Management (core dealer functionality)
2. **Sprint 10**: Basic offline support (view cached data, favorites offline)

### Medium Priority (Should Have)
3. **Sprint 9**: Maps with basic markers and current location
4. **Sprint 10**: Sync queue for favorites and messages

### Low Priority (Nice to Have)
5. **Sprint 9**: Advanced map features (clustering, directions, street view)
6. **Sprint 8**: Advanced CRM features (pipeline visualization, analytics charts)
7. **Sprint 10**: Advanced sync (conflict resolution, background sync)

---

## Estimated Lines of Code

### Sprint 8 (Complete)
- Domain: ~400 lines (entities, repository, use cases)
- Data: ~600 lines (models, mock repository)
- Presentation: ~400 lines (2 BLoCs)
- UI: ~2,000 lines (7 pages, 15+ widgets)
- **Total: ~3,400 lines**

### Sprint 9 (Complete)
- Domain: ~200 lines (entities, use cases)
- Data: ~200 lines (models, services)
- Presentation: ~250 lines (MapBloc)
- UI: ~1,000 lines (MapPage, widgets)
- **Total: ~1,650 lines**

### Sprint 10 (Complete)
- Services: ~400 lines (ConnectivityService, CacheService, SyncService)
- Repository Updates: ~300 lines (offline logic in existing repos)
- Presentation: ~200 lines (sync BLoC)
- UI: ~400 lines (offline indicators, sync UI)
- **Total: ~1,300 lines**

---

## Testing Strategy

### Unit Tests
- Test all use cases with mock repositories
- Test BLoCs with mock use cases
- Test offline sync queue logic
- Test cache expiration logic

### Widget Tests
- Test each major widget in isolation
- Test empty states, error states, loading states
- Golden tests for consistent UI

### Integration Tests
- Test full user flows (browse → detail → favorite → offline → sync)
- Test dealer listing creation flow
- Test messaging with offline queue

---

## Performance Considerations

### Maps
- Use marker clustering for 100+ markers
- Lazy load marker info windows
- Cache map tiles for offline viewing

### Offline Sync
- Batch sync operations (max 50 items per batch)
- Use debouncing for rapid operations (e.g., favorite toggle spam)
- Compress cached data with gzip

### Dealer Dashboard
- Lazy load analytics charts
- Cache dashboard stats for 5 minutes
- Use pagination for large listing lists (50 per page)

---

## Next Steps After Sprints 8-10

1. **Sprint 11**: Payments & Billing (Stripe integration, subscriptions, invoices)
2. **Sprint 12**: Performance optimization (app size, startup time, 60fps)
3. **Sprint 13**: Testing & QA (unit tests, widget tests, E2E tests)
4. **Sprint 14**: Beta testing and bug fixes
5. **Sprint 15**: Production release preparation

---

## Notes

- All Sprints 8-10 have foundational work in place (entities, some use cases)
- Mock repositories should be replaced with real API calls when backend is ready
- Push notification integration requires Firebase setup (google-services.json + GoogleService-Info.plist)
- Maps require API keys from Google Cloud Console
- Offline support should be tested extensively on real devices
- Performance testing critical before production (especially maps and sync)
