using System.Text.Json;
using System.Web;
using AuthService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[Route("oauth/authorize")]
[Authorize]
public class AuthorizationController : Controller
{
    private IDataProtectionProvider _dataProtectionProvider;

    public AuthorizationController(IDataProtectionProvider dataProtectionProvider)
    {
        _dataProtectionProvider = dataProtectionProvider;
    }
    
    [HttpGet]
    public IActionResult Authorize(HttpRequest request)
    {
        if (!request.Query.TryGetValue("response_type", out var responseType))
        {
            return BadRequest("No response_type provided");
        }

        if (!request.Query.TryGetValue("client_id", out var clientId))
        {
            return BadRequest("No client_id provided");
        }
        
        if (!request.Query.TryGetValue("code_challenge", out var codeChallenge))
        {
            return BadRequest("No code_challenge provided");
        }

        if (!request.Query.TryGetValue("code_challenge_method", out var codeChallengeMethod))
        {
            return BadRequest("No code_challenge_method provided");
        }

        if (!request.Query.TryGetValue("redirect_uri", out var redirectUrl))
        {
            return BadRequest("No redirect_uri provided");
        }

        if (!request.Query.TryGetValue("state", out var state))
        {
            return BadRequest("No state provided");
        }

        var protector = _dataProtectionProvider.CreateProtector("oauth");
        var code = new AuthCode
        {
            ClientId = clientId,
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = codeChallengeMethod,
            RedirectUrl = redirectUrl,
            Expiry = DateTime.Now.AddMinutes(10)
        };
        var codeStr = protector.Protect(JsonSerializer.Serialize(code));

        return Redirect(
            $"{redirectUrl}?code={codeStr}&state={state}&iss={HttpUtility.UrlEncode("https://localhost:5199")}");
    }
}