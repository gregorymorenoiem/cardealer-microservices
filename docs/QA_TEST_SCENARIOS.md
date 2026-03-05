# 🧪 QA Test Scenarios — OKLA Platform

**Version:** 1.0  
**Date:** 2026-03-05  
**Platform:** https://okla.com.do  
**Author:** QA Audit (Copilot)

---

## Table of Contents

1. [Test Credentials](#test-credentials)
2. [Auth Module](#1-auth-module)
3. [Vehicle Management](#2-vehicle-management)
4. [Subscription Plans](#3-subscription-plans)
5. [Admin Portal](#4-admin-portal)
6. [Contact / Communication](#5-contact--communication)
7. [Security](#6-security)
8. [Performance & Infrastructure](#7-performance--infrastructure)
9. [Homepage & Content](#8-homepage--content)
10. [Advertising System](#9-advertising-system)
11. [Code-Level Findings](#10-code-level-findings)

---

## Test Credentials

| Role   | Email                  | Password       |
| ------ | ---------------------- | -------------- |
| Admin  | admin@okla.local       | Admin123!@#    |
| Buyer  | buyer002@okla-test.com | BuyerTest2026! |
| Dealer | nmateo@okla.com.do     | Dealer2026!@#  |

---

## 1. Auth Module

### Login

| ID       | Test Name                           | Steps                                                                                      | Expected Result                      | Priority | Status     |
| -------- | ----------------------------------- | ------------------------------------------------------------------------------------------ | ------------------------------------ | -------- | ---------- |
| AUTH-001 | Login with valid admin credentials  | 1. Go to /login 2. Enter admin@okla.local / Admin123!@# 3. Click "Iniciar Sesión"          | Redirect to /admin dashboard         | P0       | NOT_TESTED |
| AUTH-002 | Login with valid buyer credentials  | 1. Go to /login 2. Enter buyer002@okla-test.com / BuyerTest2026! 3. Click "Iniciar Sesión" | Redirect to homepage or /cuenta      | P0       | NOT_TESTED |
| AUTH-003 | Login with valid dealer credentials | 1. Go to /login 2. Enter nmateo@okla.com.do / Dealer2026!@# 3. Click "Iniciar Sesión"      | Redirect to /vender/dashboard        | P0       | NOT_TESTED |
| AUTH-004 | Login with invalid email            | 1. Go to /login 2. Enter nonexistent@test.com / any password 3. Submit                     | Show error "Credenciales inválidas"  | P0       | NOT_TESTED |
| AUTH-005 | Login with invalid password         | 1. Go to /login 2. Enter admin@okla.local / WrongPassword 3. Submit                        | Show error "Credenciales inválidas"  | P0       | NOT_TESTED |
| AUTH-006 | Login with empty fields             | 1. Go to /login 2. Leave fields empty 3. Submit                                            | Show validation errors               | P1       | NOT_TESTED |
| AUTH-007 | Login CSRF token present            | 1. Go to /login 2. Inspect form for hidden CSRF input                                      | CSRF token present in form           | P0       | NOT_TESTED |
| AUTH-008 | Login rate limiting                 | 1. Submit 10+ failed login attempts rapidly                                                | Rate limit triggered after threshold | P1       | NOT_TESTED |

### Registration

| ID       | Test Name                            | Steps                                                                                                        | Expected Result                              | Priority | Status     |
| -------- | ------------------------------------ | ------------------------------------------------------------------------------------------------------------ | -------------------------------------------- | -------- | ---------- |
| AUTH-010 | Register new buyer                   | 1. Go to /registro 2. Fill name, email, password (with uppercase, lowercase, number, special char) 3. Submit | Account created, redirect to verify email    | P0       | NOT_TESTED |
| AUTH-011 | Register with weak password          | 1. Go to /registro 2. Enter password without special chars                                                   | Show password requirements error             | P0       | NOT_TESTED |
| AUTH-012 | Register with existing email         | 1. Go to /registro 2. Use an email that already exists                                                       | Show "email already registered" error        | P0       | NOT_TESTED |
| AUTH-013 | Register new dealer                  | 1. Go to /registro/dealer 2. Fill company name, RNC, contact info, password 3. Submit                        | Dealer account created, pending verification | P0       | NOT_TESTED |
| AUTH-014 | Register with XSS in name field      | 1. Go to /registro 2. Enter `<script>alert('xss')</script>` as name 3. Submit                                | Input sanitized, no XSS execution            | P0       | NOT_TESTED |
| AUTH-015 | Register with SQL injection in email | 1. Go to /registro 2. Enter `' OR 1=1 --` in email field 3. Submit                                           | Input rejected, no SQL injection             | P0       | NOT_TESTED |

### Password Recovery

| ID       | Test Name                          | Steps                                                                                                | Expected Result                                | Priority | Status     |
| -------- | ---------------------------------- | ---------------------------------------------------------------------------------------------------- | ---------------------------------------------- | -------- | ---------- |
| AUTH-020 | Request password reset             | 1. Go to /recuperar-contrasena 2. Enter valid email 3. Submit                                        | Success message, email sent                    | P0       | NOT_TESTED |
| AUTH-021 | Password reset with invalid email  | 1. Go to /recuperar-contrasena 2. Enter non-registered email 3. Submit                               | Generic success message (no email enumeration) | P1       | NOT_TESTED |
| AUTH-022 | Create new password via reset link | 1. Click reset link from email 2. Go to /crear-contrasena 3. Enter new password meeting requirements | Password updated, redirect to login            | P0       | NOT_TESTED |

### Session Management

| ID       | Test Name                        | Steps                                            | Expected Result                                         | Priority | Status     |
| -------- | -------------------------------- | ------------------------------------------------ | ------------------------------------------------------- | -------- | ---------- |
| AUTH-030 | Session persists on page refresh | 1. Login 2. Refresh page                         | User stays logged in                                    | P0       | NOT_TESTED |
| AUTH-031 | Session expiry                   | 1. Login 2. Wait for token to expire 3. Navigate | Redirect to login                                       | P1       | NOT_TESTED |
| AUTH-032 | Logout                           | 1. Login 2. Click logout                         | Session destroyed, redirect to homepage                 | P0       | NOT_TESTED |
| AUTH-033 | OAuth callback handling          | 1. Initiate social login 2. Complete OAuth flow  | User authenticated via callback at /callback/[provider] | P1       | NOT_TESTED |

---

## 2. Vehicle Management

### Vehicle Listing Creation

| ID      | Test Name                                   | Steps                                                                                                                       | Expected Result                                   | Priority | Status     |
| ------- | ------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------- | -------- | ---------- |
| VEH-001 | Create listing as individual seller         | 1. Login as buyer 2. Go to /vender/publicar 3. Fill vehicle details (make, model, year, price, condition, photos) 4. Submit | Listing created, visible in /cuenta/mis-vehiculos | P0       | NOT_TESTED |
| VEH-002 | Create listing as dealer                    | 1. Login as dealer 2. Go to /vender/publicar 3. Fill vehicle details 4. Submit                                              | Listing created with dealer badge                 | P0       | NOT_TESTED |
| VEH-003 | Create listing with smart publish wizard    | 1. Login 2. Go to /vender/publicar 3. Use method selector (manual/import/AI) 4. Complete wizard steps                       | Listing created through chosen method             | P0       | NOT_TESTED |
| VEH-004 | Photo upload                                | 1. During listing creation 2. Upload 5+ photos 3. Verify upload                                                             | Photos uploaded, thumbnails displayed             | P0       | NOT_TESTED |
| VEH-005 | Photo upload plan limits                    | 1. Login with free-tier account 2. Try to upload more photos than plan allows                                               | Show upgrade prompt or limit message              | P1       | NOT_TESTED |
| VEH-006 | Video upload (plan-gated)                   | 1. Login with eligible plan 2. Upload video to listing                                                                      | Video processed and attached to listing           | P1       | NOT_TESTED |
| VEH-007 | 360° view (plan-gated)                      | 1. Login with eligible plan 2. Use 360° step in smart publish 3. Upload images or use drag method                           | 360° view generated                               | P2       | NOT_TESTED |
| VEH-008 | Bulk import CSV (dealer)                    | 1. Login as dealer 2. Go to /vender/importar 3. Upload CSV 4. Map fields                                                    | Multiple listings created from CSV                | P1       | NOT_TESTED |
| VEH-009 | Create listing with missing required fields | 1. Start listing creation 2. Skip required fields 3. Submit                                                                 | Show validation errors for each required field    | P0       | NOT_TESTED |

### Vehicle Editing & Deletion

| ID      | Test Name             | Steps                                                                  | Expected Result                                         | Priority | Status     |
| ------- | --------------------- | ---------------------------------------------------------------------- | ------------------------------------------------------- | -------- | ---------- |
| VEH-010 | Edit existing listing | 1. Go to /vender/editar/[id] 2. Update price 3. Save                   | Changes saved and reflected                             | P0       | NOT_TESTED |
| VEH-011 | Delete listing        | 1. Go to /cuenta/mis-vehiculos 2. Click delete on a listing 3. Confirm | Listing removed from marketplace                        | P0       | NOT_TESTED |
| VEH-012 | Promote listing       | 1. Go to /vender/promover/[id] 2. Select promotion type 3. Pay         | Listing promoted with correct badge (Destacado/Premium) | P1       | NOT_TESTED |

### Vehicle Search & Filters

| ID      | Test Name                             | Steps                                                                      | Expected Result                                                   | Priority | Status     |
| ------- | ------------------------------------- | -------------------------------------------------------------------------- | ----------------------------------------------------------------- | -------- | ---------- |
| VEH-020 | Search by keyword                     | 1. Go to /vehiculos 2. Type "Toyota Corolla" in search 3. Submit           | Results show matching vehicles                                    | P0       | NOT_TESTED |
| VEH-021 | Filter by body type                   | 1. Go to /vehiculos 2. Select "SUV" filter                                 | Only SUVs shown                                                   | P0       | NOT_TESTED |
| VEH-022 | Filter by price range                 | 1. Go to /vehiculos 2. Set price range 500K-1M RD$                         | Only vehicles in range shown                                      | P0       | NOT_TESTED |
| VEH-023 | Filter by condition                   | 1. Go to /vehiculos 2. Select "Nuevo" / "Usado" / "Recién Importado"       | Correct vehicles filtered                                         | P0       | NOT_TESTED |
| VEH-024 | Filter by fuel type                   | 1. Go to /vehiculos?fuelType=hibrido                                       | Only hybrid vehicles shown                                        | P1       | NOT_TESTED |
| VEH-025 | Natural language AI search            | 1. Go to homepage 2. Type "yipeta economica para familia" 3. Search        | AI interprets query, redirects to /vehiculos with correct filters | P1       | NOT_TESTED |
| VEH-026 | Sort results (price asc/desc, newest) | 1. Go to /vehiculos 2. Change sort order                                   | Results reorder correctly                                         | P0       | NOT_TESTED |
| VEH-027 | Pagination                            | 1. Go to /vehiculos 2. Scroll through pages                                | Pages load correctly, no duplicates                               | P0       | NOT_TESTED |
| VEH-028 | Vehicle detail page                   | 1. Click on a vehicle card 2. View /vehiculos/[slug]                       | Full detail with images, price, specs, seller info                | P0       | NOT_TESTED |
| VEH-029 | Similar vehicles section              | 1. View vehicle detail page 2. Scroll to "Vehículos similares"             | Related vehicles displayed                                        | P1       | NOT_TESTED |
| VEH-030 | Compare vehicles                      | 1. Go to /comparar 2. Add 2-3 vehicles 3. View comparison                  | Side-by-side comparison with specs                                | P2       | NOT_TESTED |
| VEH-031 | Save search / alerts                  | 1. Go to /vehiculos with filters 2. Save search 3. Check /cuenta/busquedas | Search saved and alerts configured                                | P2       | NOT_TESTED |
| VEH-032 | Add to favorites                      | 1. Click heart icon on vehicle card 2. Check /cuenta/favoritos             | Vehicle added to favorites list                                   | P1       | NOT_TESTED |

---

## 3. Subscription Plans

| ID      | Test Name                              | Steps                                                                                      | Expected Result                         | Priority | Status     |
| ------- | -------------------------------------- | ------------------------------------------------------------------------------------------ | --------------------------------------- | -------- | ---------- |
| SUB-001 | View plans page                        | 1. Go to /precios or dealer plans page                                                     | Plans displayed with features & pricing | P0       | NOT_TESTED |
| SUB-002 | Upgrade from free to VISIBLE ($49/mo)  | 1. Login as dealer 2. Go to /cuenta/suscripcion 3. Select VISIBLE plan 4. Complete payment | Plan upgraded, features unlocked        | P0       | NOT_TESTED |
| SUB-003 | Upgrade from VISIBLE to PRO ($149/mo)  | 1. Login as dealer with VISIBLE plan 2. Go to /cuenta/suscripcion 3. Upgrade to PRO        | Plan upgraded with prorated billing     | P1       | NOT_TESTED |
| SUB-004 | Plan feature enforcement — photo limit | 1. Login with plan that limits photos 2. Try to upload more than allowed                   | Upload blocked with upgrade prompt      | P1       | NOT_TESTED |
| SUB-005 | Plan feature enforcement — bulk import | 1. Login without bulk import access 2. Go to /vender/importar                              | Feature gated, show upgrade banner      | P1       | NOT_TESTED |
| SUB-006 | Payment callback                       | 1. Complete payment 2. Redirect to /cuenta/pagos/callback                                  | Payment confirmed, plan activated       | P0       | NOT_TESTED |
| SUB-007 | Admin plan management                  | 1. Login as admin 2. Go to /admin/planes 3. Create/edit a plan                             | Plans CRUD working                      | P1       | NOT_TESTED |
| SUB-008 | Admin view subscriptions               | 1. Login as admin 2. Go to /admin/suscripciones                                            | All subscriptions listed with status    | P1       | NOT_TESTED |

---

## 4. Admin Portal

### Dashboard & Access

| ID      | Test Name                      | Steps                                           | Expected Result               | Priority | Status     |
| ------- | ------------------------------ | ----------------------------------------------- | ----------------------------- | -------- | ---------- |
| ADM-001 | Admin dashboard access         | 1. Login as admin 2. Navigate to /admin         | Dashboard with KPIs displayed | P0       | NOT_TESTED |
| ADM-002 | Non-admin cannot access /admin | 1. Login as buyer 2. Navigate to /admin         | Redirect to login or 403      | P0       | NOT_TESTED |
| ADM-003 | Non-admin API access blocked   | 1. As buyer, send request to admin API endpoint | 401/403 response              | P0       | NOT_TESTED |

### User Management

| ID      | Test Name                   | Steps                                        | Expected Result                            | Priority | Status     |
| ------- | --------------------------- | -------------------------------------------- | ------------------------------------------ | -------- | ---------- |
| ADM-010 | List all users              | 1. Go to /admin/usuarios                     | User list with search, filters, pagination | P0       | NOT_TESTED |
| ADM-011 | View user detail            | 1. Click on a user in /admin/usuarios        | User detail page with profile, activity    | P0       | NOT_TESTED |
| ADM-012 | Disable/enable user account | 1. Open user detail 2. Toggle account status | User account disabled/enabled              | P1       | NOT_TESTED |

### Vehicle Moderation

| ID      | Test Name                           | Steps                                                                  | Expected Result                                    | Priority | Status     |
| ------- | ----------------------------------- | ---------------------------------------------------------------------- | -------------------------------------------------- | -------- | ---------- |
| ADM-020 | List all vehicles (admin)           | 1. Go to /admin/vehiculos                                              | All vehicles with "Todos" tab, filters, pagination | P0       | NOT_TESTED |
| ADM-021 | Approve/reject vehicle listing      | 1. Go to /admin/vehiculos 2. Find pending listing 3. Approve or reject | Status updated                                     | P0       | NOT_TESTED |
| ADM-022 | Filter vehicles by status/condition | 1. Go to /admin/vehiculos 2. Use status/condition dropdown filters     | Correct vehicles filtered                          | P1       | NOT_TESTED |

### Dealer Management

| ID      | Test Name                  | Steps                                            | Expected Result                      | Priority | Status     |
| ------- | -------------------------- | ------------------------------------------------ | ------------------------------------ | -------- | ---------- |
| ADM-030 | List all dealers           | 1. Go to /admin/dealers                          | Dealer list with filters             | P0       | NOT_TESTED |
| ADM-031 | View dealer detail         | 1. Click on a dealer 2. View /admin/dealers/[id] | Dealer profile, inventory, analytics | P0       | NOT_TESTED |
| ADM-032 | Approve dealer application | 1. Find pending dealer 2. Approve                | Dealer account activated             | P0       | NOT_TESTED |

### Analytics & Reports

| ID      | Test Name                | Steps                         | Expected Result                     | Priority | Status     |
| ------- | ------------------------ | ----------------------------- | ----------------------------------- | -------- | ---------- |
| ADM-040 | View analytics dashboard | 1. Go to /admin/analytics     | Charts and metrics displayed        | P1       | NOT_TESTED |
| ADM-041 | View reports page        | 1. Go to /admin/reportes      | Reports available for download/view | P1       | NOT_TESTED |
| ADM-042 | View leads               | 1. Go to /admin/leads         | Lead management interface           | P1       | NOT_TESTED |
| ADM-043 | View transactions        | 1. Go to /admin/transacciones | Transaction history                 | P1       | NOT_TESTED |
| ADM-044 | View billing/invoicing   | 1. Go to /admin/facturacion   | Billing records                     | P1       | NOT_TESTED |

### KYC Verification

| ID      | Test Name              | Steps                                            | Expected Result                            | Priority | Status     |
| ------- | ---------------------- | ------------------------------------------------ | ------------------------------------------ | -------- | ---------- |
| ADM-050 | KYC queue list         | 1. Go to /admin/kyc                              | List of pending verifications              | P0       | NOT_TESTED |
| ADM-051 | Review KYC submission  | 1. Go to /admin/kyc/[id] 2. Review documents     | Documents viewable, approve/reject buttons | P0       | NOT_TESTED |
| ADM-052 | Approve KYC            | 1. Open KYC detail 2. Click approve              | User gets verified badge                   | P0       | NOT_TESTED |
| ADM-053 | Reject KYC with reason | 1. Open KYC detail 2. Click reject, enter reason | User notified, can resubmit                | P1       | NOT_TESTED |

### Content Management

| ID      | Test Name                   | Steps                                                                                                 | Expected Result                                             | Priority | Status     |
| ------- | --------------------------- | ----------------------------------------------------------------------------------------------------- | ----------------------------------------------------------- | -------- | ---------- |
| ADM-060 | Manage homepage sections    | 1. Go to /admin/secciones                                                                             | Sections listed with order controls, toggle active/inactive | P0       | NOT_TESTED |
| ADM-061 | Reorder homepage sections   | 1. Go to /admin/secciones 2. Use up/down arrows to reorder                                            | Section order updated, reflected on homepage                | P1       | NOT_TESTED |
| ADM-062 | Toggle section visibility   | 1. Go to /admin/secciones 2. Click "Ocultar"/"Mostrar"                                                | Section hidden/shown on homepage                            | P1       | NOT_TESTED |
| ADM-063 | Manage banners (CRUD)       | 1. Go to /admin/banners 2. Create new banner with title, subtitle, CTA, URL, image, placement 3. Save | Banner created and displayed on site                        | P0       | NOT_TESTED |
| ADM-064 | Edit existing banner        | 1. Go to /admin/banners 2. Click "Editar" on a banner 3. Update fields 4. Save                        | Banner updated                                              | P1       | NOT_TESTED |
| ADM-065 | Delete banner               | 1. Go to /admin/banners 2. Click delete icon 3. Confirm                                               | Banner removed                                              | P1       | NOT_TESTED |
| ADM-066 | Manage content (pages/blog) | 1. Go to /admin/contenido                                                                             | Tabs for Banners, Páginas, Blog                             | P1       | NOT_TESTED |
| ADM-067 | Advertising dashboard       | 1. Go to /admin/publicidad                                                                            | Platform overview, rotation config, quality scores, pricing | P0       | NOT_TESTED |
| ADM-068 | Ad spaces management        | 1. Go to /admin/espacios-publicitarios                                                                | All ad placements listed, configurable points & duration    | P1       | NOT_TESTED |
| ADM-069 | Promotions management       | 1. Go to /admin/promociones                                                                           | Promotional campaigns management                            | P2       | NOT_TESTED |

### System Configuration

| ID      | Test Name                 | Steps                                                          | Expected Result                 | Priority | Status     |
| ------- | ------------------------- | -------------------------------------------------------------- | ------------------------------- | -------- | ---------- |
| ADM-070 | System configuration page | 1. Go to /admin/configuracion                                  | System settings accessible      | P1       | NOT_TESTED |
| ADM-071 | System info page          | 1. Go to /admin/sistema                                        | System health, version info     | P2       | NOT_TESTED |
| ADM-072 | Maintenance page          | 1. Go to /admin/mantenimiento                                  | Maintenance tools accessible    | P2       | NOT_TESTED |
| ADM-073 | Roles & permissions       | 1. Go to /admin/roles                                          | Roles listed, assignable        | P1       | NOT_TESTED |
| ADM-074 | Team management           | 1. Go to /admin/equipo                                         | Team members listed             | P1       | NOT_TESTED |
| ADM-075 | Invite team member        | 1. Go to /admin/equipo/invitar 2. Enter email and role 3. Send | Invitation sent                 | P2       | NOT_TESTED |
| ADM-076 | OKLA Score configuration  | 1. Go to /admin/okla-score                                     | Score configuration accessible  | P2       | NOT_TESTED |
| ADM-077 | Early bird management     | 1. Go to /admin/early-bird                                     | Early bird registrations listed | P2       | NOT_TESTED |
| ADM-078 | Compliance dashboard      | 1. Go to /admin/compliance                                     | Compliance status visible       | P2       | NOT_TESTED |

### Support & Messaging

| ID      | Test Name                  | Steps                                            | Expected Result                         | Priority | Status     |
| ------- | -------------------------- | ------------------------------------------------ | --------------------------------------- | -------- | ---------- |
| ADM-080 | Support tickets list       | 1. Go to /admin/soporte                          | Tickets listed with filters             | P1       | NOT_TESTED |
| ADM-081 | View support ticket detail | 1. Click on a ticket 2. View /admin/soporte/[id] | Ticket details and conversation         | P1       | NOT_TESTED |
| ADM-082 | Admin messages             | 1. Go to /admin/mensajes                         | Messaging interface for admin           | P2       | NOT_TESTED |
| ADM-083 | Reviews management         | 1. Go to /admin/reviews                          | Reviews listed with moderation controls | P2       | NOT_TESTED |

---

## 5. Contact / Communication

| ID      | Test Name                          | Steps                                                                       | Expected Result                                       | Priority | Status     |
| ------- | ---------------------------------- | --------------------------------------------------------------------------- | ----------------------------------------------------- | -------- | ---------- |
| COM-001 | Contact seller from vehicle detail | 1. View a vehicle detail page 2. Click "Contactar Vendedor" 3. Send message | Message sent via platform                             | P0       | NOT_TESTED |
| COM-002 | WhatsApp integration               | 1. View vehicle detail 2. Click WhatsApp button                             | Opens WhatsApp with pre-filled message                | P0       | NOT_TESTED |
| COM-003 | Phone reveal click tracking        | 1. View vehicle detail 2. Click "Ver Teléfono"                              | Phone number revealed, analytics tracked              | P1       | NOT_TESTED |
| COM-004 | Messaging system                   | 1. Go to /mensajes                                                          | Inbox with conversations                              | P0       | NOT_TESTED |
| COM-005 | Contact form (API route)           | 1. Submit contact form                                                      | API validates all required fields, sends notification | P0       | NOT_TESTED |
| COM-006 | Chat widget (AI chatbot)           | 1. Click chat widget on any page 2. Ask a question                          | Chatbot responds with relevant info                   | P2       | NOT_TESTED |
| COM-007 | Vehicle-specific chat widget       | 1. View vehicle detail 2. Use chat widget                                   | Contextual support for that vehicle                   | P2       | NOT_TESTED |

---

## 6. Security

### Input Validation

| ID      | Test Name                                 | Steps                                                            | Expected Result                            | Priority | Status     |
| ------- | ----------------------------------------- | ---------------------------------------------------------------- | ------------------------------------------ | -------- | ---------- |
| SEC-001 | XSS prevention — script tag in form       | 1. Enter `<script>alert(1)</script>` in any text field 2. Submit | Input sanitized, script not executed       | P0       | NOT_TESTED |
| SEC-002 | XSS prevention — event handler            | 1. Enter `<img onerror="alert(1)" src="x">` in text field        | Input sanitized                            | P0       | NOT_TESTED |
| SEC-003 | SQL injection — login form                | 1. Enter `' OR '1'='1` in email field 2. Submit login            | Request rejected or sanitized              | P0       | NOT_TESTED |
| SEC-004 | SQL injection — search query              | 1. Enter `'; DROP TABLE vehicles;--` in search 2. Submit         | No SQL injection, query sanitized          | P0       | NOT_TESTED |
| SEC-005 | Backend validators (NoSqlInjection/NoXss) | 1. Send malicious payload to API endpoint 2. Check response      | FluentValidation rejects with proper error | P0       | NOT_TESTED |

### CSRF

| ID      | Test Name                             | Steps                                                       | Expected Result                                  | Priority | Status     |
| ------- | ------------------------------------- | ----------------------------------------------------------- | ------------------------------------------------ | -------- | ---------- |
| SEC-010 | CSRF token on state-changing requests | 1. Inspect any POST/PUT/DELETE form 2. Check for CSRF token | Token present via `csrfFetch()` or `<CsrfInput>` | P0       | NOT_TESTED |
| SEC-011 | Request without CSRF token rejected   | 1. Send POST request without CSRF header/body               | 403 Forbidden                                    | P0       | NOT_TESTED |

### JWT & Authentication

| ID      | Test Name                    | Steps                                                  | Expected Result                            | Priority | Status     |
| ------- | ---------------------------- | ------------------------------------------------------ | ------------------------------------------ | -------- | ---------- |
| SEC-020 | JWT token in httpOnly cookie | 1. Login 2. Inspect cookies                            | JWT stored as httpOnly, secure             | P0       | NOT_TESTED |
| SEC-021 | Expired JWT rejected         | 1. Modify JWT expiration to past 2. Send request       | 401 Unauthorized                           | P0       | NOT_TESTED |
| SEC-022 | Token refresh flow           | 1. Wait for access token to expire 2. Trigger API call | Refresh token used to get new access token | P1       | NOT_TESTED |

### Rate Limiting

| ID      | Test Name           | Steps                                     | Expected Result                       | Priority | Status     |
| ------- | ------------------- | ----------------------------------------- | ------------------------------------- | -------- | ---------- |
| SEC-030 | API rate limiting   | 1. Send 100+ requests in quick succession | 429 Too Many Requests after threshold | P1       | NOT_TESTED |
| SEC-031 | Login rate limiting | 1. Attempt 10+ failed logins              | Account locked or throttled           | P1       | NOT_TESTED |

### URL & Content Sanitization

| ID      | Test Name                  | Steps                                                 | Expected Result                   | Priority | Status     |
| ------- | -------------------------- | ----------------------------------------------------- | --------------------------------- | -------- | ---------- |
| SEC-040 | URL sanitization in links  | 1. Set profile URL to `javascript:alert(1)` 2. Render | URL sanitized via `sanitizeUrl()` | P0       | NOT_TESTED |
| SEC-041 | User content HTML escaping | 1. Enter HTML in vehicle description 2. View listing  | HTML escaped, rendered as text    | P0       | NOT_TESTED |

---

## 7. Performance & Infrastructure

### Page Load & Core Web Vitals

| ID       | Test Name                           | Steps                                               | Expected Result                                | Priority | Status     |
| -------- | ----------------------------------- | --------------------------------------------------- | ---------------------------------------------- | -------- | ---------- |
| PERF-001 | Homepage load time                  | 1. Open https://okla.com.do 2. Measure FCP          | FCP < 2.5s on 4G connection                    | P0       | NOT_TESTED |
| PERF-002 | Search results page load            | 1. Navigate to /vehiculos 2. Measure load time      | Page interactive < 3s                          | P0       | NOT_TESTED |
| PERF-003 | Vehicle detail page load            | 1. Navigate to a vehicle detail page 2. Measure LCP | LCP < 2.5s                                     | P0       | NOT_TESTED |
| PERF-004 | Image optimization                  | 1. Inspect vehicle images via DevTools              | Next.js Image component used, WebP/AVIF served | P1       | NOT_TESTED |
| PERF-005 | Lazy loading of below-fold sections | 1. Monitor network tab on homepage 2. Scroll down   | Sections load lazily with IntersectionObserver | P1       | NOT_TESTED |

### API Response Times

| ID       | Test Name                        | Steps                                                      | Expected Result | Priority | Status     |
| -------- | -------------------------------- | ---------------------------------------------------------- | --------------- | -------- | ---------- |
| PERF-010 | Vehicle search API response time | 1. GET /api/vehicles with filters 2. Measure response time | < 500ms         | P0       | NOT_TESTED |
| PERF-011 | Vehicle detail API response time | 1. GET /api/vehicles/{id} 2. Measure response time         | < 300ms         | P0       | NOT_TESTED |
| PERF-012 | Auth API response time           | 1. POST /api/auth/login 2. Measure response time           | < 1s            | P0       | NOT_TESTED |

### Caching

| ID       | Test Name                          | Steps                                                     | Expected Result                     | Priority | Status     |
| -------- | ---------------------------------- | --------------------------------------------------------- | ----------------------------------- | -------- | ---------- |
| PERF-020 | Redis caching on vehicle search    | 1. Make same search query twice 2. Compare response times | Second request significantly faster | P1       | NOT_TESTED |
| PERF-021 | TanStack Query client-side caching | 1. Navigate to /vehiculos 2. Navigate away 3. Return      | Data served from cache, no re-fetch | P1       | NOT_TESTED |

### Health Checks

| ID       | Test Name                        | Steps                                | Expected Result                           | Priority | Status     |
| -------- | -------------------------------- | ------------------------------------ | ----------------------------------------- | -------- | ---------- |
| PERF-030 | Service health endpoint /health  | 1. GET /health on each microservice  | 200 OK, no external checks                | P0       | NOT_TESTED |
| PERF-031 | Readiness endpoint /health/ready | 1. GET /health/ready on each service | DB and dependencies checked               | P1       | NOT_TESTED |
| PERF-032 | Liveness endpoint /health/live   | 1. GET /health/live on each service  | Always 200 (no checks, predicate = false) | P1       | NOT_TESTED |

### PWA

| ID       | Test Name                   | Steps                                                          | Expected Result                              | Priority | Status     |
| -------- | --------------------------- | -------------------------------------------------------------- | -------------------------------------------- | -------- | ---------- |
| PERF-040 | Service worker registration | 1. Visit site on mobile 2. Check service worker                | SW registered successfully                   | P2       | NOT_TESTED |
| PERF-041 | PWA install prompt          | 1. Visit site on supported browser 2. Check for install prompt | Install prompt appears (or iOS instructions) | P2       | NOT_TESTED |
| PERF-042 | Offline page                | 1. Go offline 2. Navigate to /offline                          | Offline page rendered                        | P2       | NOT_TESTED |

---

## 8. Homepage & Content

| ID       | Test Name                                | Steps                                       | Expected Result                                                                                                                                       | Priority | Status     |
| -------- | ---------------------------------------- | ------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | ---------- |
| HOME-001 | Hero section renders                     | 1. Visit homepage                           | Hero with background image, headline "Tu próximo vehículo está en OKLA", NL search bar                                                                | P0       | NOT_TESTED |
| HOME-002 | Hero quick filters work                  | 1. Click "SUV" quick filter on hero         | Redirects to /vehiculos?body_type=SUV                                                                                                                 | P0       | NOT_TESTED |
| HOME-003 | Trust badges displayed                   | 1. View hero section                        | "Vendedores Verificados", "Historial Garantizado", "Precios Transparentes"                                                                            | P1       | NOT_TESTED |
| HOME-004 | Featured vehicles section (FeaturedSpot) | 1. Scroll past hero                         | "⭐ Vehículos Destacados" section with vehicle cards                                                                                                  | P0       | NOT_TESTED |
| HOME-005 | Premium vehicles section (PremiumSpot)   | 1. Scroll further                           | "💎 Vehículos Premium" section with larger grid                                                                                                       | P0       | NOT_TESTED |
| HOME-006 | Dealer promo section                     | 1. Scroll to dealers section                | "Concesionarios en OKLA" with dealer cards and empty slots                                                                                            | P0       | NOT_TESTED |
| HOME-007 | Vehicle type sections render             | 1. Scroll through body type sections        | SUVs, Crossovers, Sedanes, Hatchbacks, Camionetas, Coupés, Deportivos, Convertibles, Vans, Minivans, Híbridos, Eléctricos — each with proper subtitle | P0       | NOT_TESTED |
| HOME-008 | Native dealer CTA banner                 | 1. Scroll between vehicle type sections     | "¿Eres dealer? Llega a más compradores" banner displayed                                                                                              | P1       | NOT_TESTED |
| HOME-009 | Why Choose OKLA section                  | 1. Scroll to value props                    | 6 value props: Vendedores Verificados, Fotos Profesionales, Mejor Precio, Contacto Directo, Vende Más Rápido, Pagos Seguros                           | P1       | NOT_TESTED |
| HOME-010 | CTA section at bottom                    | 1. Scroll to bottom CTA                     | "¿Listo para vender tu vehículo?" with "Publicar Gratis" and "Para Dealers" buttons                                                                   | P0       | NOT_TESTED |
| HOME-011 | Footer links present                     | 1. Scroll to footer                         | All navigation sections: Marketplace, Compañía, Legal, Soporte with proper links                                                                      | P0       | NOT_TESTED |
| HOME-012 | Footer contact info                      | 1. View footer                              | Email, phone, Santo Domingo RD, social media icons                                                                                                    | P1       | NOT_TESTED |
| HOME-013 | Testimonials (if enabled)                | 1. Check for testimonials section           | 5 testimonials with DR locations, verified roles, ratings                                                                                             | P2       | NOT_TESTED |
| HOME-014 | FAQ page content                         | 1. Go to /faq                               | All FAQ categories: Comprar, Vender, Dealers, Cuenta y Seguridad — with real answers                                                                  | P1       | NOT_TESTED |
| HOME-015 | Pricing guide page                       | 1. Go to /precios                           | Price ranges for vehicle categories in RD$                                                                                                            | P1       | NOT_TESTED |
| HOME-016 | Dark mode toggle                         | 1. Toggle dark mode                         | All homepage sections render correctly in dark mode                                                                                                   | P1       | NOT_TESTED |
| HOME-017 | Mobile responsive homepage               | 1. View homepage on mobile viewport (375px) | All sections stack correctly, search usable                                                                                                           | P0       | NOT_TESTED |

---

## 9. Advertising System

| ID      | Test Name                           | Steps                                                                      | Expected Result                                                           | Priority | Status     |
| ------- | ----------------------------------- | -------------------------------------------------------------------------- | ------------------------------------------------------------------------- | -------- | ---------- |
| ADS-001 | Seller creates advertising campaign | 1. Login as seller/dealer 2. Go to /vender/publicidad 3. Create campaign   | Campaign created with budget, targeting                                   | P1       | NOT_TESTED |
| ADS-002 | Ad impression tracking              | 1. View page with ad 2. Check impression counter                           | Impression recorded                                                       | P1       | NOT_TESTED |
| ADS-003 | Ad click tracking                   | 1. Click on promoted vehicle 2. Check click counter                        | Click recorded, CTR updated                                               | P1       | NOT_TESTED |
| ADS-004 | Admin rotation config               | 1. Go to /admin/publicidad 2. Change rotation algorithm 3. Save            | Algorithm updated (WeightedRandom/RoundRobin/CTROptimized/BudgetPriority) | P1       | NOT_TESTED |
| ADS-005 | Ad placement types                  | 1. Check FeaturedSpot placement 2. Check PremiumSpot placement             | Ads appear in correct positions                                           | P1       | NOT_TESTED |
| ADS-006 | Quality score calculation           | 1. View ad campaign metrics 2. Check quality score                         | Score calculated from CTR, budget, relevance                              | P2       | NOT_TESTED |
| ADS-007 | Admin ad spaces audit               | 1. Go to /admin/espacios-publicitarios 2. Review all 16 ad spaces          | All spaces visible with utilization, pricing, and config                  | P1       | NOT_TESTED |
| ADS-008 | Banner CRUD from admin              | 1. Go to /admin/banners 2. Create banner with all fields 3. Edit 4. Delete | Full CRUD cycle working                                                   | P0       | NOT_TESTED |

---

## 10. Code-Level Findings

### 10.1 Console.log Statements in Production Code

The following files have `console.log` statements that should be removed or replaced with a proper logger before production:

| File                                                  | Lines                           | Description                                    | Severity |
| ----------------------------------------------------- | ------------------------------- | ---------------------------------------------- | -------- |
| `src/services/auth.ts`                                | L594, L623                      | Session logging                                | Medium   |
| `src/services/settings.ts`                            | L146-179                        | Theme apply debugging (10+ statements)         | High     |
| `src/components/monitoring/web-vitals.tsx`            | L143, L215                      | Web vitals logging (acceptable for monitoring) | Low      |
| `src/components/pwa/install-prompt.tsx`               | L116, L120, L144, L149          | PWA install prompt debugging                   | Medium   |
| `src/components/pwa/service-worker-provider.tsx`      | L68, L74, L85, L103, L139, L169 | Service worker lifecycle logging               | Low      |
| `src/components/pwa/pwa-wrapper.tsx`                  | L26, L40                        | PWA install events                             | Low      |
| `src/components/chat/ChatWidget.tsx`                  | L29, L32, L35                   | Chat event logging                             | Medium   |
| `src/components/vehicle-detail/VehicleChatWidget.tsx` | L34, L37, L40                   | Vehicle chat logging                           | Medium   |
| `src/app/(auth)/callback/[provider]/page.tsx`         | L42                             | OAuth callback                                 | Medium   |
| `src/app/(admin)/admin/kyc/[id]/page.tsx`             | L106                            | KYC document fetch fallback                    | Low      |

**Total: 34 console.log statements in production code**

### 10.2 TODO / FIXME Comments

| File                                                    | Line | Comment                                                          | Severity |
| ------------------------------------------------------- | ---- | ---------------------------------------------------------------- | -------- |
| `src/components/lazy/index.tsx`                         | L168 | TODO: Create @/components/maps/Map when needed                   | Low      |
| `src/components/lazy/index.tsx`                         | L174 | TODO: Create @/components/maps/DealerMap when needed             | Low      |
| `src/components/lazy/index.tsx`                         | L191 | TODO: Create @/components/media/VideoPlayer when needed          | Medium   |
| `src/components/lazy/index.tsx`                         | L197 | TODO: Create @/components/forms/RichTextEditor when needed       | Low      |
| `src/components/lazy/index.tsx`                         | L203 | TODO: Create @/components/tables/DataTable when needed           | Low      |
| `src/components/lazy/index.tsx`                         | L209 | TODO: Create @/components/dealer/InventoryTable when needed      | Low      |
| `src/components/lazy/index.tsx`                         | L215 | TODO: Create @/components/modals/ContactModal when needed        | Medium   |
| `src/components/lazy/index.tsx`                         | L221 | TODO: Create @/components/modals/ShareModal when needed          | Low      |
| `src/components/lazy/index.tsx`                         | L227 | TODO: Create @/components/modals/Lightbox when needed            | Low      |
| `src/components/lazy/index.tsx`                         | L233 | TODO: Create @/components/modals/ComparisonModal when needed     | Low      |
| `src/components/lazy/index.tsx`                         | L239 | TODO: Create @/components/checkout/StripePaymentForm when needed | Medium   |
| `src/components/lazy/index.tsx`                         | L245 | TODO: Create @/components/checkout/AzulPaymentForm when needed   | Medium   |
| `src/components/lazy/index.tsx`                         | L251 | TODO: Create @/components/dealer/AnalyticsDashboard when needed  | Low      |
| `src/components/lazy/index.tsx`                         | L257 | TODO: Create @/components/admin/AdminDashboard when needed       | Low      |
| `src/components/vehicle-detail/seller-contact-card.tsx` | L108 | TODO: Track phone reveal analytics                               | Medium   |
| `src/components/vehicle-detail/seller-card.tsx`         | L97  | TODO: Track phone reveal event                                   | Medium   |
| `src/app/api/score/calculate/route.ts`                  | L77  | TODO: fetch live exchange rate from BCRD                         | High     |

**Total: 17 TODO items (3 High, 5 Medium, 9 Low)**

### 10.3 `any` Type Usage

| File                                                 | Line                 | Context                                  | Severity            |
| ---------------------------------------------------- | -------------------- | ---------------------------------------- | ------------------- |
| `src/lib/retargeting-pixels.ts`                      | L67                  | Facebook Pixel SDK init (7 `any` params) | Low (3rd-party SDK) |
| `src/lib/retargeting-pixels.ts`                      | L120-153             | TikTok Pixel SDK init (6 `any` params)   | Low (3rd-party SDK) |
| `src/app/api/advertising/advertiser-report/route.ts` | L67, L85, L109, L123 | Report builder with `any` types          | High                |

**Note:** The `any` usage in retargeting pixels is acceptable since these are 3rd-party SDK snippets. The advertiser report route should be typed.

### 10.4 Hardcoded Values Needing Attention

| File                                                | Line    | Issue                                                       | Severity |
| --------------------------------------------------- | ------- | ----------------------------------------------------------- | -------- |
| `src/app/api/score/calculate/route.ts`              | L77     | Hardcoded exchange rate `58.5` with TODO                    | High     |
| `src/components/homepage/category-cards.tsx`        | Various | Hardcoded vehicle counts (1250, 980, 650, etc.)             | Medium   |
| `src/components/homepage/testimonials-carousel.tsx` | Various | Hardcoded testimonials (acceptable as fallback)             | Low      |
| `src/components/homepage/why-choose-us.tsx`         | Various | Hardcoded value propositions (acceptable as static content) | Low      |

### 10.5 Missing Error Handling Patterns

| Pattern                      | Files Affected                                                       | Severity |
| ---------------------------- | -------------------------------------------------------------------- | -------- |
| Empty catch blocks           | Not found (good!)                                                    | —        |
| Missing loading states       | Most data-fetching components use TanStack Query with `isLoading` ✅ | —        |
| Unhandled promise rejections | Auth service and settings service have proper try/catch              | Low      |

### 10.6 Accessibility Observations

| Pattern                      | Status           | Notes                                                                       |
| ---------------------------- | ---------------- | --------------------------------------------------------------------------- |
| `alt` text on vehicle images | ✅ Present       | Format: "{year} {make} {model}"                                             |
| `aria-label` on icon buttons | ✅ Present       | "Subir sección", "Agregar a favoritos", etc.                                |
| Keyboard navigation          | ⚠️ Partial       | Hero search works with Enter, but some custom selects use native `<select>` |
| Color contrast in dark mode  | ⚠️ Needs testing | Several `text-muted-foreground` elements may have low contrast              |
| Screen reader support        | ⚠️ Needs testing | Focus management on modals and dialogs                                      |
| Form labels                  | ✅ Present       | All form inputs have associated `<Label>` elements                          |

### 10.7 Backend Health Check Issues (Discarded Services)

Multiple discarded services in `backend/_DESCARTADOS/` have **incorrect** health check configuration:

```
app.MapHealthChecks("/health");  // ❌ Missing external tag exclusion
```

These should use the standard pattern with `Predicate = check => !check.Tags.Contains("external")`.

**Active services** appear to use shared extensions (`UseStandardSerilog`, `AddStandardDatabase`, etc.) and follow the correct patterns.

### 10.8 CreateBootstrapLogger Violation

| File                                                           | Status                                                          |
| -------------------------------------------------------------- | --------------------------------------------------------------- |
| `backend/AdvertisingService/AdvertisingService.Api/Program.cs` | ✅ Correct (has comment "NO CreateBootstrapLogger()")           |
| `backend/_DESCARTADOS/StripePaymentService/.../Program.cs`     | ❌ Uses `CreateBootstrapLogger()` (discarded service, low risk) |

---

## Summary Statistics

| Category              | Total Tests | P0     | P1     | P2     |
| --------------------- | ----------- | ------ | ------ | ------ |
| Auth Module           | 16          | 11     | 4      | 1      |
| Vehicle Management    | 21          | 11     | 7      | 3      |
| Subscription Plans    | 8           | 3      | 4      | 1      |
| Admin Portal          | 33          | 11     | 16     | 6      |
| Contact/Communication | 7           | 3      | 2      | 2      |
| Security              | 11          | 8      | 3      | 0      |
| Performance & Infra   | 14          | 5      | 6      | 3      |
| Homepage & Content    | 17          | 5      | 8      | 4      |
| Advertising System    | 8           | 1      | 5      | 2      |
| **TOTAL**             | **135**     | **58** | **55** | **22** |

---

## Advertising Content Audit

### Homepage Content Status

All homepage sections have **real Spanish content** appropriate for the Dominican Republic market:

| Section            | Content Status | Details                                                                    |
| ------------------ | -------------- | -------------------------------------------------------------------------- |
| Hero headline      | ✅ Complete    | "Tu próximo vehículo está en OKLA"                                         |
| Hero subtitle      | ✅ Complete    | "Encuentra, compara y compra con total confianza en República Dominicana." |
| Search placeholder | ✅ Complete    | "Busca tu vehículo ideal"                                                  |
| Quick filters      | ✅ Complete    | SUV, Sedán, Camioneta, Deportivo, Híbrido, Eléctrico                       |
| Trust badges       | ✅ Complete    | Vendedores Verificados, Historial Garantizado, Precios Transparentes       |
| Featured vehicles  | ✅ Complete    | "⭐ Vehículos Destacados" (dynamic from API)                               |
| Premium vehicles   | ✅ Complete    | "💎 Vehículos Premium" (dynamic from API)                                  |
| Dealer promo       | ✅ Complete    | "Concesionarios en OKLA" with "Tu marca aquí" placeholders                 |
| Vehicle types      | ✅ Complete    | 12 sections with unique Spanish subtitles                                  |
| Native CTA banner  | ✅ Complete    | "¿Eres dealer? Llega a más compradores"                                    |
| Why Choose Us      | ✅ Complete    | 6 value props in Spanish                                                   |
| Bottom CTA         | ✅ Complete    | "¿Listo para vender tu vehículo?"                                          |
| Footer             | ✅ Complete    | 4 link sections, contact info, social links                                |
| Testimonials       | ✅ Complete    | 5 DR-specific testimonials                                                 |
| FAQ                | ✅ Complete    | 4 categories with real answers                                             |
| Pricing guide      | ✅ Complete    | DR market price ranges in RD$                                              |

### Admin Content Management Status

| Admin Page                    | Content Status | Details                                                                                    |
| ----------------------------- | -------------- | ------------------------------------------------------------------------------------------ |
| /admin/secciones              | ✅ Functional  | CRUD for homepage sections via API                                                         |
| /admin/banners                | ✅ Functional  | Full CRUD with placements: Homepage Hero, Homepage Secondary, Sidebar, Leaderboard, Inline |
| /admin/publicidad             | ✅ Functional  | Platform metrics, rotation config, pricing                                                 |
| /admin/contenido              | ✅ Functional  | Tabs: Banners, Páginas, Blog                                                               |
| /admin/espacios-publicitarios | ✅ Functional  | 16+ ad spaces defined with pricing in RD$                                                  |

### Banner Form Placeholders

The banner form has appropriate Spanish placeholders:

- Title: "ej. Financiamiento para tu vehículo"
- Subtitle: "Descripción breve del anuncio"
- CTA: "ej. Solicitar ahora"
- URL: "https://... o /ruta-interna"
- Image: "/images/banners/mi-banner.jpg"

---

## Recommended Priority Actions

### P0 — Critical (Fix Before Launch)

1. **Remove debug console.log in `services/settings.ts`** — 10+ theme debugging statements
2. **Fix hardcoded exchange rate** in `/api/score/calculate/route.ts` (currently 58.5 DOP/USD)
3. **Type the advertiser report route** — replace `any` types with proper interfaces
4. **Run all P0 test scenarios** — especially auth, vehicle CRUD, and payment flows

### P1 — Important (Fix Soon)

5. **Remove console.log in auth, chat, and PWA modules**
6. **Implement phone reveal analytics** (2 TODO items in seller cards)
7. **Test dark mode contrast** for all homepage sections
8. **Verify CSRF protection** on all state-changing API routes

### P2 — Nice to Have

9. **Create missing lazy-loaded components** (14 TODOs in `components/lazy/index.tsx`) — or remove stubs
10. **Add keyboard navigation** for custom filter dropdowns
11. **Dynamic vehicle counts** in category cards instead of hardcoded
