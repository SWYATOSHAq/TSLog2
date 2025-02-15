using System;

namespace TSLog.Links.WebApiForBoxScan;
/// <summary>
/// Основной класс-сервис для реализации API.
/// Реализованы метода API.
/// </summary>
public class Service
{
    /// <summary>
    /// Проверка пользователя.
    /// </summary>
    /// <param name="httpContext">HTTP контекст.</param>
    /// <param name="sessionService">Объект сервис сессии.</param>
    /// <param name="result">Признак того, что пользователю разрешено действие.</param>
    /// <returns></returns>
    private static bool CheckUser(HttpContext httpContext, SessionService sessionService, out IResult? result)
    {
        if (httpContext.User == null || httpContext.User.Identity == null ||
            string.IsNullOrWhiteSpace(httpContext.User.Identity.Name))
        {
            result = Results.Problem("User is missing.");
            return false;
        }

        var normalizedUsername = SessionService.NormalizeUserName(httpContext.User.Identity.Name);
        if (sessionService.IsUserLoggedOut(normalizedUsername))
        {
            result = Results.Json(new { Message = "User is logged out." }, statusCode: 401);
            return false;
        }

        result = null;
        return true;
    }
    /// <summary>
    /// Проверка расхода (заказа). Проверяет наличие расхода (заказа),
    /// наличие у него собранных ТЕ с не нулевым количеством коробов.
    /// </summary>
    /// <param name="outgoId">Идентификатор расхода (заказа).</param>
    /// <param name="dbService">Сервис работы с БД.</param>
    /// <param name="httpContext">HTTP контекст.</param>
    /// <param name="sessionService">Объект сервис сессии.</param>
    /// <returns>Объект-признак "хорошего расхода (заказа)."</returns>
    public static async Task<IResult?> CheckOutgo(string outgoId, OracleDbWorker dbService,
    HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out IResult? result))
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
    /// <summary>
    /// Проверка ТЕ. Проверает, что ТЕ существует и связана и указанным заказом,
    /// также проверяет наличие собранных коробов на ТЕ.
    /// </summary>
    /// <param name="outgoId">Идентификатор расхода (заказа).</param>
    /// <param name="tuId">Итентификаторт ТЕ.</param>
    /// <param name="dbService">Сервис работы с БД.</param>
    /// <param name="httpContext">HTTP контекст.</param>
    /// <param name="sessionService">Объект сервис сессии.</param>
    /// <returns>Объект-признак "корректной" ТЕ.</returns>
    public static async Task<IResult?> CheckOutgoTu(string outgoId, string tuId,
    OracleDbWorker dbService, HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out IResult? result))
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
    /// <summary>
    /// Возвращает количество ТЕ у собранного расхода (заказа)
    /// ДО! сканирования SSCC.
    /// </summary>
    /// <param name="outgoId">Идентификатор расхода (заказа).</param>
    /// <param name="dbService">Сервис работы с БД.</param>
    /// <param name="httpContext">HTTP контекст.</param>
    /// <param name="sessionService">Объект сервис сессии.</param>
    /// <returns>Количество ТЕ в собрнном расходе (до сканирования SSCC).</returns>
    public static async Task<IResult?> GetOutgoTusCount(string outgoId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out IResult? result))
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
    /// <summary>
    /// Возвращает количество коробов в собранном расходе (заказа),
    /// ДО! сканирования - по сути то количество коробов, которое надо просканировать.
    /// </summary>
    /// <param name="outgoId">Идентификатор расхода (заказа).</param>
    /// <param name="dbService">Сервис работы с БД.</param>
    /// <param name="httpContext">HTTP контекст.</param>
    /// <param name="sessionService">Объект сервис сессии.</param>
    /// <returns>Количество коробов в собранном расходе (заказе).</returns>
    public static async Task<IResult?> GetOutgoBoxesCount(string outgoId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out IResult? result))
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
    /// <summary>
    /// Возвращает количество коробов на ТЕ ДО! сканирования (которое надо просканировать).
    /// </summary>
    /// <param name="tuId">Идентификатор ТЕ.</param>
    /// <param name="dbService">Сервис работы с БД.</param>
    /// <param name="httpContext">HTTP контекст.</param>
    /// <param name="sessionService">Объект сервис сессии.</param>
    /// <returns>Количество коробов на ТЕ ДО! сканирования (которое надо просканировать).</returns>
    public static async Task<IResult?> GetTuBoxesCount(string tuId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out IResult? result))
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
    /// <summary>
    /// Возвращает количество ТЕ в расходе (заказе),
    /// которые хотя бы частично были просканированы (короба на них).
    /// </summary>
    /// <param name="outgoId">Идентификатор расхода (заказа).</param>
    /// <param name="dbService">Сервис работы с БД.</param>
    /// <param name="httpContext">HTTP контекст.</param>
    /// <param name="sessionService">Объект сервис сессии.</param>
    /// <returns>Количество просканированных ТЕ в расходе (заказе).</returns>
    public static async Task<IResult?> GetOutgoScannedTusCount(string outgoId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out IResult? result))
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
    /// <summary>
    /// Возвращает набор ТЕ в расходе (заказе),
    /// которые хотя бы частично были просканированы (короба на них).
    /// </summary>
    /// <param name="outgoId">Идентификатор расхода (заказа).</param>
    /// <param name="dbService">Сервис работы с БД.</param>
    /// <param name="httpContext">HTTP контекст.</param>
    /// <param name="sessionService">Объект сервис сессии.</param>
    /// <returns>Набор просканированных ТЕ в расходе (заказе).</returns>
    public static async Task<IResult?> GetOutgoScannedTus(string outgoId, OracleDbWorker dbService,
    HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out IResult? result))
            return result;

        try
        {
            string[] tus = await dbService.GetOutgoScannedTusAsync(outgoId);
            return Results.Ok(new { Message = "Outgo found.", outgoId, tus });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }
    /// <summary>
    /// Возвращает количество коробов в расходе (заказе),
    /// которые были просканированы.
    /// </summary>
    /// <param name="outgoId">Идентификатор расхода (заказа).</param>
    /// <param name="dbService">Сервис работы с БД.</param>
    /// <param name="httpContext">HTTP контекст.</param>
    /// <param name="sessionService">Объект сервис сессии.</param>
    /// <returns>Количество коробов в расходе (заказе), которые были просканированы.</returns>
    public static async Task<IResult?> GetOutgoScannedBoxesCount(string outgoId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out IResult? result))
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
    /// <summary>
    /// Возвращает набор коробов в расходе (заказе), которые были просканированы.
    /// </summary>
    /// <param name="outgoId">Идентификатор расхода (заказа).</param>
    /// <param name="dbService">Сервис работы с БД.</param>
    /// <param name="httpContext">HTTP контекст.</param>
    /// <param name="sessionService">Объект сервис сессии.</param>
    /// <returns>Набор (массив) коробов в расходе (заказе), которые были просканированы.</returns>
    public static async Task<IResult?> GetOutgoScannedBoxes(string outgoId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out IResult? result))
            return result;

        try
        {
            string[] boxes = await dbService.GetOutgoScannedBoxesAsync(outgoId);
            return Results.Ok(new { Message = "Outgo found.", outgoId, boxes });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }
    /// <summary>
    /// Возвращает количество отсканированных коробов на ТЕ.
    /// </summary>
    /// <param name="tuId">Идентификатор ТЕ.</param>
    /// <param name="dbService">Сервис работы с БД.</param>
    /// <param name="httpContext">HTTP контекст.</param>
    /// <param name="sessionService">Объект сервис сессии.</param>
    /// <returns>Количество отсканированных коробов на ТЕ.</returns>
    public static async Task<IResult?> GetTuScannedBoxesCount(string tuId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out IResult? result))
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
    /// <summary>
    /// Возвращает набор отсканированных коробов на ТЕ.
    /// </summary>
    /// <param name="tuId">Идентификатор ТЕ.</param>
    /// <param name="dbService">Сервис работы с БД.</param>
    /// <param name="httpContext">HTTP контекст.</param>
    /// <param name="sessionService">Объект сервис сессии.</param>
    /// <returns>Набор (массив) отсканированных коробов на ТЕ.</returns>
    public static async Task<IResult?> GetTuScannedBoxes(string tuId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out IResult? result))
            return result;

        try
        {
            string[] boxes = await dbService.GetTuScannedBoxesAsync(tuId);
            return Results.Ok(new { Message = "TU found.", tuId, boxes });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }
    /// <summary>
    /// Сохраняет отсканированный SSCC короба.
    /// </summary>
    /// <param name="request">Тело запроса (содержит всё нужное, в том числе сам SSCC).</param>
    /// <param name="dbService">Сервис работы с БД.</param>
    /// <param name="httpContext">HTTP контекст.</param>
    /// <param name="sessionService">Объект сервис сессии.</param>
    /// <returns>Объект-признак результата.</returns>
    public static async Task<IResult?> AddSscc(AddSsccRequest request,
        OracleDbWorker dbService, HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out IResult? result))
            return result;

        try
        {
            if (httpContext.User.Identity == null)
                return Results.Problem($"{httpContext.User.Identity} is null");
            var normalizedUsername = SessionService.NormalizeUserName(httpContext.User.Identity.Name);
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
    /// <summary>
    /// Сохраняет набор отсканированных SSCC коробов.
    /// </summary>
    /// <param name="request">Тело запроса (содержит всё нужное, в том числе набор SSCC).</param>
    /// <param name="dbService">Сервис работы с БД.</param>
    /// <param name="httpContext">HTTP контекст.</param>
    /// <param name="sessionService">Объект сервис сессии.</param>
    /// <returns>Объект-признак результата.</returns>
    public static async Task<IResult?> AddSsccs(AddSsccsRequest request,
       OracleDbWorker dbService, HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out IResult? result))
            return result;

        try
        {
            if (httpContext.User.Identity == null)
                return Results.Problem($"{httpContext.User.Identity} is null");
            var normalizedUsername = SessionService.NormalizeUserName(httpContext.User.Identity.Name);
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
    /// <summary>
    /// Удаляет ВСЕ! SSCC коробов, связанные с указанным расходом (заказом).
    /// </summary>
    /// <param name="outgoId">Идентификатор расхода (заказа).</param>
    /// <param name="dbService">Сервис работы с БД.</param>
    /// <param name="httpContext">HTTP контекст.</param>
    /// <param name="sessionService">Объект сервис сессии.</param>
    /// <returns>Объект-признак результата удаления.</returns>
    public static async Task<IResult?> RemoveOutgoSsccs(string outgoId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out IResult? result))
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
    /// <summary>
    /// Удаляет SSCC коробов, связанные с указанной ТЕ.
    /// </summary>
    /// <param name="tuId">Идентификатор ТЕ.</param>
    /// <param name="dbService">Сервис работы с БД.</param>
    /// <param name="httpContext">HTTP контекст.</param>
    /// <param name="sessionService">Объект сервис сессии.</param>
    /// <returns>Объект-признак результата удаления.</returns>
    public static async Task<IResult?> RemoveTuSsccs(string tuId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out IResult? result))
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
    /// <summary>
    /// Удаляет конкретный SSCC короба.
    /// </summary>
    /// <param name="sscc">SSCC короба.</param>
    /// <param name="dbService">Сервис работы с БД.</param>
    /// <param name="httpContext">HTTP контекст.</param>
    /// <param name="sessionService">Объект сервис сессии.</param>
    /// <returns>Объект-признак результата удаления.</returns>
    public static async Task<IResult?> RemoveSscc(string sscc, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out IResult? result))
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
