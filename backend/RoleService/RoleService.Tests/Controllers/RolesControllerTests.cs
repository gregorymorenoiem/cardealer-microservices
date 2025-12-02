using RoleService.Api.Controllers;
using RoleService.Application.DTOs;
using RoleService.Application.UseCases.LogError;
using RoleService.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RoleService.Tests.Controllers
{
    public class ErrorsControllerTests
    {
        private ErrorsController CreateControllerWithUser(Mock<IMediator> mediatorMock, string serviceClaim = "RoleService", string role = "RoleServiceAdmin")
        {
            var controller = new ErrorsController(mediatorMock.Object);

            var claims = new[]
            {
                new Claim("service", serviceClaim),
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            return controller;
        }

        [Fact]
        public async Task LogError_WithValidJwtToken_ReturnsOkResult()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var logErrorResponse = new LogErrorResponse(Guid.NewGuid());
            mediatorMock.Setup(m => m.Send(It.IsAny<LogErrorCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(logErrorResponse);

            var controller = CreateControllerWithUser(mediatorMock);
            var request = new LogErrorRequest("Service", "Exception", "Message", "Stack", DateTime.UtcNow, "/endpoint", "POST", 500, "user", null);

            // Act
            var result = await controller.LogError(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<LogErrorResponse>>(okResult.Value);
            Assert.NotNull(response.Data);
            Assert.Equal(logErrorResponse.ErrorId, response.Data.ErrorId);
        }

        [Fact]
        public async Task LogError_WithRoleServiceAccessClaim_ExecutesSuccessfully()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var errorId = Guid.NewGuid();
            var logErrorResponse = new LogErrorResponse(errorId);
            mediatorMock.Setup(m => m.Send(It.IsAny<LogErrorCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(logErrorResponse);

            var controller = CreateControllerWithUser(mediatorMock, serviceClaim: "RoleService");
            var request = new LogErrorRequest("TestService", "NullReferenceException", "Test error", "Stack trace", DateTime.UtcNow, "/api/test", "GET", 500, "testuser", null);

            // Act
            var result = await controller.LogError(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<LogErrorResponse>>(okResult.Value);
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(errorId, response.Data.ErrorId);
            mediatorMock.Verify(m => m.Send(It.IsAny<LogErrorCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task LogError_WithAdminRole_ExecutesSuccessfully()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var logErrorResponse = new LogErrorResponse(Guid.NewGuid());
            mediatorMock.Setup(m => m.Send(It.IsAny<LogErrorCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(logErrorResponse);

            var controller = CreateControllerWithUser(mediatorMock, role: "RoleServiceAdmin");
            var request = new LogErrorRequest("Service", "Exception", "Message", "Stack", DateTime.UtcNow, "/endpoint", "POST", 500, "user", null);

            // Act
            var result = await controller.LogError(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<LogErrorResponse>>(okResult.Value);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task LogError_WithReadOnlyRole_ExecutesSuccessfully()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var logErrorResponse = new LogErrorResponse(Guid.NewGuid());
            mediatorMock.Setup(m => m.Send(It.IsAny<LogErrorCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(logErrorResponse);

            var controller = CreateControllerWithUser(mediatorMock, role: "RoleServiceRead");
            var request = new LogErrorRequest("Service", "Exception", "Message", "Stack", DateTime.UtcNow, "/endpoint", "POST", 500, "user", null);

            // Act
            var result = await controller.LogError(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<LogErrorResponse>>(okResult.Value);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public void Controller_HasCorrectUserContext()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var controller = CreateControllerWithUser(mediatorMock, "RoleService", "RoleServiceAdmin");

            // Assert
            Assert.NotNull(controller.User);
            Assert.True(controller.User.HasClaim("service", "RoleService"));
            Assert.True(controller.User.IsInRole("RoleServiceAdmin"));
            Assert.True(controller.User.Identity?.IsAuthenticated);
        }

        [Fact]
        public async Task LogError_CallsMediatorOnce()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var logErrorResponse = new LogErrorResponse(Guid.NewGuid());
            mediatorMock.Setup(m => m.Send(It.IsAny<LogErrorCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(logErrorResponse);

            var controller = CreateControllerWithUser(mediatorMock);
            var request = new LogErrorRequest("Service", "Exception", "Message", "Stack", DateTime.UtcNow, "/endpoint", "POST", 500, "user", null);

            // Act
            await controller.LogError(request);

            // Assert
            mediatorMock.Verify(m => m.Send(It.IsAny<LogErrorCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
