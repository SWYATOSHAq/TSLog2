using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Globalization;
using System.Net;
using System.Text.Json;
using TSLog.Authorization;
using TSLog.BossControl.BO;
using TSLog.Common;
using TSLog.Common.BO;
using TSLog.Partners.BO;
using TSLog.Staff.BO;
using TSLog.WarehouseActions.BO;
using TSLog.Yard.BO;
using TSLog.Yard.Logic;
using TSLog.Yard.Reporting;
using static TSLog.Yard.BO.OperationType;
using OperationType = TSLog.Yard.BO.OperationType;

namespace TSLog.Yard.WebUI.Pages.api.v1
{
    /// <summary>
    /// Контроллер api модуля управления двором.
    /// </summary>
    [Route("/api/v1/[controller]")]
    [ApiController]
    public class YardController : ControllerBase
    {
        /// <summary>
        /// Максимальное количество дней для некоторых отчётов (по дням).
        /// </summary>
        private const int MaxReportPeriodDays = 45;

        /// <summary>
        /// "Постоянния" часть времени операции двора ПО УМОЛЧАНИЮ (не зависит от состава заказов/приходов),
        /// 8 минут 15 секунд на постановку машины,
        /// 6 минут 16 секунд на снятие машины (получено от Алексея Охотникова 2022.09.09).
        /// СЕЙЧАС НЕ ИСПОЛЬЗУЕТСЯ. ВМЕСТО ЭТОГО ПРОИСХОДИТ ОБРАЩЕНИЕ В БД.
        /// </summary>
        private const int DefaultConstantOperationDuration = (8 * 60 + 15) + (6 * 60 + 15);

        private readonly IConfiguration _configuration;
        private readonly IYardStorage _storage;
        private readonly IRightsChecker _rightsChecker;
        private readonly Processor _processor;
        private readonly bool _useAuthentication;
        private readonly ILogger<YardController> _logger;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// "Постоянния" часть времени операции двора (не зависит от состава заказов/приходов),
        /// СЕЙЧАС НЕ ИСПОЛЬЗУЕТСЯ. ВМЕСТО ЭТОГО ПРОИСХОДИТ ОБРАЩЕНИЕ В БД.
        /// </summary>
        public int ConstantOperationDuration { get; set; } = DefaultConstantOperationDuration;
        public YardController(IConfiguration configuration, IYardStorage storage, IRightsChecker rightsChecker, ILogger<YardController> logger, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _storage = storage;
            _rightsChecker = rightsChecker;
            _useAuthentication = _configuration.GetValue<bool>("UseAuthentication");
            _loggerFactory = loggerFactory;
            _processor = new Processor(configuration, storage, rightsChecker, _loggerFactory.CreateLogger<Processor>());
            _logger = logger;
        }

        private static async Task<dynamic?> GetInputData(HttpRequest request)
        {
            request.EnableBuffering();
            var body = request.Body;
            body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(body);
            string json = await reader.ReadToEndAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
        }
        /// <summary>
        /// Формирует хэштаблицу из параметров HTTP запроса.
        /// </summary>
        /// <param name="request">HTTP запрос.</param>
        /// <param name="keys">Массив имён параметров.</param>
        /// <returns>Хэштаблица параметров HTTP запроса.</returns>
        private static IDictionary<string, string> GetRequestParameters(HttpRequest request, string[] keys)
        {
            var result = new Dictionary<string, string>();
            foreach (var key in keys)
            {
                string? value = request.Query[key];
                if (!string.IsNullOrWhiteSpace(value))
                    result.Add(key, value);
            }
            return result;
        }

        private static dynamic GateToDynamic(Gate gate)
        {
            dynamic result = new ExpandoObject();
            result.id = gate.Id;
            result.code = gate.Code;
            result.name = gate.Name;
            result.shortName = gate.ShortName;
            result.altName = gate.AltName;
            result.displayName = gate.DisplayName;
            result.isActive = gate.IsActive;
            result.distplayOrder = gate.DisplayOrder;
            result.isHookan = gate.IsHookan;
            result.stateCode = "F";
            if (gate.State == Gate.GateState.Reserved)
                result.stateCode = "R";
            else if (gate.State == Gate.GateState.Busy)
                result.stateCode = "B";
            result.node = gate.Note;
            return result;
        }
        private static dynamic[] GatesToDynamic(Gate[] gates)
        {
            var result = new List<dynamic>();
            foreach (Gate gate in gates)
                result.Add(YardController.GateToDynamic(gate));
            return result.ToArray();
        }
       
        private dynamic[] OperationsToDynamic(Operation[] operations)
        {
            var result = new List<dynamic>();
            foreach (Operation operation in operations)
                result.Add(_processor.OperationToDynamic(operation));
            return result.ToArray();
        }
        private dynamic DoGetFillingPlanOperationLC(OperationType ot, VehicleKind vk, dynamic[] actions)
        {
            var k = GetFillingPlanOperationK(ot, vk, actions);
            var laborCost = _storage.GetLaborCost(ot, vk);
            var linearLaborCost = _storage.GetLinearLaborCost(ot, vk);

            dynamic result = new ExpandoObject();
            result.laborCost = Math.Round(laborCost * k);
            result.linearLaborCost = Math.Round(linearLaborCost * k);

            return result;
        }
        private string OperationsToJson(Operation[] operations)
        {
            var temp = OperationsToDynamic(operations);
            return JsonSerializer.Serialize(temp);
        }
        private HashSet<string> GetPartnersIds()
        {
            var userName = RightsChecker.GetUserName(User);
            return _rightsChecker.GetPartnersIdsByUser(userName);
        }

