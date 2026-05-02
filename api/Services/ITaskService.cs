using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdegaRoyal.Api.Entities;

namespace AdegaRoyal.Api.Services;

public interface ITaskService
{
    Task<List<TaskItem>> GetTaskByUser(Guid UserId);
    Task CreateTask(string Title, Guid UserId);    
    Task<TaskItem?> CompleteTask(Guid TaskId, Guid UserId);
}
