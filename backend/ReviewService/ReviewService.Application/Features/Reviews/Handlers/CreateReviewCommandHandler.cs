using MediatR;
using ReviewService.Application.Features.Reviews.Commands;
using ReviewService.Application.DTOs;
using ReviewService.Domain.Interfaces;
using ReviewService.Domain.Entities;
using CarDealer.Shared.Application.Interfaces;
using FluentValidation;

namespace ReviewService.Application.Features.Reviews.Handlers;

/// &lt;summary&gt;
/// Handler para CreateReviewCommand
/// &lt;/summary&gt;
public class CreateReviewCommandHandler : IRequestHandler&lt;CreateReviewCommand, Result&lt;ReviewDto&gt;&gt;
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IReviewSummaryRepository _summaryRepository;
    private readonly IValidator&lt;CreateReviewCommand&gt; _validator;

    public CreateReviewCommandHandler(
        IReviewRepository reviewRepository,
        IReviewSummaryRepository summaryRepository,
        IValidator&lt;CreateReviewCommand&gt; validator)
    {
        _reviewRepository = reviewRepository;
        _summaryRepository = summaryRepository;
        _validator = validator;
    }

    public async Task&lt;Result&lt;ReviewDto&gt;&gt; Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        // Validar request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e =&gt; e.ErrorMessage));
            return Result&lt;ReviewDto&gt;.Failure(errors);
        }

        // Verificar si ya existe una review de este buyer para este seller/vehicle
        var hasExisting = await _reviewRepository.HasBuyerReviewedSellerAsync(
            request.BuyerId, request.SellerId, request.VehicleId);

        if (hasExisting)
        {
            return Result&lt;ReviewDto&gt;.Failure("Ya has dejado una review para este vendedor/vehículo");
        }

        // Crear nueva review
        var review = new Review
        {
            Id = Guid.NewGuid(),
            BuyerId = request.BuyerId,
            SellerId = request.SellerId,
            VehicleId = request.VehicleId,
            OrderId = request.OrderId,
            Rating = request.Rating,
            Title = request.Title,
            Content = request.Content,
            BuyerName = request.BuyerName,
            BuyerPhotoUrl = request.BuyerPhotoUrl,
            IsVerifiedPurchase = request.OrderId.HasValue,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Guardar review
        await _reviewRepository.AddAsync(review);

        // Actualizar estadísticas del vendedor
        await _summaryRepository.RefreshMetricsAsync(request.SellerId);

        // Retornar DTO
        var reviewDto = new ReviewDto
        {
            Id = review.Id,
            BuyerId = review.BuyerId,
            SellerId = review.SellerId,
            VehicleId = review.VehicleId,
            OrderId = review.OrderId,
            Rating = review.Rating,
            Title = review.Title,
            Content = review.Content,
            IsVerifiedPurchase = review.IsVerifiedPurchase,
            BuyerName = review.BuyerName,
            BuyerPhotoUrl = review.BuyerPhotoUrl,
            HelpfulVotes = review.HelpfulVotes,
            TotalVotes = review.TotalVotes,
            CreatedAt = review.CreatedAt,
            Response = null
        };

        return Result&lt;ReviewDto&gt;.Success(reviewDto);
    }
}