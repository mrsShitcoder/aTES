namespace TaskTracker.Models;

public enum TaskStatus
{
    Assigned = 1,
    Completed = 2
}

public class TaskData
{
    public string Id { get; set; }
    public string Description { get; set; }
    public TaskStatus Status { get; set; }
    public string AssigneeId { get; set; }
}
