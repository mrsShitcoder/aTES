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

    public async Task<List<User>?> GetUsersByRole(string role)
    {
        return await _users.Find(user => user.Role == role).ToListAsync();
    }

    public async Task AddUserAsync(User user)
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

    public async Task<List<TaskData>?> GetTasks(string assigneeId)
    {
        return await _tasks.Find(data => data.AssigneeId == assigneeId).ToListAsync();
    }

    public async Task<UpdateResult> UpdateTaskAsync(TaskData taskData)
    {
        var changeStatus = Builders<TaskData>.Update.Set(data => data, taskData);
        return await _tasks.UpdateOneAsync(data => data.Id == taskData.Id, changeStatus);
    }

    public async Task<List<TaskData>> GetAssignedTasks()
    {
        return await _tasks.Find(data => data.Status == TaskStatus.Assigned).ToListAsync();
    }
}
