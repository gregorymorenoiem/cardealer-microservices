using UserService.Application.DTOs;
using UserService.Application.Metrics;
using UserService.Application.UseCases.LogError;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UserService.Tests.Application.UseCases.LogError
{
    public class LogErrorCommandHandlerTests
    {
        [Fact]
        public async Task Handle_CreatesRoleAndReturnsResponse()
        {
            // Arrange
            var repoMock = new Mock<IRoleRepository>();
            repoMock.Setup(r => r.AddAsync(It.IsAny<Role>()))
                .Returns(Task.CompletedTask);

            var metricsMock = new Mock<UserServiceMetrics>();

            var handler = new LogErrorCommandHandler(repoMock.Object, metricsMock.Object);
            var request = new LogErrorRequest("Service", "Exception", "Message", "Stack", DateTime.UtcNow, "/endpoint", "POST", 500, "user", new Dictionary<string, object>());
            var command = new LogErrorCommand(request);

            // Act
            var response = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, response.ErrorId);
            repoMock.Verify(r => r.AddAsync(It.IsAny<Role>()), Times.Once);
        }
    }
}
