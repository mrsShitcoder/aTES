using System.Runtime.InteropServices.JavaScript;

namespace Analytics.Models;

public enum TaskStatus
{
    Assigned = 1,
    Completed = 2
}

public class TaskData
{
    public string Id { get; set; }
    
    public TaskStatus Status { get; set; }
    
    public int CompletePrice { get; set; }
    
    public DateTime CompletedTime { get; set; }
}
