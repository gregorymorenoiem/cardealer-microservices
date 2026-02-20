# 🚀 OKLA Platform — Phase 2B: Reviews, Appointments, Chat & Production Validation

**version:** 3.0  
**lastUpdated:** 2026-02-20  
**author:** Engineering Team  
**priority:** CRITICAL — Production  
**estimatedScope:** Full-stack implementation + Production deployment + End-to-end validation

---

## 👥 Specialist Roles Active in This Prompt

| #   | Role                                      | Responsibility                                                   |
| --- | ----------------------------------------- | ---------------------------------------------------------------- |
| 1   | **Senior Platform Engineer (.NET 8)**     | Backend service fixes, DB migrations, auto-migrate in production |
| 2   | **Senior Frontend Engineer (Next.js 16)** | UI components — reviews, appointments, chat routing              |
| 3   | **DevOps / K8s Engineer**                 | Kubernetes deploy, secrets, CI/CD pipeline, health probes        |
| 4   | **Database Reliability Engineer**         | Idempotent SQL, migration safety, schema validation              |
| 5   | **Product Engineer**                      | UX flow: buyer reviews, dealer chatbot, seller live-chat         |
| 6   | **Security Engineer**                     | CSRF, Auth guards, rate limiting, input validation               |
| 7   | **QA Engineer**                           | Production smoke tests, E2E validation, edge-case coverage       |

---

## 📋 Executive Summary

This prompt drives the full implementation of **Phase 2B** for the OKLA platform. It resolves critical infrastructure issues identified in the current session (empty databases due to migrations not running in production), deploys three microservices currently at `replicas: 0`, and implements the complete **buyer review/rating system**, **appointment scheduling with calendar**, and **context-aware chat routing** (chatbot for dealers, live chat for sellers).

**Every feature must be tested in production at `https://okla.com.do` before marking complete.**

---

## ⚠️ CRITICAL TERMINAL RULES — Read Before Every Command

```
MANDATORY RULES (no exceptions):
1. ALWAYS use `timeout N` or `curl -m N` — never run a command that can hang indefinitely
2. NEVER run interactive commands: psql without -c "SQL", kubectl exec without timeout, vim, nano, less
3. ALWAYS chain short commands with &&, never leave a shell waiting for input
4. For kubectl exec: always use `timeout 30 kubectl exec ...`
5. For psql queries: always use -c "QUERY" flag (one-shot, no interactive mode)
6. For background processes: always check output with get_terminal_output before next step
7. Never use kubectl port-forward without a timeout wrapper
8. Token expiration: always re-login before API tests (tokens expire in 24h)
```

---

## 🔍 Phase 0 — Current State Audit (DO FIRST)

Before writing any code, verify the exact current state with these safe commands:

### 0.1 Verify active K8s pods

```bash
timeout 15 kubectl get pods -n okla --no-headers | awk '{print $1, $3, $4}' | sort
```

### 0.2 Verify which Docker images exist in GHCR

```bash
timeout 15 curl -m 10 -s -H "Authorization: Bearer $(timeout 5 gh auth token)" \
  "https://api.github.com/user/packages?package_type=container&per_page=50" \
  | grep -o '"name":"[^"]*"' | sort
```

### 0.3 Check DB status for affected services

```bash
# ReviewService DB
timeout 30 kubectl exec -n okla psql-client -- timeout 20 \
  env PGPASSWORD=REDACTED_AIVEN_PASSWORD \
  psql "host=okla-db-do-user-31493168-0.g.db.ondigitalocean.com port=25060 dbname=reviewservice user=doadmin sslmode=require" \
  -c "\dt" -c "SELECT count(*) FROM information_schema.tables WHERE table_schema='public';"

# KYCService DB
timeout 30 kubectl exec -n okla psql-client -- timeout 20 \
  env PGPASSWORD=REDACTED_AIVEN_PASSWORD \
  psql "host=okla-db-do-user-31493168-0.g.db.ondigitalocean.com port=25060 dbname=kycservice user=doadmin sslmode=require" \
  -c "\dt"

# DealerManagementService DB
timeout 30 kubectl exec -n okla psql-client -- timeout 20 \
  env PGPASSWORD=REDACTED_AIVEN_PASSWORD \
  psql "host=okla-db-do-user-31493168-0.g.db.ondigitalocean.com port=25060 dbname=dealermanagementservice user=doadmin sslmode=require" \
  -c "\dt"

# AppointmentService DB (may not exist yet)
timeout 30 kubectl exec -n okla psql-client -- timeout 20 \
  env PGPASSWORD=REDACTED_AIVEN_PASSWORD \
  psql "host=okla-db-do-user-31493168-0.g.db.ondigitalocean.com port=25060 dbname=postgres user=doadmin sslmode=require" \
  -c "SELECT datname FROM pg_database WHERE datname LIKE '%appointment%' OR datname LIKE '%review%' OR datname LIKE '%dealer%' OR datname LIKE '%kyc%';"
```

### 0.4 Confirm test accounts still work

```bash
# Seller login
ST=$(timeout 10 curl -m 8 -s -X POST "https://okla.com.do/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"seller-test@okla.com.do","password":"Test2026Seller!@#"}' \
  | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
echo "Seller token length: ${#ST}"

# Dealer login
DT=$(timeout 10 curl -m 8 -s -X POST "https://okla.com.do/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"dealer-test@okla.com.do","password":"Test2026Dealer!@#"}' \
  | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
echo "Dealer token length: ${#DT}"
```

### 0.5 Check current vehicle status (should be Draft=0, need Active=1)

```bash
timeout 30 kubectl exec -n okla psql-client -- timeout 20 \
  env PGPASSWORD=REDACTED_AIVEN_PASSWORD \
  psql "host=okla-db-do-user-31493168-0.g.db.ondigitalocean.com port=25060 dbname=vehiclessaleservice user=doadmin sslmode=require" \
  -c "SELECT id, title, status, seller_id, dealer_id FROM vehicles ORDER BY created_at DESC LIMIT 10;"
```

---

## 🗄️ Phase 1 — Database Migrations (Infrastructure Fix)

**Root Cause**: All affected services have `Program.cs` configured to run `Database.MigrateAsync()` or `EnsureCreated()` **only in Development environment**. In K8s the environment is `Production`, so migrations never execute. DBs remain empty and services either crash or return 500.

