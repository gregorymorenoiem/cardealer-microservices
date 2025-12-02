using FileStorageService.Core.Interfaces;
using FileStorageService.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

            // TODO: Implement actual ClamAV integration
            // For production, you would connect to ClamAV daemon:
            // var client = new ClamClient("localhost", 3310);
            // var scanResult = await client.SendAndScanFileAsync(fileStream);

            // For now, simulate scanning with pattern matching
            var scanOutcome = await SimulateScanAsync(fileStream, fileName, cancellationToken);

            result.IsClean = scanOutcome.IsClean;
            result.ThreatName = scanOutcome.ThreatName;
            result.ThreatLevel = scanOutcome.ThreatLevel;
            result.ThreatDescription = scanOutcome.ThreatDescription;
            result.ScannerEngine = "SimulatedScanner";
            result.ScannerVersion = "1.0.0";
            result.DefinitionVersion = DateTime.UtcNow.ToString("yyyy.MM.dd");
            result.Complete();

            _logger.LogInformation("Scan completed for {FileName}: {Status}",
                fileName, result.IsClean ? "Clean" : $"Threat: {result.ThreatName}");
        }
        catch (Exception ex)
        {
            result.Status = ScanStatus.Failed;
            result.ErrorMessage = ex.Message;
            result.IsClean = false;

            _logger.LogError(ex, "Scan failed for {FileName}", fileName);
        }

        return result;
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

    public Task<Dictionary<string, string>> GetScannerInfoAsync()
    {
        var info = new Dictionary<string, string>
        {
            ["Engine"] = _isMockMode ? "MockScanner" : "SimulatedScanner",
            ["Version"] = "1.0.0",
            ["DefinitionDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            ["Mode"] = _isMockMode ? "Mock" : "Active",
            ["Status"] = "Operational"
        };

        return Task.FromResult(info);
    }

    public Task<bool> IsHealthyAsync()
    {
        // In mock mode, always healthy
        // In production, would check ClamAV daemon connection
        return Task.FromResult(true);
    }

    public Task<bool> UpdateDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        // In production, would trigger freshclam update
        _logger.LogInformation("Virus definitions update requested (simulated)");
        return Task.FromResult(true);
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

    private async Task<ScanOutcome> SimulateScanAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken)
    {
        // Simulate scanning by reading the first few KB for patterns
        fileStream.Position = 0;
        var buffer = new byte[Math.Min(65536, fileStream.Length)];
        var bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);

        // Check for known malware signatures (EICAR test file)
        var content = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);

        if (content.Contains("X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*"))
        {
            return new ScanOutcome
            {
                IsClean = false,
                ThreatName = "EICAR-Test-File",
                ThreatLevel = ThreatLevel.High,
                ThreatDescription = "EICAR standard antivirus test file detected"
            };
        }

        // Check for suspicious file extensions in zip-like content
        if (fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".scr", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase))
        {
            // Check if content starts with MZ header (Windows executable)
            if (bytesRead >= 2 && buffer[0] == 0x4D && buffer[1] == 0x5A)
            {
                _logger.LogWarning("Executable file detected: {FileName}", fileName);
                // In a real system, you'd scan executables more thoroughly
                // For simulation, we allow but log it
            }
        }

        // Simulate processing time based on file size
        var delay = Math.Min(100, (int)(fileStream.Length / 10240)); // ~10ms per 10KB, max 100ms
        await Task.Delay(delay, cancellationToken);

        return new ScanOutcome { IsClean = true };
    }

    private class ScanOutcome
    {
        public bool IsClean { get; set; } = true;
        public string? ThreatName { get; set; }
        public ThreatLevel ThreatLevel { get; set; } = ThreatLevel.None;
        public string? ThreatDescription { get; set; }
    }
}
