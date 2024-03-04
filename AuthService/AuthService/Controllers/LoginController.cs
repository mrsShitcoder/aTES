using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Controllers;

[Route("login")]
public class LoginController : Controller
{
    private UserManager<ApplicationUser> _userManager;
    private IConfiguration _config;
    
    public LoginController(UserManager<ApplicationUser> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    [HttpGet]
    public ViewResult Login() => View();
    
    private SecurityToken GenerateJwtToken(ApplicationUser user)
    {
        var secret = _config["Secret"];
        if (secret is null)
        {
            throw new InvalidDataException("Invalid secret");
        }
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        return new JwtSecurityTokenHandler().CreateToken(new SecurityTokenDescriptor()
        {
            Issuer = "https:://localhost:7018",
            Subject = new ClaimsIdentity(new []
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName),
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = creds,
            TokenType = "Bearer"
        });
    }

    [HttpPost]
    public async Task<IActionResult> Login(HttpContext ctx, LoginInfo loginInfo, string returnUrl)
    {
        var user = await _userManager.FindByEmailAsync(loginInfo.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginInfo.Password)) return Unauthorized();
        SecurityToken token = GenerateJwtToken(user);
        return Redirect($"{returnUrl}?token={token}");
    }
}