**Solution Strategy**: Fix Program.cs to run migrations in ALL environments, **AND** manually apply pending migrations via SQL directly to the managed DB for services that need it immediately.

---

### 1.1 Fix Program.cs — Auto-Migrate in All Environments

**Role: Senior Platform Engineer**

The following files must be updated to remove the `IsDevelopment()` guard on migration code. Apply the same pattern used by `VehiclesSaleService` (which works correctly):

#### ReviewService — `backend/ReviewService/ReviewService.Api/Program.cs`

Find and replace the migration block (currently inside `if (app.Environment.IsDevelopment())`):

```csharp
// BEFORE (wrong — only runs in Development):
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ReviewDbContext>();
    await context.Database.MigrateAsync();
}

// AFTER (correct — runs in all environments):
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ReviewDbContext>();
    try
    {
        await context.Database.MigrateAsync();
        Log.Information("ReviewService database migrations applied");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "ReviewService migration failed — continuing startup");
    }
}
```

#### DealerManagementService — `backend/DealerManagementService/DealerManagementService.Api/Program.cs`

Replace `EnsureCreated()` (which doesn't apply EF migrations) with `MigrateAsync()` and move it outside the `IsDevelopment()` block. If no formal Migrations folder exists, use `EnsureCreated()` unconditionally:

```csharp
// Replace (currently inside IsDevelopment block using EnsureCreated):
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DealerDbContext>();
    try
    {
        // EnsureCreated is safe if no Migrations folder exists — creates schema from model
        dbContext.Database.EnsureCreated();
        Log.Information("DealerManagementService database schema created/verified");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "DealerManagementService DB init failed — continuing");
    }
}
// REMOVE the IsDevelopment() wrapper — must run in Production too
```

#### KYCService — `backend/KYCService/KYCService.Api/Program.cs`

Search for the migration block and ensure it runs in all environments:

```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<KYCDbContext>();
    try
    {
        await context.Database.MigrateAsync();
        Log.Information("KYCService database migrations applied");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "KYCService migration failed");
    }
}
```

#### AppointmentService — `backend/AppointmentService/AppointmentService.Api/Program.cs`

Add migration block before `app.Run()`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppointmentDbContext>();
    try
    {
        await context.Database.MigrateAsync();
        Log.Information("AppointmentService database migrations applied");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "AppointmentService migration failed");
    }
}
```

---

### 1.2 Create DB Secrets in K8s

**Role: DevOps / K8s Engineer**

The K8s deployments reference secrets `reviewservice-db-secret`, `appointmentservice-db-secret`, and `dealermanagementservice-db-secret`. These must be created/verified before enabling the services.

```bash
# Verify which secrets already exist
timeout 15 kubectl get secrets -n okla | grep -E "review|appointment|dealermanagement"
```

If any are missing, create them (replace CONNECTION_STRING with the actual managed DB value):

```bash
# ReviewService DB secret
timeout 10 kubectl create secret generic reviewservice-db-secret -n okla \
  --from-literal=ConnectionStrings__DefaultConnection="Host=okla-db-do-user-31493168-0.g.db.ondigitalocean.com;Port=25060;Database=reviewservice;Username=doadmin;Password=REDACTED_AIVEN_PASSWORD;SslMode=Require;Trust Server Certificate=true" \
  --dry-run=client -o yaml | timeout 10 kubectl apply -f -

# DealerManagementService DB secret
timeout 10 kubectl create secret generic dealermanagementservice-db-secret -n okla \
  --from-literal=ConnectionStrings__DefaultConnection="Host=okla-db-do-user-31493168-0.g.db.ondigitalocean.com;Port=25060;Database=dealermanagementservice;Username=doadmin;Password=REDACTED_AIVEN_PASSWORD;SslMode=Require;Trust Server Certificate=true" \
  --dry-run=client -o yaml | timeout 10 kubectl apply -f -

# AppointmentService DB secret
timeout 10 kubectl create secret generic appointmentservice-db-secret -n okla \
  --from-literal=ConnectionStrings__DefaultConnection="Host=okla-db-do-user-31493168-0.g.db.ondigitalocean.com;Port=25060;Database=appointmentservice;Username=doadmin;Password=REDACTED_AIVEN_PASSWORD;SslMode=Require;Trust Server Certificate=true" \
  --dry-run=client -o yaml | timeout 10 kubectl apply -f -
```

Then create the databases on the managed PostgreSQL if they don't exist:

```bash
timeout 30 kubectl exec -n okla psql-client -- timeout 20 \
  env PGPASSWORD=REDACTED_AIVEN_PASSWORD \
  psql "host=okla-db-do-user-31493168-0.g.db.ondigitalocean.com port=25060 dbname=postgres user=doadmin sslmode=require" \
  -c "CREATE DATABASE reviewservice WITH OWNER doadmin;" \
  -c "CREATE DATABASE dealermanagementservice WITH OWNER doadmin;" \
  -c "CREATE DATABASE appointmentservice WITH OWNER doadmin;"
# Note: these will fail gracefully if DBs already exist — that is expected behavior
```

---

### 1.3 Activate Vehicles for Testing

**Role: Database Reliability Engineer**

Test vehicles created in the previous session have `status=0` (Draft). Public listings only show `status=1` (Active). Update them:

```bash
timeout 30 kubectl exec -n okla psql-client -- timeout 20 \
  env PGPASSWORD=REDACTED_AIVEN_PASSWORD \
  psql "host=okla-db-do-user-31493168-0.g.db.ondigitalocean.com port=25060 dbname=vehiclessaleservice user=doadmin sslmode=require" \
  -c "UPDATE vehicles SET status=1, updated_at=NOW() WHERE id IN ('616a181b-005d-45d8-8e79-b86b30971256','4b3186dc-3adf-4f59-9ad6-eb6df0b1686b') RETURNING id, title, status;"
```

---

## 🏗️ Phase 2 — Backend: ReviewService, DealerManagementService, AppointmentService

**Role: Senior Platform Engineer + DevOps / K8s Engineer**

### 2.1 Build & Push Docker Images via CI/CD

The three services need Docker images pushed to GHCR. The CI/CD workflow `smart-cicd.yml` already handles .NET services. Trigger by:

1. Commit all Program.cs fixes (Phase 1.1) with message:
   `fix(migrations): apply DB migrations in all environments (ReviewService, DealerMgmt, KYC, Appointment)`

2. Verify CI/CD builds all three services successfully:
   ```bash
   timeout 10 curl -m 8 -s -H "Authorization: Bearer $(gh auth token)" \
     "https://api.github.com/repos/gregorymorenoiem/cardealer-microservices/actions/runs?per_page=5" \
     | grep -o '"status":"[^"]*"\|"conclusion":"[^"]*"\|"name":"[^"]*"'
   ```

### 2.2 Enable Services in K8s

Once images exist in GHCR, enable deployments:

```bash
# Enable ReviewService
timeout 15 kubectl scale deployment reviewservice --replicas=1 -n okla

