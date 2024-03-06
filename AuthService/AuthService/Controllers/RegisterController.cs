using AuthService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[Route("register")]
public class RegisterController : Controller
{
    private UserManager<ApplicationUser> _userManager;
    private RoleManager<ApplicationRole> _roleManager;

    public RegisterController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    
    [HttpGet]
    public ViewResult Register() => View();
    [HttpPost]
    public async Task<IActionResult> Register(RegInfo regInfo)
    {
        if (ModelState.IsValid)
        {
            var appUser = new ApplicationUser(regInfo.Name, regInfo.Email);
            IdentityResult identityResult = await _userManager.CreateAsync(appUser, regInfo.Password);
            if (identityResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(appUser, "Worker");
                ViewBag.Message = "Successfully registered";
            }
            else
            {
                foreach (var err in identityResult.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
            }
        }

        return View(regInfo);
    }
}