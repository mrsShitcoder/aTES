using Analytics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Analytics.Controllers;

[Authorize(Roles = "Manager, Admin")]
public class AnalyticsController : Controller
{
    private readonly AnalyticsService _analyticsService;

    public AnalyticsController(AnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }
    
    [Route("analytics/dashboard")]
    public async Task<IActionResult> GetDashboardInfo([FromBody]DateTime fromDate, [FromBody] DateTime toDate)
    {
        var revenue = await _analyticsService.GetRevenueForPeriod(fromDate, toDate);
        var usersWithNegativeBalance = await _analyticsService.GetUsersWithNegativeBalance();
        var mostExpensiveTask = await _analyticsService.GetMostExpensiveTaskForPeriod(fromDate, toDate);

        return Ok(new
        {
            revenue,
            negative_balance = usersWithNegativeBalance,
            most_expensive_task = mostExpensiveTask
        });
    }
}
