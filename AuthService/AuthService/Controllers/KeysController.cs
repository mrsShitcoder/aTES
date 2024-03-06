using Microsoft.AspNetCore.Mvc;
using AuthService.Models;

namespace AuthService.Controllers;

[Route("key")]
public class KeysController : Controller
{
    private Keys _signingKey;

    public KeysController(Keys signingKey)
    {
        _signingKey = signingKey;
    }
    
    [HttpGet]
    public IActionResult GetKey()
    {
        var pubKeyBytes = _signingKey.Rsa.ExportRSAPublicKey();
        var pubKey = Convert.ToBase64String(pubKeyBytes, Base64FormattingOptions.InsertLineBreaks);
        return Ok(pubKey);
    }
}
