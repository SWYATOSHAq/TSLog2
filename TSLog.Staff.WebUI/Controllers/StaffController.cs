using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
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
         * 
         */

        [HttpGet("Shifts")]
        public string GetShifts()
        {
            var testShifts = new Shift[] {
                new Shift(-1, DateTime.Now.AddHours(-6), DateTime.Now.AddHours(6), "5/2-40"),
                new Shift(-1, DateTime.Now.AddHours(12), DateTime.Now.AddHours(18), "5/2-40")
            };
            return JsonSerializer.Serialize(testShifts);
            // https://localhost:7019/api/v1/Staff/Shifts
        }

        [HttpPost("CreateUpdateShift")]
        public string PostCreateUpdateShift()
        {
            return "test api";

        }

        [HttpPost("DeleteShift")]
        public string PostDeleteShift()
        {
            return "test api";
        }
        // -----------------------------


        [HttpGet("Employees")]
        public string GetEmployees()
        {
            var testEmployees = new Employee[] {
                new Employee(-1, "1", "Ivanov", "Ivan", DateTime.Today.AddYears(-22), new LegalEntities.BO.LegalEntity("koton", "KOTON"), true),
                new Employee(-1, "1", "Sergeyev", "Sergei", DateTime.Today.AddYears(-21), new LegalEntities.BO.LegalEntity("koton", "KOTON"), true)
            };


            return JsonSerializer.Serialize(testEmployees);
            // https://localhost:7019/api/v1/Staff/Employees
        }

        // -----------------------------


        [HttpGet("Positions")]
        public string GetPositions()
        {
            var testPositions = new Position[]
            {
                new Position(-1, "1","engeener"),
                new Position(-1, "1","programmer"),
            }
            return JsonSerializer.Serialize(testPositions);
            // https://localhost:7019/api/v1/Staff/Departments
        }

        // -----------------------------


        [HttpGet("Departments")]
        public string GetDepartments()
        {
            var testDepartments = new Department[]
            {
                new Department(-1, "", "IT","IT","IT", "IT", true,1, new LegalEntities.BO.LegalEntity("koton", "KOTON"), new Employee(-1, "1", "Ivanov", "Ivan", DateTime.Today.AddYears(-22), new LegalEntities.BO.LegalEntity("koton", "KOTON"), true)),
                new Department(-1, "", "yard","yard","yard", "yard", true,1, new LegalEntities.BO.LegalEntity("koton", "KOTON"), new Employee(-1, "1", "Ivanov", "Ivan", DateTime.Today.AddYears(-22), new LegalEntities.BO.LegalEntity("koton", "KOTON"), true))
            }
            return JsonSerializer.Serialize(testDepartments);
            // https://localhost:7019/api/v1/Staff/Departments
        }
    }
}