using System.ComponentModel.DataAnnotations;

namespace TaskManager.StatisticsService.Models;

public class TaskItem
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public Category Category { get; set; } = Category.Other;
    public DateTime? DueDate { get; set; }
    public List<string> Tags { get; set; } = new();
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

