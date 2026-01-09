using MediatR;
using ChatbotService.Application.DTOs;

namespace ChatbotService.Application.Features.Conversations.Queries;

public record GetConversationQuery(Guid ConversationId) : IRequest<ConversationDto?>;

public record GetConversationMessagesQuery(Guid ConversationId) : IRequest<List<MessageDto>>;

public record GetUserConversationsQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<List<ConversationDto>>;

public record GetDealerConversationsQuery(Guid DealerId, int Page = 1, int PageSize = 20) : IRequest<List<ConversationDto>>;

public record GetHotLeadsQuery(int MinScore = 85, int Page = 1, int PageSize = 20) : IRequest<List<ConversationDto>>;

public record GetChatbotStatisticsQuery(Guid? DealerId = null, DateTime? StartDate = null, DateTime? EndDate = null) : IRequest<ChatbotStatsDto>;
