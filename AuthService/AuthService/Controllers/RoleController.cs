using System.ComponentModel.DataAnnotations;
using AuthService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AuthService.Controllers;

[Authorize(Roles = "Admin")]
public class RolesController : Controller
{
    private RoleManager<ApplicationRole> _roleManager;
    private UserManager<ApplicationUser> _userManager;

    public RolesController(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    [HttpGet("roles/create")]
    public ViewResult Create() => View();

    [HttpPost("roles/create")]
    public async Task<IActionResult> Create([Required] string name)
    {
        if (ModelState.IsValid)
        {
            var result = await _roleManager.CreateAsync(new ApplicationRole(name));
            if (result.Succeeded)
            {
                ViewBag.Message = "Role created successfully";
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
        }

        return View();
    }

    [HttpGet("roles/grant")]
    public ViewResult Grant() => View();

    [HttpPost("roles/grant")]
    public async Task<IActionResult> Grant([Required] [EmailAddress] string email, [Required] string role)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            ModelState.AddModelError("email", "User is not registered");
            return View();
        }

        var retrievedRole = await _roleManager.FindByNameAsync(role);
        if (retrievedRole == null)
        {
            ModelState.AddModelError("role", "Role does not exist");
            return View();
        }

        var result = await _userManager.AddToRoleAsync(user, role);
        if (result.Succeeded)
        {
            ViewBag.Message = $"Successfully granted role {role}";
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        return View();
    }
}