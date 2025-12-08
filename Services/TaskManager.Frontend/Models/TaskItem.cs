using System.ComponentModel.DataAnnotations;

namespace TaskManager.Frontend.Models;

public class TaskItem
{
    public long Id { get; set; }

    [Required(ErrorMessage = "O título é obrigatório")]
    [StringLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "A descrição deve ter no máximo 2000 caracteres")]
    public string? Description { get; set; }

    [Required]
    public Priority Priority { get; set; } = Priority.Medium;

    [Required]
    public Category Category { get; set; } = Category.Other;

    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    public List<string> Tags { get; set; } = new();

    [StringLength(100)]
    public string? AssignedTo { get; set; }

    public string UserId { get; set; } = string.Empty;

    public bool Completed { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public enum Priority
{
    Low,
    Medium,
    High,
    Urgent
}

public enum Category
{
    Work,
    Personal,
    Study,
    Health,
    Other
}

public class TaskStatistics
{
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Pending { get; set; }
    public int UrgentActive { get; set; }
    public Dictionary<string, int> ByCategory { get; set; } = new();
    public Dictionary<string, int> ByPriority { get; set; } = new();
}

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

