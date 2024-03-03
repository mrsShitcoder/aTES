using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using AuthService.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Controllers;

[Route("oauth/token")]
public class TokenController : Controller
{
    private IConfiguration _configuration;
    private IDataProtectionProvider _dataProtectionProvider;

    public TokenController(IConfiguration config, IDataProtectionProvider dataProtectionProvider)
    {
        _configuration = config;
        _dataProtectionProvider = dataProtectionProvider;
    }
    
    private SecurityToken GenerateJwtToken()
    {
        var secret = _configuration["Secret"];
        if (secret is null)
        {
            throw new InvalidDataException("Invalid secret");
        }
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        return new JwtSecurityTokenHandler().CreateToken(new SecurityTokenDescriptor()
        {
            Claims = new Dictionary<string, object>()
            {
                [JwtRegisteredClaimNames.Sub] = Guid.NewGuid().ToString(),
                ["custom"] = "custom"
            },
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = creds,
            TokenType = "Bearer"
        });
    }

    private static bool IsValidCodeVerifier(AuthCode code, string codeVerfier)
    {
        var sha256 = SHA256.Create();
        var codeChallenge = Base64UrlEncoder.Encode(sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerfier)));
        return codeChallenge == code.CodeChallenge;
    }
    
    [HttpPost]
    public IActionResult GetToken(HttpRequest request)
    {
        var parsed = HttpUtility.ParseQueryString(request.QueryString.ToString());
        string? grantType = parsed["grant_type"];
        string? code = parsed["code"];
        string? redirectUrl = parsed["redirect_uri"];
        string? codeVerifier = parsed["code_verifier"];
        if (grantType == null || code == null || redirectUrl == null || codeVerifier == null)
        {
            return BadRequest("Missing one or several required parameters");
        }
        
        var protector = _dataProtectionProvider.CreateProtector("oauth");
        var codeStr = protector.Unprotect(code);
        var authCode = JsonSerializer.Deserialize<AuthCode>(codeStr);
        if (authCode == null)
        {
            return Problem("Cannot read code details");
        }
        
        if (!IsValidCodeVerifier(authCode, codeVerifier))
        {
            return BadRequest();
        }

        if (authCode.Expiry <= DateTime.Now)
        {
            return BadRequest();
        }
        
        //TODO: code blacklisting

        return Ok(new
        {
            access_token = GenerateJwtToken(),
            token_type = "Bearer",
            expires_in = 3600
        });
    }
}