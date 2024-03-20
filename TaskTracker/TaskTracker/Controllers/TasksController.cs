using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Services;

namespace TaskTracker.Controllers;
[Authorize]
public class TasksController : Controller
{
    private readonly TaskTrackerService _taskTracker;

    [Route("tasks/all")]
    public async Task<IActionResult> GetTaskList([FromBody] string userId)
    {
        var taskList = await _taskTracker.GetTasksByAssignee(userId);

        return Ok(new
        {
            tasks = taskList
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
