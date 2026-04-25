using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KeycloakAuth.Entities;

namespace KeycloakAuth.Services;

public interface ITaskService
{
    Task<List<TaskItem>> GetTaskByUser(Guid UserId);
    Task CreateTask(string Title, Guid UserId);    
    Task<TaskItem?> CompleteTask(Guid TaskId, Guid UserId);
}
