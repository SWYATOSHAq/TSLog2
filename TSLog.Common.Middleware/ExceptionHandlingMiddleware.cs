using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TSLog.ExceptionHandler;
using TSLog.Common.Dto;
using System.Diagnostics;

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
                _logger.LogError("DepartmentNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError,
                    ex.Message);
            }
            catch (EmployeeNotFoundException ex)
            {
                _logger.LogError("EmployeeNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError,
                    ex.Message);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogError("EntityNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError,
                    ex.Message);
            }
            catch (LegalEntityNotFoundException ex)
            {
                _logger.LogError("LegalEntityNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError,
                    ex.Message);
            }
            catch (OperationNotFoundException ex)
            {
                _logger.LogError("OperationNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError,
                    ex.Message);
            }
            catch (PartnerNotFoundException ex)
            {
                _logger.LogError("PartnerNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError,
                    ex.Message);
            }
            catch (PositionNotFoundException ex)
            {
                _logger.LogError("PositionNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError,
                    ex.Message);
            }
            catch (ProductNotFoundException ex)
            {
                _logger.LogError("ProductNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError,
                    ex.Message);
            }
            catch (ShiftNotFoundException ex)
            {
                _logger.LogError("ShiftNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError,
                    ex.Message);
            }
            catch (SiteNotFoundException ex)
            {
                _logger.LogError("SiteNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError,
                    ex.Message);
            }
            catch (WarehouseActionNotFoundException ex)
            {
                _logger.LogError("WarehouseActionNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError,
                    ex.Message);
            }
            catch (NullReferenceException ex)
            {
                _logger.LogError("NullReferenceException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.ServiceUnavailable,
                    ex.Message);
            }
            catch (Oracle.ManagedDataAccess.Client.OracleException ex)
            {
                _logger.LogError("OracleException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.ServiceUnavailable,
                    $"Таблица или представление пользователя не существует!");
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError,
                    "Internal server error");
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, string exMsg, HttpStatusCode httpStatusCode, string message)
        {
            //_logger.LogError(exMsg);

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