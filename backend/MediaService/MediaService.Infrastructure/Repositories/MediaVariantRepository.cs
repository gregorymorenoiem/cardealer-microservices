using MediaService.Domain.Entities;
using MediaService.Domain.Interfaces.Repositories;
using MediaService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaService.Infrastructure.Repositories;

public class MediaVariantRepository : IMediaVariantRepository
{
    private readonly ApplicationDbContext _context;

    public MediaVariantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MediaVariant?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.MediaVariants
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<MediaVariant>> GetByMediaIdAsync(string mediaId, CancellationToken cancellationToken = default)
    {
        return await _context.MediaVariants
            .Where(v => v.MediaAssetId == mediaId)
            .OrderBy(v => v.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<MediaVariant?> GetByMediaIdAndNameAsync(string mediaId, string variantName, CancellationToken cancellationToken = default)
    {
        return await _context.MediaVariants
            .FirstOrDefaultAsync(v => v.MediaAssetId == mediaId && v.Name == variantName, cancellationToken);
    }

    public async Task AddAsync(MediaVariant mediaVariant, CancellationToken cancellationToken = default)
    {
        await _context.MediaVariants.AddAsync(mediaVariant, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<MediaVariant> mediaVariants, CancellationToken cancellationToken = default)
    {
        await _context.MediaVariants.AddRangeAsync(mediaVariants, cancellationToken);
    }

    public async Task UpdateAsync(MediaVariant mediaVariant, CancellationToken cancellationToken = default)
    {
        _context.MediaVariants.Update(mediaVariant);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(MediaVariant mediaVariant, CancellationToken cancellationToken = default)
    {
        _context.MediaVariants.Remove(mediaVariant);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByMediaIdAsync(string mediaId, CancellationToken cancellationToken = default)
    {
        var variants = await _context.MediaVariants
            .Where(v => v.MediaAssetId == mediaId)
            .ToListAsync(cancellationToken);

        _context.MediaVariants.RemoveRange(variants);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string mediaId, string variantName, CancellationToken cancellationToken = default)
    {
        return await _context.MediaVariants
            .AnyAsync(v => v.MediaAssetId == mediaId && v.Name == variantName, cancellationToken);
    }

    public async Task<int> CountByMediaIdAsync(string mediaId, CancellationToken cancellationToken = default)
    {
        return await _context.MediaVariants
            .CountAsync(v => v.MediaAssetId == mediaId, cancellationToken);
    }

    public async Task<IEnumerable<MediaVariant>> GetByDimensionsAsync(int? minWidth = null, int? maxWidth = null,
                                                                     int? minHeight = null, int? maxHeight = null,
                                                                     CancellationToken cancellationToken = default)
    {
        var query = _context.MediaVariants.AsQueryable();

        if (minWidth.HasValue)
            query = query.Where(v => v.Width >= minWidth.Value);

        if (maxWidth.HasValue)
            query = query.Where(v => v.Width <= maxWidth.Value);

        if (minHeight.HasValue)
            query = query.Where(v => v.Height >= minHeight.Value);

        if (maxHeight.HasValue)
            query = query.Where(v => v.Height <= maxHeight.Value);

        return await query.ToListAsync(cancellationToken);
    }
}