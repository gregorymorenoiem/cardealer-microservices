using Xunit;
using FluentAssertions;
using Moq;
using AzulPaymentService.Domain.Entities;
using AzulPaymentService.Domain.Enums;
using AzulPaymentService.Domain.Interfaces;
using AzulPaymentService.Application.DTOs;
using AzulPaymentService.Application.Features.Refund.Commands;
using Serilog;

namespace AzulPaymentService.Tests;

/// <summary>
/// Tests para operaciones de reembolso
/// </summary>
public class RefundCommandTests
{
    private readonly Mock<IAzulTransactionRepository> _mockTransactionRepo;
    private readonly Mock<ILogger<RefundCommandHandler>> _mockLogger;

    public RefundCommandTests()
    {
        _mockTransactionRepo = new Mock<IAzulTransactionRepository>();
        _mockLogger = new Mock<ILogger<RefundCommandHandler>>();
    }

    [Fact]
    public async Task RefundCommand_WithValidTransaction_ReturnsSuccessResponse()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var originalTransaction = new AzulTransaction
        {
            Id = transactionId,
            UserId = Guid.NewGuid(),
            Amount = 1000m,
            Currency = "DOP",
            Status = TransactionStatus.Approved
        };

        var refundRequest = new RefundRequestDto
        {
            TransactionId = transactionId,
            Reason = "Customer request"
        };

        var handler = new RefundCommandHandler(_mockTransactionRepo.Object, _mockLogger.Object);
        var command = new RefundCommand(refundRequest);

        _mockTransactionRepo
            .Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalTransaction);

        _mockTransactionRepo
            .Setup(x => x.CreateAsync(It.IsAny<AzulTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AzulTransaction tx, CancellationToken ct) => tx);

        _mockTransactionRepo
            .Setup(x => x.UpdateAsync(It.IsAny<AzulTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AzulTransaction tx, CancellationToken ct) => tx);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeTrue();
        result.Status.Should().Be("Refunded");
    }

    [Fact]
    public async Task RefundCommand_WithNonExistentTransaction_ThrowsException()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var refundRequest = new RefundRequestDto
        {
            TransactionId = transactionId,
            Reason = "Test refund"
        };

        var handler = new RefundCommandHandler(_mockTransactionRepo.Object, _mockLogger.Object);
        var command = new RefundCommand(refundRequest);

        _mockTransactionRepo
            .Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AzulTransaction?)null);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RefundCommand_WithDeclinedTransaction_ThrowsException()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var declinedTransaction = new AzulTransaction
        {
            Id = transactionId,
            Status = TransactionStatus.Declined
        };

        var refundRequest = new RefundRequestDto
        {
            TransactionId = transactionId,
            Reason = "Test refund"
        };

        var handler = new RefundCommandHandler(_mockTransactionRepo.Object, _mockLogger.Object);
        var command = new RefundCommand(refundRequest);

        _mockTransactionRepo
            .Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(declinedTransaction);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RefundCommand_WithPartialAmount_ShouldSucceed()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var originalTransaction = new AzulTransaction
        {
            Id = transactionId,
            UserId = Guid.NewGuid(),
            Amount = 1000m,
            Status = TransactionStatus.Approved
        };

        var refundRequest = new RefundRequestDto
        {
            TransactionId = transactionId,
            PartialAmount = 500m,
            Reason = "Partial refund"
        };

        var handler = new RefundCommandHandler(_mockTransactionRepo.Object, _mockLogger.Object);
        var command = new RefundCommand(refundRequest);

        _mockTransactionRepo
            .Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalTransaction);

        _mockTransactionRepo
            .Setup(x => x.CreateAsync(It.IsAny<AzulTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AzulTransaction tx, CancellationToken ct) => tx);

        _mockTransactionRepo
            .Setup(x => x.UpdateAsync(It.IsAny<AzulTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AzulTransaction tx, CancellationToken ct) => tx);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(500m);
    }
}
