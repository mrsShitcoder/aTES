using BillingService.Models;
using BillingService.Settings;
using MongoDB.Driver;

using TaskStatus = BillingService.Models.TaskStatus;

namespace BillingService.Services;

public class DbService
{
    private IMongoCollection<Account> _accounts;
    private IMongoCollection<TaskData> _tasks;
    private IMongoCollection<AuditRecord> _auditLog;
    private MongoClient _mongoClient;

    public DbService(IConfiguration config)
    {
        DbSettings? settings = config.GetSection("DbSettings").Get<DbSettings>();

        if (settings == null)
        {
            throw new Exception("Not found db settings");
        }

        _mongoClient = new MongoClient(settings.Connection);
        var database = _mongoClient.GetDatabase(settings.DbName);
        _accounts = database.GetCollection<Account>(settings.AccountsCollection);
        _tasks = database.GetCollection<TaskData>(settings.TasksCollection);
        _auditLog = database.GetCollection<AuditRecord>(settings.AuditCollection);
    }

    public async Task<Account?> GetAccountAsync(string accountId)
    {
        return await _accounts.Find(account => account.AccountId == accountId).FirstOrDefaultAsync();
    }

    public async Task<List<Account>?> GetAccountsListAsync()
    {
        return await _accounts.Find(account => true).ToListAsync();
    }

    public async Task CreateAccountAsync(Account account)
    {
        await _accounts.InsertOneAsync(account);
    }

    public async Task AddTaskAsync(TaskData taskData)
    {
        await _tasks.InsertOneAsync(taskData);
    }

    public async Task<TaskData?> GetTaskAsync(string taskId)
    {
        return await _tasks.Find(data => data.Id == taskId).FirstOrDefaultAsync();
    }

    public async Task<UpdateResult> UpdateTaskAsync(TaskData updatedData)
    {
        var updater = Builders<TaskData>.Update.Set(data => data, updatedData);
        return await _tasks.UpdateOneAsync(data => data.Id == updatedData.Id, updater);
    }

    public async Task<UpdateResult> UpdateAccountBalance(string accountId, int newBalance)
    {
        var updater = Builders<Account>.Update.Set(account => account.Balance, newBalance);

        return await _accounts.UpdateOneAsync(account => account.AccountId == accountId, updater);
    }

    public async Task UpdateAuditLog(AuditRecord newRecord)
    {
        await _auditLog.InsertOneAsync(newRecord);
    }

    public async Task<List<TaskData>> GetAssignedTasksForPeriod(DateTime from, DateTime to)
    {
        return await _tasks.Find(data =>
            data.Status == TaskStatus.Assigned && data.CreatedTime <= to && data.CreatedTime >= from).ToListAsync();
    }

    public async Task<List<TaskData>> GetCompletedTasksForPeriod(DateTime from, DateTime to)
    {
        return await _tasks.Find(data =>
                data.Status == TaskStatus.Completed && data.CompletedTime <= to && data.CompletedTime >= from)
            .ToListAsync();
    }
}
