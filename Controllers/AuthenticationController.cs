using Dtos.AccountDto;
using Microsoft.AspNetCore.Mvc;
using Services.AccountService;

namespace Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : BaseController
{
    private readonly IUserService _userService;

    public AuthenticationController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto requestDto)
    {
        return await _userService.Login();
    }

    [HttpPost("signup")]
    public async Task<ActionResult<int>> Signup([FromBody] SignupRequestDto requestDto)
    {
        //TODO: this
        return null;
    }
}