        #region Reference data
        private bool CheckPartner(string partnerId)
        {
            var userName = RightsChecker.GetUserName(User);
            var partnerIds = _rightsChecker.GetPartnersIdsByUser(userName);
            return partnerIds.Contains(partnerId);
        }
        [HttpGet("IsPartnerUser")]
        public string GetIsPartnerIser()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            if (_useAuthentication && !useYard)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            var isPartnerUser = _rightsChecker.CheckRight("IS_PARTNER_USER", User);
            var text = JsonSerializer.Serialize(isPartnerUser);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpGet("StandardPlanOperationDuration")]
        public string GetStandardPlanOperationDuration()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);            
            var getOperationDuration = _rightsChecker.CheckRight("GET_OPERATION_DURATION", User);
            if (_useAuthentication && (!useYard || !getOperationDuration))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? stringOperationType = Request.Query["operation_type"];
            string? stringVehicleKind = Request.Query["vehicle_kind"];

            if (string.IsNullOrWhiteSpace(stringOperationType))
                return "Operation type is empty";
            if (string.IsNullOrWhiteSpace(stringVehicleKind))
                return "Vehicle kind is empty";

            var operationType = _storage.GetOperationType(stringOperationType);
            var vehicleKind = _storage.GetVehicleKind(stringVehicleKind);
            var duration = _storage.GetDuration(operationType, vehicleKind, out int constantDuration, out int VariableDuration);
            var text = JsonSerializer.Serialize(duration);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        
        [HttpPost("FillingPlanOperationDuration")]
        async public Task<string> GetFillingPlanOperationDuration()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getOperationDuration = _rightsChecker.CheckRight("GET_OPERATION_DURATION", User);
            if (_useAuthentication && (!useYard || !getOperationDuration))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            dynamic? data = await GetInputData(Request);
            if (data == null)
                return "Input data is null!";

            string stringOperationType = data.operationTypeCode;
            if (string.IsNullOrWhiteSpace(stringOperationType))
                return "Operation type is empty";
            var operationType = _storage.GetOperationType(stringOperationType);
            string stringVehicleKind = data.vehicleKindCode;
            if (string.IsNullOrWhiteSpace(stringVehicleKind))
                return "Vehicle kind is empty";
            var vehicleKind = _storage.GetVehicleKind(stringVehicleKind);
            dynamic[] actions = ((JArray)data.warehouseActions).ToArray<dynamic>();

            var duration = _processor._GetFillingPlanOperationDuration(operationType, vehicleKind, actions);
            var text = JsonSerializer.Serialize(duration);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        private long _GetHoodManPlanOperationDuration(OperationType operationType, VehicleKind vehicleKind,
            Partner partner, string[] outgoesIds)
        {
            long duration = 0;
            foreach (var outgoId in outgoesIds)
                duration += _storage.GetOutgoPlanTime(outgoId, operationType.IsPallet, partner);

            //15.03.2023 Аввакумов Алексей
            // Убрал из расчета DefaultConstantOperationDuration. Вместо этого берутся данные из БД.
            //duration += DefaultConstantOperationDuration;

            // Значение по умолчанию при максимальной загрузке машины
            var defaultDuration = _storage.GetDuration(operationType, vehicleKind, out int constantDuration, out int VariableDuration);
            duration += constantDuration;
            if (duration > defaultDuration)
                duration = defaultDuration;

            return duration;
        }
        [HttpPost("HoodManPlanOperationDuration")]
        async public Task<string> GetHoodManPlanOperationDuration()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getOperationDuration = _rightsChecker.CheckRight("GET_OPERATION_DURATION", User);
            if (_useAuthentication && (!useYard || !getOperationDuration))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            dynamic? data = await GetInputData(Request);
            if (data == null)
                return "Input data is null!";

            string stringOperationType = data.operationTypeCode;
            if (string.IsNullOrWhiteSpace(stringOperationType))
                return "Operation type is empty";
            var operationType = _storage.GetOperationType(stringOperationType);
            string stringVehicleKind = data.vehicleKindCode;
            if (string.IsNullOrWhiteSpace(stringVehicleKind))
                return "Vehicle kind is empty";
            var vehicleKind = _storage.GetVehicleKind(stringVehicleKind);
            string partnerId = data.partnerId;
            var partner = _storage.GetPartner(partnerId);
            dynamic[] outgoes = ((JArray)data.warehouseActions).ToArray<dynamic>();
            var outgoesIds = new List<string>();
            foreach (var outgo in outgoes)
                outgoesIds.Add(outgo.id);

            var duration = _GetHoodManPlanOperationDuration(operationType, vehicleKind, partner, outgoesIds.ToArray());

            var text = JsonSerializer.Serialize(duration);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpGet("HoodManPlanOutgoDuration")]
        public string GetHoodManPlanOutgoDuration()
        {
            // Алгоритм Башлыкова (HoodMan algorithm).
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getOperationDuration = _rightsChecker.CheckRight("GET_OUTGO_DURATION", User);
            if (_useAuthentication && (!useYard || !getOperationDuration))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? outgoId = Request.Query["outgo_id"];
            if (string.IsNullOrWhiteSpace(outgoId))
                return "Outgo id is empty";
            bool isPallet = Request.Query["is_pallet"] == "1";
            string? partnerId = Request.Query["partner_id"];
            if (string.IsNullOrWhiteSpace(partnerId))
                return "Partner id is empty";
            var partner = _storage.GetPartner(partnerId);
            var duration = _storage.GetOutgoPlanTime(outgoId, isPallet, partner);
            var text = JsonSerializer.Serialize(duration);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpGet("StandartPlanOperationLaborCost")]
        public string GetStandartPlanOperationLaborCost()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getOperationDuration = _rightsChecker.CheckRight("GET_OPERATION_DURATION", User);
            if (_useAuthentication && (!useYard || !getOperationDuration))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? stringOperationType = Request.Query["operation_type"];
            string? stringVehicleKind = Request.Query["vehicle_kind"];

            if (string.IsNullOrWhiteSpace(stringOperationType))
                return "Operation type is empty";
            if (string.IsNullOrWhiteSpace(stringVehicleKind))
                return "Vehicle kind is empty";

            var operationType = _storage.GetOperationType(stringOperationType);
            var vehicleKind = _storage.GetVehicleKind(stringVehicleKind);
            var laborCost = _storage.GetLaborCost(operationType, vehicleKind);
            var linearLaborCost = _storage.GetLinearLaborCost(operationType, vehicleKind);

            dynamic result = new ExpandoObject();
            result.laborCost = laborCost;
            result.linearLaborCost = linearLaborCost;

            var text = JsonSerializer.Serialize(result);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        private double GetFillingPlanOperationK(OperationType operationType, VehicleKind vehicleKind, dynamic[] actions)
        {
            var standardTotalDuration = _storage.GetDuration(operationType, vehicleKind,
                out int standardConstantDuration, out int standardVariableDuration);
            var duration = _processor._GetFillingPlanOperationDuration(operationType, vehicleKind, actions);

            return (double)duration / standardTotalDuration;
        }
        [HttpPost("FillingPlanOperationLaborCost")]
        async public Task<string> GetFillingPlanOperationLaborCost()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getOperationDuration = _rightsChecker.CheckRight("GET_OPERATION_DURATION", User);
            if (_useAuthentication && (!useYard || !getOperationDuration))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            dynamic? data = await GetInputData(Request);
            if (data == null)
                return "Input data is null!";

            string stringOperationType = data.operationTypeCode;
            if (string.IsNullOrWhiteSpace(stringOperationType))
                return "Operation type is empty";
            var operationType = _storage.GetOperationType(stringOperationType);
            string stringVehicleKind = data.vehicleKindCode;
            if (string.IsNullOrWhiteSpace(stringVehicleKind))
                return "Vehicle kind is empty";
            var vehicleKind = _storage.GetVehicleKind(stringVehicleKind);
            dynamic[] actions = ((JArray)data.warehouseActions).ToArray<dynamic>();

            var k = GetFillingPlanOperationK(operationType, vehicleKind, actions);
            var laborCost = _storage.GetLaborCost(operationType, vehicleKind);
            var linearLaborCost = _storage.GetLinearLaborCost(operationType, vehicleKind);

            dynamic result = new ExpandoObject();
            result.laborCost = Math.Round(laborCost * k);
            result.linearLaborCost = Math.Round(linearLaborCost * k);

            var text = JsonSerializer.Serialize(result);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        /// <summary>
        /// Функция определяем ПЛАНОВЫЕ трудозатраты операции двора по Алгоритму Башлыкова (HoodMan algorithm)
        /// Трудозатраты определяются в зависимости от собранных ТЕ в заказах + 
        /// фиксированное время (время постановки машины, ...) * количество работников (по БоссКонтролю или по плану)
        /// </summary>
        /// <param name="outgoes">Массив заказов</param>
        /// <param name="operationType">Тип операции двора</param>
        /// <param name="vehicleKind">Вид машины</param>
        /// <param name="factWorkStartDate">Дата начала погрузки</param>
        /// <param name="factWorkFinishDate">Дата окончания погрузки</param>
        /// <param name="siteCode">Код участка работ</param>
        /// <returns>Плановые трудозатраты</returns>
        private long _GetHoodManPlanOperationLaborCost_Card(OperationType operationType, VehicleKind vehicleKind,
            Partner partner, string[] outgoesIds)
        {
            // Для карточки
            long laborCost = 0;
            var listActions = new List<dynamic>();
            var direction = operationType.Direction.ToString();
            if (direction == "Out")
            {
                //Получаем значение constantDuration
                _storage.GetDuration(operationType, vehicleKind, out int constantDuration, out int VariableDuration);

                // Проходим по заказам (только заказам) и считаем время погрузки каждого заказа по алгоритму Башлыкова
                foreach (var outgoId in outgoesIds)
                    laborCost += _storage.GetOutgoPlanTime(outgoId, operationType.IsPallet, partner);

                // При учёте трудозатрат предполагаем, что выделенные люди простаивают пока происходит постановка/снятие машины.
                // Учитываем это в плановых трудозатратах.                
                //var workersCount = _storage.GetPlanLinearWorkersCount(operationType, vehicleKind);
                //var direction = operationType.Direction;
                //var xar = direction.ToString();
                var workersCount = 1;

                //15.03.2023 Аввакумов Алексей
                // Убрал из расчета DefaultConstantOperationDuration. Вместо этого берутся данные из БД.
                //laborCost += workersCount * DefaultConstantOperationDuration; 
                laborCost += workersCount * constantDuration;

                // 2022.09.22 Буров Михаил.
                // Убрал отчёчку по указанию Алексея Охотникова
                //var defaultLaborCost = _storage.GetLaborCost(operationType, vehicleKind);
                //if (laborCost > defaultLaborCost)
                //    laborCost = defaultLaborCost;
            }
            else
            {
                if (partner.Id == "0004" || partner.Id == "0017" || partner.Id == "0018" || partner.Id == "0025")
                {
                    foreach (var outgoId in outgoesIds)
                        laborCost += _storage.GetIngoPlanTime(outgoId, operationType.IsPallet, partner);
                }
                else if (partner.Id == "0130")
                {
                    laborCost = 46800; //добавлено по заявке #5073
                }
                else
                {
                    foreach (var outgoId in outgoesIds)
                    {
                        dynamic element = new ExpandoObject();
                        element.kind = "I";
                        element.id = outgoId;
                        listActions.Add(element);
                    }

                    dynamic[] arrayActions = listActions.ToArray();
                    var getLaborCost = DoGetFillingPlanOperationLC(operationType, vehicleKind, arrayActions);
                    //#5075 убрано по заявке. laborCost = Convert.ToInt32(getLaborCost.laborCost);
                    laborCost = Convert.ToInt32(getLaborCost.linearLaborCost);
                }
            }

            return laborCost;
        }
        private long _GetHoodManPlanOperationLaborCost_Report(OperationType operationType, VehicleKind vehicleKind,
            Partner partner, DateTime factWorkStartDate, DateTime factWorkFinihDate, string siteCode, string[] outgoesIds)
        {
            // Для отчёта
            long laborCost = 0;
            //Получаем значение constantDuration
            _storage.GetDuration(operationType, vehicleKind, out int constantDuration, out int VariableDuration);
            // Проходим по заказам (только заказам) и считаем время погрузки каждого заказа по алгоритму Башлыкова
            foreach (var outgoId in outgoesIds)
                laborCost += _storage.GetOutgoPlanTime(outgoId, operationType.IsPallet, partner);

            // При учёте трудозатрат предполагаем, что выделенные люди простаивают пока происходит постановка/снятие машины.
            // Учитываем это в плановых трудозатратах.                
            var workersCount = _storage.GetBcWorkersCount(factWorkStartDate, factWorkFinihDate, siteCode);

            //15.03.2023 Аввакумов Алексей
            // Убрал из расчета DefaultConstantOperationDuration. Вместо этого берутся данные из БД.
            //laborCost += workersCount * DefaultConstantOperationDuration;

            laborCost += workersCount * constantDuration;

            // 2022.09.22 Буров Михаил.
            // Убрал отчёчку по указанию Алексея Охотникова
            //var defaultLaborCost = _storage.GetLaborCost(operationType, vehicleKind);
            //if (laborCost > defaultLaborCost)
            //    laborCost = defaultLaborCost;

            return laborCost;
        }
        [HttpPost("HoodManPlanOperationLaborCost")]
        async public Task<string> GetHoodManPlanOperationLaborCost()
        {
            // Расширенный алгоритм Башлыкова
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getOperationDuration = _rightsChecker.CheckRight("GET_OPERATION_DURATION", User);
            if (_useAuthentication && (!useYard || !getOperationDuration))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            dynamic? data = await GetInputData(Request);
            if (data == null)
                return "Input data is null!";

            string stringOperationType = data.operationTypeCode;
            if (string.IsNullOrWhiteSpace(stringOperationType))
                return "Operation type is empty";
            var operationType = _storage.GetOperationType(stringOperationType);
            string stringVehicleKind = data.vehicleKindCode;
            if (string.IsNullOrWhiteSpace(stringVehicleKind))
                return "Vehicle kind is empty";
            var vehicleKind = _storage.GetVehicleKind(stringVehicleKind);
            string partnerId = data.partnerId;
            var partner = _storage.GetPartner(partnerId);
            dynamic[] outgoes = ((JArray)data.outgoes).ToArray<dynamic>();
            long laborCost;
            if (partnerId == "0031") // #5367 04.07.23 добавлено по заявкке Башлыкова из-зи неправильного рассчета норматива
            {
                laborCost = _processor._GetFillingPlanOperationDuration(operationType, vehicleKind, outgoes);
            }
            else
            {
                var outgoesIds = new List<string>();
                foreach (var outgo in outgoes)
                {
                    string id = outgo.id;
                    outgoesIds.Add(id);
                }

                laborCost = _GetHoodManPlanOperationLaborCost_Card(operationType, vehicleKind, partner, outgoesIds.ToArray());
            }

            var text = JsonSerializer.Serialize(laborCost);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpGet("AvailableIntervals")]
        public string GetAvailableIntervals()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getBusyIntervals = _rightsChecker.CheckRight("GET_BUSY_INTERVALS", User);
            if (_useAuthentication && (!useYard || !getBusyIntervals))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? stringDate = Request.Query["date"];
            string? stringOperationType = Request.Query["operation_type"];
            string? stringVehicleKind = Request.Query["vehicle_kind"];

            if (string.IsNullOrWhiteSpace(stringOperationType))
                return "Operation type is empty";
            if (string.IsNullOrWhiteSpace(stringVehicleKind))
                return "Vehicle kind is empty";

            var date = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(stringDate) && !DateTime.TryParse(stringDate.Trim(), out date))
                return $"Wrong date {stringDate}";

            var operationType = _storage.GetOperationType(stringOperationType);
            var vehicleKind = _storage.GetVehicleKind(stringVehicleKind);

            var span = 2;
            var periods = _storage.GetAvalibleIntervals(date, date.AddDays(span), operationType, vehicleKind);
            var intervals = Interval.Union(periods);
            var text = JsonSerializer.Serialize(intervals);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }

        [HttpGet("AvailableGates")]
        public string GetAvailableGates()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getAvalibleGates = _rightsChecker.CheckRight("GET_AVALIBLE_GATES", User);
            if (_useAuthentication && (!useYard || !getAvalibleGates))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }
            string? stringOperationType = Request.Query["operation_type"];
            string? stringVehicleKind = Request.Query["vehicle_kind"];
            string? stringStartDate = Request.Query["start_date"];
            string? finishStringFinishDate = Request.Query["finish_date"];
            string? stringOperationId = Request.Query["operation_id"];

            if (string.IsNullOrWhiteSpace(stringOperationType))
                return "Operation type is empty";
            if (string.IsNullOrWhiteSpace(stringVehicleKind))
                return "Vehicle kind is empty";

            var startDate = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(stringStartDate) && !DateTime.TryParse(stringStartDate.Trim(), out startDate))
                return $"Wrong date {stringStartDate}";
            var finishDate = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(finishStringFinishDate) && !DateTime.TryParse(finishStringFinishDate.Trim(), out finishDate))
                return $"Wrong date {finishStringFinishDate}";
            if (!long.TryParse(stringOperationId, out long operationId))
                return $"Wrong operationId {stringOperationId}";

            var operationType = _storage.GetOperationType(stringOperationType);
            var vehicleKind = _storage.GetVehicleKind(stringVehicleKind);

            var gates = _storage.GetAvalibleGates(operationType, vehicleKind, startDate, finishDate, operationId);
            var dynamicGates = GatesToDynamic(gates);
            var text = JsonSerializer.Serialize(dynamicGates);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }

        [HttpGet("PartnerManager")]
        public string GetPartnerManager()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getPartnermanager = _rightsChecker.CheckRight("GET_PARTNER_MANAGER", User);
            if (_useAuthentication && (!useYard || !getPartnermanager))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? partnerId = Request.Query["partner_id"];
            if (string.IsNullOrWhiteSpace(partnerId))
                return "Partner code is empty!";

            var partner = _storage.GetPartner(partnerId);
            var managers = _storage.GetPartnerManagers(partner);
            if (managers.Length <= 0)
                return $"Partner (client) {partnerId} has no managers!";
            var text = JsonSerializer.Serialize(managers[0]);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpGet("ManagerByPartner")]
        public string GetManagerByPartner()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getPartnermanager = _rightsChecker.CheckRight("GET_MANAGER_BY_PARTNER", User);
            if (_useAuthentication && (!useYard || !getPartnermanager))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }
            var text = "";
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        #endregion

        #region Operations
        [HttpGet("Operation")]
        public string GetOperation()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getOperation = _rightsChecker.CheckRight("GET_OPERATION", User);
            var getOperationByPartner = _rightsChecker.CheckRight("GET_OPERATION_BY_PARTNER", User);
            if (_useAuthentication && (!useYard || !(getOperation || getOperationByPartner)))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? temp = Request.Query["id"];
            if (string.IsNullOrWhiteSpace(temp))
                return "Operation id is empty";

            temp = temp.Trim();
            if (long.TryParse(temp, out long id))
            {
                var operation = _storage.GetOperation(id);

                // TODO: check partner;

                var dynamicOperation = _processor.OperationToDynamic(operation);

                var dynamicActions = new List<dynamic>();
                foreach (var action in operation.GetActions())
                {
                    dynamic dynamicAction = new ExpandoObject();
                    dynamicAction.kind = action.Kind == ActionKind.Outgo ? "O" : "I";
                    dynamicAction.id = action.Id;
                    dynamicActions.Add(dynamicAction);
                }
                var k = GetFillingPlanOperationK(operation.Type, operation.VehicleKind, dynamicActions.ToArray());
                dynamicOperation.planLaborCost = Math.Round(dynamicOperation.planLaborCost * k);
                dynamicOperation.planLinearLaborCost = Math.Round(dynamicOperation.planLinearLaborCost * k);
                var user = RightsChecker.GetUserName(User);
                _logger.LogWarning("Отработал метод Operation заявки {@Заявка}, с номером {Номер заявки}. Пользователь: {Пользователь}", operation, operation.Id, user);
                var text = JsonSerializer.Serialize(dynamicOperation);
                Response.StatusCode = (int)HttpStatusCode.OK;
                return text;
            }
            else
                return $"Wrong operation id {temp}!";
        }
        [HttpGet("Operations")]
        public string GetOperations()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getOperations = _rightsChecker.CheckRight("GET_OPERATIONS", User);
            if (_useAuthentication && (!useYard || !getOperations))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? stringFromDate = Request.Query["from_date"];
            string? stringToDate = Request.Query["to_date"];
            string? stringGateCode = Request.Query["gate_code"];
            string? stringPartnerId = Request.Query["partner_id"];
            string? stringOperationTypeCode = Request.Query["operation_type_code"];
            string? stringVehicleKindCode = Request.Query["vehicle_kind_code"];
            string? stringStateCode = Request.Query["state_code"];
            string? stringId = Request.Query["id"];
            string? stringVehicleNumber = Request.Query["vehicle_number"];
            string? stringDriverName = Request.Query["driver_name"];
            string? stringDriverPhoneNumber = Request.Query["driver_phone_number"];
            string? stringPartnerActionId = Request.Query["partner_action_id"];
            string? stringWorker = Request.Query["worker"];

            var partner = !string.IsNullOrWhiteSpace(stringPartnerId) ? _storage.GetPartner(stringPartnerId) : Partner.Null;
            var gate = !string.IsNullOrWhiteSpace(stringGateCode) ? _storage.GetGate(stringGateCode) : Gate.Null;
            var operationType = !string.IsNullOrWhiteSpace(stringOperationTypeCode) ? _storage.GetOperationType(stringOperationTypeCode) : Null;
            var vehicleKind = !string.IsNullOrWhiteSpace(stringVehicleKindCode) ? _storage.GetVehicleKind(stringVehicleKindCode) : VehicleKind.Null;
            var state = !string.IsNullOrWhiteSpace(stringStateCode) ? _storage.GetOperationState(stringStateCode) : Entity.Null;

            var fromDate = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(stringFromDate))
            {
                stringFromDate = stringFromDate.Trim();
                if (!DateTime.TryParse(stringFromDate, out fromDate))
                    return $"Wrong from date {stringFromDate}";
            }
            var toDate = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(stringToDate))
            {
                stringToDate = stringToDate.Trim();
                if (!DateTime.TryParse(stringToDate, out toDate))
                    return $"Wrong to date {stringToDate}";
            }

            var workers = Array.Empty<Employee>();
            if (!string.IsNullOrWhiteSpace(stringWorker))
                workers = _storage.FindEmployees(stringWorker.Trim());

            var span = 1;
            var operations = _storage.GetOperations(stringId,
                fromDate.Date, toDate.AddDays(span).AddTicks(-1),
                operationType,
                vehicleKind,
                state,
                gate,
                partner,
                string.Empty,
                stringVehicleNumber,
                stringDriverName,
                stringDriverPhoneNumber,
                stringPartnerActionId,
                workers);
            operations = Operation.Filter(operations, partner, operationType, vehicleKind, gate, state,
                stringVehicleNumber ?? string.Empty,
                stringDriverName ?? string.Empty,
                stringPartnerActionId ?? string.Empty);

            var list = new List<Operation>();
            var partnersIds = GetPartnersIds();
            foreach (var operation in operations)
                if (operation.Partners.Length > 0 && partnersIds.Contains(operation.Partners[0].Id) || partnersIds.Count <= 0)
                    list.Add(operation);

            var text = OperationsToJson(list.ToArray());
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpGet("ActiveOperations")]
        public string GetActiveOperations()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            bool useYard = _rightsChecker.CheckRight("USE_YARD", User);
            bool getOperations = _rightsChecker.CheckRight("GET_ACTIVE_OPERATIONS", User);
            if (_useAuthentication && (!useYard || !getOperations))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            Operation[] operations = _storage.GetActiveOperations();
            DateTime now = DateTime.Now;
            foreach (Operation operation in operations)
            {
                string siteCode = _storage.GetSiteCodeByGateCode(operation.Gate.Code);
                bool isGateFree = _storage.CheckGateForFreeBeforeFinishWork(siteCode);
                int siteId = _storage.GetSiteIdBySiteCode(siteCode);
                bool isAllWorkFinished = true;
                if (operation.FactWorkStartDate != DateTime.MinValue)
                {
                    foreach (var worker in operation.OperationWorkers)
                    {
                        bool isWorkFinished = _storage.CheckWorkerLeaveSite(worker.Worker.Id, siteId, operation.PartnersIds, operation.FactWorkStartDate);
                        if (!isWorkFinished)
                        {
                            isAllWorkFinished = false;
                            break;
                        }
                    }
                }                

                DateTime factWorkNotFinishDate = DateTime.MinValue;
                TimeSpan timeToStartWork = now - operation.FactWorkStartDate;
                TimeSpan maxTimeSpanBeforeStartWork = TimeSpan.FromMinutes(10);
                bool mainConditions = timeToStartWork > maxTimeSpanBeforeStartWork
                          && operation.FactWorkFinishDate == factWorkNotFinishDate
                          && operation.State.Code != "STARTED";

                if (mainConditions && isGateFree)
                {
                    operation.Gate.State = operation.Gate.State == Gate.GateState.Busy || isAllWorkFinished
                                           ? Gate.GateState.Free
                                           : Gate.GateState.Reserved;
                }
                else if (mainConditions && !isGateFree)
                {
                    operation.Gate.State = Gate.GateState.Busy;
                }
                else
                {
                    operation.Gate.State = Gate.GateState.Free;
                }

            }
            var text = OperationsToJson(operations);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpGet("GuardOperations")]
        public string GetGuardOperations()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getOperations = _rightsChecker.CheckRight("GET_GUARD_OPERATIONS", User);
            if (_useAuthentication && (!useYard || !getOperations))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            var operations = _storage.GetGuardOperations();
            var text = OperationsToJson(operations);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpPut("CreateOperation")]
        async public Task<string> CreateOperation()
        {
            dynamic? data = await GetInputData(Request);
            if (data == null)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Input data is null!";
            }                
            string operation = _processor.CreateOperation(data, User, out int statusCode, out string response);
            if (statusCode == (int)HttpStatusCode.Created)
            {
                response = operation;
                Response.StatusCode = statusCode;
                return response;
            }
            else
            {
                Response.StatusCode = statusCode;
                return response;
            }
        }
        [HttpPut("CreateConfirmOperation")]
        async public Task<string> CreateConfirmOperation()
        {
            dynamic? data = await GetInputData(Request);
            if (data == null)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Input data is null!";
            }

            string operation = _processor.CreateConfirmOperation(data, User, out int statusCode, out string response);
            if (statusCode == (int)HttpStatusCode.Created)
            {
                response = operation;
                Response.StatusCode = statusCode;
                return response;
            }
            else
            {
                Response.StatusCode = statusCode;
                return response;
            }
        }
        [HttpPut("CreateOperationByPartner")]
        async public Task<string> CreateOperationByPartner()
        {
            dynamic? data = await GetInputData(Request);
            if (data == null)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Input data is null!";
            }

            string operation = _processor.CreateOperationByPartner(data, User, out int statusCode, out string response);
            if (statusCode == (int)HttpStatusCode.Created)
            {
                response = operation;
                Response.StatusCode = statusCode;
                return response;
            }
            else
            {
                Response.StatusCode = statusCode;
                return response;
            }            
        }
        [HttpPost("EditOperation")]
        async public Task<string> EditOperation()
        {            
            dynamic? data = await GetInputData(Request);
            if (data == null)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Input data is null!";
            }

            string operation = _processor.EditOperation(data, User, out int statusCode, out string response);
            if (statusCode == (int)HttpStatusCode.OK)
            {
                response = operation;
                Response.StatusCode = statusCode;
                return response;
            }
            else
            {
                Response.StatusCode = statusCode;
                return response;
            }           
        }
        [HttpPost("EditOperationByPartner")]
        async public Task<string> EditOperationByPartner()
        {
            dynamic? data = await GetInputData(Request);
            if (data == null)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Input data is null!";
            }

            string operation = _processor.EditOperationByPartner(data, User, out int statusCode, out string response);
            if (statusCode == (int)HttpStatusCode.OK)
            {
                response = operation;
                Response.StatusCode = statusCode;
                return response;
            }
            else
            {
                Response.StatusCode = statusCode;
                return response;
            }            
        }
        [HttpPatch("SetOperationNote")]
        async public Task<string> SetOperationNote()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var editOperationNote = _rightsChecker.CheckRight("EDIT_OPERATION_NOTE", User);
            if (_useAuthentication && (!useYard || !editOperationNote))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            dynamic? data = await GetInputData(Request);
            if (data == null)
                return "Input data is null!";

            string temp = data.id;
            if (string.IsNullOrWhiteSpace(temp))
                return "Operation id is empty";

            if (!long.TryParse(temp, out long id))
                return $"Wrong operation id {temp}";

            var operation = _storage.GetOperation(id);
            operation.Note = data.note;
            var user = RightsChecker.GetUserName(User);
            _storage.SetOperationNote(operation, RightsChecker.GetUserName(User));
            string editStatus = "(записано примечение)";
            _logger.LogWarning("Записано примечание в заявку {@Заявка}, с номером {Номер заявки}. Пользователь: {Пользователь}", operation, operation.Id, user);
            _storage.AddOperationLog(operation, editStatus, user); //логирование в бд
            var text = _processor.OperationToJson(operation);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpPatch("SetOperationGate")]
        async public Task<string> SetOperationGate()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var setOperationGate = _rightsChecker.CheckRight("SET_OPERATION_GATE", User);
            if (_useAuthentication && (!useYard || !setOperationGate))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            dynamic? data = await GetInputData(Request);
            if (data == null)
                return "Input data is null!";

            string stringId = data.id;
            string stringGateCode = data.gateCode;
            if (string.IsNullOrWhiteSpace(stringId))
                return "Operation id is empty";

            if (!long.TryParse(stringId, out long id))
                return $"Wrong operation id {stringId}";

            Gate? gate = null;
            if (!string.IsNullOrWhiteSpace(stringGateCode))
                try
                {
                    gate = _storage.GetGate(stringGateCode.Trim().ToUpper());
                }
                catch
                {
                    gate = null;
                }

            var operation = _storage.GetOperation(id);
            var user = RightsChecker.GetUserName(User);

            // Эту строку (по поиску сотрудника по пользователю AD) перенёс сюда, что бы не найся сотрудника (ответственного)
            // нельзя было присвоить ворота
            var employee = _storage.GetEmployeeByAdLogin(user);
            _storage.SetOperationGate(operation, gate, user);
            _storage.SetResponsibleOfficer(operation, employee);
            _logger.LogWarning("Назначены ворота в заявку {@Заявка}, с номером {Номер заявки}. Пользователь: {Пользователь}", operation, operation.Id, user);
            _storage.AddOperationLog(operation, "Подтверждена, назначены ворота", user); //логирование в бд

            var text = _processor.OperationToJson(operation);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpPatch("CancelOperationGate")]
        async public Task<string> CancelOperationGate()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var cancelOperationGate = _rightsChecker.CheckRight("CANCEL_OPERATION_GATE", User);
            if (_useAuthentication && (!useYard || !cancelOperationGate))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            dynamic? data = await GetInputData(Request);
            if (data == null)
                return "Input data is null!";

            string stringId = data.id;
            if (string.IsNullOrWhiteSpace(stringId))
                return "Operation id is empty";

            if (!long.TryParse(stringId, out long id))
                return $"Wrong operation id {stringId}";

            var operation = _storage.GetOperation(id);
            var user = RightsChecker.GetUserName(User);
            _storage.CancelOperationGate(operation, RightsChecker.GetUserName(User));
            _logger.LogWarning("Отменены ворота в заявку {@Заявка}, с номером {Номер заявки}. Пользователь: {Пользователь}", operation, operation.Id, user);
            _storage.AddOperationLog(operation, "CancelOperationGate", user); //логирование в бд
            var text = _processor.OperationToJson(operation);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpPatch("StartOperation")]
        async public Task<string> StartOperation()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var startOperation = _rightsChecker.CheckRight("START_OPERATION", User);
            if (_useAuthentication && (!useYard || !startOperation))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            dynamic? data = await GetInputData(Request);
            if (data == null)
                return "Input data is null!";

            string stringId = data.id;
            if (string.IsNullOrWhiteSpace(stringId))
                return "Operation id is empty";

            if (!long.TryParse(stringId, out long id))
                return $"Wrong operation id {stringId}";

            var operation = _storage.GetOperation(id);
            var user = RightsChecker.GetUserName(User);
            _storage.StartOperation(operation, RightsChecker.GetUserName(User));
            _logger.LogWarning("Прибыла в заявке {@Заявка}, с номером {Номер заявки}. Пользователь: {Пользователь}", operation, operation.Id, user);
            _storage.AddOperationLog(operation, "Прибыла", user); //логирование в бд
            var text = _processor.OperationToJson(operation);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpPatch("StartOperationWork")]
        async public Task<string> StartOperationWork()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var startOperationWork = _rightsChecker.CheckRight("START_OPERATION_WORK", User);
            if (_useAuthentication && (!useYard || !startOperationWork))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            dynamic? data = await GetInputData(Request);
            if (data == null)
                return "Input data is null!";

            string stringId = data.id;
            if (string.IsNullOrWhiteSpace(stringId))
                return "Operation id is empty";

            if (!long.TryParse(stringId, out long id))
                return $"Wrong operation id {stringId}";

            var operation = _storage.GetOperation(id);
            var user = RightsChecker.GetUserName(User);
            _storage.StartOperationWork(operation, RightsChecker.GetUserName(User));
            _logger.LogWarning("Работа начата в заявке {@Заявка}, с номером {Номер заявки}. Пользователь: {Пользователь}", operation, operation.Id, user);
            _storage.AddOperationLog(operation, "Работа начата", user); //логирование в бд
            var text = _processor.OperationToJson(operation);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpPatch("FinishOperationWork")]
        async public Task<string> FinishOperationWork()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var finishOperationWork = _rightsChecker.CheckRight("FINISH_OPERATION_WORK", User);
            if (_useAuthentication && (!useYard || !finishOperationWork))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            dynamic? data = await GetInputData(Request);
            if (data == null)
                return "Input data is null!";

            string stringId = data.id;
            if (string.IsNullOrWhiteSpace(stringId))
                return "Operation id is empty";

            if (!long.TryParse(stringId, out long id))
                return $"Wrong operation id {stringId}";

            var operation = _storage.GetOperation(id);
            string siteCode = _storage.GetSiteCodeByGateCode(operation.Gate.Code);
            bool isGateFree = _storage.CheckGateForFreeBeforeFinishWork(siteCode);
            var user = RightsChecker.GetUserName(User);
            string text = String.Empty;
            if (isGateFree)
            {
                _storage.FinishOperationWork(operation, RightsChecker.GetUserName(User));
                _logger.LogWarning("Работа завершена в заявке {@Заявка}, с номером {Номер заявки}. Пользователь: {Пользователь}", operation, operation.Id, user);
                _storage.AddOperationLog(operation, "Работа завершена", user); //логирование в бд
                text = _processor.OperationToJson(operation);
                Response.StatusCode = (int)HttpStatusCode.OK;
            } else
            {
                Response.StatusCode = (int)HttpStatusCode.PreconditionFailed;
                text = "Precondition Failed. Сотрудникам сначала нужно покинуть ворота.";
            }
            return text;
        }
        [HttpPatch("FinishOperation")]
        async public Task<string> FinishOperation()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var startOperation = _rightsChecker.CheckRight("FINISH_OPERATION", User);
            if (_useAuthentication && (!useYard || !startOperation))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            dynamic? data = await GetInputData(Request);
            if (data == null)
                return "Input data is null!";

            string stringId = data.id;
            if (string.IsNullOrWhiteSpace(stringId))
                return "Operation id is empty";

            if (!long.TryParse(stringId, out long id))
                return $"Wrong operation id {stringId}";

            var operation = _storage.GetOperation(id);
            var user = RightsChecker.GetUserName(User);
            //var gate = _storage.GetGate(stringGateCode);
            _storage.FinishOperation(operation, RightsChecker.GetUserName(User));
            _logger.LogWarning("Убыла в заявке {@Заявка}, с номером {Номер заявки}. Пользователь: {Пользователь}", operation, operation.Id, user);
            _storage.AddOperationLog(operation, "Убыла", user); //логирование в бд
            var text = _processor.OperationToJson(operation);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpPatch("CancelOperation")]
        async public Task<string> CancelOperation()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var cancelOperation = _rightsChecker.CheckRight("CANCEL_OPERATION", User);
            if (_useAuthentication && (!useYard || !cancelOperation))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            dynamic? data = await GetInputData(Request);
            if (data == null)
                return "Input data is null!";

            string stringId = data.id;
            if (string.IsNullOrWhiteSpace(stringId))
                return "Operation id is empty";

            if (!long.TryParse(stringId, out long id))
                return $"Wrong operation id {stringId}";

            var operation = _storage.GetOperation(id);
            var user = RightsChecker.GetUserName(User);
            _storage.CancelOperation(operation, RightsChecker.GetUserName(User));
            _logger.LogWarning("Отменена в заявке {@Заявка}, с номером {Номер заявки}. Пользователь: {Пользователь}", operation, operation.Id, user);
            _storage.AddOperationLog(operation, "Отменена", user); //логирование в бд
            var text = _processor.OperationToJson(operation);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpDelete("DeleteOperation")]
        async public Task<string> DeleteOperation()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var deleteOperation = _rightsChecker.CheckRight("DELETE_OPERATION", User);
            if (_useAuthentication && (!useYard || !deleteOperation))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            dynamic? data = await GetInputData(Request);
            if (data == null)
                return "Input data is null!";

            string stringId = data.id;
            if (string.IsNullOrWhiteSpace(stringId))
                return "Operation id is empty";

            if (!long.TryParse(stringId, out long id))
                return $"Wrong operation id {stringId}";

            var operation = _storage.GetOperation(id);
            var user = RightsChecker.GetUserName(User);
            _storage.DeleteOperation(operation, RightsChecker.GetUserName(User));
            _logger.LogWarning("Удалена заявка {@Заявка}, с номером {Номер заявки}. Пользователь: {Пользователь}", operation, operation.Id, user);
            _storage.AddOperationLog(operation, "Удалена", user); //логирование в бд
            var text = _processor.OperationToJson(operation);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        #endregion

        #region Warehouse actions
        private static dynamic WarehouseActionToDynamic(WarehouseAction action)
        {
            dynamic result = new ExpandoObject();
            result.kind = action.Kind == ActionKind.Outgo ? "O" : "I";
            result.id = action.Id;
            result.partnerActionId = action.PartnerActionId;
            result.partnerId = action.Partner.Id;
            result.partnerName = action.Partner.Name;
            result.planActionDate = action.PlanActionDate;
            result.insertDate = action.InsertDate;

            return result;
        }
        private static dynamic[] WarehouseActionsToDynamic(WarehouseAction[] actions)
        {
            var result = new List<dynamic>();
            foreach (var action in actions)
                result.Add(WarehouseActionToDynamic(action));
            return result.ToArray();
        }
        [HttpGet("WarehouseActions")]
        public string GetWarehouseActions()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getWarehouseActions = _rightsChecker.CheckRight("GET_WAREHOUSE_ACTIONS", User);
            if (_useAuthentication && (!useYard || !getWarehouseActions))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? temp = Request.Query["kind"];
            ActionKind actionKind;
            if (!string.IsNullOrWhiteSpace(temp))
            {
                temp = temp.Trim().ToUpper();
                if (temp == "O")
                    actionKind = ActionKind.Outgo;
                else
                    actionKind = ActionKind.Income;
            }
            else
                return "Action kind id is empty!";
            temp = Request.Query["pattern"];
            var pattern = temp;

            temp = Request.Query["partner_id"];
            Partner? partner = null;
            if (!string.IsNullOrWhiteSpace(temp))
            {
                temp = temp.Trim().ToUpper();
                if (temp == "AUTO")
                    temp = _rightsChecker.GetPartnerIdByUser(RightsChecker.GetUserName(User));
                partner = _storage.GetPartner(temp);
            }

            var isPartnerUser = _rightsChecker.CheckRight("IS_PARTNER_USER", User);

            var actions = _storage.FindWarehouseActions(actionKind, partner, pattern ?? string.Empty, isPartnerUser);
            var objects = WarehouseActionsToDynamic(actions);

            var text = JsonSerializer.Serialize(objects);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpGet("SelectedWarehouseActions")]
        public string GetSelectedWarehouseActions()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getWarehouseActions = _rightsChecker.CheckRight("GET_SELECTED_WAREHOUSE_ACTIONS", User);
            if (_useAuthentication && (!useYard || !getWarehouseActions))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? temp = Request.Query["operation_id"];
            if (string.IsNullOrWhiteSpace(temp))
                return "Operation id is empty";
            if (!long.TryParse(temp, out long id))
                return $"Wrong operation id {temp}";

            var operation = _storage.GetOperation(id);
            var actions = operation.GetActions();
            var objects = WarehouseActionsToDynamic(actions);
            var user = RightsChecker.GetUserName(User);
            _logger.LogWarning("Отработал метод GetSelectedWarehouseActions заявки {@Заявка}, с номером {Номер заявки}. Пользователь: {Пользователь}", operation, operation.Id, user);
            var text = JsonSerializer.Serialize(objects);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [Obsolete("Метод устарел!", false)]
        [HttpPatch("AddOperationWarehouseAction")]
        async public Task<string> AddOperationWarehouseAction()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var manageWarehouseActions = _rightsChecker.CheckRight("MANAGE_WAREHOUSE_ACTIONS", User);
            if (_useAuthentication && (!useYard || !manageWarehouseActions))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            dynamic? data = await GetInputData(Request);
            if (data == null)
                return "Input data is null!";

            string stringOperationId = data.operationId;
            string stringActionKind = data.actionKind;
            string stringActionId = data.actionId;

            if (string.IsNullOrWhiteSpace(stringOperationId))
                return "Operation id is empty!";
            if (stringActionKind != "I" && stringActionKind != "O")
                return $"Wrong action kind {stringActionKind}!";
            if (string.IsNullOrWhiteSpace(stringActionId))
                return "Action id is empty!";

            if (!long.TryParse(stringOperationId, out long operationId))
                return $"Wrong operation id {stringOperationId}!";

            var operation = _storage.GetOperation(operationId);
            var action = _storage.GetWarehouseAction(stringActionKind == "O" ? ActionKind.Outgo : ActionKind.Income, stringActionId);
            var user = RightsChecker.GetUserName(User);
            _storage.AddAction(operation, action);
            var operationForLog = _storage.GetOperation(operationId);
            _logger.LogWarning("Отработал метод AddOperationWarehouseAction заявки {@Заявка}, с номером {Номер заявки}. Добавлен заказ {Заказ}. Пользователь: {Пользователь}", operationForLog, operation.Id, action.Code, user);
            var text = JsonSerializer.Serialize(operation);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpPatch("RemoveOperationWarehouseAction")]
        async public Task<string> RemoveOperationWarehouseAction()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var manageWarehouseActions = _rightsChecker.CheckRight("MANAGE_WAREHOUSE_ACTIONS", User);
            if (_useAuthentication && (!useYard || !manageWarehouseActions))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            dynamic? data = await GetInputData(Request);
            if (data == null)
                return "Input data is null!";

            string stringOperationId = data.operationId;
            string stringActionKind = data.actionKind;
            string stringActionId = data.actionId;


            if (string.IsNullOrWhiteSpace(stringOperationId))
                return "Operation id is empty!";
            if (string.IsNullOrWhiteSpace(stringActionKind))
                return "Action kind is empty!";
            if (string.IsNullOrWhiteSpace(stringActionId))
                return "Action id is empty!";

            if (!long.TryParse(stringOperationId, out long operationId))
                return $"Wrong operation id {stringOperationId}!";

            var operation = _storage.GetOperation(operationId);
            var user = RightsChecker.GetUserName(User);
            _logger.LogWarning("Отработал метод RemoveOperationWarehouseAction заявки {@Заявка}, с номером {Номер заявки}. Пользователь: {Пользователь}", operation, operation.Id, user);
            var text = JsonSerializer.Serialize(operation);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpPatch("RemoveOperationWarehouseActions")]
        async public Task<string> RemoveOperationWarehouseActions()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var manageWarehouseActions = _rightsChecker.CheckRight("MANAGE_WAREHOUSE_ACTIONS", User);
            if (_useAuthentication && (!useYard || !manageWarehouseActions))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            dynamic? data = await GetInputData(Request);
            if (data == null)
                return "Input data is null!";

            string stringOperationId = data.operationId;

            if (string.IsNullOrWhiteSpace(stringOperationId))
                return "Operation id is empty!";

            if (!long.TryParse(stringOperationId, out long operationId))
                return $"Wrong operation id {stringOperationId}!";

            var operation = _storage.GetOperation(operationId);
            var user = RightsChecker.GetUserName(User);
            _storage.RemoveActions(operation);
            _logger.LogWarning("Отработал метод RemoveOperationWarehouseActions заявки {@Заявка}, с номером {Номер заявки}. Пользователь: {Пользователь}", operation, operation.Id, user);
            var text = JsonSerializer.Serialize(operation);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        #endregion

        #region Documents
        [HttpPut("UploadDocument")]
        async public Task<string> AddDocument()
        {
            var file = Request.Form.Files[0];
            var reader = file.OpenReadStream();

            //Request.EnableBuffering();
            //var body = Request.Body;
            //body.Seek(0, SeekOrigin.Begin);
            //StreamReader reader = new StreamReader(body);

            StreamWriter output = new StreamWriter("C:\\test.jpg");
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                output.BaseStream.Write(buffer, 0, len);
            output.Close();

            var text = "";
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        async public Task<string> RemoveDocument()
        {
            await Task.Delay(0);
            return string.Empty;
        }
        #endregion

        #region Operation workers
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
        private static dynamic OperationWorkerToDynamic(Operation.OperationWorker operationWorker)
        {
            dynamic result = new ExpandoObject();
            result.id = operationWorker.Worker.Id;
            result.surname = operationWorker.Worker.Surname;
            result.firstName = operationWorker.Worker.FirstName;
            result.patronymic = operationWorker.Worker.Patronymic;
            result.shortName = operationWorker.Worker.ShortName;
            result.fullName = operationWorker.Worker.FullName;
            result.positionId = operationWorker.Position.Id;
            result.positionCode = operationWorker.Position.Code;
            result.positionName = operationWorker.Position.Name;
            result.positionShortName = operationWorker.Position.ShortName;
            return result;
        }
        private static dynamic[] OperationWorkersToDynamic(Operation.OperationWorker[] operationWorkers)
        {
            var result = new List<dynamic>();
            foreach (var operationWorker in operationWorkers)
                result.Add(YardController.OperationWorkerToDynamic(operationWorker));
            return result.ToArray();
        }
        [HttpGet("PositionWorkers")]
        public string GetPositionWorkers()
        {
            try
            {
                var namePattern = Request.Query["name_pattern"].ToString();
                var positionCode = Request.Query["position_code"].ToString();
                var objects = _processor.GetPositionWorkers(namePattern, positionCode, User,
                    out int statusCode, out string response);
                if (statusCode == (int)HttpStatusCode.OK)
                {
                    response = JsonSerializer.Serialize(objects);
                    Response.StatusCode = (int)HttpStatusCode.OK;
                    return response;
                }
                else
                {
                    Response.StatusCode = statusCode;
                    return response;
                }
            }
            catch (Exception exception)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return exception.Message;
            }
        }
        [HttpGet("LinearWorkers")]
        public string GetLinearWorkers()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getWorkers = _rightsChecker.CheckRight("GET_LINEAR_WORKERS", User);
            if (_useAuthentication && (!useYard || !getWorkers))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            var workers = _storage.GetLinearEmployees();

            var objects = new List<dynamic>();
            foreach (var worker in workers)
                objects.Add(EmployeeToDynamic(worker, _storage.IsWorkerBusy(worker)));

            var text = JsonSerializer.Serialize(objects.ToArray());
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpGet("LinearFreeWorkers")]
        public string GetLinearFreeWorkers()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getWorkers = _rightsChecker.CheckRight("GET_LINEAR_FREE_WORKERS", User);
            if (_useAuthentication && (!useYard || !getWorkers))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            var workers = _storage.GetLinearEmployees();
            var temp = new List<Employee>();
            foreach (var worker in workers)
                if (!_storage.IsWorkerBusy(worker))
                    temp.Add(worker);
            var objects = EmployeesToDynamic(temp.ToArray());
            var text = JsonSerializer.Serialize(objects);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpGet("AllWorkers")]
        public string GetAllWorkers()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getWorkers = _rightsChecker.CheckRight("GET_ALL_WORKERS", User);
            if (_useAuthentication && (!useYard || !getWorkers))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            var workers = _storage.GetEmployees();

            var objects = new List<dynamic>();
            foreach (var worker in workers)
                objects.Add(EmployeeToDynamic(worker, _storage.IsWorkerBusy(worker)));

            var text = JsonSerializer.Serialize(objects.ToArray());
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpGet("OperationWorkers")]
        public string GetOperationWorkers()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getWorkers = _rightsChecker.CheckRight("GET_OPERATION_WORKERS", User);
            if (_useAuthentication && (!useYard || !getWorkers))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? temp = Request.Query["operation_id"];
            if (string.IsNullOrWhiteSpace(temp))
                return "Operation id is empty";
            if (!long.TryParse(temp, out long id))
                return $"Wrong operation id {temp}";

            var operation = _storage.GetOperation(id);
            var operationWorkers = _storage.GetOperationWorkers(operation);
            var objects = OperationWorkersToDynamic(operationWorkers);
            var text = JsonSerializer.Serialize(objects);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpPatch("AddOperationWorker")]
        async public Task<string> AddOperationWorker()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var manageOperationWorkers = _rightsChecker.CheckRight("MANAGE_OPERATION_WORKERS", User);
            if (_useAuthentication && (!useYard || !manageOperationWorkers))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            dynamic? data = await GetInputData(Request);
            if (data == null)
                return "Input data is null!";

            string stringOperationId = data.operationId;
            string stringWorkerId = data.workerId;
            string stringPositionCode = data.positionCode;

            if (string.IsNullOrWhiteSpace(stringOperationId))
                return "Operation id is empty!";
            if (string.IsNullOrWhiteSpace(stringWorkerId))
                return "Worker id is empty!";
            if (string.IsNullOrWhiteSpace(stringPositionCode))
                return "Position code is empty!";

            if (!long.TryParse(stringOperationId, out long operationId))
                return $"Wrong operation id {stringOperationId}!";
            if (!int.TryParse(stringWorkerId, out int workerId))
                return $"Wrong worker id {stringWorkerId}!";

            var operation = _storage.GetOperation(operationId);
            var worker = _storage.GetEmployee(workerId);
            var position = _storage.GetPosition(stringPositionCode);
            bool isBusy = _storage.IsWorkerBusy(worker);
            if (isBusy)
            {
                Response.StatusCode = 409;
                return "Данный сотрудник уже занят! Выберите другого. Для просмотра актуального перечня незанятых сотрудников обновите страницу.";
            }

            _storage.AddOperationWorker(operation, worker, position, 0.0, MeasureUnit.Null);
            var text = JsonSerializer.Serialize(operation);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpPatch("RemoveOperationWorker")]
        async public Task<string> RemoveOperationWorker()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var manageOperationWorkers = _rightsChecker.CheckRight("MANAGE_OPERATION_WORKERS", User);
            if (_useAuthentication && (!useYard || !manageOperationWorkers))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            dynamic? data = await GetInputData(Request);
            if (data == null)
                return "Input data is null!";

            string stringOperationId = data.operationId;
            string stringWorkerId = data.workerId;
            string stringPositionCode = data.positionCode;

            if (string.IsNullOrWhiteSpace(stringOperationId))
                return "Operation id is empty!";
            if (string.IsNullOrWhiteSpace(stringWorkerId))
                return "Worker id is empty!";
            if (string.IsNullOrWhiteSpace(stringPositionCode))
                return "Position code is empty!";

            if (!long.TryParse(stringOperationId, out long operationId))
                return $"Wrong operation id {stringOperationId}!";
            if (!int.TryParse(stringWorkerId, out int workerId))
                return $"Wrong worker id {stringWorkerId}!";

            var operation = _storage.GetOperation(operationId);
            var worker = _storage.GetEmployee(workerId);
            var position = _storage.GetPosition(stringPositionCode);

            _storage.RemoveOperationWorker(operation, worker, position);
            var text = JsonSerializer.Serialize(operation);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpPatch("AllocateOperationWorkers")]
        async public Task<string> AllocateOperationWorkers()
        {
            try
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                dynamic? data = await GetInputData(Request);
                var operation = _processor.AllocateOperationWorkers(data, User, out int statusCode, out string response);
                if (statusCode == (int)HttpStatusCode.OK)
                {
                    response = _processor.OperationToJson(operation);
                    Response.StatusCode = (int)HttpStatusCode.OK;
                    return response;
                }
                else
                {
                    Response.StatusCode = statusCode;
                    return response;
                }
            }
            catch (Exception exception)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return exception.Message;
            }
        }
        [HttpGet("BossControlWorks")]
        public string GetBossControlWorks()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var manageOperationWorkers = _rightsChecker.CheckRight("GET_BOSSCONTROL_WORKS", User);
            if (_useAuthentication && (!useYard || !manageOperationWorkers))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? temp = Request.Query["operation_id"];
            if (string.IsNullOrWhiteSpace(temp))
                return "Operation id is empty!";

            if (!long.TryParse(temp, out long operationId))
                return $"Wrong operation id {temp}!";

            var operation = _storage.GetOperation(operationId);
            var siteCode = _storage.GetSiteCodeByGateCode(operation.Gate.Code);
            var works = _storage.GetBossControlWorks(operation.FactWorkStartDate, operation.FactWorkFinishDate, siteCode);

            var text = JsonSerializer.Serialize(works);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        #endregion

        #region Reporting
        [HttpGet("OperationalReport")]
        public string GetOperationalReport()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getData = _rightsChecker.CheckRight("GET_OPERATIONAL_REPORT", User);
            if (_useAuthentication && (!useYard || !getData))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? stringDate = Request.Query["date"];
            var date = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(stringDate))
            {
                stringDate = stringDate.Trim();
                if (!DateTime.TryParse(stringDate, out date))
                    return $"Wrong date {stringDate}";
            }
            string? shift = Request.Query["shift"];
            if (string.IsNullOrWhiteSpace(shift))
                shift = "day";
            shift = shift.Trim().ToLower();
            if (shift != "day" && shift != "night")
                return $"Unknown shift {shift}";

            var fromDate = shift == "day" ? date.Date.AddHours(9) : date.Date.AddHours(21);
            var toDate = fromDate.AddHours(12);

            var operations = _storage.GetOperationalReportData(fromDate, toDate);
            var data = new List<dynamic>();
            foreach (var operation in operations)
            {
                var actions = operation.GetActions();
                foreach (var action in actions)
                {
                    var lines = _storage.GetWarehouseActionLines(action.Kind, action.Id);
                    action.AddLines(lines);
                }
                dynamic element = new ExpandoObject();
                element.id = operation.Id;
                element.typeCode = operation.Type.Code;
                element.typeName = operation.Type.Name;
                element.typeShortName = operation.Type.ShortName;
                element.vehicleKindCode = operation.VehicleKind.Code;
                element.vehicleKindName = operation.VehicleKind.Name;
                element.vehicleKindShortName = operation.VehicleKind.ShortName;
                element.stateCode = operation.State.Code;
                element.stateName = operation.State.Name;
                element.stateShortName = operation.State.ShortName;
                element.startDate = operation.StartDate;
                element.planWorkStartDate = operation.PlanWorkStartDate;
                element.factWorkStartDate = operation.FactWorkStartDate;
                element.planWorkFinishDate = operation.PlanWorkFinishDate;
                element.factWorkFinishDate = operation.FactWorkFinishDate;
                element.finishDate = operation.FinishDate;
                element.planDuration = operation.PlanDuration;
                element.factDuration = operation.FactDuration;
                element.presenceTime = operation.PresenceTime;

                element.planLinearLaborCost = operation.PlanLinearLaborCost;
                element.planLaborCost = operation.PlanLaborCost;
                element.factLaborCost = operation.FactLaborCost;

                element.warehouseActionsCount = operation.ActionsCount;
                element.partnersIds = operation.PartnersIds;
                element.partnersNames = operation.PartnersNames;

                element.workersCount = operation.WorkersCount;
                element.planNetWeight = operation.PlanNetWeight;
                element.planGrossWeight = operation.PlanGrossWeight;
                element.factNetWeight = operation.FactNetWeight;
                element.factGrossWeight = operation.FactGrossWeight;
                element.planVolume = operation.PlanVolume;
                element.factVolume = operation.FactVolume;

                element.planPalletsCount = operation.PlanPalletsCount;
                element.planBoxesCount = operation.PlanBoxesCount;
                element.factPalletsCount = operation.FactPalletsCount;
                element.factBoxesCount = operation.FactBoxesCount;

                element.tusCount = operation.TusCount;

                data.Add(element);
            }

            var text = JsonSerializer.Serialize(data);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpGet("DispatcherReport")]
        public string GetDispatcherReport()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getData = _rightsChecker.CheckRight("GET_DISPATCHER_REPORT", User);
            if (_useAuthentication && (!useYard || !getData))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? temp = Request.Query["from_date"];
            var fromDate = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(temp))
            {
                temp = temp.Trim();
                if (!DateTime.TryParse(temp, out fromDate))
                    return $"Wrong from date {temp}";
            }
            temp = Request.Query["to_date"];
            var toDate = DateTime.Today.AddDays(1);
            if (!string.IsNullOrWhiteSpace(temp))
            {
                temp = temp.Trim();
                if (!DateTime.TryParse(temp, out toDate))
                    return $"Wrong to date {temp}";
                toDate = toDate.AddDays(1);
            }
            if (fromDate >= toDate)
                throw new ArgumentException("Wrong dates!");
            if ((toDate - fromDate).TotalDays > MaxReportPeriodDays)
                toDate = fromDate.AddDays(MaxReportPeriodDays);

            var data = new Dictionary<string, dynamic>();

            void addElement(string key, DateTime date)
            {
                if (data.ContainsKey(key))
                    return;
                dynamic element = new ExpandoObject();
                element.date = date;
                element.stringDate = date.ToString("dd.MM.yyyy");
                element.day = date.ToString("dd.MM");

                var dayOfWeek2 = DateTimeFormatInfo.CurrentInfo.GetAbbreviatedDayName(date.DayOfWeek);
                element.dayOfWeek = dayOfWeek2;
                element.isHoliday = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;

                element.key = key;
                element.nextDayOperations = 0;
                element.dayToDayOperations = 0;
                element.dayOperations = 0;
                element.dayToDayOperationsPercent = 0.0;
                element.deletedOperations = 0;
                element.idleOperations = 0;
                element.overPlanIncomes = 0;
                element.overPlanOutgoes = 0;
                element.palletIncomes = 0;
                element.boxIncomes = 0;
                element.palletOutgoues = 0;
                element.boxOutgoes = 0;
                element.gates1_6Operations = 0;
                element.gate7Operations = 0;
                element.gates8_16Operations = 0;
                data.Add(key, element);
            }

            var operations = _storage.GetDispatcherReportData(fromDate, toDate);
            foreach (var operation in operations)
            {
                // Смотрим удалённые операции (те, которые планировались на указанную дату, но были удалены).
                var key = operation.PlanWorkFinishDate.ToString("yyyy.MM.dd");
                addElement(key, operation.PlanWorkFinishDate.Date);
                if (operation.IsDeleted)
                    data[key].deletedOperations++;
                else if (operation.State.Code != "CREATED" && operation.State.Code != "CANCELED")
                {
                    // Все заявки, запланированные на указанный день.
                    data[key].dayOperations++;

                    // Опоздавшие машины (на сутки) или не приехавшие.
                    if (operation.PlanWorkFinishDate.Date < operation.FactWorkFinishDate.Date
                        || operation.FactWorkFinishDate <= DateTime.MinValue)
                        data[key].idleOperations++;

                    // Количество операций двора по группам ворот.
                    if (operation.Gate.Code is "EIN01" or "EIN02" or "EIN03" or "EIN04" or "EIN05" or "EIN06")
                        data[key].gates1_6Operations += operation.FactDuration;
                    else if (operation.Gate.Code == "EIN07")
                        data[key].gate7Operations += operation.FactDuration;
                    else if (operation.Gate.Code is "EIN08" or "EIN09" or "EIN10" or "EIN11" or "EIN12" or "EIN13" or "EIN14" or "EIN15" or "EIN16")
                        data[key].gates8_16Operations += operation.FactDuration;

                    // Операции, с превышением плана.
                    if (operation.FactDuration > 0 && operation.PlanDuration < operation.FactDuration ||
                        operation.FactLaborCost > 0 && operation.PlanLaborCost < operation.FactLaborCost)
                    {
                        if (operation.Type.Direction == TypeDirection.In)
                            data[key].overPlanIncomes++;
                        else if (operation.Type.Direction == TypeDirection.Out)
                            data[key].overPlanOutgoes++;
                    }

                    // Смотрим оформленные операции.
                    key = operation.CreateDate.ToString("yyyy.MM.dd");
                    addElement(key, operation.CreateDate.Date);
                    if (operation.PlanWorkFinishDate.Date == operation.CreateDate.Date) // Заявка оформлена день в день.
                        data[key].dayToDayOperations++;
                    else if (operation.CreateDate < operation.PlanWorkFinishDate.Date) // Заявка оформлена в указанную смену на последующие дни.
                        data[key].nextDayOperations++;

                    // Смотрим завершённе операции (те, которые фактически завершились в указанную дату и не были удалены).
                    if (operation.FactWorkFinishDate > DateTime.MinValue)
                    {
                        key = operation.FactWorkFinishDate.ToString("yyyy.MM.dd");
                        addElement(key, operation.FactWorkFinishDate.Date);
                        if (operation.Type.Direction == TypeDirection.In)
                        {
                            if (operation.Type.IsPallet)
                                data[key].palletIncomes++;
                            else
                                data[key].boxIncomes++;
                        }
                        else if (operation.Type.Direction == TypeDirection.Out)
                        {
                            if (operation.Type.IsPallet)
                                data[key].palletOutgoues++;
                            else
                                data[key].boxOutgoes++;
                        }
                    }
                }
            }

            var elements = data.Values.OrderBy(e => e.key).ToArray();
            var result = new List<dynamic>();
            foreach (var element in elements)
            {
                if (element.date < fromDate.Date || element.date >= toDate.AddTicks(-1).Date.AddDays(1))
                    continue;
                element.dayToDayOperationsPercent = 100.0 * element.dayToDayOperations / element.dayOperations;
                result.Add(element);
            }

            var text = JsonSerializer.Serialize(result);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        private static dynamic LaborReportElementToDynamic(LaborReport.Element element)
        {
            dynamic result = new ExpandoObject();
            result.partnerId = element.Partner.Id;
            result.partnerName = element.Partner.Name;
            result.positionId = element.Position.Id;
            result.positionName = element.Position.Name;

            result.bcWorkersCount = element.BcWorkersCount;
            result.bcLaborCost = element.BcLaborCost;

            result.ysPlanLaborCost = element.YSPlanLaborCost;
            result.ysFactLaborCost = element.YSFactLaborCost;

            result.yaWorkersCount = element.YAWorkersCount;
            result.yaPlanLaborCost = element.YAPlanLaborCost;
            result.yaFactLaborCost = element.YAFactLaborCost;

            return result;
        }
        private static dynamic[] LaborReportElementsToDynamic(LaborReport.Element[] elements)
        {
            var objects = new List<dynamic>();
            foreach (var element in elements)
                objects.Add(YardController.LaborReportElementToDynamic(element));
            return objects.ToArray();
        }
        [HttpGet("LaborReport")]
        public string GetLaborReport()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getData = _rightsChecker.CheckRight("GET_LABOR_REPORT", User);
            if (_useAuthentication && (!useYard || !getData))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? stringFromDate = Request.Query["from_date"];
            string? stringToDate = Request.Query["to_date"];

            var fromDate = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(stringFromDate))
            {
                stringFromDate = stringFromDate.Trim();
                if (!DateTime.TryParse(stringFromDate, out fromDate))
                    return $"Wrong from date {stringFromDate}";
            }
            var toDate = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(stringToDate))
            {
                stringToDate = stringToDate.Trim();
                if (!DateTime.TryParse(stringToDate, out toDate))
                    return $"Wrong to date {stringToDate}";
            }
            toDate = toDate.AddDays(1);

            var report = new LaborReport(fromDate, toDate);
            report.Create(_storage);
            var elements = report.Elements;
            var result = LaborReportElementsToDynamic(elements);

            var text = JsonSerializer.Serialize(result);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        private static dynamic ShiftLaborReportElementToDynamic(ShiftLaborReport.Element element)
        {
            dynamic result = new ExpandoObject();
            result.positionId = element.Position.Id;
            result.positionName = element.Position.Name;

            result.bcLaborCost = element.BcLaborCost;
            result.bcStandardLaborCost = element.BcStandardLaborCost;
            result.bcEfficiency = element.BcEfficiency;

            result.yardLaborCost = element.YLaborCost;
            result.yardG01LaborCost = element.YG01LaborCost;
            result.yardG02LaborCost = element.YG02LaborCost;
            result.yardG03LaborCost = element.YG03LaborCost;
            result.yardG04LaborCost = element.YG04LaborCost;
            result.yardG05LaborCost = element.YG05LaborCost;
            result.yardG06LaborCost = element.YG06LaborCost;
            result.yardG07LaborCost = element.YG07LaborCost;
            result.yardG08LaborCost = element.YG08LaborCost;
            result.yardG09LaborCost = element.YG09LaborCost;
            result.yardG10LaborCost = element.YG10LaborCost;
            result.yardG11LaborCost = element.YG11LaborCost;
            result.yardG12LaborCost = element.YG12LaborCost;
            result.yardG13LaborCost = element.YG13LaborCost;
            result.yardG14LaborCost = element.YG14LaborCost;
            result.yardG15LaborCost = element.YG15LaborCost;
            result.yardG16LaborCost = element.YG16LaborCost;

            return result;
        }
        private static dynamic[] ShiftLaborReportElementsToDynamic(ShiftLaborReport.Element[] elements)
        {
            var objects = new List<dynamic>();
            foreach (var element in elements)
                objects.Add(YardController.ShiftLaborReportElementToDynamic(element));
            return objects.ToArray();
        }
        [HttpGet("ShiftLaborReport2")]
        public string GetShiftLaborReport2()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getData = _rightsChecker.CheckRight("GET_SHIFT_LABOR_REPORT", User);
            if (_useAuthentication && (!useYard || !getData))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? stringDate = Request.Query["date"];
            var date = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(stringDate))
            {
                stringDate = stringDate.Trim();
                if (!DateTime.TryParse(stringDate, out date))
                    return $"Wrong to date {stringDate}";
            }
            string? shift = Request.Query["shift"];
            if (string.IsNullOrWhiteSpace(shift))
                shift = "day";
            shift = shift.Trim().ToLower();
            if (shift != "day" && shift != "night")
                return $"Unknown shift {shift}";

            var fromDate = shift == "day" ? date.Date.AddHours(9) : date.Date.AddHours(21);
            var toDate = fromDate.AddHours(12);

            var report = new ShiftLaborReport(fromDate, toDate);
            report.Create(_storage);
            var elements = report.Elements;
            var result = ShiftLaborReportElementsToDynamic(elements);

            var text = JsonSerializer.Serialize(result);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }

        [HttpGet("GateManagement")]
        public string GetGateManagement()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getData = _rightsChecker.CheckRight("GET_SHIFT_LABOR_REPORT", User);
            if (_useAuthentication && (!useYard || !getData))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? stringDate = Request.Query["date"];
            var date = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(stringDate))
            {
                stringDate = stringDate.Trim();
                if (!DateTime.TryParse(stringDate, out date))
                    return $"Wrong to date {stringDate}";
            }

            var fromDate = date.Date;
            var toDate = fromDate.AddHours(23).AddMinutes(59);

            var operations = _storage.GetGateManagementData(fromDate, toDate);
            foreach (dynamic operation in operations)
            {
                string partnerIds = operation.partnerIds;
                var ids = partnerIds.Split(",");
                var partnerNames = string.Empty;
                var partners = new HashSet<string>();
                foreach (var id in ids)
                {
                    var tempId = id.Trim();
                    if (!string.IsNullOrWhiteSpace(tempId) && !partners.Contains(tempId))
                    {
                        var tempPartner = _storage.GetPartner(tempId);
                        if (partnerNames.Length > 0)
                            partnerNames = partnerNames += ", ";
                        partnerNames += tempPartner.Name;
                        partners.Add(tempId);
                    }
                }
                operation.partnerNames = partnerNames;

                var siteCode = _storage.GetSiteCodeByGateCode(operation.gateCode);
            }

            var text = JsonSerializer.Serialize(operations);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }

        [HttpGet("ShiftPerfomanceReport")]
        public string GetShiftPerfomanceReport()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getData = _rightsChecker.CheckRight("GET_SHIFT_LABOR_REPORT", User);
            if (_useAuthentication && (!useYard || !getData))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? stringDate = Request.Query["date"];
            var date = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(stringDate))
            {
                stringDate = stringDate.Trim();
                if (!DateTime.TryParse(stringDate, out date))
                    return $"Wrong to date {stringDate}";
            }
            string? shift = Request.Query["shift"];
            if (string.IsNullOrWhiteSpace(shift))
                shift = "day";
            shift = shift.Trim().ToLower();
            if (shift != "day" && shift != "night")
                return $"Unknown shift {shift}";

            var fromDate = shift == "day" ? date.Date.AddHours(9) : date.Date.AddHours(21);
            var toDate = fromDate.AddHours(23).AddMinutes(59);

            var operations = _storage.GetYardPerfomanceData(fromDate, toDate);
            foreach (dynamic operation in operations)
            {
                string partnerIds = operation.partnerIds;
                var ids = partnerIds.Split(",");
                var partnerNames = string.Empty;
                var partners = new HashSet<string>();
                foreach (var id in ids)
                {
                    var tempId = id.Trim();
                    if (!string.IsNullOrWhiteSpace(tempId) && !partners.Contains(tempId))
                    {
                        var tempPartner = _storage.GetPartner(tempId);
                        if (partnerNames.Length > 0)
                            partnerNames = partnerNames += ", ";
                        partnerNames += tempPartner.Name;
                        partners.Add(tempId);
                    }
                }
                operation.partnerNames = partnerNames;

                var siteCode = _storage.GetSiteCodeByGateCode(operation.gateCode);
                operation.works = _storage.GetBcPerfomanceData(operation.factWorkStartDate, operation.factWorkFinishDate, siteCode);
                operation.factLaborCost = 0.0;
                foreach (var work in operation.works)
                    operation.factLaborCost += work.duration;

                // 2022.09.19 Буров Михаил.
                // Изменён алгоритм рассчёта производительности, теперь определяем прановые трудозатраты по алгоритму Башлыкова

                // Определяем заказы, которые относятся к данной операции двора
                var tempOperation = _storage.GetOperation(operation.id);
                if (tempOperation.Type.Direction == TypeDirection.Out)
                {
                    var outgoesIds = new List<string>();
                    foreach (var outgo in tempOperation.GetActions())
                        outgoesIds.Add(outgo.Id);
                    var partner = ids.Length > 0 ? _storage.GetPartner(ids[0]) : Partner.Null;

                    operation.planLaborCost = _GetHoodManPlanOperationLaborCost_Report(tempOperation.Type,
                        tempOperation.VehicleKind, partner,
                        tempOperation.FactWorkStartDate, tempOperation.FactWorkFinishDate,
                        siteCode,
                        outgoesIds.ToArray());
                }
                else // Приходы - берём стадартный план
                    operation.planLaborCost = operation.planLaborCost;
            }

            var text = JsonSerializer.Serialize(operations);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }

        [HttpGet("ECReport")]
        public string GetECReport()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getData = _rightsChecker.CheckRight("GET_EC_REPORT", User);
            if (_useAuthentication && (!useYard || !getData))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? temp = Request.Query["from_date"];
            var fromDate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(temp))
            {
                temp = temp.Trim();
                if (!DateTime.TryParse(temp, out fromDate))
                    return $"Wrong to date {temp}";
            }
            temp = Request.Query["to_date"];
            var toDate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(temp))
            {
                temp = temp.Trim();
                if (!DateTime.TryParse(temp, out toDate))
                    return $"Wrong to date {temp}";
                toDate = toDate.AddDays(1).AddTicks(-1);
            }
            if (fromDate >= toDate)
                throw new ArgumentException("Wrong dates!");

            temp = Request.Query["partners_ids"];
            var ids = string.IsNullOrWhiteSpace(temp) ? Array.Empty<string>() : temp.Split(",");

            var elements = _storage.GetEcData(fromDate, toDate, ids);
            var text = JsonSerializer.Serialize(elements);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }

        [HttpGet("ECReportDetailed")]
        public string GetECReportDetailed()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getData = _rightsChecker.CheckRight("GET_EC_REPORT", User);
            if (_useAuthentication && (!useYard || !getData))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? temp = Request.Query["from_date"];
            var fromDate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(temp))
            {
                temp = temp.Trim();
                if (!DateTime.TryParse(temp, out fromDate))
                    return $"Wrong to date {temp}";
            }
            temp = Request.Query["to_date"];
            var toDate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(temp))
            {
                temp = temp.Trim();
                if (!DateTime.TryParse(temp, out toDate))
                    return $"Wrong to date {temp}";
                toDate = toDate.AddDays(1).AddTicks(-1);
            }
            if (fromDate >= toDate)
                throw new ArgumentException("Wrong dates!");

            temp = Request.Query["partners_id"];
            string id = string.IsNullOrWhiteSpace(temp) ? string.Empty : temp;

            var elements = _storage.GetEcDetailedData(fromDate, toDate, id);
            var text = JsonSerializer.Serialize(elements);
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }

        /// <summary>
        /// Первая версия отчёта 
        /// </summary>
        /// <returns>Данные отчёта</returns>
        [HttpGet("WorkersPerfomanceReport")]
        public string GetWorkersPerfomanceReport()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getData = _rightsChecker.CheckRight("GET_WORKERS_PERFOMANCE_REPORT", User);
            if (_useAuthentication && (!useYard || !getData))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? temp = Request.Query["from_date"];
            var fromDate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(temp))
            {
                temp = temp.Trim();
                if (!DateTime.TryParse(temp, out fromDate))
                    return $"Wrong to date {temp}";
            }
            temp = Request.Query["to_date"];
            var toDate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(temp))
            {
                temp = temp.Trim();
                if (!DateTime.TryParse(temp, out toDate))
                    return $"Wrong to date {temp}";
                toDate = toDate.AddDays(1).AddTicks(-1);
            }
            if (fromDate >= toDate)
                throw new ArgumentException("Wrong dates!");

            var operations = _storage.GetOperationsByFactWork(fromDate, toDate);
            var data = new Dictionary<Employee, IDictionary<Operation, int>>();
            foreach (var operation in operations)
            {
                if (operation.FactWorkStartDate == DateTime.MinValue || operation.FactWorkFinishDate == DateTime.MinValue)
                    continue;

                var siteCode = _storage.GetSiteCodeByGateCode(operation.Gate.Code);
                var works = _storage.GetBossControlWorks(operation.FactWorkStartDate, operation.FactWorkFinishDate, siteCode);

                foreach (var work in works)
                {
                    var employee = _storage.GetEmployee(work.workerId);
                    if (!data.ContainsKey(employee))
                        data.Add(employee, new Dictionary<Operation, int>());
                    var workerOperations = data[employee];
                    if (!workerOperations.ContainsKey(operation))
                        workerOperations.Add(operation, 0);
                    workerOperations[operation] += work.duration;
                }
            }

            var elements = new List<dynamic>();
            foreach (var employee in data.Keys)
            {
                var workerOperations = data[employee];
                var perfomance = 0.0;
                var totalNormDuration = 0.0;
                foreach (var operation in workerOperations.Keys)
                {
                    perfomance += operation.PlanDuration * operation.PlanDuration / workerOperations[operation];
                    totalNormDuration += operation.PlanDuration;
                }
                dynamic element = new ExpandoObject();
                element.workerId = employee.Id;
                element.workerFullName = employee.FullName;
                element.workerPositionId = employee.MajorPosition.Id;
                element.workerPositionName = employee.MajorPosition.Name;
                element.perfomance = totalNormDuration != 0.0 ? 100.0 * perfomance / totalNormDuration : 0.0;
                elements.Add(element);
            }

            var text = JsonSerializer.Serialize(elements.ToArray());
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        /// <summary>
        /// Вторая версия отчёта (детали смотри в комментариях в теле процедуры).
        /// </summary>
        /// <returns>Данные отчёта</returns>
        //[HttpGet("WorkersPerfomanceReport2")]
        public string GetWorkersPerfomanceReport20()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getData = _rightsChecker.CheckRight("GET_WORKERS_PERFOMANCE_REPORT", User);
            if (_useAuthentication && (!useYard || !getData))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? temp = Request.Query["from_date"];
            var fromDate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(temp))
            {
                temp = temp.Trim();
                if (!DateTime.TryParse(temp, out fromDate))
                    return $"Wrong to date {temp}";
            }
            temp = Request.Query["to_date"];
            var toDate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(temp))
            {
                temp = temp.Trim();
                if (!DateTime.TryParse(temp, out toDate))
                    return $"Wrong to date {temp}";
                toDate = toDate.AddDays(1).AddTicks(-1);
            }
            if (fromDate >= toDate)
                throw new ArgumentException("Wrong dates!");

            var operations = _storage.GetOperationsByFactWork(fromDate, toDate);
            var data = new Dictionary<Employee, IDictionary<Operation, double>>();
            var data2 = new Dictionary<Employee, IDictionary<Operation, double>>(); // Для производительности = План. продолжительность / Факт. продолжительность
                                                                                    // Смотрим все операции двора за указанный период,
                                                                                    // для каждой операции определяем список работников, которые по БоссКонтролю "задели" фактический промежуток работ по операции,
                                                                                    // определяем суммарное время всех работ (на данном участке) по БоссКонтролю, которые "задели" фактический промежуток работ по операции,
                                                                                    // определяем производительность каждого сотрудника на данной операции как:
                                                                                    // отношение плановой продолжительности работ по операции, умноженной на количество сотрудников
                                                                                    // и суммарного времени всех работ (на данном участке) по БоссКонтролю
            foreach (var operation in operations)
            {
                if (operation.FactWorkStartDate == DateTime.MinValue || operation.FactWorkFinishDate == DateTime.MinValue)
                    continue;

                var siteCode = _storage.GetSiteCodeByGateCode(operation.Gate.Code);
                var works = _storage.GetBossControlWorks(operation.FactWorkStartDate, operation.FactWorkFinishDate, siteCode);

                var bcDuration = 0; // Длительность всех работ по БК для конкретной операции двора.
                var workers = new HashSet<Employee>(); // Работники, зарегистрированные на данном участке в течение работ по данной операции.
                foreach (var work in works)
                {
                    bcDuration += work.duration;
                    var employee = _storage.GetEmployee(work.workerId);
                    if (!workers.Contains(employee))
                        workers.Add(employee);
                }
                var workersCount = workers.Count;
                var perfomance = bcDuration > 0 ? (operation.PlanDuration * workersCount / bcDuration) : 0.0;
                var perfomance2 = operation.PlanDuration / operation.FactDuration; // Для производительности = План. продолжительность / Факт. продолжительность.
                foreach (var worker in workers)
                {
                    if (!data.ContainsKey(worker))
                        data.Add(worker, new Dictionary<Operation, double>());
                    var workerOperations = data[worker];
                    workerOperations.Add(operation, perfomance);

                    // Для производительности = План. продолжительность / Факт. продолжительность
                    if (!data2.ContainsKey(worker))
                        data2.Add(worker, new Dictionary<Operation, double>());
                    var workerOperations2 = data2[worker];
                    workerOperations2.Add(operation, perfomance2);
                }
            }

            // Вычисляем уже средневзвешенную производительность каждого работника:
            // сумма производельностей по конкретной операции, умноженных на плановое время операции,
            // делённая на суммарное время фактичнских продолжительностей операций,
            // Производительности работника =
            // Сумма(Производительность работника по конкретной операции * Плановая продолжительность операции) / Сумма(Фактическая продолжительность операции)
            var elements = new List<dynamic>();
            foreach (var worker in data.Keys)
            {
                var workerOperations = data[worker];
                var workerOperations2 = data2[worker]; // Для производительности = План. продолжительность / Факт. продолжительность
                var perfomance = 0.0;
                var perfomance2 = 0.0; // Для производительности = План. продолжительность / Факт. продолжительность
                var totalFactDuration = 0.0;
                foreach (var operation in workerOperations.Keys)
                {
                    perfomance += operation.PlanDuration * workerOperations[operation];
                    perfomance2 += operation.PlanDuration * workerOperations2[operation]; // Для производительности = План. продолжительность / Факт. продолжительность
                    totalFactDuration += operation.FactDuration;
                }
                perfomance /= totalFactDuration;
                perfomance2 /= totalFactDuration; // Для производительности = План. продолжительность / Факт. продолжительность

                dynamic element = new ExpandoObject();
                element.workerId = worker.Id;
                element.workerFullName = worker.FullName;
                element.workerPositionId = worker.MajorPosition.Id;
                element.workerPositionName = worker.MajorPosition.Name;
                element.perfomance = perfomance * 100.0;
                element.perfomance2 = perfomance2 * 100.0; // Для производительности = План. продолжительность / Факт. продолжительность
                elements.Add(element);
            }

            var text = JsonSerializer.Serialize(elements.ToArray());
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        [HttpGet("WorkersPerfomanceReport2")]
        public string GetWorkersPerfomanceReport21()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var useYard = _rightsChecker.CheckRight("USE_YARD", User);
            var getData = _rightsChecker.CheckRight("GET_WORKERS_PERFOMANCE_REPORT", User);
            if (_useAuthentication && (!useYard || !getData))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "Forbidden";
            }

            string? temp = Request.Query["from_date"];
            var fromDate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(temp))
            {
                temp = temp.Trim();
                if (!DateTime.TryParse(temp, out fromDate))
                    return $"Wrong to date {temp}";
            }
            temp = Request.Query["to_date"];
            var toDate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(temp))
            {
                temp = temp.Trim();
                if (!DateTime.TryParse(temp, out toDate))
                    return $"Wrong to date {temp}";
                toDate = toDate.AddDays(1);
            }
            if (fromDate >= toDate)
                throw new ArgumentException("Wrong dates!");

            var operationStarts = new Dictionary<DateTime, List<Operation>>();
            var operationFinishes = new Dictionary<DateTime, List<Operation>>();
            var operations = _storage.GetOperationsByFactWork(fromDate, toDate);
            foreach (var operation in operations)
            {
                if (!operationStarts.ContainsKey(operation.FactWorkStartDate))
                {
                    var tempList = new List<Operation>();
                    tempList.Add(operation);
                    operationStarts.Add(operation.FactWorkStartDate, tempList);
                }
                else
                    operationStarts[operation.FactWorkStartDate].Add(operation);
                if (!operationFinishes.ContainsKey(operation.FactWorkFinishDate))
                {
                    var tempList = new List<Operation>();
                    tempList.Add(operation);
                    operationFinishes.Add(operation.FactWorkFinishDate, tempList);
                }
                else
                    operationFinishes[operation.FactWorkFinishDate].Add(operation);
            }

            var worksStarts = new Dictionary<DateTime, List<SiteWorker>>();
            var worksFinishes = new Dictionary<DateTime, List<SiteWorker>>();
            var works = _storage.GetSitesWorkers(fromDate, toDate, "");
            foreach (var work in works)
            {
                if (!worksStarts.ContainsKey(work.StartDate))
                {
                    var tempList = new List<SiteWorker>();
                    tempList.Add(work);
                    worksStarts.Add(work.StartDate, tempList);
                }
                else
                    worksStarts[work.StartDate].Add(work);
                if (!worksFinishes.ContainsKey(work.FinishDate))
                {
                    var tempList = new List<SiteWorker>();
                    tempList.Add(work);
                    worksFinishes.Add(work.FinishDate, tempList);
                }
                else
                    worksFinishes[work.FinishDate].Add(work);
            }

            var keys = operationStarts.Keys.ToHashSet();
            keys.UnionWith(operationFinishes.Keys);
            keys.UnionWith(worksStarts.Keys);
            keys.UnionWith(worksFinishes.Keys);

            // Определяем промежутки работ по БК, который относятся к той или иной операции
            var moments = keys.ToArray();
            Array.Sort(moments);
            var data = new Dictionary<Operation, HashSet<SiteWorker>>();
            var activeOperations = new HashSet<Operation>();
            var activeWorks = new HashSet<SiteWorker>();
            var dummyOperations = new List<Operation>();
            var dummyWorks = new List<SiteWorker>();
            foreach (var moment in moments)
            {
                var startedOperations = operationStarts.ContainsKey(moment) ? operationStarts[moment] : dummyOperations;
                var finishedOperations = operationFinishes.ContainsKey(moment) ? operationFinishes[moment] : dummyOperations;
                activeOperations.UnionWith(startedOperations);
                activeOperations.ExceptWith(finishedOperations);

                var startedWorks = worksStarts.ContainsKey(moment) ? worksStarts[moment] : dummyWorks;
                var finishedWorks = worksFinishes.ContainsKey(moment) ? worksFinishes[moment] : dummyWorks;
                activeWorks.UnionWith(startedWorks);
                activeWorks.ExceptWith(finishedWorks);

                foreach (var operation in activeOperations)
                {
                    if (!data.ContainsKey(operation))
                        data.Add(operation, new HashSet<SiteWorker>());
                    foreach (var work in activeWorks)
                        if (_storage.GetSiteCodeByGateCode(operation.Gate.Code) == work.Site.Code)
                            data[operation].Add(work);
                }
            }

            // Определяем производительность операции и сотрудников, работающих на данной операции (по БК)
            var perfomances = new Dictionary<Operation, double>();
            var operationsEmployees = new Dictionary<Operation, HashSet<Employee>>();
            foreach (var operation in data.Keys)
            {
                var workers = new HashSet<Employee>();
                var bcDuration = 0.0;
                foreach (var work in data[operation])
                {
                    workers.Add(work.Worker);
                    bcDuration += work.Duration;
                }
                var perfomance = bcDuration > 0.0 ? operation.PlanDuration * workers.Count / bcDuration : 0.0;
                perfomances.Add(operation, perfomance);
                operationsEmployees.Add(operation, workers);
            }

            // Определяем операции двора каждого сотрудника
            var employeesOperations = new Dictionary<Employee, HashSet<Operation>>();
            foreach (var operation in operationsEmployees.Keys)
            {
                var tempEmployees = operationsEmployees[operation];
                foreach (var employee in tempEmployees)
                {
                    if (!employeesOperations.ContainsKey(employee))
                        employeesOperations.Add(employee, new HashSet<Operation>());
                    employeesOperations[employee].Add(operation);
                }
            }

            // Непосредственно определяем средневзвешенную производительность каждого сотрудника
            var result = new List<dynamic>();
            foreach (var employee in employeesOperations.Keys)
            {
                dynamic element = new ExpandoObject();
                element.workerId = employee.Id;
                element.workerFullName = employee.FullName;
                element.workerPositionId = employee.MajorPosition.Id;
                element.workerPositionName = employee.MajorPosition.Name;

                var perfomance = 0.0;
                var totalFactDuration = 0.0;
                foreach (var operation in employeesOperations[employee])
                {
                    perfomance += perfomances[operation] * operation.PlanDuration;
                    totalFactDuration += operation.FactDuration;
                }
                perfomance /= totalFactDuration;

                element.perfomance = perfomance * 100.0;
                //element.perfomance2 = 0.0; // Альтернативная производиетльность (пока не актуально - эксперименты)
                result.Add(element);
            }

            var text = JsonSerializer.Serialize(result.ToArray());
            Response.StatusCode = (int)HttpStatusCode.OK;
            return text;
        }
        #endregion
    }
}
