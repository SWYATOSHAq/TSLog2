using Microsoft.AspNetCore.Mvc;
using Auth.Models;
using Auth.Services;
using TSLog.Links;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Auth.Helpers;
using Microsoft.Extensions.Options;
using TSLog.BO;
using Microsoft.IdentityModel.Tokens;
using System;

namespace Auth.Controllers
{
    // основная точка доступа для получения токена Berrier

    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateRequest model)
        {
            var response =  _userService.Authenticate(model);


            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(response);
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            // no snitch :)

            var users = _userService.GetAll();
            return Ok(users);
        }

    }
}
