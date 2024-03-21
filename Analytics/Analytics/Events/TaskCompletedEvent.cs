namespace Analytics.Events;

public class TaskCompletedEvent
{
    public string Id { get; set; }
    
    public string Title { get; set; }
    
    public string AssigneeId { get; set; }
    
    public DateTime CompletedTime { get; set; }
}