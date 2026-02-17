using FileStorageService.Core.Models;

namespace FileStorageService.Core.Interfaces;

/// <summary>
/// Interface for virus scanning operations
/// </summary>
public interface IVirusScanService
{
    /// <summary>
    /// Scans a file stream for viruses
    /// </summary>
    /// <param name="fileStream">File content stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Scan result</returns>
    Task<ScanResult> ScanAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Scans a file from storage by key
    /// </summary>
    /// <param name="storageKey">Storage key of the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Scan result</returns>
    Task<ScanResult> ScanByKeyAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Scans multiple files
    /// </summary>
    /// <param name="files">Dictionary of file name to stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Batch scan result</returns>
    Task<BatchScanResult> ScanBatchAsync(IDictionary<string, Stream> files, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets scanner version and definition info
    /// </summary>
    /// <returns>Scanner info dictionary</returns>
    Task<Dictionary<string, string>> GetScannerInfoAsync();

    /// <summary>
    /// Checks if the scanner service is healthy
    /// </summary>
    /// <returns>True if healthy</returns>
    Task<bool> IsHealthyAsync();

    /// <summary>
    /// Updates virus definitions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdateDefinitionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a content type should be scanned
    /// </summary>
    /// <param name="contentType">MIME type</param>
    /// <returns>True if should scan</returns>
    bool ShouldScan(string contentType);
}
