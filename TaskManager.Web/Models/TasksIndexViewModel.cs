namespace TaskManager.Web.Models;

public class TasksIndexViewModel
{
    public List<TaskItem> Tasks { get; set; } = new();

    public TaskItem NewTask { get; set; } = new();

    public string? SuccessMessage { get; set; }

    public string? ErrorMessage { get; set; }
}