namespace MediaService.Domain.Entities;

/// <summary>
/// Represents a document media asset
/// </summary>
public class DocumentMedia : MediaAsset
{
    public int? PageCount { get; private set; }
    public string? Author { get; private set; }
    public string? Title { get; private set; }
    public string? Description { get; private set; }
    public bool IsSearchable { get; private set; }
    public string? Language { get; private set; }

    public DocumentMedia(
        Guid dealerId,
        string ownerId,
        string? context,
        string originalFileName,
        string contentType,
        long sizeBytes,
        string storageKey)
        : base(dealerId, ownerId, context, Enums.MediaType.Document, originalFileName, contentType, sizeBytes, storageKey)
    {
    }

    public void SetPageCount(int pageCount)
    {
        if (pageCount < 0) throw new ArgumentException("Page count cannot be negative", nameof(pageCount));

        PageCount = pageCount;
        MarkAsUpdated();
    }

    public void SetAuthor(string author)
    {
        Author = author;
        MarkAsUpdated();
    }

    public void SetTitle(string title)
    {
        Title = title;
        MarkAsUpdated();
    }

    public void SetDescription(string description)
    {
        Description = description;
        MarkAsUpdated();
    }

    public void SetSearchable(bool searchable)
    {
        IsSearchable = searchable;
        MarkAsUpdated();
    }

    public void SetLanguage(string language)
    {
        Language = language;
        MarkAsUpdated();
    }

    public bool IsPdf()
    {
        return ContentType?.ToLower() == "application/pdf";
    }

    public bool IsWordDocument()
    {
        return ContentType?.ToLower() is "application/msword" or
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    }

    public override bool IsValid()
    {
        return base.IsValid();
    }
}