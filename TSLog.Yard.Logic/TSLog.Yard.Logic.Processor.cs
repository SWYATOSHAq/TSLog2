using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Serilog.Core;
using System.Dynamic;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using TSLog.Authorization;
using TSLog.Common;
using TSLog.Partners.BO;
using TSLog.Staff.BO;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static TSLog.Yard.BO.OperationType;

namespace TSLog.Yard.Logic;

public class Processor
{
    private readonly IConfiguration _configuration;
    private readonly IYardStorage _storage;
    private readonly IRightsChecker _rightsChecker;
    private readonly bool _useAuthentication;
    private readonly ILogger<Processor> _logger;

    /// <summary>
    /// Максимальное количество дней для некоторых отчётов (по дням).
    /// </summary>
    private const int MaxReportPeriodDays = 45;

    // Средее время (ПО УМОЛЧАНИЮ) выгрузки/погрузки паллеты/коробки/штуки.
    private const int PalletUnloadTime = 93;
    private const int BoxUnloadTime = 20;
    private const int PieceUnloadTime = 25;
    private const int PalletLoadTime = 93;
    private const int BoxLoadTime = 20;
    private const int PieceLoadTime = 25;

    /// <summary>
    /// "Постоянния" часть времени операции двора ПО УМОЛЧАНИЮ (не зависит от состава заказов/приходов),
    /// 8 минут 15 секунд на постановку машины,
    /// 6 минут 16 секунд на снятие машины (получено от Алексея Охотникова 2022.09.09).
    /// СЕЙЧАС НЕ ИСПОЛЬЗУЕТСЯ. ВМЕСТО ЭТОГО ПРОИСХОДИТ ОБРАЩЕНИЕ В БД.
    /// </summary>
    private const int DefaultConstantOperationDuration = (8 * 60 + 15) + (6 * 60 + 15);

    /// <summary>
    /// "Постоянния" часть времени операции двора (не зависит от состава заказов/приходов),
    /// СЕЙЧАС НЕ ИСПОЛЬЗУЕТСЯ. ВМЕСТО ЭТОГО ПРОИСХОДИТ ОБРАЩЕНИЕ В БД.
    /// </summary>
    public int ConstantOperationDuration { get; set; } = DefaultConstantOperationDuration;
    public Processor(IConfiguration configuration, IYardStorage storage, IRightsChecker rightsChecker, ILogger<Processor> logger)
    {
        // TODO: check nulls.
        _configuration = configuration;
        _storage = storage;
        _rightsChecker = rightsChecker;
        _useAuthentication = _configuration.GetValue<bool>("UseAuthentication");
        _logger = logger;
    }

    private static dynamic EmployeeToDynamic(Employee employee, bool isBusy = false)
    {
        dynamic result = new ExpandoObject();
        result.id = employee.Id;
        result.surname = employee.Surname;
        result.firstName = employee.FirstName;
        result.patronymic = employee.Patronymic;
        result.shortName = employee.ShortName;
        result.fullName = employee.FullName;
        result.lowerFullName = employee.FullName.ToLower();
        result.positionId = employee.MajorPosition != null && employee.MajorPosition != Position.Null && employee.MajorPosition != Position.Empty ?
            employee.MajorPosition.Id :
            -1;
        result.positionCode = employee.MajorPosition != null && employee.MajorPosition != Position.Null && employee.MajorPosition != Position.Empty ?
            employee.MajorPosition.Code :
            string.Empty;
        result.positionName = employee.MajorPosition != null && employee.MajorPosition != Position.Null && employee.MajorPosition != Position.Empty ?
            employee.MajorPosition.Name :
            string.Empty;
        result.positionShortName = employee.MajorPosition != null && employee.MajorPosition != Position.Null && employee.MajorPosition != Position.Empty ?
            employee.MajorPosition.ShortName :
            string.Empty;
        result.isBusy = isBusy;
        return result;
    }
    private static dynamic[] EmployeesToDynamic(Employee[] employees)
    {
        var result = new List<dynamic>();
        foreach (var employee in employees)
            result.Add(EmployeeToDynamic(employee));
        return result.ToArray();
    }
    /// <summary>
    /// Возвращает сотрудников, которые могут занимать указанную должность.
    /// </summary>
    /// <param name="namePattern">Строка поиска сотрудника по имени (по подобию).</param>
    /// <param name="positionCode">Код должности</param>
    /// <param name="user">Пользователь.</param>
    /// <param name="statusCode">Код статуса выполнения (подобен HTTP status code).Ко</param>
    /// <param name="statusMessage">Текст ответа/сообщение об ошибке.</param>
    /// <returns>Массив dynamic объектов сотрудников.</returns>
    public dynamic[] GetPositionWorkers(string namePattern, string positionCode, ClaimsPrincipal user,
        out int statusCode, out string statusMessage)
    {
        try
        {
            var useYard = _rightsChecker.CheckRight("USE_YARD", user);
            var getWorkers = _rightsChecker.CheckRight("GET_WORKERS", user);
            if (_useAuthentication && (!useYard || !getWorkers))
            {
                statusCode = 403;
                statusMessage = "Forbidden";
                return Array.Empty<dynamic>();
            }

            if (string.IsNullOrWhiteSpace(positionCode))
            {
                statusCode = 500;
                statusMessage = "Position code is empty";
                return Array.Empty<dynamic>();
            }

            var position = _storage.GetPosition(positionCode);
            var workers = _storage.FindEmployees(position, namePattern);
            var objects = EmployeesToDynamic(workers);
            statusCode = 200;
            statusMessage = string.Empty;
            return objects;
        }
        catch (Exception exception)
        {
            statusCode = 500;
            statusMessage = exception.Message;                
            return Array.Empty<dynamic>();
        }
    }

    public Operation? AllocateOperationWorkers(dynamic? data, ClaimsPrincipal user,
        out int statusCode, out string statusMessage)
    {
        try
        {
            statusCode = 500;
            var useYard = _rightsChecker.CheckRight("USE_YARD", user);
            var manageOperationWorkers = _rightsChecker.CheckRight("MANAGE_OPERATION_WORKERS", user);
            if (_useAuthentication && (!useYard || !manageOperationWorkers))
            {
                statusCode = 403;
                statusMessage = "Forbidden";
                return null;
            }
            if (data == null)
            {
                statusCode = 500;
                statusMessage = "Input data is null!";
                return null;
            }

            string stringId = data.id;
            if (string.IsNullOrWhiteSpace(stringId))
            {
                statusCode = 500;
                statusMessage = "Operation id is empty";
                return null;
            }

            if (!long.TryParse(stringId, out long id))
            {
                statusCode = 500;
                statusMessage = $"Wrong operation id {stringId}";
                return null;
            }

            var operation = _storage.GetOperation(id);
            _storage.AllocateOperationWorkers(operation);
            statusCode = 200;
            statusMessage = string.Empty;
            return operation;
        }
        catch (Exception exception)
        {
            statusCode = 500;
            statusMessage = exception.Message;
            return null;
        }
    }

