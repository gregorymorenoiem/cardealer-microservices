using ChatbotService.Domain.Enums;
using ChatbotService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChatbotService.Infrastructure.Services.Strategies;

/// <summary>
/// Factory que resuelve la estrategia correcta según el ChatMode.
/// Usa IServiceProvider para resolver las estrategias registradas por DI.
/// </summary>
public class ChatModeStrategyFactory : IChatModeStrategyFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<ChatMode, Type> _strategyTypes;

    public ChatModeStrategyFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _strategyTypes = new Dictionary<ChatMode, Type>
        {
            { ChatMode.SingleVehicle, typeof(SingleVehicleStrategy) },
            { ChatMode.DealerInventory, typeof(DealerInventoryStrategy) },
            { ChatMode.General, typeof(GeneralChatStrategy) }
        };
    }

    public IChatModeStrategy GetStrategy(ChatMode mode)
    {
        if (!_strategyTypes.TryGetValue(mode, out var strategyType))
        {
            // Fallback a GeneralChatStrategy si el modo no es reconocido
            strategyType = typeof(GeneralChatStrategy);
        }

        return (IChatModeStrategy)_serviceProvider.GetRequiredService(strategyType);
    }
}
