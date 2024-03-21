using AuthService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[Authorize]
[Route("userinfo")]
public class UserInfoController : Controller
{
    private UserManager<ApplicationUser> _userManager;

    public UserInfoController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    
    [HttpGet]
    public async Task<IActionResult> UserInfo()
    {
        var subClaim = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        if (subClaim == null)
        {
            return Unauthorized();
        }
        var user = await _userManager.FindByIdAsync(subClaim);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new UserInfo
        {
            UserId = user.Id.ToString(),
            Email = user.Email ?? "",
            Name = user.UserName ?? "",
            Roles = roles
        });
    }
}