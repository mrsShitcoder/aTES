using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Services;

namespace TaskTracker.Controllers;
[Authorize]
public class TasksController : Controller
{
    private readonly TaskTrackerService _taskTracker;

    [Route("tasks/all")]
    public async Task<IActionResult> GetTaskList()
    {
        var userId = HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)
            ?.ToString();
        if (userId == null)
        {
            return Unauthorized();
        }
        
        var taskList = await _taskTracker.GetTasksByAssignee(userId);

        return Ok(new
        {
            tasks = taskList
        });
    }

    [Route("tasks/shuffle")]
    [Authorize(Roles = "Manager, Admin")]
    public async Task<IActionResult> ShuffleTasks()
    {
        var reshuffled = await _taskTracker.ShuffleTasks();

        return Ok(new
        {
            tasks = reshuffled
        });
    }

    [Route("tasks/create")]
    public async Task<IActionResult> CreateTask([FromBody] string title, [FromBody] string description)
    {
        var taskData = await _taskTracker.CreateTask(title, description);
        return Ok(new
        {
            task = taskData
        });
    }

    [Route("tasks/complete")]
    public async Task<IActionResult> CompleteTask([FromBody] string taskId)
    {
        await _taskTracker.CompleteTask(taskId);
        return Ok();
    }
}
