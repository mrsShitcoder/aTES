namespace Analytics.Events;

public class AccountBalanceChangedEvent
{
    public string AccountId { get; set; }
    
    public int CurrentBalance { get; set; }
    
    public int BalanceDiff { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