    public string CreateOperation(dynamic data, ClaimsPrincipal? user,
        out int statusCode, out string statusMessage)
    {
        var useYard = _rightsChecker.CheckRight("USE_YARD", user);
        var createOperation = _rightsChecker.CheckRight("CREATE_OPERATION", user);
        statusCode = (int)HttpStatusCode.InternalServerError;
        statusMessage = string.Empty;

        if (_useAuthentication && (!useYard || !createOperation))
        {
            statusCode = (int)HttpStatusCode.Forbidden;
            statusMessage = "Forbidden";
            return string.Empty;
        }

        string temp = data.operationTypeCode;
        if (string.IsNullOrWhiteSpace(temp))
        {
            statusMessage = "Operation type code is empty!";
            return string.Empty; 
        }
            
        var operationType = _storage.GetOperationType(temp);

        temp = data.vehicleKindCode;
        if (string.IsNullOrWhiteSpace(temp))
        {
            statusMessage = "Vehicle kind code is empty!";
            return string.Empty;
        }

        var vehicleKind = _storage.GetVehicleKind(temp);
        temp = data.startDate;
        if (string.IsNullOrWhiteSpace(temp))
        {
            statusMessage = "Start date is empty!";
            return string.Empty;
        }

        if (!Utils.DateFromString(temp, out DateTime planWorkStartDate, out string message))
        {
            statusMessage = message;
            return string.Empty;
        }            

        dynamic[] actions = ((JArray)data.warehouseActions).ToArray<dynamic>();

        temp = data.finishDate;
        var planWorkFinishDate = planWorkStartDate.Add(new TimeSpan(0, 0, _GetFillingPlanOperationDuration(operationType, vehicleKind, actions)));
        if (!string.IsNullOrWhiteSpace(temp))
        {
            if (!DateTime.TryParse(temp, out DateTime tempFinishDate))
            {
                statusMessage = $"Wrong finish date {temp}";
                return string.Empty;
            }
            if (tempFinishDate > planWorkFinishDate)
                planWorkFinishDate = tempFinishDate;
        }

        string vehicleBrand = data.vehicleBrand;
        if (string.IsNullOrWhiteSpace(vehicleBrand))
            vehicleBrand = "<Не указано>";
        string vehicleModel = data.vehicleModel;
        if (string.IsNullOrWhiteSpace(vehicleModel))
            vehicleModel = "<Не указано>";
        string vehicleNumber = data.vehicleNumber;
        if (string.IsNullOrWhiteSpace(vehicleNumber))
            vehicleNumber = "<Не указано>";
        string trailerNumber = data.trailerNumber;

        string driverName = data.driverName;
        if (string.IsNullOrWhiteSpace(driverName))
            driverName = "<Не указано>";
        string driverPhoneNumber = data.driverPhoneNumber;

        var state = _storage.GetOperationState("CREATED");

        temp = data.priority;
        var priority = Operation.DefaultPriority;

        if (!string.IsNullOrWhiteSpace(temp))
        {
            if (!short.TryParse(temp, out priority))
            {
                statusMessage = $"Wrong priority {temp}";
                return string.Empty;
            }
        }

        string partnerManagerName = data.partnerManagerName;
        string partnerManagerPhoneNumber = data.partnerManagerPhoneNumber;
        string partnerManagerEmail = data.partnerManagerEmail;
        string note = data.note;

        string partnerId = data.partnerId;
        if (string.IsNullOrWhiteSpace(partnerId))
        {
            statusMessage = "Partner id is empty!";
            return string.Empty;
        }

        var partner = _storage.GetPartner(partnerId);
        var manager = _storage.GetPartnerManagers(partner)[0];

        var operation = new Operation(-1, null, operationType, planWorkStartDate, planWorkFinishDate,
            vehicleKind, vehicleBrand, vehicleModel, vehicleNumber, trailerNumber,
            driverName, driverPhoneNumber,
            state,
            priority,
            partnerManagerName, partnerManagerPhoneNumber, partnerManagerEmail,
            manager)
        {
            Note = note
        };

        operation.Parking = false;
        var userForLog = RightsChecker.GetUserName(user);
        var operationFromStorage = _storage.AddOperation(operation, userForLog);

        foreach (var action in actions)
        {
            string stringActionKind = action.kind;
            string stringActionId = action.id;
            
            if (stringActionKind != "I" && stringActionKind != "O")
            {
                statusMessage = $"Wrong action kind {stringActionKind}!";
                return string.Empty;
            }
            if (string.IsNullOrWhiteSpace(stringActionId))
            {
                statusMessage = "Action id is empty!";
                return string.Empty;
            }          
            
            var actionFromStorage = _storage.GetWarehouseAction(stringActionKind == "O" ? ActionKind.Outgo : ActionKind.Income, stringActionId);
            _storage.AddAction(operationFromStorage, actionFromStorage);
            var operationForLog = _storage.GetOperation(operationFromStorage.Id);
            _logger.LogWarning("Отработал метод AddAction из метода CreateOperation заявки {@Заявка}, с номером {Номер заявки}. Добавлен заказ {Заказ}. Пользователь: {Пользователь}", operationForLog, operationForLog.Id, actionFromStorage.Code, userForLog);
        }
        var operationWithActions = _storage.GetOperation(operationFromStorage.Id);
        _logger.LogWarning("Создана заявка {@Заявка}, с номером {Номер заявки}. Пользователь: {Пользователь}", operationWithActions, operationWithActions.Id, userForLog);
        _storage.AddOperationLog(operationWithActions, "Создана", userForLog); //логирование в бд

        string? response = OperationToJson(operationWithActions);
        statusCode = (int)HttpStatusCode.Created;
        return response;
    }

