# ===========================================================================
# Sprint 0 - Phase 5.1: Assets Audit Script
# ===========================================================================
# Este script identifica todas las imágenes y videos hardcodeados en el frontend
# que provienen de URLs externas (Unsplash, placeholders, Lorem Picsum, etc.)
# ===========================================================================

param(
    [string]$FrontendPath = "frontend/web/original",
    [string]$OutputFile = "docs/sprints/frontend-backend-integration/ASSETS_AUDIT_REPORT.md"
)

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "PHASE 5.1: ASSETS AUDIT" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Patrones de URLs externas a buscar
$patterns = @(
    @{Name="Unsplash"; Pattern="unsplash\.com"},
    @{Name="Lorem Picsum"; Pattern="picsum\.photos"},
    @{Name="Placeholder.com"; Pattern="placeholder\.com"},
    @{Name="Via Placeholder"; Pattern="via\.placeholder"},
    @{Name="PlaceIMG"; Pattern="placeimg\.com"},
    @{Name="LoremFlickr"; Pattern="loremflickr\.com"},
    @{Name="FakeIMG"; Pattern="fakeimg\.pl"},
    @{Name="DummyImage"; Pattern="dummyimage\.com"},
    @{Name="YouTube"; Pattern="youtube\.com|youtu\.be"},
    @{Name="Vimeo"; Pattern="vimeo\.com"},
    @{Name="External CDN"; Pattern="https?://[^'\"]+\.(jpg|jpeg|png|gif|svg|webp|mp4|webm|mov)"}
)

# Extensiones de archivo a buscar
$extensions = @("*.tsx", "*.ts", "*.jsx", "*.js", "*.css", "*.scss")

$results = @{}
$totalMatches = 0
$uniqueUrls = @{}

Write-Host "Scanning frontend directory: $FrontendPath" -ForegroundColor Yellow
Write-Host "Looking for external asset URLs...`n" -ForegroundColor Yellow

