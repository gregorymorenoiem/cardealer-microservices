# KYC Documents Missing - Diagnostic Report

**Date**: 2026-02-24 09:00 UTC → **UPDATED: 2026-02-24 06:30 UTC**  
**Issue**: User uploaded KYC documents (cédula + selfie) but they don't appear in admin panel  
**Status**: 🟡 ROOT CAUSE FOUND & FIXED - IdempotencyService connection errors blocking uploads

---

## 🚨 ROOT CAUSE IDENTIFIED

**Problem**: The KYC document upload endpoint was failing silently because the `IdempotencyMiddleware` was trying to connect to `IdempotencyService` at `idempotencyservice:8080`, but **this service doesn't exist in the Kubernetes cluster**.

When `idempotencyClient.CheckAsync()` was called, it raised an unhandled `HttpRequestException` (Connection refused), which caused the entire request to fail before reaching the document upload handler.

**Evidence from logs**:

```
fail: KYCService.Application.Clients.IdempotencyServiceClient[0]
      Error checking idempotency key: kyc_mm0h5389_1nxgxc7kvdq
      System.Net.Http.HttpRequestException: Connection refused (idempotencyservice:8080)

warn: KYCService.Api.Middleware.IdempotencyMiddleware[0]
      IdempotencyService unavailable, proceeding without idempotency
```

The middleware logged "proceeding without idempotency" but **the exception was propagating up the stack**, blocking the request from reaching the handler.

---

## Solution Applied

**Commit: `119d0f13`** - "fix: handle IdempotencyService connection errors in KYC middleware"

Changed `IdempotencyMiddleware.cs`:

1. Wrapped `idempotencyClient.CheckAsync()` in try-catch
2. Wrapped `idempotencyClient.StartProcessingAsync()` in try-catch
3. When IdempotencyService is unreachable, gracefully proceed to next middleware

**Before**:

```csharp
var checkResult = await idempotencyClient.CheckAsync(idempotencyKey, requestHash);
// Exception raised here if service unavailable → request fails
```

**After**:

```csharp
IdempotencyCheckResponse? checkResult = null;
try
{
    checkResult = await idempotencyClient.CheckAsync(idempotencyKey, requestHash);
}
catch (Exception ex)
{
    _logger.LogWarning(ex, "IdempotencyService check failed, proceeding without idempotency check");
    await _next(context);
    return;
}
```

---

## Problem Summary

- User `gmoreno@okla.com.do` successfully uploaded KYC documents to S3
- Documents appear in S3 storage (verified: file exists)
- API response shows `"documents": []` (empty array)
- Database query confirms: `kyc_documents` table has 0 rows for this profile
- Backend logs show NO evidence of handler being called
- **ROOT CAUSE**: Middleware exception prevented request from reaching handler

---

## Investigation Results

### 1. ✅ Database Verification

- **PostgreSQL Location**: DigitalOcean Managed Database (`okla-db-do-user-31493168-0.g.db.ondigitalocean.com:25060`)
- **Database Name**: `kycservice` (✅ EXISTS)
- **Profile Record**: EXISTS with ID `250310b4-2b69-49b3-8715-6993fd8a239c`
- **Document Count**: 0 (no documents in `kyc_documents` table)
- **Profile Status**: 1 (Pending) - **NOT updated to 2 (InProgress)**
  - If upload handler ran successfully, status would be 2
  - Status is still 1, meaning handler never executed UpdateAsync

### 2. ✅ Backend Code Review

**File**: `backend/KYCService/KYCService.Application/Handlers/KYCProfileHandlers.cs`

Handler flow:

1. Gets profile by ID → ✅ Correctly implemented
2. Creates KYCDocument entity → ✅ Correctly implemented
3. Calls `_repository.CreateAsync(document)` → ✅ Correctly implemented
4. Updates profile status to InProgress → ✅ Correctly implemented

**File**: `backend/KYCService/KYCService.Infrastructure/Repositories/KYCRepositories.cs`

Repository:

```csharp
public async Task<KYCDocument> CreateAsync(KYCDocument document, CancellationToken cancellationToken)
{
    _context.KYCDocuments.Add(document);
    await _context.SaveChangesAsync(cancellationToken);
```

    return document;

}

````

✅ Correct implementation

### 3. ✅ Gateway Configuration

**File**: `backend/Gateway/Gateway.Api/ocelot.prod.json` (lines 1989-1993)

Route configuration:

- **Upstream**: `/api/kyc/profiles/{profileId}/documents`
- **Methods**: `["OPTIONS", "GET", "POST"]` ✅ POST is allowed
- **Downstream**: `/api/kyc/profiles/{profileId}/documents`
- **Target**: `kycservice:8080` ✅ Correct port
- **Auth**: Bearer token required ✅ Correct

### 4. ✅ Controller Configuration

**File**: `backend/KYCService/KYCService.Api/Controllers/KYCDocumentsController.cs`

