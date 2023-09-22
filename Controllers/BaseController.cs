using System.Security.Claims;
using Constant;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

// Mục đích là lấy Role và Id dễ dàng hơn
public abstract class BaseController : ControllerBase
{
    protected int? GetUserId()
    {
        try
        {
            return int.Parse(User.FindFirstValue("id"));
        }
        catch (Exception e)
        {
            return null;
        }
    }

    protected UserRole? GetRole()
        => Enum.TryParse<UserRole>(User.FindFirstValue("user-role"), out var userRole) ? userRole : null;
}