    public string CreateConfirmOperation(dynamic data, ClaimsPrincipal? user, out int statusCode, out string statusMessage)
    {
        statusCode = (int)HttpStatusCode.InternalServerError;
        var useYard = _rightsChecker.CheckRight("USE_YARD", user);
        var createOperation = _rightsChecker.CheckRight("CREATE_CONFIRM_OPERATION", user);
        statusMessage = string.Empty;
        if (_useAuthentication && (!useYard || !createOperation))
        {
            statusCode = (int)HttpStatusCode.Forbidden;
            statusMessage = "Forbidden";
            return string.Empty;
        }
        string temp = data.operationTypeCode;
        if (string.IsNullOrWhiteSpace(temp))
        {
            statusMessage = "Operation type code is empty!";
            return string.Empty;
        }

        var operationType = _storage.GetOperationType(temp);

        temp = data.vehicleKindCode;
        if (string.IsNullOrWhiteSpace(temp))
        {
            statusMessage = "Vehicle kind code is empty!";
            return string.Empty;
        }

        var vehicleKind = _storage.GetVehicleKind(temp);

        temp = data.startDate;
        if (string.IsNullOrWhiteSpace(temp))
        {
            statusMessage = "Start date is empty!";
            return string.Empty;
        }

        if (!Utils.DateFromString(temp, out DateTime startDate, out string message))
        {
            statusMessage = message;
            return string.Empty;
        }

        dynamic[] actions = ((JArray)data.warehouseActions).ToArray<dynamic>();

        temp = data.finishDate;
        var finishDate = startDate.Add(new TimeSpan(0, 0, _storage.GetDuration(operationType, vehicleKind, out int constantDuration, out int variableDuration)));
        if (!string.IsNullOrWhiteSpace(temp))
        {
            if (!DateTime.TryParse(temp, out DateTime tempFinishDate))
            {
                statusMessage = $"Wrong finish date {temp}";
                return string.Empty;
            }

            if (tempFinishDate > finishDate)
                finishDate = tempFinishDate;
        }

        string vehicleBrand = data.vehicleBrand;
        if (string.IsNullOrWhiteSpace(vehicleBrand))
            vehicleBrand = "<Не указано>";
        string vehicleModel = data.vehicleModel;
        if (string.IsNullOrWhiteSpace(vehicleModel))
            vehicleModel = "<Не указано>";
        string vehicleNumber = data.vehicleNumber;
        if (string.IsNullOrWhiteSpace(vehicleNumber))
            vehicleNumber = "<Не указано>";
        string trailerNumber = data.trailerNumber;

        string driverName = data.driverName;
        if (string.IsNullOrWhiteSpace(driverName))
            driverName = "<Не указано>";
        string driverPhoneNumber = data.driverPhoneNumber;

        var state = _storage.GetOperationState("CREATED");

        temp = data.priority;
        var priority = Operation.DefaultPriority;
        if (!string.IsNullOrWhiteSpace(temp))
        {
            if (!short.TryParse(temp, out priority))
            {
                statusMessage = $"Wrong priority {temp}";
                return string.Empty;
            }
        }

        string partnerManagerName = data.partnerManagerName;
        string partnerManagerPhoneNumber = data.partnerManagerPhoneNumber;
        string partnerManagerEmail = data.partnerManagerEmail;
        string note = data.note;

        string partnerId = data.partnerId;
        if (string.IsNullOrWhiteSpace(partnerId))
        {
            statusMessage = "Partner id is empty!";
            return string.Empty;
        }

        var partner = _storage.GetPartner(partnerId);
        var manager = _storage.GetPartnerManagers(partner)[0];
        var actionKind = operationType.Direction == TypeDirection.Out ? ActionKind.Outgo : ActionKind.Income;

        var operation = new Operation(-1, null, operationType, startDate, finishDate,
            vehicleKind, vehicleBrand, vehicleModel, vehicleNumber, trailerNumber,
            driverName, driverPhoneNumber,
            state,
            priority,
            partnerManagerName, partnerManagerPhoneNumber, partnerManagerEmail,
            manager)
        {
            Note = note
        };
        operation.Parking = true;
        var userForLog = RightsChecker.GetUserName(user);
        operation = _storage.AddOperation(operation, userForLog);

        // Эту строку (по поиску сотрудника по пользователю AD) перенёс сюда, что бы не найдя сотрудника (ответственного)
        // нельзя было присвоить ворота
        var employee = _storage.GetEmployeeByAdLogin(userForLog);
        //operation.Gate = _storage.SetOperationGate(operation, null, user);  до 26.06.23 подтверждение было на воротах
        _storage.SetResponsibleOfficer(operation, employee, userForLog);
        var operationFromStorage = _storage.GetOperation(operation.Id);
        foreach (var action in actions)
        {
            string stringActionKind = action.kind;
            string stringActionId = action.id;

            if (stringActionKind != "I" && stringActionKind != "O")
            {
                statusMessage = $"Wrong action kind {stringActionKind}!";
                return string.Empty;
            }
            if (string.IsNullOrWhiteSpace(stringActionId))
            {
                statusMessage = "Action id is empty!";
                return string.Empty;
            }

            var actionFromStorage = _storage.GetWarehouseAction(stringActionKind == "O" ? ActionKind.Outgo : ActionKind.Income, stringActionId);
            _storage.AddAction(operationFromStorage, actionFromStorage);
            var operationForLog = _storage.GetOperation(operationFromStorage.Id);
            _logger.LogWarning("Отработал метод AddAction из метода CreateConfirmOperation заявки {@Заявка}, с номером {Номер заявки}. Добавлен заказ {Заказ}. Пользователь: {Пользователь}", operationForLog, operationForLog.Id, actionFromStorage.Code, userForLog);
        }
        var operationWithActions = _storage.GetOperation(operationFromStorage.Id);
        _logger.LogWarning("Создана заявка с подтверждением {@Заявка}, с номером {Номер заявки}. Пользователь: {Пользователь}", operationWithActions, operationWithActions.Id, userForLog);
        _storage.AddOperationLog(operationWithActions, "Создана с подтверждением", userForLog); //логирование в бд

        string? response = OperationToJson(operationWithActions);
        statusCode = (int)HttpStatusCode.Created;
        return response;
    }

