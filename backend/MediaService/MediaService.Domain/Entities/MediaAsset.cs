using MediaService.Domain.Common;
using MediaService.Domain.Events;

namespace MediaService.Domain.Entities;

/// <summary>
/// Represents a media asset (image, video, document, audio)
/// </summary>
public class MediaAsset : EntityBase, IAggregateRoot
{
    public string OwnerId { get; private set; } = string.Empty;
    public string? Context { get; private set; }
    public Enums.MediaType Type { get; private set; }
    public string OriginalFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public Enums.MediaStatus Status { get; private set; }
    public string? ProcessingError { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public string? CdnUrl { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    // Navigation properties
    public virtual ICollection<MediaVariant> Variants { get; private set; } = new List<MediaVariant>();

    // Private constructor for Entity Framework
    protected MediaAsset() { }

    public MediaAsset(
        string ownerId,
        string? context,
        Enums.MediaType type,
        string originalFileName,
        string contentType,
        long sizeBytes,
        string storageKey)
    {
        Id = GenerateMediaId(type);
        OwnerId = ownerId ?? throw new ArgumentNullException(nameof(ownerId));
        Context = context;
        Type = type;
        OriginalFileName = originalFileName ?? throw new ArgumentNullException(nameof(originalFileName));
        ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
        SizeBytes = sizeBytes > 0 ? sizeBytes : throw new ArgumentException("File size must be greater than 0", nameof(sizeBytes));
        StorageKey = storageKey ?? throw new ArgumentNullException(nameof(storageKey));
        Status = Enums.MediaStatus.Uploaded;

        AddDomainEvent(new MediaUploadedEvent(Id, OwnerId, Context, Type, OriginalFileName, ContentType, SizeBytes, StorageKey));
    }

    private string GenerateMediaId(Enums.MediaType type)
    {
        var prefix = type switch
        {
            Enums.MediaType.Image => "img_",
            Enums.MediaType.Video => "vid_",
            Enums.MediaType.Document => "doc_",
            Enums.MediaType.Audio => "aud_",
            _ => "med_"
        };
        return $"{prefix}{Guid.NewGuid():N}";
    }

    public void MarkAsProcessing()
    {
        Status = Enums.MediaStatus.Processing;
        MarkAsUpdated();
    }

    public void MarkAsProcessed(string cdnUrl)
    {
        Status = Enums.MediaStatus.Processed;
        CdnUrl = cdnUrl;
        ProcessedAt = DateTime.UtcNow;
        MarkAsUpdated();

        AddDomainEvent(new MediaProcessedEvent(Id, OwnerId, Type, Enums.ProcessingStatus.Completed, Variants.Count, cdnUrl));
    }

    public void MarkAsFailed(string error)
    {
        Status = Enums.MediaStatus.Failed;
        ProcessingError = error;
        MarkAsUpdated();

        AddDomainEvent(new MediaProcessedEvent(Id, OwnerId, Type, Enums.ProcessingStatus.Failed, 0, null, null, error));
    }

    public void AddVariant(MediaVariant variant)
    {
        Variants.Add(variant);
        MarkAsUpdated();
    }

    public void RemoveVariant(MediaVariant variant)
    {
        Variants.Remove(variant);
        MarkAsUpdated();
    }

    public void UpdateContext(string? newContext)
    {
        Context = newContext;
        MarkAsUpdated();
    }

    public List<string> GetAllStorageKeys()
    {
        var keys = new List<string> { StorageKey };
        keys.AddRange(Variants.Select(v => v.StorageKey));
        return keys;
    }

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id) &&
               !string.IsNullOrWhiteSpace(OwnerId) &&
               !string.IsNullOrWhiteSpace(OriginalFileName) &&
               !string.IsNullOrWhiteSpace(ContentType) &&
               !string.IsNullOrWhiteSpace(StorageKey) &&
               SizeBytes > 0 &&
               Enum.IsDefined(typeof(Enums.MediaType), Type) &&
               Enum.IsDefined(typeof(Enums.MediaStatus), Status);
    }

    public string GetUserDisplayName()
    {
        return OwnerId switch
        {
            "system" => "System",
            "anonymous" => "Anonymous",
            _ => OwnerId
        };
    }

    public string GetSummary()
    {
        var status = Status.ToString().ToUpper();
        var user = GetUserDisplayName();
        return $"{Type} upload by {user} - {status}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return OwnerId;
        yield return Type;
        yield return CreatedAt;
    }

    public override string ToString()
    {
        return $"{GetType().Name} [Id={Id}, Type={Type}, Owner={GetUserDisplayName()}, Status={Status}]";
    }
}