using MediaService.Domain.Entities;
using MediaService.Domain.Interfaces.Repositories;
using MediaService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MediaService.Infrastructure.Repositories;

public class MediaRepository : IMediaRepository
{
    private readonly ApplicationDbContext _context;

    public MediaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MediaAsset?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.MediaAssets
            .Include(x => x.Variants)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<MediaAsset>> GetByOwnerIdAsync(string ownerId, string? context = null, CancellationToken cancellationToken = default)
    {
        var query = _context.MediaAssets
            .Where(x => x.OwnerId == ownerId);

        if (!string.IsNullOrEmpty(context))
        {
            query = query.Where(x => x.Context == context);
        }

        return await query
            .Include(x => x.Variants)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MediaAsset>> GetByTypeAsync(Domain.Enums.MediaType type, CancellationToken cancellationToken = default)
    {
        return await _context.MediaAssets
            .Where(x => x.Type == type)
            .Include(x => x.Variants)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MediaAsset>> GetByStatusAsync(Domain.Enums.MediaStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.MediaAssets
            .Where(x => x.Status == status)
            .Include(x => x.Variants)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ImageMedia>> GetUnprocessedImagesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ImageMedia
            .Where(x => x.Status == Domain.Enums.MediaStatus.Uploaded)
            .Include(x => x.Variants)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<VideoMedia>> GetUnprocessedVideosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.VideoMedia
            .Where(x => x.Status == Domain.Enums.MediaStatus.Uploaded)
            .Include(x => x.Variants)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<MediaAsset> items, int totalCount)> GetPaginatedAsync(
        string? ownerId = null,
        string? context = null,
        Domain.Enums.MediaType? type = null,
        Domain.Enums.MediaStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 50,
        string? sortBy = null,
        bool sortDescending = true)
    {
        var query = _context.MediaAssets.AsQueryable();

        if (!string.IsNullOrEmpty(ownerId))
        {
            query = query.Where(x => x.OwnerId == ownerId);
        }

        if (!string.IsNullOrEmpty(context))
        {
            query = query.Where(x => x.Context == context);
        }

        if (type.HasValue)
        {
            query = query.Where(x => x.Type == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= toDate.Value);
        }

        var totalCount = await query.CountAsync();

        // Sorting
        if (!string.IsNullOrEmpty(sortBy))
        {
            switch (sortBy.ToLower())
            {
                case "createdat":
                    query = sortDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt);
                    break;
                case "sizebytes":
                    query = sortDescending ? query.OrderByDescending(x => x.SizeBytes) : query.OrderBy(x => x.SizeBytes);
                    break;
                default:
                    query = sortDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt);
                    break;
            }
        }
        else
        {
            query = query.OrderByDescending(x => x.CreatedAt);
        }

        var items = await query
            .Include(x => x.Variants)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(MediaAsset mediaAsset, CancellationToken cancellationToken = default)
    {
        _context.MediaAssets.Add(mediaAsset);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(MediaAsset mediaAsset, CancellationToken cancellationToken = default)
    {
        _context.MediaAssets.Update(mediaAsset);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(MediaAsset mediaAsset, CancellationToken cancellationToken = default)
    {
        _context.MediaAssets.Remove(mediaAsset);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRangeAsync(IEnumerable<MediaAsset> mediaAssets, CancellationToken cancellationToken = default)
    {
        _context.MediaAssets.RemoveRange(mediaAssets);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.MediaAssets
            .AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MediaAssets.CountAsync(cancellationToken);
    }

    public async Task<MediaStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.MediaAssets.AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= toDate.Value);
        }

        var totalMedia = await query.CountAsync(cancellationToken);
        var images = await query.CountAsync(x => x.Type == Domain.Enums.MediaType.Image, cancellationToken);
        var videos = await query.CountAsync(x => x.Type == Domain.Enums.MediaType.Video, cancellationToken);
        var documents = await query.CountAsync(x => x.Type == Domain.Enums.MediaType.Document, cancellationToken);
        var audio = await query.CountAsync(x => x.Type == Domain.Enums.MediaType.Audio, cancellationToken);
        var processed = await query.CountAsync(x => x.Status == Domain.Enums.MediaStatus.Processed, cancellationToken);
        var processing = await query.CountAsync(x => x.Status == Domain.Enums.MediaStatus.Processing, cancellationToken);
        var failed = await query.CountAsync(x => x.Status == Domain.Enums.MediaStatus.Failed, cancellationToken);
        var totalStorageBytes = await query.SumAsync(x => x.SizeBytes, cancellationToken);
        var firstMediaDate = await query.MinAsync(x => (DateTime?)x.CreatedAt, cancellationToken);
        var lastMediaDate = await query.MaxAsync(x => (DateTime?)x.CreatedAt, cancellationToken);

        var mediaByContext = await query
            .Where(x => x.Context != null)
            .GroupBy(x => x.Context!)
            .Select(g => new { Context = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Context, x => x.Count, cancellationToken);

        return new MediaStatistics
        {
            TotalMedia = totalMedia,
            Images = images,
            Videos = videos,
            Documents = documents,
            Audio = audio,
            Processed = processed,
            Processing = processing,
            Failed = failed,
            TotalStorageBytes = totalStorageBytes,
            FirstMediaDate = firstMediaDate,
            LastMediaDate = lastMediaDate,
            MediaByContext = mediaByContext
        };
    }
}