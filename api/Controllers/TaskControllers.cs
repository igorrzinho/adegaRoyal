using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdegaRoyal.Api.Services;
using System.Security.Claims;
using AdegaRoyal.Api.DTOs;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var tasks = await taskService.GetTaskByUser(userId);
        var taskDtos = tasks.Select(t => new TaskDto
        {
            Id = t.Id,
            Title = t.Title,
            IsCompleted = t.IsCompleted,
            CreatedAt = t.CreatedAt
        }).ToList();

        if (!taskDtos.Any())
        {
            return Ok(new { message = "Nenhuma tarefa encontrada.", tasks = taskDtos });
        }

        return Ok(taskDtos);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] string title)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await taskService.CreateTask(title, userId);
        return Ok();
    }
    [HttpPatch("{id:guid}/toggle-complete")]
    public async Task<IActionResult> ToggleComplete(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var task = await taskService.CompleteTask(id, userId);

        if (task == null)
        {
            return NotFound("Tarefa não encontrada ou você não tem permissão para editá-la.");
        }

        var taskDto = new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt
        };

        return Ok(taskDto);
    }
}
