using FileStorageService.Core.Interfaces;
using FileStorageService.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using nClam;

namespace FileStorageService.Core.Services;

/// <summary>
/// Virus scanning service implementation
/// Supports ClamAV daemon or runs in mock mode for development
/// </summary>
public class VirusScanService : IVirusScanService
{
    private readonly StorageProviderConfig _config;
    private readonly IStorageProvider _storageProvider;
    private readonly ILogger<VirusScanService> _logger;
    private readonly bool _isMockMode;
    private readonly ClamClient? _clamClient;

    // Content types that should always be scanned
    private static readonly HashSet<string> ScannableTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/octet-stream",
        "application/x-msdownload",
        "application/x-executable",
        "application/zip",
        "application/x-rar-compressed",
        "application/x-7z-compressed",
        "application/x-tar",
        "application/gzip",
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    };

    // Content types that are generally safe (but can still be scanned)
    private static readonly HashSet<string> GenerallySafeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "text/plain",
        "text/csv"
    };

    public VirusScanService(
        IOptions<StorageProviderConfig> config,
        IStorageProvider storageProvider,
        ILogger<VirusScanService> logger)
    {
        _config = config.Value;
        _storageProvider = storageProvider;
        _logger = logger;

        // Run in mock mode if virus scanning is disabled
        _isMockMode = !_config.EnableVirusScan;

        if (_isMockMode)
        {
            _logger.LogWarning("Virus scanning is running in mock mode - all files will be marked as clean");
        }
        else
        {
            // Initialize ClamAV client with configuration
            var clamHost = _config.ClamAvHost ?? "localhost";
            var clamPort = _config.ClamAvPort > 0 ? _config.ClamAvPort : 3310;

            _clamClient = new ClamClient(clamHost, clamPort)
            {
                MaxStreamSize = _config.MaxFileSizeBytes > 0 ? _config.MaxFileSizeBytes : 26214400 // 25MB default
            };

            _logger.LogInformation("ClamAV client initialized - Host: {Host}, Port: {Port}", clamHost, clamPort);
        }
    }

    public async Task<ScanResult> ScanAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var result = new ScanResult
        {
            FileId = fileName,
            FileSizeBytes = fileStream.Length
        };

        if (_isMockMode)
        {
            // Mock mode - simulate a quick scan
            await Task.Delay(50, cancellationToken);
            result.Status = ScanStatus.Completed;
            result.IsClean = true;
            result.ScannerEngine = "MockScanner";
            result.ScannerVersion = "1.0.0";
            result.Complete();

            _logger.LogDebug("Mock scan completed for {FileName}", fileName);
            return result;
        }

        try
        {
            result.Status = ScanStatus.Scanning;

            // Ensure stream is at the beginning
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            // Use ClamAV for real scanning
            var clamResult = await ScanWithClamAvAsync(fileStream, fileName, cancellationToken);

            result.IsClean = clamResult.IsClean;
            result.ThreatName = clamResult.ThreatName;
            result.ThreatLevel = clamResult.ThreatLevel;
            result.ThreatDescription = clamResult.ThreatDescription;
            result.ScannerEngine = "ClamAV";
            result.ScannerVersion = clamResult.EngineVersion ?? "unknown";
            result.DefinitionVersion = clamResult.DefinitionVersion ?? DateTime.UtcNow.ToString("yyyy.MM.dd");
            result.Complete();

            _logger.LogInformation("ClamAV scan completed for {FileName}: {Status}",
                fileName, result.IsClean ? "Clean" : $"Threat: {result.ThreatName}");
        }
        catch (Exception ex)
        {
            result.Status = ScanStatus.Failed;
            result.ErrorMessage = ex.Message;

            // On ClamAV failure, fall back to simulation if configured, otherwise fail safe
            if (_config.FailOpenOnScanError)
            {
                _logger.LogWarning(ex, "ClamAV scan failed for {FileName}, failing open (allowing file)", fileName);
                result.IsClean = true;
                result.Status = ScanStatus.Completed;
            }
            else
            {
                _logger.LogError(ex, "ClamAV scan failed for {FileName}, failing closed (rejecting file)", fileName);
                result.IsClean = false;
            }
        }

        return result;
    }

    private async Task<ClamScanOutcome> ScanWithClamAvAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken)
    {
        if (_clamClient == null)
        {
            throw new InvalidOperationException("ClamAV client is not initialized");
        }

        var outcome = new ClamScanOutcome();

        try
        {
            // Get version info for reporting
            var versionInfo = await _clamClient.GetVersionAsync(cancellationToken);
            outcome.EngineVersion = versionInfo;

            // Parse definition version from ClamAV version string if possible
            // Format is usually: ClamAV 1.0.0/26789/Mon Feb 27 14:41:32 2024
            var parts = versionInfo?.Split('/');
            if (parts?.Length >= 2)
            {
                outcome.DefinitionVersion = parts[1];
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get ClamAV version info");
        }

        // Perform the actual scan
        var scanResult = await _clamClient.SendAndScanFileAsync(fileStream, cancellationToken);

        switch (scanResult.Result)
        {
            case ClamScanResults.Clean:
                outcome.IsClean = true;
                _logger.LogDebug("ClamAV: File {FileName} is clean", fileName);
                break;

            case ClamScanResults.VirusDetected:
                outcome.IsClean = false;
                outcome.ThreatName = scanResult.InfectedFiles?.FirstOrDefault()?.VirusName ?? "Unknown";
                outcome.ThreatLevel = DetermineThreatLevel(outcome.ThreatName);
                outcome.ThreatDescription = $"ClamAV detected: {outcome.ThreatName}";
                _logger.LogWarning("ClamAV: Virus detected in {FileName}: {VirusName}",
                    fileName, outcome.ThreatName);
                break;

            case ClamScanResults.Error:
                throw new InvalidOperationException($"ClamAV scan error: {scanResult.RawResult}");

            default:
                throw new InvalidOperationException($"Unknown ClamAV result: {scanResult.Result}");
        }

        return outcome;
    }

    private ThreatLevel DetermineThreatLevel(string? threatName)
    {
        if (string.IsNullOrEmpty(threatName))
            return ThreatLevel.None;

        var lowerName = threatName.ToLowerInvariant();

        // High severity patterns
        if (lowerName.Contains("trojan") ||
            lowerName.Contains("ransomware") ||
            lowerName.Contains("backdoor") ||
            lowerName.Contains("rootkit") ||
            lowerName.Contains("exploit"))
        {
            return ThreatLevel.Critical;
        }

        // Medium severity
        if (lowerName.Contains("worm") ||
            lowerName.Contains("virus") ||
            lowerName.Contains("malware"))
        {
            return ThreatLevel.High;
        }

        // Low severity
        if (lowerName.Contains("adware") ||
            lowerName.Contains("pup") ||
            lowerName.Contains("potentially"))
        {
            return ThreatLevel.Medium;
        }

        // Test files
        if (lowerName.Contains("eicar"))
        {
            return ThreatLevel.Low;
        }

        return ThreatLevel.High; // Default to high if threat detected but unknown type
    }

    public async Task<ScanResult> ScanByKeyAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        await using var stream = await _storageProvider.DownloadAsync(storageKey, cancellationToken);
        return await ScanAsync(stream, storageKey, cancellationToken);
    }

    public async Task<BatchScanResult> ScanBatchAsync(
        IDictionary<string, Stream> files,
        CancellationToken cancellationToken = default)
    {
        var batchResult = new BatchScanResult();

        foreach (var (fileName, stream) in files)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var result = await ScanAsync(stream, fileName, cancellationToken);
            batchResult.AddResult(result);
        }

        batchResult.Complete();

        _logger.LogInformation("Batch scan completed: {Total} files, {Clean} clean, {Infected} infected",
            batchResult.TotalFiles, batchResult.CleanFiles, batchResult.InfectedFiles);

        return batchResult;
    }

    public async Task<Dictionary<string, string>> GetScannerInfoAsync()
    {
        var info = new Dictionary<string, string>
        {
            ["Engine"] = _isMockMode ? "MockScanner" : "ClamAV",
            ["Mode"] = _isMockMode ? "Mock" : "Active",
            ["Status"] = "Unknown"
        };

        if (_isMockMode)
        {
            info["Version"] = "1.0.0";
            info["DefinitionDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd");
            info["Status"] = "Operational";
            return info;
        }

        try
        {
            if (_clamClient != null)
            {
                var version = await _clamClient.GetVersionAsync();
                info["Version"] = version ?? "unknown";

                // Parse version string for more details
                var parts = version?.Split('/');
                if (parts?.Length >= 2)
                {
                    info["Engine"] = parts[0].Trim();
                    info["DefinitionVersion"] = parts[1].Trim();
                }
                if (parts?.Length >= 3)
                {
                    info["DefinitionDate"] = parts[2].Trim();
                }

                info["Status"] = "Operational";
            }
        }
        catch (Exception ex)
        {
            info["Status"] = "Error";
            info["Error"] = ex.Message;
            _logger.LogWarning(ex, "Failed to get ClamAV info");
        }

        return info;
    }

    public async Task<bool> IsHealthyAsync()
    {
        if (_isMockMode)
            return true;

        try
        {
            if (_clamClient != null)
            {
                var pingResult = await _clamClient.PingAsync();
                return pingResult;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ClamAV health check failed");
            return false;
        }
    }

    public async Task<bool> UpdateDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        if (_isMockMode)
        {
            _logger.LogInformation("Virus definitions update requested (mock mode - skipped)");
            return true;
        }

        // ClamAV definition updates are typically handled by freshclam daemon
        // We can only verify that definitions are current
        try
        {
            var info = await GetScannerInfoAsync();
            _logger.LogInformation("ClamAV definitions checked: {DefinitionVersion}",
                info.GetValueOrDefault("DefinitionVersion", "unknown"));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check ClamAV definitions");
            return false;
        }
    }

    public bool ShouldScan(string contentType)
    {
        // Always scan if it's a known risky type
        if (ScannableTypes.Contains(contentType))
            return true;

        // Skip generally safe types in mock mode
        if (_isMockMode && GenerallySafeTypes.Contains(contentType))
            return false;

        // Scan everything else
        return true;
    }

    private class ClamScanOutcome
    {
        public bool IsClean { get; set; } = true;
        public string? ThreatName { get; set; }
        public ThreatLevel ThreatLevel { get; set; } = ThreatLevel.None;
        public string? ThreatDescription { get; set; }
        public string? EngineVersion { get; set; }
        public string? DefinitionVersion { get; set; }
    }
}
