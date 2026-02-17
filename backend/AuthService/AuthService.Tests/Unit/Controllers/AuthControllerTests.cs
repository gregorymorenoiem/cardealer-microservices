using Xunit;
using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using AuthService.Api.Controllers;
using AuthService.Application.Features.Auth.Commands.Register;
using AuthService.Application.DTOs.Auth;
using AuthService.Shared;

namespace AuthService.Tests.Unit.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new AuthController(_mediatorMock.Object);
        }

        [Fact]
        public async Task Register_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new RegisterRequest(
                UserName: null,
                Email: "test@example.com",
                Password: "Password123!",
                FirstName: "Test",
                LastName: "User",
                AcceptTerms: true
            );
            var expectedResponse = new RegisterResponse(
                Guid.NewGuid().ToString(),
                "testuser",
                "test@example.com",
                "access_token",
                "refresh_token",
                DateTime.UtcNow.AddHours(1)
            );

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<RegisterResponse>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(expectedResponse, apiResponse.Data);
        }
    }
}
