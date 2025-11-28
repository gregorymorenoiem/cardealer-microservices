using ErrorService.Application.DTOs;
using ErrorService.Domain.Entities;
using ErrorService.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ErrorService.Application.UseCases.LogError
{
    public class LogErrorCommandHandler : IRequestHandler<LogErrorCommand, LogErrorResponse>
    {
        private readonly IErrorLogRepository _errorLogRepository;

        public LogErrorCommandHandler(IErrorLogRepository errorLogRepository)
        {
            _errorLogRepository = errorLogRepository;
        }

        public async Task<LogErrorResponse> Handle(LogErrorCommand command, CancellationToken cancellationToken)
        {
            var errorLog = new ErrorLog
            {
                Id = System.Guid.NewGuid(),
                ServiceName = command.Request.ServiceName,
                ExceptionType = command.Request.ExceptionType,
                Message = command.Request.Message,
                StackTrace = command.Request.StackTrace,
                OccurredAt = command.Request.OccurredAt ?? System.DateTime.UtcNow,
                Endpoint = command.Request.Endpoint,
                HttpMethod = command.Request.HttpMethod,
                StatusCode = command.Request.StatusCode,
                UserId = command.Request.UserId,
                Metadata = command.Request.Metadata ?? new System.Collections.Generic.Dictionary<string, object>()
            };

            await _errorLogRepository.AddAsync(errorLog);
            
            return new LogErrorResponse(errorLog.Id);
        }
    }
}