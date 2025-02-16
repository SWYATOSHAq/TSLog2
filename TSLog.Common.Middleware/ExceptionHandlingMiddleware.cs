using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TSLog.ExceptionHandler;

namespace TSLog.Common.Middleware;

/// <summary>
/// Данный класс реализует глобальную обработку исключений на уровне конвейера Middleware.
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

    private async void HandleExceptionCatch(HttpContext httpContext, Exception ex, HttpStatusCode httpStatusCode) 
    {
        _logger.LogError("{Message} with {stackTrace} and {targetSite}", ex.Message, ex.StackTrace, ex.TargetSite);
        await HandleExceptionAsync(httpContext, ex.Message, httpStatusCode);
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (DepartmentNotFoundException ex)
        {
            HandleExceptionCatch(httpContext, ex, HttpStatusCode.InternalServerError);
        }
        catch (EmployeeNotFoundException ex)
        {
            HandleExceptionCatch(httpContext, ex, HttpStatusCode.InternalServerError);
        }
        catch (EntityNotFoundException ex)
        {
            HandleExceptionCatch(httpContext, ex, HttpStatusCode.InternalServerError);
        }
        catch (LegalEntityNotFoundException ex)
        {
            HandleExceptionCatch(httpContext, ex, HttpStatusCode.InternalServerError);
        }
        catch (OperationNotFoundException ex)
        {
            HandleExceptionCatch(httpContext, ex, HttpStatusCode.InternalServerError);
        }
        catch (PartnerNotFoundException ex)
        {
            HandleExceptionCatch(httpContext, ex, HttpStatusCode.InternalServerError);
        }
        catch (PositionNotFoundException ex)
        {
            HandleExceptionCatch(httpContext, ex, HttpStatusCode.InternalServerError);
        }
        catch (ProductNotFoundException ex)
        {
            HandleExceptionCatch(httpContext, ex, HttpStatusCode.InternalServerError);
        }
        catch (ShiftNotFoundException ex)
        {
            HandleExceptionCatch(httpContext, ex, HttpStatusCode.InternalServerError);
        }
        catch (SiteNotFoundException ex)
        {
            HandleExceptionCatch(httpContext, ex, HttpStatusCode.InternalServerError);
        }
        catch (WarehouseActionNotFoundException ex)
        {
            HandleExceptionCatch(httpContext, ex, HttpStatusCode.InternalServerError);
        }
        catch (NullReferenceException ex)
        {
            HandleExceptionCatch(httpContext, ex, HttpStatusCode.ServiceUnavailable);
        }
        catch (Oracle.ManagedDataAccess.Client.OracleException ex)
        {
            HandleExceptionCatch(httpContext, ex, HttpStatusCode.ServiceUnavailable);
        }
        catch (Exception ex)
        {
            HandleExceptionCatch(httpContext, ex, HttpStatusCode.InternalServerError);
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
