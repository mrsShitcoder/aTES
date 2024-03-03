using System.Security.Claims;
using AuthService.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[Route("login")]
public class LoginController : Controller
{
    private UserManager<ApplicationUser> _userManager;
    
    public LoginController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public ViewResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(HttpContext ctx, LoginInfo loginInfo, string returnUrl)
    {
        var user = await _userManager.FindByEmailAsync(loginInfo.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginInfo.Password)) return Unauthorized();
        await ctx.SignInAsync("cookie", new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Email, loginInfo.Email),
            new(ClaimTypes.NameIdentifier, user.PublicId.ToString()),
        }, "cookie")));
        return Redirect(returnUrl);
    }
}