    public string CreateOperationByPartner(dynamic data, ClaimsPrincipal? user, out int statusCode, out string statusMessage)
    {
        statusCode = (int)HttpStatusCode.InternalServerError;
        var useYard = _rightsChecker.CheckRight("USE_YARD", user);
        var createOperation = _rightsChecker.CheckRight("CREATE_OPERATION_BY_PARTNER", user);
        statusMessage = string.Empty;
        if (_useAuthentication && (!useYard || !createOperation))
        {
            statusCode = (int)HttpStatusCode.Forbidden;
            statusMessage = "Forbidden";
            return string.Empty;
        }
        string temp = data.operationTypeCode;
        if (string.IsNullOrWhiteSpace(temp))
        {
            statusMessage = "Operation type code is empty!";
            return string.Empty;
        }
        var operationType = _storage.GetOperationType(temp);

        temp = data.vehicleKindCode;
        if (string.IsNullOrWhiteSpace(temp))
        {
            statusMessage = "Vehicle kind code is empty!";
            return string.Empty;
        }

        var vehicleKind = _storage.GetVehicleKind(temp);

        temp = data.startDate;
        if (string.IsNullOrWhiteSpace(temp))
        {
            statusMessage = "Start date is empty!";
            return string.Empty;
        }

        if (!Utils.DateFromString(temp, out DateTime startDate, out string message))
        {
            statusMessage = message;
            return string.Empty;
        }

        var userForLog = RightsChecker.GetUserName(user);
        // Плановая дата и время начало работ не должно быть "день в день"!
        var now = DateTime.Now;
        var today = DateTime.Today;
        var today900 = today.AddHours(9);
        if (userForLog != "mvi") // #3999 по заявке сделано исключние для клиента "MVI"
        {
            if (now >= today900 && startDate < today.AddHours(24 + 9))
            {
                statusMessage = "Too early start time (start time must be later then 9:00 tomorrow)!";
                return string.Empty;
            }
        };
        if (startDate < today900)
        {
            statusMessage = "Too early start time (start time must be later then 9:00 today)!";
            return string.Empty;
        }

        dynamic[] actions = ((JArray)data.warehouseActions).ToArray<dynamic>();

        temp = data.finishDate;
        var finishDate = startDate.Add(new TimeSpan(0, 0, _GetFillingPlanOperationDuration(operationType, vehicleKind, actions)));
        if (!string.IsNullOrWhiteSpace(temp))
        {
            if (!DateTime.TryParse(temp, out DateTime tempFinishDate))
            {
                statusMessage = $"Wrong finish date {temp}";
                return string.Empty;
            }

            if (tempFinishDate > finishDate)
                finishDate = tempFinishDate;
        }

        string vehicleBrand = data.vehicleBrand;
        if (string.IsNullOrWhiteSpace(vehicleBrand))
            vehicleBrand = "<Не указано>";
        string vehicleModel = data.vehicleModel;
        if (string.IsNullOrWhiteSpace(vehicleModel))
            vehicleModel = "<Не указано>";
        string vehicleNumber = data.vehicleNumber;
        if (string.IsNullOrWhiteSpace(vehicleNumber))
            vehicleNumber = "<Не указано>";
        string trailerNumber = data.trailerNumber;

        string driverName = data.driverName;
        if (string.IsNullOrWhiteSpace(driverName))
            driverName = "<Не указано>";
        string driverPhoneNumber = data.driverPhoneNumber;

        var state = _storage.GetOperationState("CREATED");

        temp = data.priority;
        var priority = Operation.DefaultPriority;
        if (!string.IsNullOrWhiteSpace(temp))
        {
            if (!short.TryParse(temp, out priority))
            {
                statusMessage = $"Wrong priority {temp}";
                return string.Empty;
            }
        }

        string partnerManagerName = data.partnerManagerName;
        string partnerManagerPhoneNumber = data.partnerManagerPhoneNumber;
        string partnerManagerEmail = data.partnerManagerEmail;
        string note = data.note;

        var partnerId = _rightsChecker.GetPartnerIdByUser(userForLog);
        var partner = _storage.GetPartner(partnerId);
        var manager = _storage.GetPartnerManagers(partner)[0];

        var operation = new Operation(-1, null, operationType, startDate, finishDate,
            vehicleKind, vehicleBrand, vehicleModel, vehicleNumber, trailerNumber,
            driverName, driverPhoneNumber,
            state,
            priority,
            partnerManagerName, partnerManagerPhoneNumber, partnerManagerEmail,
            manager)
        {
            Note = note
        };
        operation.Parking = false;
        operation = _storage.AddOperation(operation, userForLog);
        var operationFromStorage = _storage.GetOperation(operation.Id);
        foreach (var action in actions)
        {
            string stringActionKind = action.kind;
            string stringActionId = action.id;

            if (stringActionKind != "I" && stringActionKind != "O")
            {
                statusMessage = $"Wrong action kind {stringActionKind}!";
                return string.Empty;
            }
            if (string.IsNullOrWhiteSpace(stringActionId))
            {
                statusMessage = "Action id is empty!";
                return string.Empty;
            }

            var actionFromStorage = _storage.GetWarehouseAction(stringActionKind == "O" ? ActionKind.Outgo : ActionKind.Income, stringActionId);
            _storage.AddAction(operationFromStorage, actionFromStorage);
            var operationForLog = _storage.GetOperation(operationFromStorage.Id);
            _logger.LogWarning("Отработал метод AddAction из метода CreateOperationByPartner заявки {@Заявка}, с номером {Номер заявки}. Добавлен заказ {Заказ}. Пользователь: {Пользователь}", operationForLog, operationForLog.Id, actionFromStorage.Code, userForLog);
        }
        var operationWithActions = _storage.GetOperation(operationFromStorage.Id);
        _logger.LogWarning("Создана заявка партнером {@Заявка}, с номером {Номер заявки}. Пользователь: {Пользователь}", operationWithActions, operationWithActions.Id, userForLog);
        _storage.AddOperationLog(operationWithActions, "Создана (партнером)", userForLog); //логирование в бд

        string? response = OperationToJson(operationWithActions);
        statusCode = (int)HttpStatusCode.Created;
        return response;
    }

