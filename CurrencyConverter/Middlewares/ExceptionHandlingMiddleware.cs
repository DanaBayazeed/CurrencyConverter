using CurrencyConverter.Exceptions;
using Newtonsoft.Json;
using System.Net;

namespace CurrencyConverter.Middlewares
{
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;

        private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An error occurred.");

            _ = new Dictionary<string, string>();

            context.Response.ContentType = "application/json";

            context.Response.StatusCode = exception switch
            {
                BaseException => ((BaseException) exception).Code,
                _ => (int) HttpStatusCode.InternalServerError,
            };

            Dictionary<string, string>? errors = exception switch
            {
                BaseException => ((BaseException) exception).Errors,
                _ => [],
            };

            var result = JsonConvert.SerializeObject(new { Message = exception.Message, Errors = errors });

            await context.Response.WriteAsync(result);
        }
    }
}
