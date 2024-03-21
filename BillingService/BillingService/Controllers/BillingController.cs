using BillingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BillingService.Controllers;

[Authorize]
public class BillingController : Controller
{
    private readonly AccountingService _accountingService;

    public BillingController(AccountingService accountingService)
    {
        _accountingService = accountingService;
    }
    
    [Route("billing/all")]
    [Authorize(Roles = "Manager, Admin")]
    public async Task<IActionResult> GetAccountsList()
    {
        var accountsList = await _accountingService.GetAccountsList();

        return Ok(new
        {
            accounts = accountsList
        });
    }
    
    [Route("billing/revenue")]
    [Authorize(Roles = "Manager, Admin")]
    public async Task<IActionResult> GetRevenue([FromBody] DateTime from, [FromBody] DateTime to)
    {
        int revenue = await _accountingService.GetRevenueForPeriod(from, to);
        return Ok(new
        {
            amount = revenue
        });
    }

    [Route("billing")]
    public async Task<IActionResult> GetCurrentAccount()
    {
        string? userId = HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)
            ?.ToString();

        if (userId == null)
        {
            return Unauthorized();
        }

        var accountData = await _accountingService.GetAccountData(userId);

        return Ok(new
        {
            account = accountData
        });
    }
}
