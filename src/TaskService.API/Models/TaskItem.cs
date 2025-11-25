using System.ComponentModel.DataAnnotations;

namespace TaskService.API.Models;

/// <summary>
/// Entidade Task representando uma tarefa no sistema
/// </summary>
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

    [Required(ErrorMessage = "O ID do usuário é obrigatório")]
    [StringLength(100)]
    public string UserId { get; set; } = string.Empty;

    public bool Completed { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
