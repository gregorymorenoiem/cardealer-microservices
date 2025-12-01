using System.Collections.Generic;

namespace UserService.Application.DTOs
{
    public record GetServiceNamesResponse(List<string> ServiceNames);
}
