namespace BillingService.Events;

public class TaskReassignedEvent
{
    public string TaskId { get; set; }
    
    public string AssigneeId { get; set; }
}
