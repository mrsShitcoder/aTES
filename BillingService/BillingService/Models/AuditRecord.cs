namespace BillingService.Models;

public enum EventType
{
    Credit = 1,
    Withdraw = 2
}

public class AuditRecord
{
    public string AccountId { get; set; }
    public EventType EventType { get; set; }
    public int Amount { get; set; }
}