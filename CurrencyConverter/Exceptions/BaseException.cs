using System.Net;

namespace CurrencyConverter.Exceptions
{
    public class BaseException : Exception
    {
        public readonly int Code = (int) HttpStatusCode.BadRequest;

        public readonly Dictionary<string, string> Errors = [];

        public BaseException(string message) : base(message) {}

        public BaseException(string message, Exception innerException) : base(message, innerException) { }

        public BaseException(string message, int code) : base(message) { Code = code; }

        public BaseException(string message, int code, Exception innerException) : base(message, innerException) { Code = code; }

        public BaseException(string message, int code, Dictionary<string, string> errors) : base(message) { Code = code; Errors = errors; }

        public BaseException(string message, int code, Dictionary<string, string> errors, Exception innerException) : base(message, innerException) { Code = code; Errors = errors;}
    }
}
