using System.Text.Json;
using MongoDB.Bson;
using TaskTracker.Events;
using TaskTracker.Models;
using TaskStatus = TaskTracker.Models.TaskStatus;

namespace TaskTracker.Services;

public class TaskTrackerService
{
    private readonly DbService _dbService;

    private readonly KafkaProducer _kafkaProducer;
    
    public TaskTrackerService(DbService dbService, KafkaProducer kafkaProducer, EventBus eventBus)
    {
        _dbService = dbService;
        _kafkaProducer = kafkaProducer;
        eventBus.Subscribe<UserCreatedEvent>(OnUserCreatedAsync);
    }

    public async Task<TaskData> CreateTask(string title, string description)
    {
        var assignees = await _dbService.GetUsersByRole("Worker");
        if (assignees == null)
        {
            throw new Exception("Failed to create task. No available assignees");
        }

        var randomInd = new Random().Next(0, assignees.Count - 1);

        string assigneeId = assignees[randomInd].Id;

        var newTask = new TaskData
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Title = title,
            AssigneeId = assigneeId,
            Description = description,
            Status = TaskStatus.Assigned,
            CreatedTime = DateTime.Now
        };

        await _dbService.AddTaskAsync(newTask);

        var kafkaEvent = new TaskCreatedEvent
        {
            Id = newTask.Id,
            Title = newTask.Title,
            AssigneeId = newTask.AssigneeId
        };

        await _kafkaProducer.ProduceAsync("task-stream", JsonSerializer.Serialize(kafkaEvent));

        return newTask;
    }

    public async Task CompleteTask(string taskId)
    {
        var task = await _dbService.GetTaskAsync(taskId);
        if (task == null)
        {
            throw new Exception($"Task {taskId} not found");
        }

        if (task.Status == TaskStatus.Completed)
        {
            throw new Exception($"Task {taskId} is already completed");
        }

        task.Status = TaskStatus.Completed;
        task.CompletedTime = DateTime.Now;

        var result = await _dbService.UpdateTaskAsync(task);

        if (result.ModifiedCount == 0)
        {
            throw new Exception($"Failed to complete task {taskId}");
        }

        var taskCompleted = new TaskCompletedEvent
        {
            Id = task.Id,
            Title = task.Title,
            AssigneeId = task.AssigneeId
        };

        await _kafkaProducer.ProduceAsync("task-events", JsonSerializer.Serialize(taskCompleted));
    }

    public async Task<List<TaskData>> GetTasksByAssignee(string assigneeId)
    {
        var tasks = await _dbService.GetTasks(assigneeId);
        if (tasks == null)
        {
            return new List<TaskData>();
        }

        return tasks;
    }

    public async Task OnUserCreatedAsync(UserCreatedEvent consumedEvent)
    {
        var newUser = new User
        {
            Id = consumedEvent.UserId,
            Name = consumedEvent.Name,
            Role = consumedEvent.Role
        };
        await _dbService.AddUserAsync(newUser);
    }

    public async Task<List<TaskData>> ShuffleTasks()
    {
        var tasks = await _dbService.GetAssignedTasks();

        var assignees = await _dbService.GetUsersByRole("Worker");

        if (assignees == null || assignees.Count == 0)
        {
            throw new Exception($"No workers to assign tasks");
        }

        var rand = new Random();
        foreach (var task in tasks)
        {
            var randomInd = rand.Next(0, assignees.Count - 1);
            string assigneeId = assignees[randomInd].Id;
            task.AssigneeId = assigneeId;
            await _dbService.UpdateTaskAsync(task);

            var taskReassigned = new TaskReassignedEvent
            {
                TaskId = task.Id,
                AssigneeId = task.AssigneeId
            };

            await _kafkaProducer.ProduceAsync("task-events", JsonSerializer.Serialize(taskReassigned));
        }

        return await _dbService.GetAssignedTasks();
    }
}