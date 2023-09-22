using Dtos.AccountDto;
using Microsoft.AspNetCore.Mvc;
using Services.AccountService;

namespace Controllers.CustomerControllers;

[Route("api/customer/[controller]")]
[ApiController]
public class UserController : BaseController
{
    private readonly IUserService _userService;

    [HttpGet]
    public async Task<ActionResult<AccountInfoFullDto>> FindUserById(int id)
    {
        //TODO: thís
        return null;
    }

    [HttpPost]
    public async Task<ActionResult<AccountInfoFullDto>> Create(int id)
    {
        //TODO: thís
        return null;
    }
}
