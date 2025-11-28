using System.Collections.Generic;

namespace ErrorService.Application.DTOs
{
    public record GetServiceNamesResponse(List<string> ServiceNames);
}