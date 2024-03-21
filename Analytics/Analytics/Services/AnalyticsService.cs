using Analytics.Events;
using Analytics.Models;

namespace Analytics.Services;

public class AnalyticsService
{
    private readonly DbService _dbService;
    
    public AnalyticsService(DbService dbService, EventBus eventBus)
    {
        _dbService = dbService;
        eventBus.Subscribe<UserCreatedEvent>(OnUserCreated);
        eventBus.Subscribe<AccountBalanceChangedEvent>(OnAccountBalanceChanged);
        eventBus.Subscribe<TaskCreatedEvent>(OnTaskCreated);
        eventBus.Subscribe<TaskPriceUpdatedEvent>(OnTaskPriceUpdated);
        eventBus.Subscribe<TaskCompletedEvent>(OnTaskCompleted);
    }

    private async Task OnUserCreated(UserCreatedEvent eventData)
    {
        await _dbService.CreateUserAsync(new User
        {
            UserId = eventData.UserId,
            Role = eventData.Role
        });
    }

    private async Task OnAccountBalanceChanged(AccountBalanceChangedEvent eventData)
    {
        var user = await _dbService.GetUserAsync(eventData.AccountId);

        if (user == null)
        {
            throw new Exception($"Not found user {eventData.AccountId}");
        }
        
        await _dbService.UpdateUserBalanceAsync(eventData.AccountId, eventData.CurrentBalance);
        await _dbService.UpdateBalanceDiffLogAsync(new BalanceDiffLogRecord
        {
            UserId = user.UserId,
            BalanceDiff = eventData.BalanceDiff,
            CreatedAt = eventData.CreatedAt
        });
    }

    private async Task OnTaskCreated(TaskCreatedEvent eventData)
    {
        await _dbService.AddTaskAsync(new TaskData
        {
            Id = eventData.Id
        });
    }

    private async Task OnTaskPriceUpdated(TaskPriceUpdatedEvent eventData)
    {
        var task = await _dbService.GetTaskAsync(eventData.TaskId);

        if (task == null)
        {
            throw new Exception($"Not found task {eventData.TaskId}");
        }

        task.CompletePrice = eventData.CompletePrice;

        await _dbService.UpdateTaskAsync(task);
    }

    private async Task OnTaskCompleted(TaskCompletedEvent eventData)
    {
        var task = await _dbService.GetTaskAsync(eventData.Id);

        if (task == null)
        {
            throw new Exception($"Not found task {eventData.Id}");
        }

        task.CompletedTime = eventData.CompletedTime;
    }

    public async Task<int> GetRevenueForPeriod(DateTime fromDate, DateTime toDate)
    {
        var records = await _dbService.GetRecordsForPeriod(fromDate, toDate);

        int revenue = 0;

        foreach (var record in records)
        {
            revenue -= record.BalanceDiff;
        }

        return revenue;
    }

    public async Task<TaskData?> GetMostExpensiveTaskForPeriod(DateTime fromDate, DateTime toDate)
    {
        var completedTasks = await _dbService.GetCompletedTasksForPeriod(fromDate, toDate);

        TaskData? mostExpensiveTask = null;
        
        foreach (var task in completedTasks)
        {
            if (mostExpensiveTask == null || task.CompletePrice > mostExpensiveTask.CompletePrice)
            {
                mostExpensiveTask = task;
            }
        }

        return mostExpensiveTask;
    }

    public async Task<long> GetUsersWithNegativeBalance()
    {
        return await _dbService.CountUsersWithNegativeBalance();
    }
    
}