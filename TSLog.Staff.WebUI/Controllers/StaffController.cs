using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Text.Json;
using TSLog.LegalEntities.BO;
using TSLog.Staff.BO;

namespace TSLog.Staff.WebUI.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class StaffController : ControllerBase
    {

        private readonly ILogger<StaffController> _logger;
        private readonly IStaffStorage _storage;

        public StaffController(ILogger<StaffController> logger, IStaffStorage storage)
        {
            _logger = logger;
            _storage = storage;
        }
        /*
         * get shifts
         * create / update shift
         * delete shift
         * 
         * get employees
         * create / update employee
         * delete employee
         * 
         * get positions
         * create / update position
         * delete position
         * 
         * get departments
         * create / update department
         * delete department
         * 
         */

        private static async Task<dynamic?> GetInputData(HttpRequest request)
        {
            request.EnableBuffering();
            var body = request.Body;
            body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(body);
            string json = await reader.ReadToEndAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
        }

        #region get lists of data

        [HttpGet("Shift")]
        public string GetShifts()
        {
            // TODO выбирать период по которому будет загружен список смен
            var shifts = _storage.GetShifts(DateTime.Today.AddYears(-1), DateTime.Today.AddYears(1));

            return JsonSerializer.Serialize(shifts);
            // https://localhost:7019/api/v1/Staff/Shift
        }

        [HttpGet("Employee")]
        public string GetEmployees()
        {
            var employees = _storage.GetEmployees();
            return JsonSerializer.Serialize(employees);
            // https://localhost:7019/api/v1/Staff/Employee
        }

        [HttpGet("Employer")]
        public string GetEmployers()
        {
            var employers = _storage.GetEmployers();
            return JsonSerializer.Serialize(employers);
            // https://localhost:7019/api/v1/Staff/Employer
        }

        [HttpGet("Position")]
        public string GetPositions()
        {
            var positions = _storage.GetPositions();

            Response.Headers.Add("Access-Control-Allow-Origin", "Hello Alex!!!");

            return JsonSerializer.Serialize(positions);
            // https://localhost:7019/api/v1/Staff/Position
        }

        [HttpGet("Department")]
        public string GetDepartments()
        {
            var departments = _storage.GetDepartments();

            // to use with recursive referencing
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };
            return JsonSerializer.Serialize(departments, options);
            // https://localhost:7019/api/v1/Staff/Department
        }

        #endregion

        // -----------------------------

        #region Create/Update
        [HttpPost("Shift")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public string PostCreateUpdateShift(Shift shift)
        {
            var retShift = shift;
            if (shift.Id > 0)
            {
                // update
                retShift = _storage.EditShift(shift);
            }
            else
            {
                //create new
                retShift = _storage.AddShift(shift);
            }
            return JsonSerializer.Serialize(retShift);
        }

        [HttpPost("Employee")]       
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public string PostCreateUpdateEmployee(Employee employee)
        //{
        //    var retEmployee = employee;
        //    if (employee.Id > 0)
        //    {
        //        // update
        //        retEmployee = _storage.EditEmployee(employee);
        //    }
        //    else
        //    {
        //        //create new
        //        retEmployee = _storage.AddEmployee(employee);
        //    }
        //    return JsonSerializer.Serialize(retEmployee);
        //}
        async public Task<string> PostCreateUpdateEmployee()
        {
            try
            {
                Response.StatusCode = 500;
                dynamic? data = await GetInputData(Request);
                if (data == null)
                    return "Input data is null!";

                Employee newEmployee = new Employee
                {
                    FirstName = data.FirstName,
                    Surname = data.Surname,
                    Patronymic = data.Patronymic,
                    Sex = data.Sex,
                    Nationality = data.Nationality, 
                    Company = data.Company?.ToObject<LegalEntity>(),
                    BirthDate = data.BirthDate
                    //BirthDate = DateTime.ParseExact(Convert.ToString(data.BirthDate), "yyyy-MM-dd", CultureInfo.InvariantCulture)
                };


                _storage.AddEmployee(newEmployee);

                return "";
            }
            catch (Exception exception)
            {
                return exception.Message; 
            }
        }

        [HttpPost("Position")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public string PostCreateUpdatePosition(Position pisition)
        {
            var retPosition = pisition;
            if (pisition.Id > 0)
            {
                // update
                retPosition = _storage.EditPosition(pisition);
            }
            else
            {
                //create new
                retPosition = _storage.AddPosition(pisition);
            }
            return JsonSerializer.Serialize(retPosition);
        }

        [HttpPost("Department")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public string PostCreateUpdateDepartment(Department pisition)
        {
            var retDepartment = pisition;
            if (pisition.Id > 0)
            {
                // update
                retDepartment = _storage.EditDepartment(pisition);
            }
            else
            {
                //create new
                retDepartment = _storage.AddDepartment(pisition);
            }
            return JsonSerializer.Serialize(retDepartment);
        }
        #endregion

        // -----------------------------

        #region delete

        [HttpDelete("Shift")]
        public string DeleteShift(Shift shift)
        {
            _storage.DeleteShift(shift);
            return "deleted";
        }

        [HttpDelete("Employee")]
        public string DeleteEmployee(Employee employee)
        {
            _storage.DeleteEmployee(employee);
            return "deleted";
        }

        [HttpDelete("Position")]
        public string DeletePosition(Position shift)
        {
            //_storage.DeletePosition(shift);
            return "cancelled";
        }

        [HttpDelete("Department")]
        public string DeleteShift(Department shift)
        {
            _storage.DeleteDepartment(shift);
            return "deleted";
        }


        #endregion

    }
}