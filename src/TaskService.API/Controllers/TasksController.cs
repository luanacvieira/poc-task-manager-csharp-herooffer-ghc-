using Microsoft.AspNetCore.Mvc;
using TaskService.API.Models;
using TaskService.API.Services;

namespace TaskService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetAll()
    {
        try
        {
            var tasks = await _taskService.GetAllTasksAsync();
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar tarefas");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetById(long id)
    {
        try
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound(new { error = "Tarefa não encontrada" });

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar tarefa {TaskId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> Create([FromBody] TaskItem task)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            task.UserId = "default-user";
            var created = await _taskService.CreateTaskAsync(task);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar tarefa");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TaskItem>> Update(long id, [FromBody] TaskItem task)
    {
        if (id != task.Id)
            return BadRequest(new { error = "ID não corresponde" });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var updated = await _taskService.UpdateTaskAsync(task);
            if (updated == null)
                return NotFound(new { error = "Tarefa não encontrada" });

            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar tarefa {TaskId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            var deleted = await _taskService.DeleteTaskAsync(id);
            if (!deleted)
                return NotFound(new { error = "Tarefa não encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir tarefa {TaskId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<TaskStatistics>> GetStatistics()
    {
        try
        {
            var stats = await _taskService.GetStatisticsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar estatísticas");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }
}
