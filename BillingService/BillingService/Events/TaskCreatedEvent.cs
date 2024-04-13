namespace BillingService.Events;

public class TaskCreatedEvent
{
    public string Id { get; set; }
    
    public string Title { get; set; }
    public string AssigneeId { get; set; }
}