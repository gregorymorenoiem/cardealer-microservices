using RabbitMQ.Client;
using MessageBusService.Application.Interfaces;
using MessageBusService.Domain.Entities;
using MessageBusService.Domain.Enums;
using MessageBusService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace MessageBusService.Infrastructure.Services;

public class DeadLetterManager : IDeadLetterManager
{
    private readonly MessageBusDbContext _dbContext;
    private readonly IConnection _connection;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<DeadLetterManager> _logger;

    public DeadLetterManager(
        MessageBusDbContext dbContext,
        IConnection connection,
        IMessagePublisher messagePublisher,
        ILogger<DeadLetterManager> logger)
    {
        _dbContext = dbContext;
        _connection = connection;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task<List<DeadLetterMessage>> GetDeadLettersAsync(int pageNumber = 1, int pageSize = 50)
    {
        return await _dbContext.DeadLetterMessages
            .Where(d => !d.IsDiscarded)
            .OrderByDescending(d => d.FailedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> RetryAsync(Guid deadLetterMessageId)
    {
        try
        {
            var deadLetter = await _dbContext.DeadLetterMessages.FindAsync(deadLetterMessageId);
            if (deadLetter == null)
            {
                _logger.LogWarning("Dead letter message {MessageId} not found", deadLetterMessageId);
                return false;
            }

            // Republicar el mensaje
            var success = await _messagePublisher.PublishAsync(
                deadLetter.Topic,
                deadLetter.Payload,
                MessagePriority.Normal,
                deadLetter.Headers);

            if (success)
            {
                deadLetter.RetriedAt = DateTime.UtcNow;
                deadLetter.RetryCount++;
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Retried dead letter message {MessageId}", deadLetterMessageId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying dead letter message {MessageId}", deadLetterMessageId);
            return false;
        }
    }

    public async Task<bool> DiscardAsync(Guid deadLetterMessageId)
    {
        try
        {
            var deadLetter = await _dbContext.DeadLetterMessages.FindAsync(deadLetterMessageId);
            if (deadLetter == null)
            {
                _logger.LogWarning("Dead letter message {MessageId} not found", deadLetterMessageId);
                return false;
            }

            deadLetter.IsDiscarded = true;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Discarded dead letter message {MessageId}", deadLetterMessageId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discarding dead letter message {MessageId}", deadLetterMessageId);
            return false;
        }
    }

    public async Task<DeadLetterMessage?> GetByIdAsync(Guid deadLetterMessageId)
    {
        return await _dbContext.DeadLetterMessages.FindAsync(deadLetterMessageId);
    }
}
