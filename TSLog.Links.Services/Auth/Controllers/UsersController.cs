using Microsoft.AspNetCore.Mvc;
using Auth.Models;
using Auth.Services;
using TSLog.Links;

namespace Auth.Controllers
{
    // основная точка доступа для получения токена Berrier

    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private ILinksUsers _databaseUsers;

        public UsersController(ILinksUsers databaseUsers)
        {
            _databaseUsers = databaseUsers;
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateRequest model)
        {
            var response = _databaseUsers.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(response);
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            // no snitch :)

            var users = _databaseUsers.GetAllUsers();
            return Ok(users);
        }
    }
}
