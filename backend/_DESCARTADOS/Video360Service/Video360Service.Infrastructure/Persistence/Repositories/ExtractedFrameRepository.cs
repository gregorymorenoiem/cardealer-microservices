using Microsoft.EntityFrameworkCore;
using Video360Service.Domain.Entities;
using Video360Service.Domain.Interfaces;

namespace Video360Service.Infrastructure.Persistence.Repositories;

public class ExtractedFrameRepository : IExtractedFrameRepository
{
    private readonly Video360DbContext _context;

    public ExtractedFrameRepository(Video360DbContext context)
    {
        _context = context;
    }

    public async Task<ExtractedFrame?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ExtractedFrames
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ExtractedFrame>> GetByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return await _context.ExtractedFrames
            .Where(f => f.Video360JobId == jobId)
            .OrderBy(f => f.SequenceNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<ExtractedFrame?> GetPrimaryFrameByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return await _context.ExtractedFrames
            .Where(f => f.Video360JobId == jobId && f.IsPrimary)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ExtractedFrame> CreateAsync(ExtractedFrame frame, CancellationToken cancellationToken = default)
    {
        _context.ExtractedFrames.Add(frame);
        await _context.SaveChangesAsync(cancellationToken);
        return frame;
    }

    public async Task<IEnumerable<ExtractedFrame>> CreateManyAsync(IEnumerable<ExtractedFrame> frames, CancellationToken cancellationToken = default)
    {
        var frameList = frames.ToList();
        _context.ExtractedFrames.AddRange(frameList);
        await _context.SaveChangesAsync(cancellationToken);
        return frameList;
    }

    public async Task<ExtractedFrame> UpdateAsync(ExtractedFrame frame, CancellationToken cancellationToken = default)
    {
        _context.ExtractedFrames.Update(frame);
        await _context.SaveChangesAsync(cancellationToken);
        return frame;
    }

    public async Task DeleteByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var frames = await _context.ExtractedFrames
            .Where(f => f.Video360JobId == jobId)
            .ToListAsync(cancellationToken);

        _context.ExtractedFrames.RemoveRange(frames);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
