namespace MediaService.Infrastructure.Services.Storage;

public class LocalStorageOptions
{
    public string BasePath { get; set; } = "wwwroot/uploads";
    public string BaseUrl { get; set; } = "https://localhost:7000/uploads";
}