using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskTracker.Controllers;
[Route("user")]
[Authorize]
public class SecureController : Controller
{
    public IActionResult Secure()
    {
        return Ok();
    }
}