    public string EditOperation(dynamic data, ClaimsPrincipal? user, out int statusCode, out string statusMessage)
    {
        statusCode = (int)HttpStatusCode.InternalServerError;
        var useYard = _rightsChecker.CheckRight("USE_YARD", user);
        var editOperation = _rightsChecker.CheckRight("EDIT_OPERATION", user);
        statusMessage = string.Empty;
        if (_useAuthentication && (!useYard || !editOperation))
        {
            statusCode = (int)HttpStatusCode.Forbidden;
            statusMessage = "Forbidden";
            return string.Empty;
        }
        string temp = data.id;
        if (string.IsNullOrWhiteSpace(temp))
        {
            statusMessage = "Operation id is empty";
            return string.Empty;
        }

        if (!long.TryParse(temp, out long id))
        {
            statusMessage = $"Wrong operation id {temp}!";
            return string.Empty;
        }

        var operation = _storage.GetOperation(id);

        var operationType = operation.Type;
        temp = data.operationTypeCode;
        if (!string.IsNullOrWhiteSpace(temp))
        {
            operationType = _storage.GetOperationType(temp);
        }            
        else
        {
            statusMessage = $"Operation type is empty!";
            return string.Empty;
        }

        var vehicleKind = operation.VehicleKind;
        temp = data.vehicleKindCode;
        if (!string.IsNullOrWhiteSpace(temp))
        {
            vehicleKind = _storage.GetVehicleKind(temp);
        }            
        else
        {
            statusMessage = $"Vehicle kind is empty!";
            return string.Empty;
        }            

        var planWorkStartDate = operation.PlanWorkStartDate;
        var planWorkFinishDate = operation.PlanWorkFinishDate;
        dynamic[] actions = ((JArray)data.warehouseActions).ToArray<dynamic>();
        temp = data.startDate;
        if (!string.IsNullOrWhiteSpace(temp))
        {
            if (!Utils.DateFromString(temp, out planWorkStartDate, out string message))
            {
                statusMessage = message;
                return string.Empty;
            }
            planWorkFinishDate = planWorkStartDate.Add(new TimeSpan(0, 0, _GetFillingPlanOperationDuration(operation.Type, vehicleKind, actions)));
        }

        temp = data.finishDate;
        if (!string.IsNullOrWhiteSpace(temp))
        {
            if (!DateTime.TryParse(temp, out DateTime tempFinishDate))
            {
                statusMessage = $"Wrong finish date {temp}";
                return string.Empty;
            }

            if (tempFinishDate > planWorkFinishDate)
                planWorkFinishDate = tempFinishDate;
        }

        operation.VehicleBrand = data.vehicleBrand;
        operation.VehicleModel = data.vehicleModel;
        operation.VehicleNumber = data.vehicleNumber;
        operation.TrailerNumber = data.trailerNumber;
        operation.DriverName = data.driverName;
        operation.DriverPhoneNumber = data.driverPhoneNumber;
        operation.Parking = data.onParking ?? false;

        var gate = operation.Gate;
        temp = data.gateCode;
        if (!string.IsNullOrWhiteSpace(temp))
            gate = _storage.GetGate(temp);

        var priority = operation.Priority;
        temp = data.priority;
        if (!string.IsNullOrWhiteSpace(temp))
        {
            if (!short.TryParse(temp, out priority))
            {
                statusMessage = $"Wrong priority {temp}";
                return string.Empty;
            }
            operation.Priority = priority;
        }
        else
            operation.Priority = 1;

        operation.PartnerManagerName = data.partnerManagerName;
        operation.PartnerManagerPhoneNumber = data.partnerManagerPhoneNumber;
        operation.PartnerManagerEmail = data.partnerManagerEmail;
        operation.Note = data.note;
        temp = data.partnerId;
        if (string.IsNullOrWhiteSpace(temp))
        {
            statusMessage = "Partner id is empty";
            return string.Empty;
        }

        var manager = operation.Manager;
        temp = data.partnerId;
        if (!string.IsNullOrWhiteSpace(temp))
        {
            var partner = _storage.GetPartner(temp);
            var managers = _storage.GetPartnerManagers(partner);
            if (managers.Length <= 0)
            {
                statusMessage = "Partner has not managers!";
                return string.Empty;
            }
            operation.Manager = managers[0];
        }
        else
        {
            statusMessage = "Partner id is empty!";
            return string.Empty;
        }

        // Признак того, что изменились ключевые параметры, влияющие на выбор свободного окна и ворот
        var changeGate = operation.Type.Code != operation.Type.Code
            || operation.VehicleKind.Code != vehicleKind.Code
            || operation.PlanWorkStartDate != planWorkStartDate
            || operation.PlanWorkFinishDate != planWorkFinishDate;

        operation.Type = operationType;
        operation.VehicleKind = vehicleKind;
        operation.PlanWorkStartDate = planWorkStartDate;
        operation.PlanWorkFinishDate = planWorkFinishDate;
        var userForLog = RightsChecker.GetUserName(user);
        var employee = _storage.GetEmployeeByAdLogin(userForLog);
        _storage.EditOperation(operation, userForLog);
        _storage.SetResponsibleOfficer(operation, employee);
        if (operation.Gate.Code != gate.Code)
        {
            try
            {
                _storage.SetOperationGate(operation, gate, userForLog);
            }
            catch
            {
                _storage.SetOperationGate(operation, null, userForLog);
            }
            _storage.SetResponsibleOfficer(operation, employee);
        }
        else if (changeGate && !operation.Parking)
        {
            _storage.SetOperationGate(operation, null, userForLog);
            _storage.SetResponsibleOfficer(operation, employee);
        }
        else if (changeGate)
        {
            _storage.SetResponsibleOfficer(operation, employee);
        }
        var operationFromStorage = _storage.GetOperation(operation.Id);
        var actionsBeforeEdit = operationFromStorage.GetActions();
        WarehouseAction[] actionsConvertedFromDynamic = Array.Empty<WarehouseAction>();
        foreach (var action in actions)
        {
            string stringActionKind = action.kind;
            string stringActionId = action.id;

            if (stringActionKind != "I" && stringActionKind != "O")
            {
                statusMessage = $"Wrong action kind {stringActionKind}!";
                return string.Empty;
            }
            if (string.IsNullOrWhiteSpace(stringActionId))
            {
                statusMessage = "Action id is empty!";
                return string.Empty;
            }
            
            var actionFromStorage = _storage.GetWarehouseAction(stringActionKind == "O" ? ActionKind.Outgo : ActionKind.Income, stringActionId);
            actionsConvertedFromDynamic = actionsConvertedFromDynamic.Append(actionFromStorage).ToArray();
            if (actionsBeforeEdit.Contains(actionFromStorage))
                continue;
            operationFromStorage.AddAction(actionFromStorage);            
            _storage.AddAction(operationFromStorage, actionFromStorage);
            var operationForLog = _storage.GetOperation(operationFromStorage.Id);
            _logger.LogWarning("Отработал метод AddAction из метода EditOperation заявки {@Заявка}, с номером {Номер заявки}. Добавлен заказ {Заказ}. Пользователь: {Пользователь}", operationForLog, operationForLog.Id, actionFromStorage.Code, userForLog);
        }
        var actionsAfterEdit = operationFromStorage.GetActions();
        foreach (var action in actionsAfterEdit)
        {
            if (!actionsConvertedFromDynamic.Contains(action))
            {
                operationFromStorage.RemoveAction(action);
                _storage.RemoveAction(operationFromStorage, action);
                var operationForLog = _storage.GetOperation(operationFromStorage.Id);
                _logger.LogWarning("Отработал метод RemoveAction из метода EditOperation заявки {@Заявка}, с номером {Номер заявки}. Добавлен заказ {Заказ}. Пользователь: {Пользователь}", operationForLog, operationForLog.Id, action.Code, userForLog);
            }
        }
        var operationWithActions = _storage.GetOperation(operationFromStorage.Id);
        string editStatus = operationWithActions.State.DisplayName + "(отредактирована)";
        _logger.LogWarning("Подтверждена(отредактирована) заявка {@Заявка}, с номером {Номер заявки}. Пользователь: {Пользователь}", operationWithActions, operationWithActions.Id, userForLog);
        _storage.AddOperationLog(operationWithActions, editStatus, userForLog); //логирование в бд

        string? response = OperationToJson(operationWithActions);
        statusCode = (int)HttpStatusCode.OK;
        return response;
    }

