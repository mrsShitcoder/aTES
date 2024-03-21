namespace Analytics.Settings;

public class DbSettings
{
    public string Connection { get; set; }
    public string DbName { get; set; }
    
    public string UsersCollection { get; set; }
    public string TasksCollection { get; set; }
    
    public string BalanceDiffLogCollection { get; set; }
}
