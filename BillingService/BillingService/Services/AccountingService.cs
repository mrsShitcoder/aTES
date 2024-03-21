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
    
    private Timer _timer;

    public AccountingService(DbService dbService, KafkaProducer kafkaProducer, EventBus eventBus)
    {
        _dbService = dbService;
        _kafkaProducer = kafkaProducer;
        eventBus.Subscribe<UserCreatedEvent>(OnUserCreated);
        eventBus.Subscribe<TaskCreatedEvent>(OnTaskCreated);
        eventBus.Subscribe<TaskCompletedEvent>(OnTaskCompleted);
        eventBus.Subscribe<TaskReassignedEvent>(OnTaskReassigned);
        InitTimer();
    }

    private void InitTimer()
    {
        var now = DateTime.Now;
        var timeToMidnight = (TimeSpan.FromDays(1) - now.TimeOfDay).TotalMilliseconds;
        _timer = new Timer(Checkout, null, (int)timeToMidnight, (int)TimeSpan.FromDays(1).TotalMilliseconds);
    }

    private void Checkout(object? state)
    {
        Task.Run(async () =>
        {
            var accounts = await _dbService.GetAccountsListAsync();
            if (accounts == null)
            {
                return;
            }

            foreach (var account in accounts)
            {
                if (account.Balance < 0)
                {
                    continue;
                }

                await _dbService.UpdateAuditLogAsync(new AuditRecord
                {
                    AccountId = account.AccountId,
                    Amount = account.Balance,
                    CreatedAt = DateTime.Now,
                    EventType = EventType.Checkout
                });
                
                await _dbService.UpdateAccountBalanceAsync(account.AccountId, 0);
            }
        });
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
        await _dbService.UpdateAccountBalanceAsync(account.AccountId, account.Balance - assignPrice);
        await _dbService.UpdateAuditLogAsync(new AuditRecord
        {
            AccountId = account.AccountId,
            Amount = assignPrice,
            EventType = EventType.Withdrawal,
            CreatedAt = DateTime.Now
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
        
        await _dbService.UpdateAccountBalanceAsync(account.AccountId, account.Balance + task.CompletePrice);
        await _dbService.UpdateTaskAsync(task);
        await _dbService.UpdateAuditLogAsync(new AuditRecord
        {
            AccountId = account.AccountId,
            Amount = task.CompletePrice,
            EventType = EventType.Credit,
            CreatedAt = DateTime.Now
        });
    }

    public async Task OnTaskReassigned(TaskReassignedEvent consumedEvent)
    {
        var account = await _dbService.GetAccountAsync(consumedEvent.AssigneeId);

        if (account == null)
        {
            throw new Exception($"Account {consumedEvent.AssigneeId} not found");
        }

        var task = await _dbService.GetTaskAsync(consumedEvent.TaskId);
        
        if (task == null)
        {
            throw new Exception($"Task {consumedEvent.TaskId} not found");
        }

        await _dbService.UpdateAccountBalanceAsync(account.AccountId, account.Balance - task.AssignPrice);
        await _dbService.UpdateAuditLogAsync(new AuditRecord
        {
            AccountId = account.AccountId,
            Amount = task.AssignPrice,
            EventType = EventType.Withdrawal,
            CreatedAt = DateTime.Now
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

    public async Task<int> GetRevenueForPeriod(DateTime fromDate, DateTime toDate)
    {
        int revenue = 0;

        var records = await _dbService.GetRecordsForPeriod(fromDate, toDate);

        foreach (var record in records)
        {
            if (record.EventType == EventType.Credit)
            {
                revenue -= record.Amount;
            }
            else if (record.EventType == EventType.Withdrawal)
            {
                revenue += record.Amount;
            }
        }

        return revenue;
    }
}
