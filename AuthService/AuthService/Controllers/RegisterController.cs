using System.Text.Json;
using AuthService.Events;
using AuthService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AuthService.Services;

namespace AuthService.Controllers;

[Route("register")]
public class RegisterController : Controller
{
    private UserManager<ApplicationUser> _userManager;
    private KafkaProducer _kafkaProducer;
    public RegisterController(UserManager<ApplicationUser> userManager, KafkaProducer kafkaProducer)
    {
        _userManager = userManager;
        _kafkaProducer = kafkaProducer;
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
                var userCreated = new UserCreatedEvent
                {
                    UserId = appUser.Id.ToString(),
                    Email = appUser.Email,
                    Name = appUser.UserName,
                    Roles = new[] { "Worker" }
                };
                await _kafkaProducer.ProduceAsync("user-stream", JsonSerializer.Serialize(userCreated));
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