foreach ($pattern in $patterns) {
    Write-Host "Searching for: $($pattern.Name)" -ForegroundColor Cyan
    $results[$pattern.Name] = @()
    
    foreach ($ext in $extensions) {
        $files = Get-ChildItem -Path $FrontendPath -Include $ext -Recurse -File -ErrorAction SilentlyContinue
        
        foreach ($file in $files) {
            $relativePath = $file.FullName.Replace((Get-Location).Path + "\", "")
            $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
            
            if ($content -match $pattern.Pattern) {
                # Extraer URLs específicas
                $matches = [regex]::Matches($content, "https?://[^\s'\"<>]+")
                
                foreach ($match in $matches) {
                    $url = $match.Value
                    if ($url -match $pattern.Pattern) {
                        $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
                        
                        $result = @{
                            File = $relativePath
                            Line = $lineNumber
                            URL = $url
                        }
                        
                        $results[$pattern.Name] += $result
                        $totalMatches++
                        
                        # Guardar URLs únicas
                        if (-not $uniqueUrls.ContainsKey($url)) {
                            $uniqueUrls[$url] = @()
                        }
                        $uniqueUrls[$url] += $relativePath
                    }
                }
            }
        }
    }
    
    if ($results[$pattern.Name].Count -gt 0) {
        Write-Host "  Found: $($results[$pattern.Name].Count) matches" -ForegroundColor Green
    } else {
        Write-Host "  Found: 0 matches" -ForegroundColor Gray
    }
}

# Generar reporte Markdown
Write-Host "`nGenerating audit report..." -ForegroundColor Yellow

$reportContent = @"
# 📊 Frontend Assets Audit Report

**Generated:** $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")  
**Sprint:** 0 - Phase 5.1  
**Frontend Path:** $FrontendPath

---

## 📋 Executive Summary

- **Total External URLs Found:** $totalMatches
- **Unique URLs:** $($uniqueUrls.Count)
- **Files Affected:** $(($results.Values | ForEach-Object { $_.File } | Select-Object -Unique).Count)

### Breakdown by Source

| Source | Count |
|--------|-------|
"@

foreach ($pattern in $patterns) {
    $count = $results[$pattern.Name].Count
    if ($count -gt 0) {
        $reportContent += "| $($pattern.Name) | $count |`n"
    }
}

$reportContent += @"

---

## 🔍 Detailed Findings

"@

foreach ($pattern in $patterns) {
    if ($results[$pattern.Name].Count -gt 0) {
        $reportContent += @"

### $($pattern.Name) ($($results[$pattern.Name].Count) matches)

| File | Line | URL |
|------|------|-----|
"@
        
        foreach ($match in $results[$pattern.Name] | Select-Object -First 50) {
            $truncatedUrl = if ($match.URL.Length -gt 80) { 
                $match.URL.Substring(0, 77) + "..." 
            } else { 
                $match.URL 
            }
            $reportContent += "| ``$($match.File)`` | $($match.Line) | ``$truncatedUrl`` |`n"
        }
        
        if ($results[$pattern.Name].Count -gt 50) {
            $reportContent += "`n*... and $($results[$pattern.Name].Count - 50) more matches*`n"
        }
        
        $reportContent += "`n"
    }
}

$reportContent += @"

---

## 📦 Unique URLs Summary

Total unique URLs: $($uniqueUrls.Count)

"@

$categoryUrls = @{
    "Vehicles" = @()
    "Properties" = @()
    "Avatars" = @()
    "UI/Backgrounds" = @()
    "Videos" = @()
}

foreach ($url in $uniqueUrls.Keys) {
    if ($url -match "car|vehicle|auto|truck|motorcycle") {
        $categoryUrls["Vehicles"] += $url
    }
    elseif ($url -match "house|property|real-estate|apartment|building") {
        $categoryUrls["Properties"] += $url
    }
    elseif ($url -match "avatar|profile|user|person|face") {
        $categoryUrls["Avatars"] += $url
    }
    elseif ($url -match "mp4|webm|mov|youtube|vimeo") {
        $categoryUrls["Videos"] += $url
    }
    else {
        $categoryUrls["UI/Backgrounds"] += $url
    }
}

$reportContent += "### By Category`n`n"
foreach ($category in $categoryUrls.Keys) {
    $count = $categoryUrls[$category].Count
    if ($count -gt 0) {
        $reportContent += "- **${category}:** $count URLs`n"
    }
}

$reportContent += @"

---

## ⚠️ Impact Analysis

### 🔴 Critical Issues

1. **Production Blocker:** All $totalMatches external URLs are non-production ready
2. **Dependency Risk:** Reliance on third-party services (Unsplash, placeholders)
3. **Performance:** External requests add latency to page loads
4. **Availability:** Services can become unavailable or rate-limited

### 📊 Estimated Migration Effort

| Phase | Task | Estimated Hours |
|-------|------|----------------|
| 5.1 | Audit (this report) | ✅ 4-5h |
| 5.2 | Download & Optimize | 🔴 3-4h |
| 5.3 | MediaService Seed | 🔴 6-8h |
| 5.4 | Frontend Update | 🔴 3-4h |
| **TOTAL** | **Full Migration** | **16-21h** |

---

## 🚀 Next Steps (Phase 5.2)

1. **Download Assets**
   ``````powershell
   # Run download script
   .\scripts\Download-Frontend-Assets.ps1
   ``````

2. **Organize Structure**
   ``````
   temp-assets/
   ├── vehicles/
   ├── properties/
   ├── avatars/
   └── ui/
   ``````

3. **Optimize Images**
   - Resize to max 1920x1080
   - Compress (85% quality)
   - Convert to WebP
   - Generate thumbnails

---

## 📝 Recommendations

### Immediate Actions

- [ ] Execute Phase 5.2: Download all assets
- [ ] Create backup of original URLs (this report serves as reference)
- [ ] Set up local asset storage structure
- [ ] Prepare MediaService database

### Long-term Solutions

- [ ] Implement CDN (CloudFlare, AWS CloudFront)
- [ ] Set up image optimization pipeline
- [ ] Configure lazy loading for all images
- [ ] Implement responsive images (srcset)
- [ ] Add fallback images for missing assets

---

## 📚 Files Most Affected

"@

# Top 10 archivos más afectados
$topFiles = $results.Values | ForEach-Object { $_.File } | Group-Object | Sort-Object Count -Descending | Select-Object -First 10

$reportContent += "`n| File | External URLs Count |`n|------|---------------------|`n"
foreach ($file in $topFiles) {
    $reportContent += "| ``$($file.Name)`` | $($file.Count) |`n"
}

$reportContent += @"

---

## 🔗 Related Documentation

- [Sprint 0 Setup Inicial](SPRINT_0_SETUP_INICIAL.md)
- [Assets Migration Guide](../../guides/ASSETS_MIGRATION_GUIDE.md)
- [MediaService API Documentation](../../api/MediaService.md)

---

**Report Status:** ✅ Complete  
**Recommended Action:** Proceed to Phase 5.2 - Assets Download

"@

# Guardar reporte
$reportDir = Split-Path $OutputFile -Parent
if (-not (Test-Path $reportDir)) {
    New-Item -ItemType Directory -Path $reportDir -Force | Out-Null
}

$reportContent | Out-File -FilePath $OutputFile -Encoding UTF8

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "AUDIT COMPLETE" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "✅ Audit completed successfully" -ForegroundColor Green
Write-Host "📄 Report saved to: $OutputFile" -ForegroundColor Green
Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Total External URLs: $totalMatches" -ForegroundColor White
Write-Host "  Unique URLs: $($uniqueUrls.Count)" -ForegroundColor White
Write-Host "  Files Affected: $(($results.Values | ForEach-Object { $_.File } | Select-Object -Unique).Count)" -ForegroundColor White
Write-Host ""

if ($totalMatches -gt 0) {
    Write-Host "⚠️  WARNING: $totalMatches external asset URLs detected" -ForegroundColor Yellow
    Write-Host "   This is a PRODUCTION BLOCKER" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Next step: Run Phase 5.2 - Assets Download" -ForegroundColor Cyan
    Write-Host "  .\scripts\Download-Frontend-Assets.ps1" -ForegroundColor White
    exit 1
} else {
    Write-Host "✅ No external asset URLs found - Frontend is production ready" -ForegroundColor Green
    exit 0
}
