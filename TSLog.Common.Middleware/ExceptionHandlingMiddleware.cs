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
                    HttpStatusCode.InternalServerError);
            }
            catch (EmployeeNotFoundException ex)
            {
                _logger.LogError("EmployeeNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogError("EntityNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError);
            }
            catch (LegalEntityNotFoundException ex)
            {
                _logger.LogError("LegalEntityNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError);
            }
            catch (OperationNotFoundException ex)
            {
                _logger.LogError("OperationNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError);
            }
            catch (PartnerNotFoundException ex)
            {
                _logger.LogError("PartnerNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError);
            }
            catch (PositionNotFoundException ex)
            {
                _logger.LogError("PositionNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError);
            }
            catch (ProductNotFoundException ex)
            {
                _logger.LogError("ProductNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError);
            }
            catch (ShiftNotFoundException ex)
            {
                _logger.LogError("ShiftNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError);
            }
            catch (SiteNotFoundException ex)
            {
                _logger.LogError("SiteNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError);
            }
            catch (WarehouseActionNotFoundException ex)
            {
                _logger.LogError("WarehouseActionNotFoundException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError);
            }
            catch (NullReferenceException ex)
            {
                _logger.LogError("NullReferenceException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.ServiceUnavailable);
            }
            catch (Oracle.ManagedDataAccess.Client.OracleException ex)
            {
                _logger.LogError("OracleException {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception {Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
                await HandleExceptionAsync(httpContext,
                    ex.Message,
                    HttpStatusCode.InternalServerError);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, string exMsg, HttpStatusCode httpStatusCode)
        {
            HttpResponse response = context.Response;
            response.ContentType = "application/text";
            response.StatusCode = (int)httpStatusCode;
            await response.WriteAsync(exMsg);
        }
    }
}