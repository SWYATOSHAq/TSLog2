namespace TSLog.Links.WebApiForBoxScan;

public class Service
{
    private static bool CheckUser(HttpContext httpContext, SessionService sessionService, out IResult result)
    {
        if (httpContext.User == null || httpContext.User.Identity == null ||
            string.IsNullOrWhiteSpace(httpContext.User.Identity.Name))
        {
            result = Results.Problem("User is missing.");
            return false;
        }

        var normalizedUsername = SessionService.NormalizeUsername(httpContext.User.Identity.Name);
        if (sessionService.IsUserLoggedOut(normalizedUsername))
        {
            result = Results.Json(new { Message = "User is logged out." }, statusCode: 401);
            return false;
        }

        result = null;
        return true;
    }
    public static async Task<IResult> CheckOutgo(string outgoId, OracleDbWorker dbService,
    HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            bool outgoExists = await dbService.CheckOutgoAsync(outgoId);
            if (outgoExists)
                return Results.Ok(new { Message = "Outgo found.", outgoId });
            else
                return Results.NotFound(new { Message = "Outgo not found.", outgoId });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }
    public static async Task<IResult> CheckOutgoTu(string outgoId, string tuId,
    OracleDbWorker dbService, HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            bool outgoExists = await dbService.CheckOutgoTuAsync(outgoId, tuId);
            if (outgoExists)
                return Results.Ok(new { Message = "TU for outgo found.", outgoId, tuId });
            else
                return Results.NotFound(new { Message = "TU for outgo not found.", outgoId, tuId });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }
    public static async Task<IResult> GetOutgoTusCount(string outgoId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            int tusCount = await dbService.GetOutgoTusCountAsync(outgoId);
            return Results.Ok(new { Message = "Outgo found.", outgoId, tusCount });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }
    public static async Task<IResult> GetOutgoBoxesCount(string outgoId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            int boxesCount = await dbService.GetOutgoBoxesCountAsync(outgoId);
            return Results.Ok(new { Message = "Outgo found.", outgoId, boxesCount });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }
    public static async Task<IResult> GetTuBoxesCount(string tuId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            int boxesCount = await dbService.GetTuBoxesCountAsync(tuId);
            return Results.Ok(new { Message = "TU found.", tuId, boxesCount });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }
    public static async Task<IResult> GetOutgoScannedTusCount(string outgoId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            int tusCount = await dbService.GetOutgoScannedTusCountAsync(outgoId);
            return Results.Ok(new { Message = "Outgo found.", outgoId, tusCount });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }
    public static async Task<IResult> GetOutgoScannedBoxesCount(string outgoId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            int boxesCount = await dbService.GetOutgoScannedBoxesCountAsync(outgoId);
            return Results.Ok(new { Message = "Outgo found.", outgoId, boxesCount });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }
    public static async Task<IResult> GetTuScannedBoxesCount(string tuId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            int boxesCount = await dbService.GetTuScannedBoxesCountAsync(tuId);
            return Results.Ok(new { Message = "TU found.", tuId, boxesCount });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }
    public static async Task<IResult> AddSscc(AddSsccRequest request,
        OracleDbWorker dbService, HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            var normalizedUsername = SessionService.NormalizeUsername(httpContext.User.Identity.Name);
            await dbService.AddSsccAsync(request.OutgoId, request.TuId, request.Sscc, normalizedUsername);
            return Results.Ok(new
            {
                Message = "Box added.",
                outgoId = request.OutgoId,
                tuId = request.TuId,
                boxSscc = request.Sscc
            });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }
    public static async Task<IResult> AddSsccs(AddSsccsRequest request,
       OracleDbWorker dbService, HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            var normalizedUsername = SessionService.NormalizeUsername(httpContext.User.Identity.Name);
            await dbService.AddSsccsAsync(request.OutgoId, request.TuId, request.Ssccs, normalizedUsername);
            return Results.Ok(new
            {
                Message = "Boxes added.",
                outgoId = request.OutgoId,
                tuId = request.TuId,
                ssccsCount = request.Ssccs.Length
            });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }
    public static async Task<IResult> RemoveOutgoSsccs(string outgoId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            await dbService.RemoveOutgoSsccsAsync(outgoId);
            return Results.Ok(new { Message = "Outgo SSCCs removed.", outgoId });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }
    public static async Task<IResult> RemoveTuSsccs(string tuId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            await dbService.RemoveTuSsccsAsync(tuId);
            return Results.Ok(new { Message = "TU SSCCs removed.", tuId });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }
    public static async Task<IResult> RemoveSscc(string sscc, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            await dbService.RemoveSsccAsync(sscc);
            return Results.Ok(new { Message = "SSCC removed.", sscc });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }
}
