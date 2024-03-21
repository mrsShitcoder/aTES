using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Controllers;

[Route("login")]
public class LoginController : Controller
{
    private UserManager<ApplicationUser> _userManager;
    private Keys _signingKey;
    private readonly ILogger<LoginController> _logger;
    
    public LoginController(UserManager<ApplicationUser> userManager, Keys signingKey, ILogger<LoginController> logger)
    {
        _userManager = userManager;
        _signingKey = signingKey;
        _logger = logger;
    }

    [HttpGet]
    public ViewResult Login() => View();
    
    private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var signingKey = _signingKey.RsaSecurityKey;
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256);
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.UserName)
        };

        
        // Add roles as claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var handler = new JwtSecurityTokenHandler();
        SecurityToken token = handler.CreateToken(new SecurityTokenDescriptor()
        {
            Issuer = "https://localhost:7018",
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = creds,
            TokenType = "Bearer"
        });

        return handler.WriteToken(token);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginInfo loginInfo)
    {
        string? returnUrl = HttpContext.Request.Query["redirectUrl"];
        var user = await _userManager.FindByEmailAsync(loginInfo.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginInfo.Password))
        {
            ModelState.AddModelError("", "Invalid email or password");
            return View();
        }

        var roles = await _userManager.GetRolesAsync(user);
        
        string token = GenerateJwtToken(user, roles);
        
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(1)
        };

        Response.Cookies.Append("jwt", token, cookieOptions);
        
        _logger.LogInformation($"returnUrl: {returnUrl}");
        
        return Redirect(returnUrl ?? "/");
    }
}
