using System;
using System.Threading.Tasks;

namespace AdminService.Application.Interfaces
{
    public interface IErrorServiceClient
    {
        Task LogErrorAsync(string exceptionType, string message, string? stackTrace = null, string? endpoint = null, int? statusCode = null);
    }
}