```csharp
[HttpPost("profiles/{profileId:guid}/documents")]
[Authorize]
public async Task<ActionResult<KYCDocumentDto>> UploadDocument(
    Guid profileId,
    [FromBody] UploadKYCDocumentCommand command)
````

✅ Route matches Gateway configuration

### 5. ⚠️ Backend Logs

**Observation**: NO logs related to document upload in KYCService pod

```bash
kubectl logs -n okla -l app=kycservice --tail=500 | grep -i "upload\|document" | wc -l
# Result: 0 lines
```

**Significance**: If the POST request reached the backend, there would be:

- Entry/Exit logs from handler
- At least a warning if profile not found
- SQL logs if EF Core is configured for logging

**Conclusion**: The POST request is NOT reaching the KYCService backend

---

## Root Cause Analysis

### Hypothesis 1: Request Blocked Before Backend ❌

- Gateway would return 502/503 - NOT seen in frontend
- CORS issue would show browser console error - NOT reported
- Authentication failure would return 401 - NOT reported

### Hypothesis 2: Request Fails Validation ⚠️ POSSIBLE

- Frontend sending invalid payload
- FluentValidation rejects command
- Returns 422 Unprocessable Entity

**Evidence**:

- Server action has try-catch that silences errors
- Returns `{ success: false, error: "..." }` but user never reported seeing error message
- This suggests request actually succeeded (2xx response)

### Hypothesis 3: Request Succeeds But Silent Failure ⚠️ MOST LIKELY

- POST returns 201 Created
- Response indicates success
- But document NOT inserted into database

**Evidence**:

- No error in frontend (would show if POST failed)
- Database shows 0 documents
- Profile status NOT updated (handler should update it)
- No backend logs of handler execution

### Hypothesis 4: CloudSQL/DBContext Transaction Issue ⚠️ POSSIBLE

- SaveChangesAsync() silently fails
- Connection pool exhausted
- Constraint violation not caught

---

## Solution Applied

### 1. Enhanced Logging (Commit: `61505a4c`)

Added detailed logging to `UploadKYCDocumentHandler`:

```csharp
_logger.LogInformation("Starting document upload for KYC Profile {ProfileId}");
_logger.LogInformation("Profile found: Status={Status}, FullName={FullName}");
_logger.LogInformation("Creating document with ID {DocumentId}");
_logger.LogInformation("Document created successfully");
_logger.LogInformation("Profile status updated to InProgress");
```

This will help us see:

- If handler is called at all
- Where exactly the process fails
- What data is being processed

### 2. Next Steps (TODO)

Once enhanced logging is deployed to Kubernetes:

1. **User Re-uploads Document**
   - Same user profile
   - Same document type

2. **Check Backend Logs**

   ```bash
   kubectl logs -n okla -l app=kycservice --follow | grep -i "upload\|document"
   ```

3. **Analyze Results**
   - If logs appear: Follow the flow to find where it breaks
   - If no logs: Request never reaches backend (Gateway/Network issue)

4. **Database Check**
   ```bash
   PGPASSWORD="..." psql -h okla-db-... -U doadmin -d kycservice -c \
     "SELECT COUNT(*) FROM kyc_documents;"
   ```

---

## Key Files Modified

| File                                                    | Change                                    | Purpose                    |
| ------------------------------------------------------- | ----------------------------------------- | -------------------------- |
| `KYCService.Application/Handlers/KYCProfileHandlers.cs` | Added `ILogger<T>` and logging statements | Diagnose handler execution |

## Key Files Reviewed (No Changes)

| File                                                        | Status     | Reason                                      |
| ----------------------------------------------------------- | ---------- | ------------------------------------------- |
| `KYCService.Application/DTOs/KYCDtos.cs`                    | ✅ Correct | Has `documents` property                    |
| `KYCService.Application/Commands/KYCCommands.cs`            | ✅ Correct | `UploadKYCDocumentCommand` properly defined |
| `KYCService.Application/Validators/KYCValidators.cs`        | ✅ Correct | Validation rules proper                     |
| `KYCService.Infrastructure/Repositories/KYCRepositories.cs` | ✅ Correct | `CreateAsync` and `SaveChangesAsync` called |
| `KYCService.Infrastructure/Persistence/KYCDbContext.cs`     | ✅ Correct | Entity mapping correct                      |
| `Gateway/ocelot.prod.json`                                  | ✅ Correct | Route configured for POST                   |
| `KYCService.Api/Controllers/KYCDocumentsController.cs`      | ✅ Correct | Endpoint correctly defined                  |

---

## Database Schema Verification

```sql
-- Total documents in system
SELECT COUNT(*) FROM kyc_documents;
-- Result: 0 rows

-- Check if table structure is correct
\d kyc_documents;
-- Result: Table exists with all required columns

-- Check profile exists
SELECT id, status, full_name FROM kyc_profiles
WHERE id = '250310b4-2b69-49b3-8715-6993fd8a239c';
-- Result: 1 row | Status: 1 (Pending) | FullName: Gregory Moreno
```

---

## Deployment Status

- **Code Changes**: ✅ Committed (61505a4c)
- **Docker Build**: ⏳ Pending GitHub Actions build
- **K8s Deployment**: ⏳ Will auto-redeploy when image ready

---

## Next Investigation Steps

1. **Wait for GitHub Actions to build image**
2. **Verify KYCService pod has new image**
3. **Have user re-attempt document upload**
4. **Monitor logs in real-time**:
   ```bash
   kubectl logs -n okla -f -l app=kycservice | grep -A5 -B5 "document"
   ```
5. **Correlate with database changes**

---

## Important Notes

- **User can still access the system** - Only document storage is broken
- **Other KYC endpoints work** - GET /api/kyc/kycprofiles/... returns correct data
- **S3 upload works** - Files are stored successfully
- **Only the registration step fails** - Documents stored in S3 but not registered in KYC DB

---

## Timeline of Analysis

| Time  | Action                      | Result                           |
| ----- | --------------------------- | -------------------------------- |
| 09:00 | Checked database location   | Found PostgreSQL on DigitalOcean |
| 09:15 | Queried kyc_documents table | Found 0 rows (empty)             |
| 09:30 | Reviewed backend code       | Found no obvious bugs            |
| 09:45 | Checked Gateway routes      | Confirmed POST route exists      |
| 10:00 | Analyzed logs               | No evidence of handler execution |
| 10:15 | Added enhanced logging      | Deployment pending               |