    public string EditOperationByPartner(dynamic data, ClaimsPrincipal? user, out int statusCode, out string statusMessage)
    {
        statusCode = (int)HttpStatusCode.InternalServerError;
        var useYard = _rightsChecker.CheckRight("USE_YARD", user);
        var editOperation = _rightsChecker.CheckRight("EDIT_OPERATION_BY_PARTNER", user);
        statusMessage = string.Empty;
        if (_useAuthentication && (!useYard || !editOperation))
        {
            statusCode = (int)HttpStatusCode.Forbidden;
            statusMessage = "Forbidden";
            return string.Empty;
        }

        string temp = data.id;
        if (string.IsNullOrWhiteSpace(temp))
        {
            statusMessage = "Operation id is empty";
            return string.Empty;
        }

        if (!long.TryParse(temp, out long id))
        {
            statusMessage = $"Wrong operation id {temp}!";
            return string.Empty;
        }

        var operation = _storage.GetOperation(id);

        var operationType = operation.Type;
        temp = data.operationTypeCode;
        if (!string.IsNullOrWhiteSpace(temp))
        {
            operationType = _storage.GetOperationType(temp);
        }            
        else
        {
            statusMessage = $"Operation type is empty!";
            return string.Empty;
        }

        var vehicleKind = operation.VehicleKind;
        temp = data.vehicleKindCode;
        if (!string.IsNullOrWhiteSpace(temp))
        {
            vehicleKind = _storage.GetVehicleKind(temp);
        }            
        else
        {
            statusMessage = $"Vehicle kind is empty!";
            return string.Empty;
        }

        var planWorkStartDate = operation.StartDate;
        var planWorkFinishDate = operation.FinishDate;
        dynamic[] actions = ((JArray)data.warehouseActions).ToArray<dynamic>();
        var userForLog = RightsChecker.GetUserName(user);
        temp = data.startDate;
        if (!string.IsNullOrWhiteSpace(temp))
        {
            if (!Utils.DateFromString(temp, out DateTime tempPlanWorkStartDate, out string message))
            {
                statusMessage = message;
                return string.Empty;
            }

            // Если редактируем плановую дату и время начало работ то новое значение не должно быть "день в день"!
            if (planWorkStartDate != tempPlanWorkStartDate)
            {
                var now = DateTime.Now;
                var today = DateTime.Today;
                var today900 = today.AddHours(9);
                if (userForLog != "mvi") // #3999 по заявке сделано исключние для клиента "MVI"
                {
                    if (now >= today900 && tempPlanWorkStartDate < today.AddHours(24 + 9))
                    {
                        statusMessage = $"Too early start time (start time must be later then 9:00 tomorrow)!";
                        return string.Empty;
                    }
                }
                if (tempPlanWorkStartDate < today900)
                {
                    statusMessage = $"Too early start time (start time must be later then 9:00 today)!";
                    return string.Empty;
                }
            }
            planWorkStartDate = tempPlanWorkStartDate;
            planWorkFinishDate = planWorkStartDate.Add(new TimeSpan(0, 0, _GetFillingPlanOperationDuration(operation.Type, vehicleKind, actions)));
        }

        temp = data.finishDate;
        if (!string.IsNullOrWhiteSpace(temp))
        {
            if (!Utils.DateFromString(temp, out DateTime tempFinishDate, out string message))
            {
                statusMessage = message;
                return string.Empty;
            }

            if (tempFinishDate > planWorkFinishDate)
                planWorkFinishDate = tempFinishDate;
        }

        operation.VehicleBrand = data.vehicleBrand;
        operation.VehicleModel = data.vehicleModel;
        operation.VehicleNumber = data.vehicleNumber;
        operation.TrailerNumber = data.trailerNumber;

        operation.DriverName = data.driverName;
        operation.DriverPhoneNumber = data.driverPhoneNumber;

        var priority = operation.Priority;
        temp = data.priority;
        if (!string.IsNullOrWhiteSpace(temp))
        {
            if (!short.TryParse(temp, out priority))
            {
                statusMessage = $"Wrong priority {temp}";
                return string.Empty;
            }
            operation.Priority = priority;
        }
        else
            operation.Priority = 1;

        operation.PartnerManagerName = data.partnerManagerName;
        operation.PartnerManagerPhoneNumber = data.partnerManagerPhoneNumber;
        operation.PartnerManagerEmail = data.partnerManagerEmail;
        operation.Note = data.note;

        var partnerId = _rightsChecker.GetPartnerIdByUser(userForLog);
        var partner = _storage.GetPartner(partnerId);
        var managers = _storage.GetPartnerManagers(partner);
        if (managers.Length <= 0)
        {
            statusMessage = "Partner has not managers!";
            return string.Empty;
        }

        operation.Manager = managers[0];

        operation.Type = operationType;
        operation.VehicleKind = vehicleKind;
        operation.PlanWorkStartDate = planWorkStartDate;
        operation.PlanWorkFinishDate = planWorkFinishDate;
        _storage.EditOperation(operation, userForLog);

        var operationFromStorage = _storage.GetOperation(operation.Id);
        var actionsBeforeEdit = operationFromStorage.GetActions();
        WarehouseAction[] actionsConvertedFromDynamic = Array.Empty<WarehouseAction>();
        foreach (var action in actions)
        {
            string stringActionKind = action.kind;
            string stringActionId = action.id;

            if (stringActionKind != "I" && stringActionKind != "O")
            {
                statusMessage = $"Wrong action kind {stringActionKind}!";
                return string.Empty;
            }
            if (string.IsNullOrWhiteSpace(stringActionId))
            {
                statusMessage = "Action id is empty!";
                return string.Empty;
            }

            var actionFromStorage = _storage.GetWarehouseAction(stringActionKind == "O" ? ActionKind.Outgo : ActionKind.Income, stringActionId);
            actionsConvertedFromDynamic = actionsConvertedFromDynamic.Append(actionFromStorage).ToArray();
            if (actionsBeforeEdit.Contains(actionFromStorage))
                continue;
            operationFromStorage.AddAction(actionFromStorage);
            _storage.AddAction(operationFromStorage, actionFromStorage);
            var operationForLog = _storage.GetOperation(operationFromStorage.Id);
            _logger.LogWarning("Отработал метод AddAction из метода EditOperationByParner заявки {@Заявка}, с номером {Номер заявки}. Добавлен заказ {Заказ}. Пользователь: {Пользователь}", operationForLog, operationForLog.Id, actionFromStorage.Code, userForLog);
        }
        var actionsAfterEdit = operationFromStorage.GetActions();
        foreach (var action in actionsAfterEdit)
        {
            if (!actionsConvertedFromDynamic.Contains(action))
            {
                operationFromStorage.RemoveAction(action);
                _storage.RemoveAction(operationFromStorage, action);
                var operationForLog = _storage.GetOperation(operationFromStorage.Id);
                _logger.LogWarning("Отработал метод RemoveAction из метода EditOperationByParner заявки {@Заявка}, с номером {Номер заявки}. Добавлен заказ {Заказ}. Пользователь: {Пользователь}", operationForLog, operationForLog.Id, action.Code, userForLog);
            }
        }
        var operationWithActions = _storage.GetOperation(operationFromStorage.Id);
        string editStatus = operationWithActions.State.DisplayName + "(отредактирована партнером)";
        _logger.LogWarning("Подтверждена(отредактирована) партнером заявка {@Заявка}, с номером {Номер заявки}. Пользователь: {Пользователь}", operationWithActions, operationWithActions.Id, userForLog);
        _storage.AddOperationLog(operationWithActions, editStatus, userForLog); //логирование в бд

        string? response = OperationToJson(operationWithActions);
        statusCode = (int)HttpStatusCode.OK;
        return response;
    }

