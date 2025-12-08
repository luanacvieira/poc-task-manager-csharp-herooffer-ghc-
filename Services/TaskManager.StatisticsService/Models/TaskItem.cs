using System.ComponentModel.DataAnnotations;

namespace TaskManager.StatisticsService.Models;

public class TaskItem
{
    public long Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public Priority Priority { get; set; } = Priority.Medium;

    [Required]
    public Category Category { get; set; } = Category.Other;

    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    [StringLength(100)]
    public string? AssignedTo { get; set; }

    [Required]
    [StringLength(100)]
    public string UserId { get; set; } = string.Empty;

    public bool Completed { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
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

