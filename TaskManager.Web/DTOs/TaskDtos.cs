using TaskManager.Web.Models;

namespace TaskManager.Web.DTOs;

/// <summary>
/// DTO para criar uma nova tarefa
/// </summary>
public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public Category Category { get; set; } = Category.Other;
    public DateTime? DueDate { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? AssignedTo { get; set; }
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// DTO para atualizar uma tarefa existente
/// </summary>
public class UpdateTaskDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Priority Priority { get; set; }
    public Category Category { get; set; }
    public DateTime? DueDate { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? AssignedTo { get; set; }
    public bool Completed { get; set; }
    public byte[]? RowVersion { get; set; }
}

/// <summary>
/// DTO para resposta de tarefa
/// </summary>
public class TaskDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Priority Priority { get; set; }
    public Category Category { get; set; }
    public DateTime? DueDate { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? AssignedTo { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public byte[]? RowVersion { get; set; }
}

/// <summary>
/// DTO para estatísticas de tarefas
/// </summary>
public class TaskStatisticsDto
{
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Pending { get; set; }
    public int UrgentActive { get; set; }
}
