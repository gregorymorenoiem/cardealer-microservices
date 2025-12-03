using MessageBusService.Application.Commands;
using MessageBusService.Application.Interfaces;
using MessageBusService.Domain.Entities;
using MessageBusService.Domain.Enums;
using Moq;
using Xunit;

namespace MessageBusService.Tests.Commands;

public class StartSagaCommandHandlerTests
{
    private readonly Mock<ISagaRepository> _mockRepository;
    private readonly Mock<ISagaOrchestrator> _mockOrchestrator;
    private readonly StartSagaCommandHandler _handler;

    public StartSagaCommandHandlerTests()
    {
        _mockRepository = new Mock<ISagaRepository>();
        _mockOrchestrator = new Mock<ISagaOrchestrator>();
        _handler = new StartSagaCommandHandler(_mockRepository.Object, _mockOrchestrator.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateSaga_WithCorrectProperties()
    {
        // Arrange
        var command = new StartSagaCommand
        {
            Name = "TestSaga",
            Description = "Test saga description",
            Type = SagaType.Orchestration,
            CorrelationId = "test-correlation-id",
            Steps = new List<SagaStepDefinition>
            {
                new SagaStepDefinition
                {
                    Name = "Step1",
                    ServiceName = "ServiceA",
                    ActionType = "create",
                    ActionPayload = "{\"data\":\"test\"}",
                    CompensationActionType = "delete",
                    CompensationPayload = "{\"id\":\"test\"}"
                }
            }
        };

        Saga? capturedSaga = null;
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Saga>(), It.IsAny<CancellationToken>()))
            .Callback<Saga, CancellationToken>((saga, _) => capturedSaga = saga)
            .ReturnsAsync((Saga saga, CancellationToken _) => saga);

        var executedSaga = new Saga
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Status = SagaStatus.Running
        };

        _mockOrchestrator.Setup(o => o.StartSagaAsync(It.IsAny<Saga>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(executedSaga);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedSaga);
        Assert.Equal(command.Name, capturedSaga.Name);
        Assert.Equal(command.Description, capturedSaga.Description);
        Assert.Equal(command.Type, capturedSaga.Type);
        Assert.Equal(SagaStatus.Created, capturedSaga.Status);
        Assert.Equal(command.CorrelationId, capturedSaga.CorrelationId);
        Assert.Single(capturedSaga.Steps);
        Assert.Equal("Step1", capturedSaga.Steps[0].Name);
    }

    [Fact]
    public async Task Handle_ShouldGenerateCorrelationId_WhenNotProvided()
    {
        // Arrange
        var command = new StartSagaCommand
        {
            Name = "TestSaga",
            CorrelationId = string.Empty,
            Steps = new List<SagaStepDefinition>()
        };

        Saga? capturedSaga = null;
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Saga>(), It.IsAny<CancellationToken>()))
            .Callback<Saga, CancellationToken>((saga, _) => capturedSaga = saga)
            .ReturnsAsync((Saga saga, CancellationToken _) => saga);

        _mockOrchestrator.Setup(o => o.StartSagaAsync(It.IsAny<Saga>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Saga saga, CancellationToken _) => saga);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedSaga);
        Assert.False(string.IsNullOrEmpty(capturedSaga.CorrelationId));
        Assert.True(Guid.TryParse(capturedSaga.CorrelationId, out _));
    }

    [Fact]
    public async Task Handle_ShouldSetStepOrders_Correctly()
    {
        // Arrange
        var command = new StartSagaCommand
        {
            Name = "TestSaga",
            Steps = new List<SagaStepDefinition>
            {
                new SagaStepDefinition { Name = "Step1", ServiceName = "ServiceA", ActionType = "create", ActionPayload = "{}" },
                new SagaStepDefinition { Name = "Step2", ServiceName = "ServiceB", ActionType = "create", ActionPayload = "{}" },
                new SagaStepDefinition { Name = "Step3", ServiceName = "ServiceC", ActionType = "create", ActionPayload = "{}" }
            }
        };

        Saga? capturedSaga = null;
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Saga>(), It.IsAny<CancellationToken>()))
            .Callback<Saga, CancellationToken>((saga, _) => capturedSaga = saga)
            .ReturnsAsync((Saga saga, CancellationToken _) => saga);

        _mockOrchestrator.Setup(o => o.StartSagaAsync(It.IsAny<Saga>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Saga saga, CancellationToken _) => saga);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedSaga);
        Assert.Equal(3, capturedSaga.Steps.Count);
        Assert.Equal(0, capturedSaga.Steps[0].Order);
        Assert.Equal(1, capturedSaga.Steps[1].Order);
        Assert.Equal(2, capturedSaga.Steps[2].Order);
    }

    [Fact]
    public async Task Handle_ShouldCallOrchestrator_AfterCreatingSaga()
    {
        // Arrange
        var command = new StartSagaCommand
        {
            Name = "TestSaga",
            Steps = new List<SagaStepDefinition>()
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Saga>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Saga saga, CancellationToken _) => saga);

        _mockOrchestrator.Setup(o => o.StartSagaAsync(It.IsAny<Saga>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Saga saga, CancellationToken _) => saga);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockOrchestrator.Verify(o => o.StartSagaAsync(It.IsAny<Saga>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
