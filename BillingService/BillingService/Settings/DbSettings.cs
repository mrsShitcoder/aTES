namespace BillingService.Settings;

public class DbSettings
{
    public string Connection { get; set; }
    public string DbName { get; set; }
    
    public string AccountsCollection { get; set; }
    public string TasksCollection { get; set; }
    
    public string AuditCollection { get; set; }
}
