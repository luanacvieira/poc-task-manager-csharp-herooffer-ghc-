using Microsoft.AspNetCore.Mvc;
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
            var tasks = await _taskService.GetAllTasksAsync();
            return View(new TasksIndexViewModel
            {
                Tasks = tasks.ToList(),
                NewTask = new TaskItem()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar lista de tarefas");
            return View(new TasksIndexViewModel
            {
                Tasks = new List<TaskItem>(),
                NewTask = new TaskItem(),
                ErrorMessage = "Erro ao carregar tarefas"
            });
        }
    }

    // GET: /Tasks/Create
    public IActionResult Create()
    {
        return RedirectToAction(nameof(Index));
    }

    // POST: /Tasks/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TasksIndexViewModel viewModel)
    {
        var task = viewModel.NewTask;

        task.UserId = "default-user";
        ModelState.Remove("NewTask.UserId");

        if (!ModelState.IsValid)
        {
            viewModel.Tasks = (await _taskService.GetAllTasksAsync()).ToList();
            viewModel.ErrorMessage = "Não foi possível incluir a tarefa. Corrija os campos e tente novamente.";
            return View(nameof(Index), viewModel);
        }

        try
        {
            var currentTasks = (await _taskService.GetAllTasksAsync()).ToList();

            task.CreatedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;
            await _taskService.CreateTaskAsync(task);
            return View(nameof(Index), new TasksIndexViewModel
            {
                Tasks = currentTasks,
                NewTask = new TaskItem(),
                SuccessMessage = "Tarefa incluída. A lista abaixo nao foi atualizada. Recarregue a pagina para ver o novo item."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar tarefa");
            viewModel.Tasks = (await _taskService.GetAllTasksAsync()).ToList();
            viewModel.ErrorMessage = "Erro ao criar tarefa";
            return View(nameof(Index), viewModel);
        }
    }

    // GET: /Tasks/Edit/5
    public async Task<IActionResult> Edit(long id)
    {
        try
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                TempData["Error"] = "Tarefa não encontrada";
                return RedirectToAction(nameof(Index));
            }
            return View(task);
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
            var updated = await _taskService.UpdateTaskAsync(task);
            if (updated == null)
            {
                TempData["Error"] = "Tarefa não encontrada";
                return RedirectToAction(nameof(Index));
            }
            TempData["Success"] = "Tarefa atualizada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar tarefa {TaskId}", id);
            ModelState.AddModelError("", "Erro ao atualizar tarefa");
            return View(task);
        }
    }

    // GET: /Tasks/Delete/5
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                TempData["Error"] = "Tarefa não encontrada";
                return RedirectToAction(nameof(Index));
            }
            return View(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar tarefa {TaskId}", id);
            TempData["Error"] = "Erro ao carregar tarefa";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: /Tasks/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        try
        {
            var deleted = await _taskService.DeleteTaskAsync(id);
            if (!deleted)
            {
                TempData["Error"] = "Tarefa não encontrada";
            }
            else
            {
                TempData["Success"] = "Tarefa excluída com sucesso!";
            }
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir tarefa {TaskId}", id);
            TempData["Error"] = "Erro ao excluir tarefa";
            return RedirectToAction(nameof(Index));
        }
    }
}
