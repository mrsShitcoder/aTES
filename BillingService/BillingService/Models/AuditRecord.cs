namespace BillingService.Models;

public enum EventType
{
    Credit = 1,
    Withdrawal = 2,
    Checkout = 3
}

public class AuditRecord
{
    public string AccountId { get; set; }
    public EventType EventType { get; set; }
    public int Amount { get; set; }
    
    public DateTime CreatedAt { get; set; }
}