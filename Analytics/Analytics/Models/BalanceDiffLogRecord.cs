namespace Analytics.Models;

public class BalanceDiffLogRecord
{
    public string UserId { get; set; }
    
    public int BalanceDiff { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
