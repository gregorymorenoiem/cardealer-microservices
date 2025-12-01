using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserService.Application.DTOs;
using UserService.Domain.Interfaces;

namespace UserService.Application.UseCases.Users.GetUsers
{
    public record GetUsersQuery(int Page = 1, int PageSize = 10) : IRequest<PaginatedUsersResponse>;

    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedUsersResponse>
    {
        private readonly IUserRepository _userRepository;

        public GetUsersQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<PaginatedUsersResponse> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetAllAsync(request.Page, request.PageSize);
            var totalCount = await _userRepository.CountAsync();

            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                IsActive = u.IsActive,
                EmailConfirmed = u.EmailConfirmed,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                LastLoginAt = u.LastLoginAt
            }).ToList();

            return new PaginatedUsersResponse
            {
                Users = userDtos,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
