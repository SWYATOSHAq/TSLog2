using Microsoft.AspNetCore.Mvc;

namespace TSLog.Staff.WebUI.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class StaffController : ControllerBase
    {

        private readonly ILogger<StaffController> _logger;

        public StaffController(ILogger<StaffController> logger)
        {
            _logger = logger;
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