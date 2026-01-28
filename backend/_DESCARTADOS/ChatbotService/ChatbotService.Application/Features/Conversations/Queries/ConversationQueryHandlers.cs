using MediatR;
using ChatbotService.Domain.Interfaces;
using ChatbotService.Application.DTOs;
using ChatbotService.Domain.Entities;

namespace ChatbotService.Application.Features.Conversations.Queries;

public class GetConversationQueryHandler : IRequestHandler<GetConversationQuery, ConversationDto?>
{
    private readonly IConversationRepository _repository;

    public GetConversationQueryHandler(IConversationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ConversationDto?> Handle(GetConversationQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _repository.GetByIdAsync(request.ConversationId, cancellationToken);
        return conversation != null ? MapToDto(conversation) : null;
    }

    private ConversationDto MapToDto(Conversation c)
    {
        return new ConversationDto(c.Id, c.UserId, c.UserName, c.UserEmail, c.UserPhone,
            c.VehicleId, c.VehicleTitle, c.VehiclePrice, c.DealerId, c.DealerName,
            c.Status, c.LeadScore, c.LeadTemperature, c.BuyingSignals,
            c.IsHandedOff, c.StartedAt, c.EndedAt, c.MessageCount, c.Duration);
    }
}

public class GetConversationMessagesQueryHandler : IRequestHandler<GetConversationMessagesQuery, List<MessageDto>>
{
    private readonly IConversationRepository _repository;

    public GetConversationMessagesQueryHandler(IConversationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<MessageDto>> Handle(GetConversationMessagesQuery request, CancellationToken cancellationToken)
    {
        var messages = await _repository.GetMessagesAsync(request.ConversationId, cancellationToken);
        return messages.Select(MapToDto).ToList();
    }

    private MessageDto MapToDto(Message m)
    {
        return new MessageDto(m.Id, m.ConversationId, m.Role, m.Content, m.Type,
            m.Timestamp, m.DetectedIntent, m.ExtractedSignals, m.SentimentScore);
    }
}

public class GetUserConversationsQueryHandler : IRequestHandler<GetUserConversationsQuery, List<ConversationDto>>
{
    private readonly IConversationRepository _repository;

    public GetUserConversationsQueryHandler(IConversationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ConversationDto>> Handle(GetUserConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await _repository.GetByUserIdAsync(request.UserId, request.Page, request.PageSize, cancellationToken);
        return conversations.Select(MapToDto).ToList();
    }

    private ConversationDto MapToDto(Conversation c)
    {
        return new ConversationDto(c.Id, c.UserId, c.UserName, c.UserEmail, c.UserPhone,
            c.VehicleId, c.VehicleTitle, c.VehiclePrice, c.DealerId, c.DealerName,
            c.Status, c.LeadScore, c.LeadTemperature, c.BuyingSignals,
            c.IsHandedOff, c.StartedAt, c.EndedAt, c.MessageCount, c.Duration);
    }
}

public class GetDealerConversationsQueryHandler : IRequestHandler<GetDealerConversationsQuery, List<ConversationDto>>
{
    private readonly IConversationRepository _repository;

    public GetDealerConversationsQueryHandler(IConversationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ConversationDto>> Handle(GetDealerConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await _repository.GetByDealerIdAsync(request.DealerId, request.Page, request.PageSize, cancellationToken);
        return conversations.Select(MapToDto).ToList();
    }

    private ConversationDto MapToDto(Conversation c)
    {
        return new ConversationDto(c.Id, c.UserId, c.UserName, c.UserEmail, c.UserPhone,
            c.VehicleId, c.VehicleTitle, c.VehiclePrice, c.DealerId, c.DealerName,
            c.Status, c.LeadScore, c.LeadTemperature, c.BuyingSignals,
            c.IsHandedOff, c.StartedAt, c.EndedAt, c.MessageCount, c.Duration);
    }
}

public class GetHotLeadsQueryHandler : IRequestHandler<GetHotLeadsQuery, List<ConversationDto>>
{
    private readonly IConversationRepository _repository;

    public GetHotLeadsQueryHandler(IConversationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ConversationDto>> Handle(GetHotLeadsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await _repository.GetHotLeadsAsync(request.MinScore, request.Page, request.PageSize, cancellationToken);
        return conversations.Select(MapToDto).ToList();
    }

    private ConversationDto MapToDto(Conversation c)
    {
        return new ConversationDto(c.Id, c.UserId, c.UserName, c.UserEmail, c.UserPhone,
            c.VehicleId, c.VehicleTitle, c.VehiclePrice, c.DealerId, c.DealerName,
            c.Status, c.LeadScore, c.LeadTemperature, c.BuyingSignals,
            c.IsHandedOff, c.StartedAt, c.EndedAt, c.MessageCount, c.Duration);
    }
}

public class GetChatbotStatisticsQueryHandler : IRequestHandler<GetChatbotStatisticsQuery, ChatbotStatsDto>
{
    private readonly IConversationRepository _repository;

    public GetChatbotStatisticsQueryHandler(IConversationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ChatbotStatsDto> Handle(GetChatbotStatisticsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _repository.GetStatisticsAsync(request.DealerId, request.StartDate, request.EndDate, cancellationToken);

        return new ChatbotStatsDto(
            (int)stats["TotalConversations"],
            (int)stats["ActiveConversations"],
            (int)stats["HandedOffConversations"],
            (int)stats["HotLeads"],
            (int)stats["WarmLeads"],
            (int)stats["ColdLeads"],
            (double)stats["AverageLeadScore"],
            (double)stats["ConversionRate"],
            (TimeSpan)stats["AverageConversationDuration"],
            (Dictionary<string, int>)stats["TopBuyingSignals"]
        );
    }
}
