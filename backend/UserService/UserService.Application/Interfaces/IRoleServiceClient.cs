using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserService.Application.DTOs;

namespace UserService.Application.Interfaces
{
    public interface IRoleServiceClient
    {
        Task<bool> RoleExistsAsync(Guid roleId);
        Task<RoleDetailsDto?> GetRoleByIdAsync(Guid roleId);
        Task<List<RoleDetailsDto>> GetRolesByIdsAsync(List<Guid> roleIds);
    }
}