    public int _GetFillingPlanOperationDuration(OperationType operationType, VehicleKind vehicleKind, dynamic[] actions)
    {
        var standardTotalDuration = _storage.GetDuration(operationType, vehicleKind, out int standardConstantDuration, out int standardVariableDuration);
        var totalTime = 0.0;
        foreach (var action in actions)
        {
            /*
                Автор - Буров Михаил.
                Определяем ПЛАНОВОЕ время операции двора в зависимости от состава заказов/приходов в операции двора.
                Смотрим все товарные позиции всех заказов/приходов в операции двора.
                Для каждой позиции смотрим сначала затарку, если затарка есть считаем количество паллет и коробок по затарке (от количества товаров),
                есть фиксированное время загрузки/выгрузки паллеты/коробки - из этого и складывается плановое время работ с конкретной товарной позицией.
                Если затарки нет, то пробуем определить плановое время по объёму или весу товара в товарной позицией.
                Сначала смотрим вес, потом объём.
                Смотри какую часть объёма (веса) занимает конкретная товарная позиция от ВСЕГО объёма (веса), 
                умножаем плановое максимальное время (при условии загрузки/выгрузки машины ЦЕЛИКОМ) на соответствующий коэффициент.
                Если не удалось рассчитать плановое время ни по затарке, ни по весу ни по объёму - считаем время по СОБРАННЫМ ТЕ заказа (ТОЛЬКО для отгрузок).
            */
            var kind = action.kind == "O" ? ActionKind.Outgo : ActionKind.Income;
            var palletTime = action.kind == "O" ? PalletLoadTime : PalletUnloadTime;
            var boxTime = action.kind == "O" ? BoxLoadTime : BoxUnloadTime;
            var pieceTime = action.kind == "O" ? PieceLoadTime : PieceUnloadTime;
            string id = action.id;
            var lines = _storage.GetWarehouseActionLines(kind, id);
            var actionTime = 0.0;
            foreach (var line in lines)
            {
                // Смотрим по затарке
                var lineTime = 0.0;
                if (line.PlanPalletsCount > 0.0 || line.PlanBoxesCount > 0.0)
                    lineTime += line.PlanPalletsCount * palletTime + line.PlanBoxesCount * boxTime;
                else
                {
                    // Смотрим по весу
                    if (line.PlanGrossWeight > 0.0)
                        // Килограммы переводим в милиграммы
                        lineTime += line.PlanGrossWeight / (vehicleKind.Capacity * 1000 * 1000) * standardTotalDuration;
                    // Смотрим по объёму
                    else if (line.PlanVolume > 0.0)
                        lineTime += line.PlanVolume / (vehicleKind.CargoVolume * 1000 * 1000 * 1000) * standardTotalDuration;

                    // Новый блок по задаче #2416 Двор. Новая функция. План факт.
                    /*
                      Идея в том, что мы определяем количество "штук" в товарной позиции (SET = ST, 5 кг = 1 шт., 50 м = 1 шт).
                      Считаем "альтернативное" время загрузки/выгрузки товарной позиции как количество "штук", умноженное на время загрузки/выгрузке 1 штуки.
                      Если "альтернативное" время загрузки/выгрузки позиции более времени, рассчитанного по алгоритму, то берём "альтернативное" время.
                    */
                    var eachesCount = 0.0;
                    if (line.QuantityMeasureUnit.Code == "ST" || line.QuantityMeasureUnit.Code == "SET")
                        eachesCount = line.PlanQuantity;
                    else if (line.QuantityMeasureUnit.Code == "KG")
                        eachesCount = line.PlanQuantity / 5.0;
                    else if (line.QuantityMeasureUnit.Code == "G")
                        eachesCount = line.PlanQuantity / 5.0 / 1000.0;
                    else if (line.QuantityMeasureUnit.Code == "M")
                        eachesCount = line.PlanQuantity / 50.0;
                    else if (line.QuantityMeasureUnit.Code == "CM")
                        eachesCount = line.PlanQuantity / 50.0 / 100.0;

                    var altLineTime = eachesCount * boxTime;
                    if (lineTime < altLineTime)
                        lineTime = altLineTime;
                }
                actionTime += lineTime;
            }
            if (actionTime <= 0.0 && action.kind == "O")
            {
                // Смотри по собранным заказам (ТОЛЬКО отгрузки)
                _storage.GetAssembliedOutgo(id, out double palletsCount, out double eachesCount);
                actionTime = palletsCount * palletTime + eachesCount * pieceTime;
            }
            totalTime += actionTime;
        }
        totalTime = Math.Round(totalTime);

        if (totalTime <= 0.0)
            totalTime = standardTotalDuration;
        //  2022.09.12 добавил учёт постоянного (не зависящего от состава заказов) времени операции по указанию Алексея Охотникова 
        else
            totalTime += ConstantOperationDuration;
        // 2022.06.30 убрал учёт постоянных микроопераций по указанию Алексея Охотникова
        //else
        //    totalTime += standardConstantDuration;

        // Если продолжительность операции двора больше чем "стандартное" (при загрузке/выгрузке полной машины) то берём "стандартное".
        if (totalTime > standardTotalDuration)
            totalTime = standardTotalDuration;

        return (int)totalTime;
    }

