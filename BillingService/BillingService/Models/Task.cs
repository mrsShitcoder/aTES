using System.Runtime.InteropServices.JavaScript;

namespace BillingService.Models;

public enum TaskStatus
{
    Assigned = 1,
    Completed = 2
}

public class TaskData
{
    public string Id { get; set; }
    
    public string Title { get; set; }
    public TaskStatus Status { get; set; }
    public string AssigneeId { get; set; }
    
    public int AssignPrice { get; set; }
    
    public int CompletePrice { get; set; }
    
    public DateTime CreatedTime { get; set; }
    
    public DateTime CompletedTime { get; set; }
}
