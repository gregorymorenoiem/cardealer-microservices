using ModerationAgent.Domain.Models;

namespace ModerationAgent.Domain.Interfaces;

public interface ILlmModerationService
{
    Task<ModerationVerdict> ModerateContentAsync(ModerationInput input, CancellationToken ct = default);
}
