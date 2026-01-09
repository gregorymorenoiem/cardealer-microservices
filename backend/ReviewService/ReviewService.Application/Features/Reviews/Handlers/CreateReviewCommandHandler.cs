using MediatR;
using ReviewService.Application.Features.Reviews.Commands;
using ReviewService.Application.DTOs;
using ReviewService.Domain.Interfaces;
using ReviewService.Domain.Entities;
using ReviewService.Domain.Base;
using FluentValidation;

namespace ReviewService.Application.Features.Reviews.Handlers;

/// <summary>
/// Handler para CreateReviewCommand
/// </summary>
public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Result<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IReviewSummaryRepository _summaryRepository;
    private readonly IValidator<CreateReviewCommand> _validator;

    public CreateReviewCommandHandler(
        IReviewRepository reviewRepository,
        IReviewSummaryRepository summaryRepository,
        IValidator<CreateReviewCommand> validator)
    {
        _reviewRepository = reviewRepository;
        _summaryRepository = summaryRepository;
        _validator = validator;
    }

    public async Task<Result<ReviewDto>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        // Validar request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result<ReviewDto>.Failure(errors);
        }

        // Verificar si ya existe una review de este buyer para este seller/vehicle
        var hasExisting = await _reviewRepository.HasBuyerReviewedSellerAsync(
            request.BuyerId, request.SellerId, request.VehicleId);

        if (hasExisting)
        {
            return Result<ReviewDto>.Failure("Ya has dejado una review para este vendedor/vehículo");
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

        return Result<ReviewDto>.Success(reviewDto);
    }
}