# Enable DealerManagementService
timeout 15 kubectl scale deployment dealermanagementservice --replicas=1 -n okla

# Enable AppointmentService
timeout 15 kubectl scale deployment appointmentservice --replicas=1 -n okla
```

Verify they start successfully (wait up to 90s):

```bash
timeout 15 kubectl get pods -n okla | grep -E "reviewservice|appointmentservice|dealermanagement"
```

Check logs for any startup error:

```bash
timeout 15 kubectl logs --tail=30 deployment/reviewservice -n okla
timeout 15 kubectl logs --tail=30 deployment/dealermanagementservice -n okla
timeout 15 kubectl logs --tail=30 deployment/appointmentservice -n okla
```

### 2.3 Validate Service Health via Gateway

```bash
timeout 10 curl -m 8 -s "https://okla.com.do/api/reviews/seller/00000000-0000-0000-0000-000000000000" | head -100
timeout 10 curl -m 8 -s "https://okla.com.do/api/dealers" | head -100
timeout 10 curl -m 8 -s "https://okla.com.do/api/appointments/slots?dealerId=9710694a-fb35-44cf-85c2-afb0bc0c4706&date=$(date +%Y-%m-%d)" | head -100
```

Expected: 200 or 404 with proper JSON — NOT 502/503.

---

## ⭐ Phase 3 — ReviewService: Complete Buyer Rating System

**Role: Senior Frontend Engineer + Product Engineer**

### 3.1 Backend: Verify Review Endpoints

The ReviewService already has:

- `GET /api/reviews/seller/{sellerId}` — list reviews
- `GET /api/reviews/seller/{sellerId}/summary` — rating stats
- `POST /api/reviews` — create review (auth required)

Verify these endpoints work after service is running:

```bash
# Get seller reviews (should return empty list, not 500)
timeout 10 curl -m 8 -s \
  "https://okla.com.do/api/reviews/seller/cd93c047-2185-47d5-9578-25b7f4bd31c8" | head -200
```

### 3.2 Frontend: Review Display on Vehicle Detail Page

**File: `frontend/web-next/src/app/(main)/vehiculos/[slug]/vehicle-detail-client.tsx`**

Add a Reviews section after `<VehicleTabs>` and before Similar Vehicles. The section must:

- Show star rating summary (avg + distribution bars) using real data from `useReviewStats`
- Show list of reviews with star icons, reviewer name, date, verified badge
- Show "Write Review" button — visible ONLY to authenticated buyers who have NOT already reviewed
- Star rating bars must use **real data** (not hardcoded percentages)
- Display seller type label: "Vendedor Individual" vs "Dealer" based on vehicle.sellerType

```tsx
// Import the hooks (they already exist):
import { useReviewsForTarget, useReviewStats } from "@/hooks/use-reviews";
import { useAuthStore } from "@/store/auth-store"; // or appropriate auth hook
import { WriteReviewDialog } from "@/components/reviews/write-review-dialog";
import { ReviewCard } from "@/components/reviews/review-card";
import { StarRating } from "@/components/reviews/star-rating";
import { ReviewSummaryBar } from "@/components/reviews/review-summary-bar";
```

The review section renders after tabs:

```tsx
{
  /* Reviews Section */
}
<ReviewsSection
  targetId={vehicle.sellerId || vehicle.dealerId}
  targetType={vehicle.dealerId ? "dealer" : "seller"}
  vehicleId={vehicle.id}
  vehicleTitle={`${vehicle.year} ${vehicle.make} ${vehicle.model}`}
