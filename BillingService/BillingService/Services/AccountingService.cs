using System.Text.Json;
using BillingService.Events;
using BillingService.Models;
using MongoDB.Bson;
using TaskStatus = BillingService.Models.TaskStatus;

namespace BillingService.Services;

public class AccountingService
{
    private readonly DbService _dbService;

    private readonly KafkaProducer _kafkaProducer;

    public AccountingService(DbService dbService, KafkaProducer kafkaProducer, EventBus eventBus)
    {
        _dbService = dbService;
        _kafkaProducer = kafkaProducer;
        eventBus.Subscribe<UserCreatedEvent>(OnUserCreated);
        eventBus.Subscribe<TaskCreatedEvent>(OnTaskCreated);
        eventBus.Subscribe<TaskCompletedEvent>(OnTaskCompleted);
    }

    public async Task OnUserCreated(UserCreatedEvent consumedEvent)
    {
        var newAccount = new Account
        {
            AccountId = consumedEvent.UserId,
            Balance = 0,
            Role = consumedEvent.Role
        };

        await _dbService.CreateAccountAsync(newAccount);
    }

    public async Task OnTaskCreated(TaskCreatedEvent consumedEvent)
    {
        var account = await _dbService.GetAccountAsync(consumedEvent.AssigneeId);

        if (account == null)
        {
            throw new Exception(
                $"Cannot process event {nameof(consumedEvent)}. No account with id {consumedEvent.AssigneeId}");
        }

        var rand = new Random();
        var assignPrice = rand.Next(10, 20);
        var completePrice = rand.Next(20, 40);
        var newTask = new TaskData
        {
            Id = consumedEvent.Id,
            AssigneeId = consumedEvent.AssigneeId,
            Status = TaskStatus.Assigned,
            Title = consumedEvent.Title,
            AssignPrice = assignPrice,
            CompletePrice = completePrice
        };
        
        await _dbService.AddTaskAsync(newTask);
        await _dbService.UpdateAccountBalance(account.AccountId, account.Balance - assignPrice);
        await _dbService.UpdateAuditLog(new AuditRecord
        {
            AccountId = account.AccountId,
            Amount = assignPrice,
            EventType = EventType.Withdraw
        });

        var eventToProduce = new TaskPriceUpdatedEvent
        {
            TaskId = newTask.Id,
            AssignPrice = newTask.AssignPrice,
            CompletePrice = newTask.CompletePrice
        };

        await _kafkaProducer.ProduceAsync("task-stream",JsonSerializer.Serialize(eventToProduce));
    }

    public async Task OnTaskCompleted(TaskCompletedEvent consumedEvent)
    {
        var task = await _dbService.GetTaskAsync(consumedEvent.Id);
        if (task == null)
        {
            throw new Exception($"Task {consumedEvent.Id} not found");
        }

        if (task.Status == TaskStatus.Completed)
        {
            throw new Exception($"Task {task.Id} is already completed");
        }

        var account = await _dbService.GetAccountAsync(consumedEvent.AssigneeId);
        if (account == null)
        {
            throw new Exception($"Account {consumedEvent.AssigneeId} not found");
        }

        task.Status = TaskStatus.Completed;
        
        await _dbService.UpdateAccountBalance(account.AccountId, account.Balance + task.CompletePrice);
        await _dbService.UpdateTaskAsync(task);
        await _dbService.UpdateAuditLog(new AuditRecord
        {
            AccountId = account.AccountId,
            Amount = task.CompletePrice,
            EventType = EventType.Credit
        });
    }

    public async Task<Account> GetAccountData(string accountId)
    {
        var account = await _dbService.GetAccountAsync(accountId);

        if (account == null)
        {
            throw new Exception($"Account {accountId} not found");
        }

        return account;
    }

    public async Task<List<Account>> GetAccountsList()
    {
        var accounts = await _dbService.GetAccountsListAsync();
        if (accounts == null)
        {
            return new List<Account>();
        }

        return accounts;
    }

    public async Task<int> GetRevenueForPeriod(DateTime from, DateTime to)
    {
        var assignedTasks = await _dbService.GetAssignedTasksForPeriod(from, to);
        var completedTasks = await _dbService.GetCompletedTasksForPeriod(from, to);
        int assignedTaskFee = 0;
        int completedTaskAmount = 0;

        foreach (var task in assignedTasks)
        {
            assignedTaskFee += task.AssignPrice;
        }

        foreach (var task in completedTasks)
        {
            completedTaskAmount += task.CompletePrice;
        }

        return assignedTaskFee - completedTaskAmount;
    }
}