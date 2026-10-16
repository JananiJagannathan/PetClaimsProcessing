using System.Net;
using System.Text.Json;
using PetClaims.Api.Exceptions;

namespace PetClaims.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while processing the request.");

                if (!context.Response.HasStarted)
                {
                    await HandleExceptionAsync(
                        context,
                        ex);
                }
            }
        }

        private static async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            var statusCode = exception switch
            {
                NotFoundException =>
                    HttpStatusCode.NotFound,

                DuplicateInvoiceException =>
                    HttpStatusCode.Conflict,

                BusinessException =>
                    HttpStatusCode.BadRequest,

                _ =>
                    HttpStatusCode.InternalServerError
            };

            var response = new
            {
                statusCode = (int)statusCode,
                message = exception.Message
            };

            context.Response.Clear();

            context.Response.ContentType =
                "application/json";

            context.Response.StatusCode =
                (int)statusCode;

            var json =
                JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }
}