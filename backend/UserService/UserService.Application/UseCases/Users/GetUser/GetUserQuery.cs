using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserService.Application.DTOs;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;

namespace UserService.Application.UseCases.Users.GetUser
{
    public record GetUserQuery(Guid UserId) : IRequest<UserDto>;

    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto>
    {
        private readonly IUserRepository _userRepository;

        public GetUserQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new NotFoundException($"User {request.UserId} not found");
            }

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Phone = user.PhoneNumber,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.ProfilePicture,
                City = user.City,
                Province = user.Province,
                AccountType = user.AccountType.ToString().ToLowerInvariant(),
                IsVerified = user.IsEmailVerified,
                IsEmailVerified = user.IsEmailVerified,
                IsPhoneVerified = false,
                VehicleCount = 0,
                ReviewCount = 0,
                AverageRating = 0,
                ResponseRate = 0,
                MemberSince = user.CreatedAt,
                LastActive = user.LastLoginAt,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}
