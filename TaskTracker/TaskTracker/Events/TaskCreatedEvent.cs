namespace TaskTracker.Events;

public class TaskCreatedEvent
{
    public string Id { get; set; }
    public string AssigneeId { get; set; }
}