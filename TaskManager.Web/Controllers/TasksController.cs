using Microsoft.AspNetCore.Mvc;
using TaskManager.Web.DTOs;
using TaskManager.Web.Models;
using TaskManager.Web.Services;

namespace TaskManager.Web.Controllers;

public class TasksController : Controller
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    // GET: /Tasks
    public async Task<IActionResult> Index()
    {
        try
        {
            var result = await _taskService.GetAllTasksAsync();
            if (result.IsSuccess)
            {
                return View(result.Value);
            }
            
            _logger.LogError("Erro ao carregar lista de tarefas: {Error}", result.Error?.Message);
            TempData["Error"] = result.Error?.Message ?? "Erro ao carregar tarefas";
            return View(new List<TaskDto>());
        }
        catch (Exception ex)
        {
            // Log sem informações sensíveis
            _logger.LogError(ex, "Erro ao carregar lista de tarefas");
            TempData["Error"] = "Erro ao carregar tarefas. Por favor, tente novamente.";
            return View(new List<TaskDto>());
        }
    }

    // GET: /Tasks/Create
    public IActionResult Create()
    {
        return View(new CreateTaskDto());
    }

    // POST: /Tasks/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTaskDto createDto)
    {
        // Validações adicionais de segurança
        if (createDto != null)
        {
            // Sanitizar entrada: limitar tamanho
            if (!string.IsNullOrEmpty(createDto.Title) && createDto.Title.Length > 200)
            {
                ModelState.AddModelError(nameof(createDto.Title), "O título não pode exceder 200 caracteres");
            }

            if (!string.IsNullOrEmpty(createDto.Description) && createDto.Description.Length > 2000)
            {
                ModelState.AddModelError(nameof(createDto.Description), "A descrição não pode exceder 2000 caracteres");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(createDto);
        }

        try
        {
            // Por enquanto, usar ID de usuário padrão (sem autenticação)
            // TODO: Implementar autenticação e obter usuário do contexto
            if (createDto == null)
            {
                ModelState.AddModelError("", "Dados inválidos");
                return View(new CreateTaskDto());
            }

            createDto.UserId = User.Identity?.Name ?? "default-user";
            
            var result = await _taskService.CreateTaskAsync(createDto);
            if (result.IsSuccess)
            {
                TempData["Success"] = "Tarefa criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            if (result.Error?.ValidationErrors != null)
            {
                foreach (var error in result.Error.ValidationErrors)
                {
                    foreach (var message in error.Value)
                    {
                        ModelState.AddModelError(error.Key, message);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", result.Error?.Message ?? "Erro ao criar tarefa");
            }
            
            return View(createDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar tarefa");
            ModelState.AddModelError("", "Erro ao criar tarefa");
            return View(createDto);
        }
    }

    // GET: /Tasks/Edit/5
    public async Task<IActionResult> Edit(long id)
    {
        // Validar ID
        if (id <= 0)
        {
            TempData["Error"] = "ID inválido";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var result = await _taskService.GetTaskByIdAsync(id);
            if (result.IsSuccess && result.Value != null)
            {
                var updateDto = new UpdateTaskDto
                {
                    Id = result.Value.Id,
                    Title = result.Value.Title,
                    Description = result.Value.Description,
                    Priority = result.Value.Priority,
                    Category = result.Value.Category,
                    DueDate = result.Value.DueDate,
                    Tags = result.Value.Tags,
                    AssignedTo = result.Value.AssignedTo,
                    Completed = result.Value.Completed,
                    RowVersion = result.Value.RowVersion
                };
                return View(updateDto);
            }
            
            TempData["Error"] = result.Error?.Message ?? "Tarefa não encontrada";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar tarefa {TaskId}", id);
            TempData["Error"] = "Erro ao carregar tarefa";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: /Tasks/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, UpdateTaskDto updateDto)
    {
        if (id != updateDto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(updateDto);
        }

        try
        {
            var result = await _taskService.UpdateTaskAsync(id, updateDto);
            if (result.IsSuccess)
            {
                TempData["Success"] = "Tarefa atualizada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            if (result.Error?.ValidationErrors != null)
            {
                foreach (var error in result.Error.ValidationErrors)
                {
                    foreach (var message in error.Value)
                    {
                        ModelState.AddModelError(error.Key, message);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", result.Error?.Message ?? "Erro ao atualizar tarefa");
            }
            
            return View(updateDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar tarefa {TaskId}", id);
            ModelState.AddModelError("", "Erro ao atualizar tarefa");
            return View(updateDto);
        }
    }

    // GET: /Tasks/Delete/5
    public async Task<IActionResult> Delete(long id)
    {
        // Validar ID
        if (id <= 0)
        {
            TempData["Error"] = "ID inválido";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var result = await _taskService.GetTaskByIdAsync(id);
            if (result.IsSuccess && result.Value != null)
            {
                return View(result.Value);
            }
            
            TempData["Error"] = "Tarefa não encontrada";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            // Log sem informações sensíveis
            _logger.LogError(ex, "Erro ao carregar tarefa para exclusão. ID: {TaskId}", id);
            TempData["Error"] = "Erro ao carregar tarefa. Por favor, tente novamente.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: /Tasks/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        // Validar ID
        if (id <= 0)
        {
            TempData["Error"] = "ID inválido";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var result = await _taskService.DeleteTaskAsync(id);
            if (result.IsSuccess)
            {
                TempData["Success"] = "Tarefa excluída com sucesso!";
            }
            else
            {
                TempData["Error"] = "Erro ao excluir tarefa. Por favor, tente novamente.";
            }
            
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            // Log sem informações sensíveis
            _logger.LogError(ex, "Erro ao excluir tarefa. ID: {TaskId}", id);
            TempData["Error"] = "Erro ao excluir tarefa. Por favor, tente novamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
