using FluentValidation;
using TaskManager.Web.Models;

namespace TaskManager.Web.Validators;

/// <summary>
/// Validador para TaskItem usando FluentValidation
/// </summary>
public class TaskItemValidator : AbstractValidator<TaskItem>
{
    public TaskItemValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("O título é obrigatório")
            .MaximumLength(200).WithMessage("O título deve ter no máximo 200 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("A descrição deve ter no máximo 2000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Prioridade inválida");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Categoria inválida");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("A data de vencimento não pode ser no passado")
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.AssignedTo)
            .MaximumLength(100).WithMessage("O campo AssignedTo deve ter no máximo 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.AssignedTo));

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("O ID do usuário é obrigatório")
            .MaximumLength(100).WithMessage("O ID do usuário deve ter no máximo 100 caracteres");

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 10)
            .WithMessage("O número máximo de tags é 10")
            .Must(tags => tags == null || tags.All(t => t.Length <= 50))
            .WithMessage("Cada tag deve ter no máximo 50 caracteres");
    }
}
