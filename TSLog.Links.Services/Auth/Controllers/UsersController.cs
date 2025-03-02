using Auth.Services;
using Microsoft.AspNetCore.Mvc;
using TSLog.Links;

namespace Auth.Controllers;

// Основная точка доступа для получения токена Berrier.

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
            return BadRequest(new { message = "Username or password is incorrect." });

        return Ok(response);
    }

    [Authorize]
    [HttpGet]
    public IActionResult GetAll()
    {
        // No snitch :).

        var users = _userService.GetAll();
        return Ok(users);
    }

}
