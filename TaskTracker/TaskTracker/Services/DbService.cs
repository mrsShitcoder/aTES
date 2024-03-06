using MongoDB.Driver;
using TaskTracker.Models;
using TaskTracker.Settings;
using TaskStatus = TaskTracker.Models.TaskStatus;

namespace TaskTracker.Services;

public class DbService
{
    private IMongoCollection<User> _users;
    private IMongoCollection<TaskData> _tasks;
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
        _users = database.GetCollection<User>(settings.UsersCollection);
        _tasks = database.GetCollection<TaskData>(settings.TasksCollection);
    }

    public async Task<User?> GetUserAsync(string userId)
    {
        return await _users.Find(user => user.Id == userId).FirstOrDefaultAsync();
    }

    public async Task AddUserAsync(User user)
    {
        await _users.InsertOneAsync(user);
    }

    public async Task AddTaskAsync(TaskData taskData)
    {
        await _tasks.InsertOneAsync(taskData);
    }

    public async Task<TaskData> GetMyTasksAsync(string userId)
    {
        return await _tasks.Find(data => data.AssigneeId == userId).FirstOrDefaultAsync();
    }

    public async Task AssignTaskAsync(string taskId, string userId)
    {
        User? user = await GetUserAsync(userId);
        if (user == null)
        {
            throw new Exception($"Cannot assign task. User {userId} not found");
        }
        var assignTask = Builders<TaskData>.Update.Set(data => data.AssigneeId, userId);
        await _tasks.UpdateOneAsync(data => data.Id == taskId, assignTask);
    }

    public async Task ChangeTaskStatusAsync(string taskId, TaskStatus status)
    {
        var changeStatus = Builders<TaskData>.Update.Set(data => data.Status, status);
        await _tasks.UpdateOneAsync(data => data.Id == taskId, changeStatus);
    }
}
