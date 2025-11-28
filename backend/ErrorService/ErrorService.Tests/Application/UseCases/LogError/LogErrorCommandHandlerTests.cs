using ErrorService.Application.DTOs;
using ErrorService.Application.UseCases.LogError;
using ErrorService.Domain.Entities;
using ErrorService.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ErrorService.Tests.Application.UseCases.LogError
{
    public class LogErrorCommandHandlerTests
    {
        [Fact]
        public async Task Handle_CreatesErrorLogAndReturnsResponse()
        {
            // Arrange
            var repoMock = new Mock<IErrorLogRepository>();
            repoMock.Setup(r => r.AddAsync(It.IsAny<ErrorLog>())).Returns(Task.CompletedTask);
            var handler = new LogErrorCommandHandler(repoMock.Object);
            var request = new LogErrorRequest("Service", "Exception", "Message", "Stack", DateTime.UtcNow, "/endpoint", "POST", 500, "user", new Dictionary<string, object>());
            var command = new LogErrorCommand(request);

            // Act
            var response = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, response.ErrorId);
            repoMock.Verify(r => r.AddAsync(It.IsAny<ErrorLog>()), Times.Once);
        }
    }
}
