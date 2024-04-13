namespace BillingService.Events;

public class TaskPriceUpdatedEvent
{
    public string TaskId { get; set; }
    
    public int AssignPrice { get; set; }
    
    public int CompletePrice { get; set; }
}
