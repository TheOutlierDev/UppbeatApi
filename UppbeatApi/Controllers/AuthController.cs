using Microsoft.AspNetCore.Mvc;
using UppbeatApi.Data.Models;
using UppbeatApi.Interfaces;

namespace UppbeatApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;

    public AuthController(ITokenService tokenService, IUserService userService)
    {
        _tokenService = tokenService;
        _userService = userService;
    }

    /// <summary>
    /// Generates a jwt token for the user
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("token")]
    public async Task<IActionResult> Token([FromBody] LoginRequest request)
    {
        var existingUser = await _userService.GetUserAsync(request.UserId);

        if (existingUser == null)
        {
            return Unauthorized("Invalid username or password");
        }

        var token = _tokenService.GenerateToken(existingUser.Name, existingUser.Role);
        return Ok(new { Token = token });
    }
}