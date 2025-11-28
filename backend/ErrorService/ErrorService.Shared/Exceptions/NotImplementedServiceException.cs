namespace ErrorService.Shared.Exceptions
{
    public class NotImplementedServiceException : AppException
    {
        public NotImplementedServiceException(string message) : base(message, 501) { }
    }
}