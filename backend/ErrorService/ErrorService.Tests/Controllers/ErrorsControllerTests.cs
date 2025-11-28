using ErrorService.Api.Controllers;
using ErrorService.Application.DTOs;
using ErrorService.Application.UseCases.LogError;
using ErrorService.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ErrorService.Tests.Controllers
{
    public class ErrorsControllerTests
    {
        [Fact]
        public async Task LogError_ReturnsOkResult_WithErrorId()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var logErrorResponse = new LogErrorResponse(Guid.NewGuid());
            mediatorMock.Setup(m => m.Send(It.IsAny<LogErrorCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(logErrorResponse);
            var controller = new ErrorsController(mediatorMock.Object);
            var request = new LogErrorRequest("Service", "Exception", "Message", "Stack", DateTime.UtcNow, "/endpoint", "POST", 500, "user", null);

            // Act
            var result = await controller.LogError(request);

            // Assert
            var okResult = Assert.IsType<ActionResult<ApiResponse<LogErrorResponse>>>(result);
            Assert.NotNull(okResult.Value);
            Assert.NotNull(okResult.Value.Data);
            Assert.Equal(logErrorResponse.ErrorId, okResult.Value.Data.ErrorId);
        }
    }
}