/>;
```

### 3.3 Create Review Components

**Files to create:**

#### `frontend/web-next/src/components/reviews/star-rating.tsx`

Interactive star rating (1-5), supports read-only mode and interactive mode:

```tsx
interface StarRatingProps {
  value: number;
  onChange?: (rating: number) => void;
  readonly?: boolean;
  size?: "sm" | "md" | "lg";
}
```

- Uses Lucide `Star` with `fill-amber-500 text-amber-500` for filled stars
- Hover effect in interactive mode (highlight stars up to hovered index)
- Accessible: `aria-label`, keyboard navigation with arrow keys

#### `frontend/web-next/src/components/reviews/review-card.tsx`

Single review display card:

```tsx
interface ReviewCardProps {
  review: {
    id: string;
    buyerName: string;
    buyerPhotoUrl?: string;
    rating: number;
    title: string;
    content: string;
    createdAt: string;
    isVerifiedPurchase: boolean;
    helpfulVotes: number;
    response?: { content: string; createdAt: string };
  };
}
```

- Shows avatar (fallback to initials), name, date, star rating
- Shows "Compra Verificada" badge with CheckCircle icon if `isVerifiedPurchase`
- Shows seller/dealer response (if any) in a quoted block
- "¿Fue útil?" helpful vote button

#### `frontend/web-next/src/components/reviews/review-summary-bar.tsx`

Rating distribution bar (1-5 stars):

```tsx
interface ReviewSummaryBarProps {
  stats: {
    averageRating: number;
    totalReviews: number;
    ratingDistribution: { stars: number; count: number; percentage: number }[];
  };
}
```

- Renders the number, star icon, filled progress bar, and count for each rating
- Bars use `className="h-2 rounded-full bg-amber-500"` with `style={{ width: \`${pct}%\` }}`
- **MUST use real percentages from API**, not hardcoded values

#### `frontend/web-next/src/components/reviews/write-review-dialog.tsx`

Modal dialog for submitting a review:

```tsx
interface WriteReviewDialogProps {
  targetId: string;
  targetType: "seller" | "dealer";
  vehicleId?: string;
  vehicleTitle?: string;
  onSuccess?: () => void;
}
```

Required fields (Zod validation):

- `rating`: integer 1–5 (required — must click stars)
- `title`: string, 5–100 chars
- `content`: string, 20–1000 chars

Form uses `react-hook-form` + `zodResolver`. On submit:

```typescript
POST /api/reviews
Authorization: Bearer {token}
X-CSRF-Token: {csrfToken}
cookie: csrf_token={csrfToken}

{
  "sellerId": "uuid",
  "vehicleId": "uuid", // optional
  "rating": 5,
  "title": "Excelente servicio",
  "content": "El dealer fue muy profesional...",
  "buyerName": "Juan Pérez"
}
```

Show success toast: "¡Gracias por tu reseña!" using `sonner`  
Show error toast if user already reviewed this seller.

#### `frontend/web-next/src/components/reviews/reviews-section.tsx`

Main orchestrator component:

```tsx
// Combines ReviewSummaryBar + list of ReviewCards + WriteReviewDialog trigger
// Only shows "Escribir reseña" button if:
// 1. User is authenticated
// 2. User accountType is 'buyer' OR 'individual' (buyers can review)
// 3. User has NOT already reviewed this seller/dealer (check in reviews list)
```

### 3.4 Dealer Profile — Fix Hardcoded Rating Bars

**File: `frontend/web-next/src/app/(main)/dealers/[slug]/dealer-profile-client.tsx`**

The current rating distribution bars are hardcoded (75%, 20%, 3%, 1%, 1%). Replace with real data:

```tsx
// Replace the hardcoded percentage block:
// BEFORE:
width: `${stars === 5 ? 75 : stars === 4 ? 20 : ...}%`

// AFTER — use reviewStats from the hook already in scope:
const distEntry = reviewStats?.ratingDistribution?.find(d => d.stars === stars);
const pct = distEntry ? distEntry.percentage : 0;
width: `${pct}%`
```

Also add the "Write Review" button in the Reviews tab (same WriteReviewDialog component).

---

## 📅 Phase 4 — AppointmentService: Calendar & Scheduling

**Role: Senior Platform Engineer + Senior Frontend Engineer + Product Engineer**

### 4.1 Backend: AppointmentService Entities & Migrations

**Check existing structure:**

```bash
find /path/to/backend/AppointmentService -name "*.cs" | head -30
```

The `AppointmentDbContext` must have these entities:

- `Appointment` — dealerId, buyerId, vehicleId (optional), scheduledAt, status (Pending/Confirmed/Cancelled/Completed), notes
- `TimeSlot` — dealerId, date, startTime, endTime, isAvailable, maxAppointments, bookedCount
- `DealerSchedule` — dealerId, dayOfWeek (0-6), openTime, closeTime, isOpen, slotDurationMinutes (default 30)

If these entities exist but `TimeSlot` or `DealerSchedule` are missing, add them and create a new migration:

```bash
cd backend/AppointmentService
dotnet ef migrations add AddDealerScheduleAndTimeSlots \
  --project AppointmentService.Infrastructure \
  --startup-project AppointmentService.Api
```

Required REST endpoints (verify or add controllers):

- `GET /api/appointments/slots?dealerId={id}&startDate={date}&endDate={date}` — available slots
- `GET /api/appointments/schedule/{dealerId}` — dealer's weekly schedule
- `POST /api/appointments` — book appointment (auth required: buyer)
- `GET /api/appointments/my` — buyer's appointments (auth required)
- `GET /api/appointments/dealer/{dealerId}` — dealer's appointments (auth required: dealer)
- `PUT /api/appointments/{id}/confirm` — confirm (auth required: dealer)
- `PUT /api/appointments/{id}/cancel` — cancel (auth required: buyer or dealer)
- `PUT /api/appointments/schedule` — update dealer schedule (auth required: dealer)

### 4.2 Frontend: AppointmentCalendar Component

**File: `frontend/web-next/src/components/appointments/appointment-calendar.tsx`**

A full booking calendar widget that:

1. Shows month grid with days that have available slots highlighted
2. On day click → shows time slots as pills (e.g., "10:00 AM", "10:30 AM")
3. On slot click → opens confirmation dialog with note field
4. Only enabled for authenticated users (buyers)
5. Shows dealer's actual available schedule (from `/api/appointments/slots`)
6. Respects dealer's `DealerSchedule` (closed days shown as disabled/grayed)

```tsx
interface AppointmentCalendarProps {
  dealerId: string;
  dealerName: string;
  vehicleId?: string; // if booking from a specific vehicle listing
  vehicleTitle?: string;
  businessHours: {
    monday?: { open: string; close: string; isClosed: boolean };
    tuesday?: { open: string; close: string; isClosed: boolean };
    // ... etc
  };
}
```

State machine:

- `idle` → user sees month calendar
- `day_selected` → shows time slots for selected day
- `slot_selected` → shows booking form (notes field + confirm button)
- `submitting` → loading state
- `success` → confirmation with appointment details

**Important**: The calendar does NOT need a library (no react-calendar or react-datepicker). Build it with:

- `useState` for currentMonth (Date object)
- `useMemo` to compute days grid (fill leading/trailing days)
- Tailwind grid: `grid grid-cols-7`
- Highlight available days with `bg-primary/10 text-primary font-semibold`
- Disable past days: `opacity-40 cursor-not-allowed`

### 4.3 Integrate Calendar into Dealer Profile Page

**File: `frontend/web-next/src/app/(main)/dealers/[slug]/dealer-profile-client.tsx`**

Replace the current "Agendar Visita" button placeholder with the full `AppointmentCalendar` component:

```tsx
{
  /* Schedule Visit — was a placeholder button */
}
<Card className="border-primary bg-primary/10">
  <CardContent className="p-6">
    <Calendar className="mx-auto mb-3 h-10 w-10 text-primary" />
    <h3 className="mb-2 font-semibold">Agenda tu visita</h3>
    <AppointmentCalendar
      dealerId={dealer.id}
      dealerName={dealer.name}
      businessHours={primaryLocation?.businessHours}
    />
  </CardContent>
</Card>;
```

### 4.4 Integrate Calendar into Vehicle Detail Page (Dealer Vehicles Only)

When the vehicle belongs to a dealer (not a seller), show appointment calendar in the right sidebar under the `SellerCard`:

```tsx
{
  /* Only for dealer vehicles */
}
{
  vehicle.dealerId && (
    <AppointmentCalendar
      dealerId={vehicle.dealerId}
      dealerName={vehicle.sellerName}
      vehicleId={vehicle.id}
      vehicleTitle={`${vehicle.year} ${vehicle.make} ${vehicle.model}`}
      businessHours={vehicle.dealerBusinessHours}
    />
  );
}
```

---

## 💬 Phase 5 — Context-Aware Chat Routing

**Role: Product Engineer + Senior Frontend Engineer**

### 5.1 Business Rule Definition

| Vehicle Owner Type      | Chat Mode                    | Component to Show                         |
| ----------------------- | ---------------------------- | ----------------------------------------- |
| **Dealer**              | Chatbot (AI assistant "Ana") | `<DealerChatbotButton dealerId={...} />`  |
| **Seller (Individual)** | Live chat                    | `<SellerLiveChatButton sellerId={...} />` |

**Sellers do NOT get the chatbot.** They only get a live chat link. The chatbot is a premium feature exclusive to dealers.

### 5.2 VehicleDetail — Add Chat CTA to SellerCard

**File: `frontend/web-next/src/components/vehicle-detail/seller-card.tsx`**

If this file doesn't exist, check:

```bash
find frontend/web-next/src/components/vehicle-detail -name "*.tsx" | head -20
```

The SellerCard component must be updated to include:

For **Dealer** vehicles:

```tsx
<Button asChild className="w-full bg-primary hover:bg-primary/90">
  <Link href={`/dealers/${vehicle.dealerSlug}?chat=open`}>
    <MessageSquare className="mr-2 h-4 w-4" />
    Chatear con el dealer (Ana AI)
  </Link>
</Button>
```

For **Seller** (Individual) vehicles:

```tsx
<Button asChild variant="outline" className="w-full">
  <Link
    href={`/mensajes/nuevo?sellerId=${vehicle.sellerId}&vehicleId=${vehicle.id}`}
  >
    <MessageSquare className="mr-2 h-4 w-4" />
    Enviar mensaje al vendedor
  </Link>
</Button>
```

Also add "Ver perfil del dealer" link for dealer vehicles:

```tsx
{
  vehicle.dealerId && (
    <Button asChild variant="ghost" className="w-full text-sm">
      <Link href={`/dealers/${vehicle.dealerSlug}`}>
        <ExternalLink className="mr-2 h-4 w-4" />
        Ver inventario del dealer
      </Link>
    </Button>
  );
}
```

### 5.3 Dealer Profile — Chatbot Integration

**File: `frontend/web-next/src/app/(main)/dealers/[slug]/dealer-profile-client.tsx`**

Add a floating chatbot button that opens the OKLA chatbot (Ana) pre-configured for this dealer's inventory. Check if the chatbot widget already exists in:

```bash
find frontend/web-next/src -name "*chatbot*" -o -name "*chat-widget*" | head -10
```

If `ChatbotWidget` or similar component exists, embed it:

```tsx
import { ChatbotWidget } from "@/components/chatbot/chatbot-widget";

// Inside DealerProfileClient component, at bottom of return:
{
  /* Dealer Chatbot — fixed bottom-right */
}
<ChatbotWidget
  dealerId={dealer.id}
  dealerName={dealer.name}
  dealerLogo={dealer.logo}
  initialMessage={`Hola, soy Ana. ¿En qué te puedo ayudar con el inventario de ${dealer.name}?`}
/>;
```

If no chatbot widget exists yet:

**Create: `frontend/web-next/src/components/chatbot/chatbot-launcher.tsx`**

A floating action button that:

- Renders `position: fixed; bottom: 24px; right: 24px` (use Tailwind `fixed bottom-6 right-6`)
- Shows dealer avatar + "Pregunta a Ana" label on hover
- On click: opens `ChatbotPanel` (slide-in from right, 380px wide)
- `ChatbotPanel` calls `POST /api/chatbot/sessions/start` with dealerId
- Then uses the existing chat UI pattern from `frontend/web-next/src/app/(main)/test-chatbot/`

Check what exists in the test-chatbot route:

```bash
find frontend/web-next/src/app -path "*test-chatbot*" -name "*.tsx" | head -5
```

Use that as a reference for the chatbot API integration pattern.

### 5.4 URL Parameter — Auto-Open Chat on Dealer Landing

When navigating from vehicle detail to dealer profile with `?chat=open`, auto-open the chatbot:

```tsx
// In dealer-profile-client.tsx:
const searchParams = useSearchParams(); // React import from 'next/navigation'
const shouldOpenChat = searchParams.get("chat") === "open";

useEffect(() => {
  if (shouldOpenChat) {
    // Small delay to let the page render first
    const timer = setTimeout(() => setChatOpen(true), 500);
    return () => clearTimeout(timer);
  }
}, [shouldOpenChat]);
```

---

## 🏢 Phase 6 — Admin Portal: Publication Review & Validation

**Role: Senior Frontend Engineer + Product Engineer**

### 6.1 Verify Admin Portal Accessibility

```bash
# Admin login (use the admin account from the platform)
AT=$(timeout 10 curl -m 8 -s -X POST "https://okla.com.do/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@okla.com.do","password":"YOUR_ADMIN_PASSWORD"}' \
  | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
echo "Admin token: ${#AT} chars"

# Verify admin route loads
timeout 10 curl -m 8 -s -o /dev/null -w "%{http_code}" \
  -H "Cookie: access-token=$AT" \
  "https://okla.com.do/admin"
```

### 6.2 Check Admin Vehicles Page

Navigate to and verify the admin panel shows dealer and seller vehicles:

```bash
# Get all active vehicles (should show vehicles from both seller and dealer)
timeout 10 curl -m 8 -s -H "Authorization: Bearer $AT" \
  "https://okla.com.do/api/vehicles?page=1&pageSize=20" | head -300
```

Verify the response includes both:

- Vehicle `616a181b-005d-45d8-8e79-b86b30971256` (Toyota Corolla — seller `cd93c047...`)
- Vehicle `4b3186dc-3adf-4f59-9ad6-eb6df0b1686b` (Honda CR-V — dealer `428c82f6...`)

### 6.3 Admin Reviews Moderation

The admin panel has a `/admin/reviews` route. Verify it:

- Lists pending reviews (IsApproved = false)
- Allows approve/reject actions
- Shows reviewer info + content + star rating

**File to check:** `frontend/web-next/src/app/(admin)/admin/reviews/`

Verify the route renders correctly and that moderation actions call:

- `PUT /api/reviews/{id}/approve`
- `PUT /api/reviews/{id}/reject`

### 6.4 Admin KYC Review

The admin panel `/admin/kyc` should show pending KYC verifications:

```bash
timeout 10 curl -m 8 -s -H "Authorization: Bearer $AT" \
  "https://okla.com.do/api/kyc/profiles?status=pending&page=1" | head -200
```

If KYC DB is empty (no tables), the service needs its migration applied. After fixing Program.cs (Phase 1.1), restart the pod:

```bash
timeout 15 kubectl rollout restart deployment/kycservice -n okla
timeout 30 kubectl rollout status deployment/kycservice -n okla --timeout=60s
```

---

## 🧪 Phase 7 — Production Smoke Tests (REQUIRED — Run All)

**Role: QA Engineer**

All tests must be run with `-m 10` timeout. Test accounts:

- **Seller**: `seller-test@okla.com.do` / `Test2026Seller!@#` — userId: `cd93c047-2185-47d5-9578-25b7f4bd31c8`
- **Dealer**: `dealer-test@okla.com.do` / `Test2026Dealer!@#` — userId: `428c82f6-66d0-4294-868e-e01c3971fb3c` — dealerId: `9710694a-fb35-44cf-85c2-afb0bc0c4706`
- **Buyer**: Create a new test buyer account via `POST /api/auth/register` with `accountType: "buyer"`

### 7.1 Public Routes

```bash
for ROUTE in "/" "/vehiculos" "/buscar" "/dealers" "/login" "/registro"; do
  CODE=$(timeout 10 curl -m 8 -s -o /dev/null -w "%{http_code}" "https://okla.com.do$ROUTE")
  echo "$ROUTE → $CODE"
done
# Expected: all 200
```

### 7.2 Vehicle Detail Page Loads with Reviews Section

```bash
# Get slug of test vehicle
SLUG=$(timeout 30 kubectl exec -n okla psql-client -- timeout 20 \
  env PGPASSWORD=REDACTED_AIVEN_PASSWORD \
  psql "host=okla-db-do-user-31493168-0.g.db.ondigitalocean.com port=25060 dbname=vehiclessaleservice user=doadmin sslmode=require" \
  -t -c "SELECT slug FROM vehicles WHERE id='616a181b-005d-45d8-8e79-b86b30971256';" | xargs)

echo "Vehicle slug: $SLUG"

# Verify page loads
timeout 10 curl -m 8 -s -o /dev/null -w "%{http_code}" \
  "https://okla.com.do/vehiculos/$SLUG"
# Expected: 200
```

### 7.3 ReviewService API Tests

```bash
# Get fresh seller token
ST=$(timeout 10 curl -m 8 -s -X POST "https://okla.com.do/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"seller-test@okla.com.do","password":"Test2026Seller!@#"}' \
  | grep -o '"token":"[^"]*"' | cut -d'"' -f4)

# Test 1: Get reviews for seller (should return empty list, not 500)
timeout 10 curl -m 8 -s \
  "https://okla.com.do/api/reviews/seller/cd93c047-2185-47d5-9578-25b7f4bd31c8"
# Expected: { "items": [], "totalCount": 0, ... } or { "data": [] }

# Test 2: Get review summary
timeout 10 curl -m 8 -s \
  "https://okla.com.do/api/reviews/seller/cd93c047-2185-47d5-9578-25b7f4bd31c8/summary"
# Expected: { "averageRating": 0, "totalReviews": 0, ... }

# Test 3: Create a review as buyer (use buyer token — BT must be set first)
# First create buyer account:
BT=$(timeout 10 curl -m 8 -s -X POST "https://okla.com.do/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"email":"buyer-test@okla.com.do","password":"Test2026Buyer!@#","firstName":"Test","lastName":"Buyer","acceptTerms":true}' \
  | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
echo "Buyer token: ${#BT} chars"

CSRF_VAL="csrf_$(date +%s)"
timeout 10 curl -m 8 -s -X POST "https://okla.com.do/api/reviews" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $BT" \
  -H "X-CSRF-Token: $CSRF_VAL" \
  -b "csrf_token=$CSRF_VAL" \
  -d "{\"sellerId\":\"cd93c047-2185-47d5-9578-25b7f4bd31c8\",\"vehicleId\":\"616a181b-005d-45d8-8e79-b86b30971256\",\"rating\":5,\"title\":\"Excelente vendedor\",\"content\":\"El proceso fue muy profesional y transparente. Recomendado 100%.\",\"buyerName\":\"Test Buyer\"}"
# Expected: 200/201 with review object
```

### 7.4 DealerManagementService Tests

```bash
# Test: Get dealer profile (dealerId from previous session)
timeout 10 curl -m 8 -s \
  "https://okla.com.do/api/dealers/9710694a-fb35-44cf-85c2-afb0bc0c4706"
# Expected: 200 with dealer data (name: "Auto Test Premium RD")

# Test: Get dealer vehicles
timeout 10 curl -m 8 -s \
  "https://okla.com.do/api/dealers/9710694a-fb35-44cf-85c2-afb0bc0c4706/vehicles?page=1&pageSize=10"
# Expected: 200 with Honda CR-V vehicle
```

### 7.5 AppointmentService Tests

```bash
# Test: Get available slots for dealer
TODAY=$(date +%Y-%m-%d)
NEXTWEEK=$(date -v+7d +%Y-%m-%d 2>/dev/null || date -d "+7 days" +%Y-%m-%d)

timeout 10 curl -m 8 -s \
  "https://okla.com.do/api/appointments/slots?dealerId=9710694a-fb35-44cf-85c2-afb0bc0c4706&startDate=$TODAY&endDate=$NEXTWEEK"
# Expected: 200 with array of available date+time slots

# Test: Book appointment as buyer
CSRF_VAL="csrf_appt_$(date +%s)"
timeout 10 curl -m 8 -s -X POST "https://okla.com.do/api/appointments" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $BT" \
  -H "X-CSRF-Token: $CSRF_VAL" \
  -b "csrf_token=$CSRF_VAL" \
  -d "{\"dealerId\":\"9710694a-fb35-44cf-85c2-afb0bc0c4706\",\"vehicleId\":\"4b3186dc-3adf-4f59-9ad6-eb6df0b1686b\",\"scheduledAt\":\"${NEXTWEEK}T10:00:00Z\",\"notes\":\"Interesado en el Honda CR-V 2023\"}"
# Expected: 201 with appointment object
```

### 7.6 Seller Portal Tests

```bash
# Test: Seller dashboard route
timeout 10 curl -m 8 -s -o /dev/null -w "%{http_code}" \
  "https://okla.com.do/vender/dashboard"
# Expected: 307 (redirect to login if not authenticated)

# Test: Seller can view their own vehicle
ST=$(timeout 10 curl -m 8 -s -X POST "https://okla.com.do/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"seller-test@okla.com.do","password":"Test2026Seller!@#"}' \
  | grep -o '"token":"[^"]*"' | cut -d'"' -f4)

timeout 10 curl -m 8 -s \
  -H "Authorization: Bearer $ST" \
  "https://okla.com.do/api/vehicles/616a181b-005d-45d8-8e79-b86b30971256"
# Expected: 200 with vehicle data
```

### 7.7 Dealer Portal Tests

```bash
DT=$(timeout 10 curl -m 8 -s -X POST "https://okla.com.do/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"dealer-test@okla.com.do","password":"Test2026Dealer!@#"}' \
  | grep -o '"token":"[^"]*"' | cut -d'"' -f4)

# Test: Dealer dashboard
timeout 10 curl -m 8 -s -o /dev/null -w "%{http_code}" \
  "https://okla.com.do/dealer/dashboard"
# Expected: 307 (redirect to login without auth header)

# Test: Dealer profile accessible publicly
DEALER_SLUG=$(timeout 30 kubectl exec -n okla psql-client -- timeout 20 \
  env PGPASSWORD=REDACTED_AIVEN_PASSWORD \
  psql "host=okla-db-do-user-31493168-0.g.db.ondigitalocean.com port=25060 dbname=dealermanagementservice user=doadmin sslmode=require" \
  -t -c "SELECT slug FROM dealers WHERE id='9710694a-fb35-44cf-85c2-afb0bc0c4706';" 2>/dev/null | xargs)
echo "Dealer slug: $DEALER_SLUG"

timeout 10 curl -m 8 -s -o /dev/null -w "%{http_code}" \
  "https://okla.com.do/dealers/$DEALER_SLUG"
# Expected: 200
```

### 7.8 ChatbotService Test

```bash
# Test chatbot session start with dealer context
CSRF_VAL="csrf_chat_$(date +%s)"
timeout 15 curl -m 12 -s -X POST "https://okla.com.do/api/chatbot/sessions/start" \
  -H "Content-Type: application/json" \
  -H "X-CSRF-Token: $CSRF_VAL" \
  -b "csrf_token=$CSRF_VAL" \
  -d "{\"dealerId\":\"9710694a-fb35-44cf-85c2-afb0bc0c4706\",\"mode\":\"DealerInventory\"}"
# Expected: 200/201 with sessionId
```

---

## 🔁 Phase 8 — Final Deployment & Verification

**Role: DevOps / K8s Engineer + QA Engineer**

### 8.1 Commit All Frontend Changes

```bash
cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices
timeout 10 git add frontend/backend
timeout 10 git status --short | head -30
# Verify only expected files changed

timeout 10 git commit -m "feat(reviews,appointments,chat): buyer ratings, appointment calendar, context-aware chat routing

- ReviewService: display reviews with real star ratings on vehicle detail and dealer pages
- WriteReviewDialog: buyers can rate sellers/dealers 1-5 stars with title + comment
- AppointmentCalendar: dealers show available slots, buyers can book test drives
- Chat routing: dealer vehicles → chatbot (Ana), seller vehicles → live chat
- Dealer landing page: link from vehicle detail, chatbot auto-open via ?chat=open param
- DealerManagementService: fix auto-migrate in production (not only Development)
- KYCService: fix auto-migrate in production
- ReviewService: fix auto-migrate in production
- AppointmentService: fix auto-migrate in production"

timeout 10 git push origin main
```

### 8.2 Wait for CI/CD and Verify Deployment

```bash
# Wait 2 minutes for CI/CD, then check
sleep 120
timeout 15 kubectl get pods -n okla --no-headers | awk '{print $1, $2, $3}' | sort
```

### 8.3 Restart Services with New Images

```bash
timeout 15 kubectl rollout restart deployment/reviewservice -n okla
timeout 15 kubectl rollout restart deployment/dealermanagementservice -n okla
timeout 15 kubectl rollout restart deployment/appointmentservice -n okla
timeout 15 kubectl rollout restart deployment/kycservice -n okla
```

### 8.4 Verify Pod Startup and Migration Logs

```bash
# Wait 30s then check logs for migration confirmation
sleep 30
timeout 15 kubectl logs --tail=20 deployment/reviewservice -n okla | grep -i "migrat\|error\|start"
timeout 15 kubectl logs --tail=20 deployment/dealermanagementservice -n okla | grep -i "migrat\|error\|start"
timeout 15 kubectl logs --tail=20 deployment/appointmentservice -n okla | grep -i "migrat\|error\|start"
timeout 15 kubectl logs --tail=20 deployment/kycservice -n okla | grep -i "migrat\|error\|start"
```

### 8.5 Final Comprehensive Route Test

```bash
ROUTES=(
  "/" "/vehiculos" "/buscar" "/dealers" "/login" "/registro"
  "/vender/dashboard" "/dealer/dashboard" "/publicar"
)
for ROUTE in "${ROUTES[@]}"; do
  CODE=$(timeout 10 curl -m 8 -s -o /dev/null -w "%{http_code}" "https://okla.com.do$ROUTE")
  echo "[$CODE] $ROUTE"
done
```

Expected codes: 200 for public routes, 307 for auth-protected routes.

### 8.6 Verify DB Tables Were Created

```bash
# ReviewService tables
timeout 30 kubectl exec -n okla psql-client -- timeout 20 \
  env PGPASSWORD=REDACTED_AIVEN_PASSWORD \
  psql "host=okla-db-do-user-31493168-0.g.db.ondigitalocean.com port=25060 dbname=reviewservice user=doadmin sslmode=require" \
  -c "\dt"
# Expected: reviews, review_summaries, review_responses, review_helpful_votes, seller_badges, review_requests, fraud_detection_logs

# AppointmentService tables
timeout 30 kubectl exec -n okla psql-client -- timeout 20 \
  env PGPASSWORD=REDACTED_AIVEN_PASSWORD \
  psql "host=okla-db-do-user-31493168-0.g.db.ondigitalocean.com port=25060 dbname=appointmentservice user=doadmin sslmode=require" \
  -c "\dt"
# Expected: appointments, time_slots, dealer_schedules (or similar names)
```

---

## ✅ Definition of Done (DoD)

A task is **COMPLETE** only when ALL of the following pass:

### Infrastructure

- [ ] `reviewservice` pod Running (replicas: 1), DB has tables
- [ ] `dealermanagementservice` pod Running (replicas: 1), DB has tables
- [ ] `appointmentservice` pod Running (replicas: 1), DB has tables
- [ ] `kycservice` DB has tables (migrations applied)
- [ ] Test vehicles status=1 (Active/Published), visible in `/vehiculos` public listing
- [ ] All 4 DB secrets exist in K8s namespace `okla`

### ReviewService

- [ ] `GET /api/reviews/seller/{id}` → 200 (not 502/503)
- [ ] `GET /api/reviews/seller/{id}/summary` → 200
- [ ] `POST /api/reviews` → 201 with valid JWT + CSRF
- [ ] Star rating visible on vehicle detail page (vehicle owned by seller)
- [ ] Star rating visible on dealer profile page (reviews tab)
- [ ] Star distribution bars use **real data** (not hardcoded)
- [ ] "Write Review" button visible only to authenticated buyers
- [ ] WriteReviewDialog validates: rating required, title 5-100 chars, content 20-1000 chars
- [ ] Success toast shown after review submission

### AppointmentService

- [ ] `GET /api/appointments/slots?dealerId=...` → 200 with available slots array
- [ ] `POST /api/appointments` → 201 with valid buyer JWT + CSRF
- [ ] AppointmentCalendar renders on dealer profile page
- [ ] AppointmentCalendar renders on vehicle detail page (dealer vehicles only)
- [ ] Past days are disabled in calendar
- [ ] Closed days (per dealer schedule) are disabled
- [ ] Booking confirmation dialog shows appointment details

### Chat Routing

- [ ] Dealer vehicle detail: "Chatear con el dealer (Ana AI)" button present
- [ ] Seller vehicle detail: "Enviar mensaje al vendedor" button present (no chatbot)
- [ ] Dealer vehicle detail: "Ver inventario del dealer" link present
- [ ] Dealer landing page: chatbot widget present (floating button)
- [ ] `?chat=open` param auto-opens chatbot on dealer page

### Admin Portal

- [ ] Admin `/admin/vehiculos` shows both seller and dealer vehicles
- [ ] Admin `/admin/reviews` accessible and shows review list
- [ ] Admin `/admin/kyc` accessible (even if empty initially)

### Production Smoke Tests

- [ ] All 7 test suites in Phase 7 pass without 5xx errors
- [ ] Both seller and dealer can view their respective dashboards
- [ ] New buyer account created + can submit review
- [ ] Appointment booked via API returns 201

---

## 🐛 Known Issues & Mitigations

| Issue                                                    | Root Cause                     | Mitigation                                          |
| -------------------------------------------------------- | ------------------------------ | --------------------------------------------------- |
| ReviewService migration only in Dev                      | `if (IsDevelopment())` guard   | Remove guard (Phase 1.1)                            |
| DealerManagement uses `EnsureCreated` not `MigrateAsync` | No formal migrations           | Move outside Dev guard (Phase 1.1)                  |
| KYC DB empty despite pod running                         | Same Dev-only migration guard  | Fix Program.cs + restart pod                        |
| VIN unique constraint on empty VIN                       | `IX_vehicles_VIN` unique index | Pass unique VIN: `$(date +%s)` suffix               |
| CSRF 403 on POST APIs                                    | Double Submit Cookie pattern   | Always add `-H "X-CSRF-Token: V" -b "csrf_token=V"` |
| Vehicle status=0 (Draft) not visible                     | Default status is Draft        | Run SQL UPDATE status=1 (Phase 1.3)                 |
| ReviewService image not in GHCR                          | Never built/pushed             | Commit triggers smart-cicd.yml                      |
| AppointmentService image not in GHCR                     | Never built/pushed             | Commit triggers smart-cicd.yml                      |

---

## 📎 Reference Data

```
# Production URL
https://okla.com.do

# Managed PostgreSQL
Host: okla-db-do-user-31493168-0.g.db.ondigitalocean.com
Port: 25060
User: doadmin
Password: REDACTED_AIVEN_PASSWORD
sslmode: require

# Test Accounts
Seller: seller-test@okla.com.do / Test2026Seller!@#
  UserId: cd93c047-2185-47d5-9578-25b7f4bd31c8
Dealer: dealer-test@okla.com.do / Test2026Dealer!@#
  UserId: 428c82f6-66d0-4294-868e-e01c3971fb3c
  DealerId: 9710694a-fb35-44cf-85c2-afb0bc0c4706

# Test Vehicles
Seller vehicle (Toyota Corolla 2024): 616a181b-005d-45d8-8e79-b86b30971256
Dealer vehicle (Honda CR-V 2023):    4b3186dc-3adf-4f59-9ad6-eb6df0b1686b

# K8s
Cluster: okla-cluster
Namespace: okla
psql-client pod: postgres:16-alpine with sleep 3600

# CSRF Pattern
Header: X-CSRF-Token: VALUE
Cookie: csrf_token=VALUE
(VALUE must be identical in both — any non-empty string works)

# Vehicle Status Enum (integer)
0 = Draft (not visible publicly)
1 = Active / Published (visible)
2 = Sold
3 = Paused
```

---

## 🚫 Anti-Patterns — Never Do These

```
❌ kubectl exec ... without timeout wrapper
❌ kubectl port-forward (blocks terminal)
❌ psql without -c "SQL" flag (interactive — blocks terminal)
❌ git push --force (rewrites history)
❌ Installing npm packages (use pnpm)
❌ Using `variant="destructive"` in Badge (use `variant="danger"`)
❌ Using Jest (project uses Vitest)
❌ Hardcoding JWT or DB passwords in code
❌ Using CreateBootstrapLogger() + UseStandardSerilog() together
❌ Using OpenTelemetry > 1.9.0 (breaks .NET 8 build)
❌ Setting env var ASPNETCORE_ENVIRONMENT to Development on K8s pods
```

---

_Prompt Engineering by: Platform Architecture Team_  
_Role Coverage: Backend · Frontend · DevOps · Database · Product · Security · QA_  
_Last Validated: 2026-02-20 against production cluster state_
