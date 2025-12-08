using Microsoft.AspNetCore.Mvc;
using TaskManager.Frontend.Models;
using TaskManager.Frontend.Services;

namespace TaskManager.Frontend.Controllers;

public class TasksController : Controller
{
    private readonly TaskApiService _taskApiService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(TaskApiService taskApiService, ILogger<TasksController> logger)
    {
        _taskApiService = taskApiService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var tasks = await _taskApiService.GetAllTasksAsync();
            var statistics = await _taskApiService.GetStatisticsAsync();
            ViewBag.Statistics = statistics;
            return View(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tasks");
            TempData["Error"] = "Erro ao carregar tarefas";
            return View(new List<TaskItem>());
        }
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskItem task)
    {
        if (!ModelState.IsValid)
        {
            return View(task);
        }

        try
        {
            task.UserId = "default-user";
            var created = await _taskApiService.CreateTaskAsync(task);
            if (created != null)
            {
                TempData["Success"] = "Tarefa criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            
            ModelState.AddModelError("", "Erro ao criar tarefa");
            return View(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            ModelState.AddModelError("", "Erro ao criar tarefa");
            return View(task);
        }
    }

    public async Task<IActionResult> Edit(long id)
    {
        try
        {
            var task = await _taskApiService.GetTaskByIdAsync(id);
            if (task == null)
            {
                TempData["Error"] = "Tarefa não encontrada";
                return RedirectToAction(nameof(Index));
            }
            return View(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading task {TaskId}", id);
            TempData["Error"] = "Erro ao carregar tarefa";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, TaskItem task)
    {
        if (id != task.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(task);
        }

        try
        {
            var updated = await _taskApiService.UpdateTaskAsync(task);
            if (updated != null)
            {
                TempData["Success"] = "Tarefa atualizada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Tarefa não encontrada";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", id);
            ModelState.AddModelError("", "Erro ao atualizar tarefa");
            return View(task);
        }
    }

    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            var task = await _taskApiService.GetTaskByIdAsync(id);
            if (task == null)
            {
                TempData["Error"] = "Tarefa não encontrada";
                return RedirectToAction(nameof(Index));
            }
            return View(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading task {TaskId}", id);
            TempData["Error"] = "Erro ao carregar tarefa";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        try
        {
            var deleted = await _taskApiService.DeleteTaskAsync(id);
            if (deleted)
            {
                TempData["Success"] = "Tarefa excluída com sucesso!";
            }
            else
            {
                TempData["Error"] = "Tarefa não encontrada";
            }
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            TempData["Error"] = "Erro ao excluir tarefa";
            return RedirectToAction(nameof(Index));
        }
    }
}

