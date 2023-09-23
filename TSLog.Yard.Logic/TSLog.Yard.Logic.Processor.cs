using Microsoft.Extensions.Configuration;
using System.Dynamic;
using System.Security.Claims;
using TSLog.Authorization;
using TSLog.Staff.BO;
using TSLog.Yard.BO;

namespace TSLog.Yard.Logic
{
    public class Processor
    {
        private readonly IConfiguration _configuration;
        private readonly IYardStorage _storage;
        private readonly IRightsChecker _rightsChecker;
        private readonly bool _useAuthentication;
        public Processor(IConfiguration configuration, IYardStorage storage, IRightsChecker rightsChecker)
        {
            // TODO: check nulls.
            _configuration = configuration;
            _storage = storage;
            _rightsChecker = rightsChecker;
            _useAuthentication = _configuration.GetValue<bool>("UseAuthentication");
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
    }
}
