using System;
using System.Threading.Tasks;

namespace RoleService.Application.Interfaces
{
    public interface IErrorServiceClient
    {
        Task LogErrorAsync(string exceptionType, string message, string? stackTrace, string? endpoint = null, int? statusCode = null);
    }
}