    public string OperationToJson(Operation operation)
    {
        var temp = OperationToDynamic(operation);
        return JsonSerializer.Serialize(temp);
    }

    public dynamic OperationToDynamic(Operation operation)
    {
        dynamic result = new ExpandoObject();
        result.id = operation.Id;

        result.stateId = operation.State.Id;
        result.stateCode = operation.State.Code;
        result.stateName = operation.State.Name;
        result.stateShortName = operation.State.ShortName;
        result.stateDisplayName = operation.State.DisplayName;

        result.typeId = operation.Type.Id;
        result.typeCode = operation.Type.Code;
        result.typeName = operation.Type.Name;
        result.typeShortName = operation.Type.ShortName;
        result.typeDisplayName = operation.Type.DisplayName;
        result.directionName = operation.Type.DirectionName;

        result.vehicleKindId = operation.VehicleKind.Id;
        result.vehicleKindCode = operation.VehicleKind.Code;
        result.vehicleKindName = operation.VehicleKind.Name;
        result.vehicleKindShortName = operation.VehicleKind.ShortName;
        result.vehicleKindDisplayName = operation.VehicleKind.DisplayName;

        result.startDate = operation.StartDate;
        result.planWorkStartDate = operation.PlanWorkStartDate;
        result.factWorkStartDate = operation.FactWorkStartDate;
        result.planWorkFinishDate = operation.PlanWorkFinishDate;
        result.factWorkFinishDate = operation.FactWorkFinishDate;
        result.finishDate = operation.FinishDate;
        result.planDuration = (operation.PlanWorkFinishDate - operation.PlanWorkStartDate).TotalSeconds;
        result.factDuration = (operation.FactWorkFinishDate - operation.FactWorkStartDate).TotalSeconds;
        result.planLaborCost = operation.PlanLaborCost;
        result.planLinearLaborCost = operation.PlanLinearLaborCost;
        result.factLaborCost = operation.FactLaborCost;
        result.onParking = operation.Parking;

        result.gateId = operation.Gate != null && operation.Gate != Gate.Null && operation.Gate != Gate.Empty ?
            operation.Gate.Code :
            string.Empty;
        result.gateCode = operation.Gate != null && operation.Gate != Gate.Null && operation.Gate != Gate.Empty ?
            operation.Gate.Code :
            string.Empty;
        result.gateShortName = operation.Gate != null && operation.Gate != Gate.Null && operation.Gate != Gate.Empty ?
            operation.Gate.ShortName :
            string.Empty;
        result.gateShortName = operation.Gate != null && operation.Gate != Gate.Null && operation.Gate != Gate.Empty ?
            operation.Gate.ShortName :
            string.Empty;
        result.gateDiaplayName = operation.Gate != null && operation.Gate != Gate.Null && operation.Gate != Gate.Empty ?
            operation.Gate.DisplayName :
            string.Empty;
        result.gateStatus = operation.Gate != null && operation.Gate != Gate.Null && operation.Gate != Gate.Empty ?
            operation.Gate.State.ToString() :
            string.Empty;

        result.vehicleBrand = operation.VehicleBrand;
        result.lowerVehicleBrand = operation.VehicleBrand.ToLower();
        result.vehicleModel = operation.VehicleModel;
        result.lowerVehicleModel = operation.VehicleModel.ToLower();
        result.vehicleNumber = operation.VehicleNumber;
        result.lowerVehicleNumber = operation.VehicleNumber.ToLower();
        result.trailerNumber = operation.TrailerNumber;
        result.lowerTrailerNumber = operation.TrailerNumber.ToLower();

        result.driverName = operation.DriverName;
        result.lowerDriverName = operation.DriverName.ToLower();

        result.driverPhoneNumber = operation.DriverPhoneNumber;
        result.normalizedDriverPhoneNumber = operation.DriverPhoneNumber;
        result.normalizedDriverPhoneNumber = result.normalizedDriverPhoneNumber.Replace(" ", "");
        result.normalizedDriverPhoneNumber = result.normalizedDriverPhoneNumber.Replace("-", "");
        result.normalizedDriverPhoneNumber = result.normalizedDriverPhoneNumber.Replace("+", "");
        result.normalizedDriverPhoneNumber = result.normalizedDriverPhoneNumber.Replace("(", "");
        result.normalizedDriverPhoneNumber = result.normalizedDriverPhoneNumber.Replace(")", "");

        result.partnerManagerName = operation.PartnerManagerName;
        result.partnerManagerPhoneNumber = operation.PartnerManagerPhoneNumber;
        result.partnerManagerEmail = operation.PartnerManagerEmail;
        result.responsibleOfficer = operation.ResponsibleOfficerFullName;
        result.note = operation.Note;

        result.partnerId = operation.AnyPartnerId;
        result.partnerActionId = operation.PartnerActionsIds;
        result.lowerPartnerActionId = result.partnerActionId.ToLower();
        result.partnersNames = Partner.Serialize(operation.GetPartners());

        result.createDate = operation.CreateDate;
        try
        {
            result.creator = _storage.GetEmployeeByAdLogin(operation.Creator).FullName;
        }
        catch
        {
            result.creator = operation.Creator;
        }

        result.editDate = operation.EditDate;
        try
        {
            result.editor = _storage.GetEmployeeByAdLogin(operation.Editor).FullName;
        }
        catch
        {
            result.editor = operation.Editor;
        }

        result.displayColor = operation.DisplayColor;

        return result;
    }
}
