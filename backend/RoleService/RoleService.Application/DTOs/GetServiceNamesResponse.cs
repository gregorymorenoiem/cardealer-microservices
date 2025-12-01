using System.Collections.Generic;

namespace RoleService.Application.DTOs
{
    public record GetServiceNamesResponse(List<string> ServiceNames);
}
