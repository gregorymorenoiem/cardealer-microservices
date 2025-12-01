namespace UserService.Shared.Exceptions
{
    public class UnsupportedMediaTypeException : AppException
    {
        public UnsupportedMediaTypeException(string message) : base(message, 415) { }
    }
}
