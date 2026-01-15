using System;
using System.Collections.Generic;

namespace CarDealer.DataSeeding.DataBuilders;

/// <summary>
/// Builder para crear imágenes de vehículos usando Picsum Photos
/// Uso: new ImageBuilder().ForVehicle(vehicleId).GenerateUrls(50).Build()
/// </summary>
public class ImageBuilder
{
    private Guid _vehicleId = Guid.Empty;
    private int _imageCount = 50;
    private int _width = 800;
    private int _height = 600;
    private int _thumbnailWidth = 400;
    private int _thumbnailHeight = 300;

    private const string PICSUM_URL = "https://picsum.photos";

    public ImageBuilder ForVehicle(Guid vehicleId)
    {
        _vehicleId = vehicleId;
        return this;
    }

    public ImageBuilder WithImageCount(int count)
    {
        _imageCount = Math.Max(1, Math.Min(count, 100)); // Min 1, Max 100
        return this;
    }

    public ImageBuilder WithSize(int width, int height)
    {
        _width = width;
        _height = height;
        return this;
    }

    public ImageBuilder WithThumbnailSize(int width, int height)
    {
        _thumbnailWidth = width;
        _thumbnailHeight = height;
        return this;
    }

    /// <summary>
    /// Genera lista de imágenes con URLs de Picsum
    /// </summary>
    public List<VehicleImageDto> Build()
    {
        if (_vehicleId == Guid.Empty)
            throw new ArgumentException("VehicleId es requerido");

        var images = new List<VehicleImageDto>();

        for (int i = 0; i < _imageCount; i++)
        {
            // Usar ID del vehículo + índice como seed para consistencia
            var seed = $"{_vehicleId:N}{i:D3}".GetHashCode();

            var image = new VehicleImageDto
            {
                Id = Guid.NewGuid(),
                VehicleId = _vehicleId,
                // URL con parámetros para consistencia y blur
                Url = $"{PICSUM_URL}/{_width}/{_height}?random={Math.Abs(seed)}&blur=1",
                ThumbnailUrl = $"{PICSUM_URL}/{_thumbnailWidth}/{_thumbnailHeight}?random={Math.Abs(seed)}&blur=2",
                IsPrimary = i == 0,
                SortOrder = i,
                Caption = i == 0 ? "Imagen Principal" : $"Foto {i + 1}",
                ImageType = i switch
                {
                    0 => "ExteriorFront",
                    1 => "ExteriorBack",
                    2 => "ExteriorLeft",
                    3 => "ExteriorRight",
                    < 10 => "Exterior",
                    < 30 => "Interior",
                    < 40 => "Engine",
                    _ => "Details"
                },
                CreatedAt = DateTime.UtcNow,
                MimeType = "image/jpeg",
                FileSize = _faker.Random.Long(500_000, 5_000_000)
            };

            images.Add(image);
        }

        return images;
    }

    /// <summary>
    /// Genera imágenes para múltiples vehículos
    /// </summary>
    public static List<VehicleImageDto> GenerateBatchForVehicles(
        List<Guid> vehicleIds,
        int imagesPerVehicle = 50)
    {
        var allImages = new List<VehicleImageDto>();

        foreach (var vehicleId in vehicleIds)
        {
            var images = new ImageBuilder()
                .ForVehicle(vehicleId)
                .WithImageCount(imagesPerVehicle)
                .Build();

            allImages.AddRange(images);
        }

        return allImages;
    }

    /// <summary>
    /// Genera URLs individuales para download manualmente
    /// Útil para debugging o pre-validación
    /// </summary>
    public static List<string> GenerateImageUrls(
        Guid vehicleId,
        int count = 50,
        int width = 800,
        int height = 600)
    {
        var urls = new List<string>();
        var seed = vehicleId.GetHashCode();

        for (int i = 0; i < count; i++)
        {
            var url = $"{PICSUM_URL}/{width}/{height}?random={Math.Abs(seed + i)}";
            urls.Add(url);
        }

        return urls;
    }

    private static Bogus.Faker _faker = new();
}

public class VehicleImageDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
    public string ImageType { get; set; } = "Exterior"; // ExteriorFront, Interior, Engine, etc.
    public string? MimeType { get; set; }
    public long? FileSize { get; set; }
    public int? Width { get; set; } = 800;
    public int? Height { get; set; } = 600;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Helper para descargar imágenes localmente (opcional)
/// Útil si deseas hospedar en S3 local o local storage
/// </summary>
public class ImageDownloadHelper
{
    private readonly HttpClient _httpClient;
    private readonly string _localStoragePath;

    public ImageDownloadHelper(string localStoragePath)
    {
        _httpClient = new HttpClient();
        _localStoragePath = localStoragePath;
    }

    /// <summary>
    /// Descarga imagen de Picsum y la guarda localmente
    /// </summary>
    public async Task<string> DownloadAndSaveAsync(
        string imageUrl,
        Guid vehicleId,
        int index,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(imageUrl, ct);
            response.EnsureSuccessStatusCode();

            var fileName = $"vehicle_{vehicleId:N}_{index:D3}.jpg";
            var filePath = Path.Combine(_localStoragePath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await response.Content.CopyToAsync(fileStream, ct);
            }

            return filePath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Error descargando imagen {imageUrl}", ex);
        }
    }

    /// <summary>
    /// Descarga múltiples imágenes en paralelo
    /// </summary>
    public async Task<List<string>> DownloadBatchAsync(
        List<string> imageUrls,
        Guid vehicleId,
        int maxParallel = 5,
        CancellationToken ct = default)
    {
        var semaphore = new SemaphoreSlim(maxParallel);
        var tasks = new List<Task<string>>();

        for (int i = 0; i < imageUrls.Count; i++)
        {
            await semaphore.WaitAsync(ct);

            var url = imageUrls[i];
            var index = i;

            var task = DownloadAndSaveAsync(url, vehicleId, index, ct)
                .ContinueWith(_ => semaphore.Release(), ct);

            tasks.Add(task);
        }

        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }
}
