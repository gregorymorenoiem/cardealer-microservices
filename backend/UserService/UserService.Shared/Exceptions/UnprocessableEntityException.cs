namespace UserService.Shared.Exceptions
{
    public class UnprocessableEntityException : AppException
    {
        public UnprocessableEntityException(string message) : base(message, 422) { }
    }
}
