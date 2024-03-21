using Analytics.Models;
using Analytics.Settings;
using MongoDB.Driver;
using TaskStatus = Analytics.Models.TaskStatus;

namespace Analytics.Services;

public class DbService
{
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<TaskData> _tasks;
    private readonly IMongoCollection<BalanceDiffLogRecord> _balanceDiff;
    private readonly MongoClient _mongoClient;

    public DbService(IConfiguration config)
    {
        DbSettings? settings = config.GetSection("DbSettings").Get<DbSettings>();

        if (settings == null)
        {
            throw new Exception("Not found db settings");
        }

        _mongoClient = new MongoClient(settings.Connection);
        var database = _mongoClient.GetDatabase(settings.DbName);
        _users = database.GetCollection<User>(settings.UsersCollection);
        _tasks = database.GetCollection<TaskData>(settings.TasksCollection);
        _balanceDiff = database.GetCollection<BalanceDiffLogRecord>(settings.BalanceDiffLogCollection);
    }

    public async Task<User?> GetUserAsync(string userId)
    {
        return await _users.Find(user => user.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<long> CountUsersWithNegativeBalance()
    {
        return await _users.CountDocumentsAsync(user => user.Balance < 0);
    }

    public async Task CreateUserAsync(User user)
    {
        await _users.InsertOneAsync(user);
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

    public async Task<UpdateResult> UpdateUserBalanceAsync(string userId, int newBalance)
    {
        var updater = Builders<User>.Update.Set(user => user.Balance, newBalance);

        return await _users.UpdateOneAsync(user => user.UserId == userId, updater);
    }

    public async Task UpdateBalanceDiffLogAsync(BalanceDiffLogRecord newRecord)
    {
        await _balanceDiff.InsertOneAsync(newRecord);
    }

    public async Task<List<BalanceDiffLogRecord>> GetRecordsForPeriod(DateTime fromDate, DateTime toDate)
    {
        return await _balanceDiff.Find(record => record.CreatedAt <= toDate && record.CreatedAt >= fromDate).ToListAsync();
    }

    public async Task<List<TaskData>> GetCompletedTasksForPeriod(DateTime fromDate, DateTime toDate)
    {
        return await _tasks.Find(data =>
                data.Status == TaskStatus.Completed && data.CompletedTime <= toDate && data.CompletedTime >= fromDate)
            .ToListAsync();
    }
}
