using System.Net;

namespace CurrencyConverter.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string message) : base(message, (int) HttpStatusCode.NotFound) { }

        public NotFoundException(string message, Exception innerException) : base(message, (int) HttpStatusCode.NotFound, innerException) { }
    }
}
