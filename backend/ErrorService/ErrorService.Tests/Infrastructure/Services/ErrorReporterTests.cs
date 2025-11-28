using ErrorService.Application.DTOs;
using ErrorService.Application.UseCases.LogError;
using ErrorService.Domain.Interfaces;
using ErrorService.Infrastructure.Services;
using MediatR;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ErrorService.Tests.Infrastructure.Services
{
    public class ErrorReporterTests
    {
        [Fact]
        public async Task ReportErrorAsync_ReturnsErrorId()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var errorId = Guid.NewGuid();
            mediatorMock.Setup(m => m.Send(It.IsAny<LogErrorCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LogErrorResponse(errorId));
            var reporter = new ErrorReporter(mediatorMock.Object);
            var request = new ErrorReport
            {
                ServiceName = "Service",
                ExceptionType = "Exception",
                Message = "Message",
                StackTrace = "Stack",
                OccurredAt = DateTime.UtcNow,
                Endpoint = "/endpoint",
                HttpMethod = "POST",
                StatusCode = 500,
                UserId = "user"
            };

            // Act
            var result = await reporter.ReportErrorAsync(request);

            // Assert
            Assert.Equal(errorId, result);
        }
    }
}
