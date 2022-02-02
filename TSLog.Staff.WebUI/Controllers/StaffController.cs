using Microsoft.AspNetCore.Mvc;
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
            return "test api";
            //_storage.GetShifts();
        }

        [HttpPost("CreateUpdateShift")]
        public string PostCreateUpdateShift()
        {
            return "test api";
        }

        [HttpPost("DeleteShift")]
        public string POstDeleteShift()
        {
            return "test api";
        }
    }
}