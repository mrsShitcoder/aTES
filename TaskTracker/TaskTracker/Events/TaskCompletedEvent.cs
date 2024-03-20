namespace TaskTracker.Events;

public class TaskCompletedEvent
{
    public string Id { get; set; }
    
    public string AssigneeId { get; set; }
}