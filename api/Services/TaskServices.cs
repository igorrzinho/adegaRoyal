using KeycloakAuth.Data;
using KeycloakAuth.Entities;
using Microsoft.EntityFrameworkCore;

namespace KeycloakAuth.Services;

/// <summary>
/// Legacy task management service — kept for backward compatibility.
/// Uses context.Set&lt;TaskItem&gt;() since the Tasks DbSet was removed from AppDbContext.
/// </summary>
public class TaskServices(AppDbContext context) : ITaskService
{
    private Microsoft.EntityFrameworkCore.DbSet<TaskItem> Tasks => context.Set<TaskItem>();

    public async Task<List<TaskItem>> GetTaskByUser(Guid UserId)
    {
        return await Tasks
            .Where(t => t.UserId == UserId)
            .ToListAsync();
    }

    public async Task CreateTask(string Title, Guid UserId)
    {
        var task = new TaskItem
        {
            Title = Title,
            UserId = UserId
        };

        Tasks.Add(task);
        await context.SaveChangesAsync();
    }

    public async Task<TaskItem?> CompleteTask(Guid TaskId, Guid UserId)
    {
        var task = await Tasks.FirstOrDefaultAsync(t => t.Id == TaskId && t.UserId == UserId);

        if (task == null) return null;

        task.IsCompleted = !task.IsCompleted;
        await context.SaveChangesAsync();
        return task;
    }
}