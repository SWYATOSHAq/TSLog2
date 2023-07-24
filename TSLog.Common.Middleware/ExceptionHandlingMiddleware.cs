using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TSLog.ExceptionHandler;
using TSLog.Common.Dto;

namespace TSLog.Common.Middleware
{
    /// <summary>
    /// Данный класс реализует глобальную обработку исключений на уровне конвейера Middleware
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (DepartmentNotFoundException ex)
            {
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.NotFound,
                    "Department not found!");
            }
            catch (EmployeeNotFoundException ex)
            {
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.NotFound,
                    "Employee not found!");
            }
            catch (EntityNotFoundException ex)
            {
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.NotFound,
                    "Entity not found!");
            }
            catch (LegalEntityNotFoundException ex)
            {
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.NotFound,
                    "LegalEntity not found!");
            }
            catch (NullReferenceException ex)
            {
                _logger.LogWarning("NullReferenceException with {stackTrace} and {targetSite}", ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.ServiceUnavailable,
                    $"NullReferenceException has happened!");
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError,
                    "Internal server error");
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, string exMsg, HttpStatusCode httpStatusCode, string message)
        {
            _logger.LogError(exMsg);

            HttpResponse response = context.Response;

            response.ContentType = "application/json";
            response.StatusCode = (int)httpStatusCode;

            ErrorWebDto errorWebDto = new()
            {
                Message = message,
                StatusCode = (int)httpStatusCode
            };

            await response.WriteAsJsonAsync(errorWebDto);
        }
    }
}