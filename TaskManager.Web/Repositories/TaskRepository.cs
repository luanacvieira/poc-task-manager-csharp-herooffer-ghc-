using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskManager.Web.Common;
using TaskManager.Web.Data;
using TaskManager.Web.Models;

namespace TaskManager.Web.Repositories;

/// <summary>
/// Implementação do repositório de tarefas usando Entity Framework Core
/// </summary>
public class TaskRepository : ITaskRepository
{
    private readonly TaskManagerDbContext _context;

    public TaskRepository(TaskManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        return await _context.Tasks.ToListAsync();
    }

    public async Task<PaginatedResult<TaskItem>> GetPagedAsync(QueryParameters parameters)
    {
        var query = _context.Tasks.AsQueryable();

        // Aplicar filtros
        query = ApplyFilters(query, parameters);

        // Contar total de registros após filtros
        var totalCount = await query.CountAsync();

        // Aplicar ordenação
        query = ApplyOrdering(query, parameters);

        // Aplicar paginação
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return PaginatedResult<TaskItem>.Create(
            items,
            totalCount,
            parameters.PageNumber,
            parameters.PageSize);
    }

    private IQueryable<TaskItem> ApplyFilters(IQueryable<TaskItem> query, QueryParameters parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.Title))
        {
            query = query.Where(t => t.Title.Contains(parameters.Title));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Priority) && 
            Enum.TryParse<Priority>(parameters.Priority, true, out var priority))
        {
            query = query.Where(t => t.Priority == priority);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Category) && 
            Enum.TryParse<Category>(parameters.Category, true, out var category))
        {
            query = query.Where(t => t.Category == category);
        }

        if (parameters.Completed.HasValue)
        {
            query = query.Where(t => t.Completed == parameters.Completed.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.UserId))
        {
            query = query.Where(t => t.UserId == parameters.UserId);
        }

        if (!string.IsNullOrWhiteSpace(parameters.AssignedTo))
        {
            query = query.Where(t => t.AssignedTo == parameters.AssignedTo);
        }

        if (parameters.DueDateFrom.HasValue)
        {
            query = query.Where(t => t.DueDate >= parameters.DueDateFrom.Value);
        }

        if (parameters.DueDateTo.HasValue)
        {
            query = query.Where(t => t.DueDate <= parameters.DueDateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Tag))
        {
            // Filtro por tag - verifica se a string de tags contém a tag procurada
            query = query.Where(t => t.Tags.Contains(parameters.Tag));
        }

        return query;
    }

    private IQueryable<TaskItem> ApplyOrdering(IQueryable<TaskItem> query, QueryParameters parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            return query.OrderByDescending(t => t.CreatedAt);
        }

        var isDescending = parameters.SortDirection?.ToLower() == "desc";

        return parameters.SortBy.ToLower() switch
        {
            "title" => isDescending 
                ? query.OrderByDescending(t => t.Title) 
                : query.OrderBy(t => t.Title),
            "priority" => isDescending 
                ? query.OrderByDescending(t => t.Priority) 
                : query.OrderBy(t => t.Priority),
            "category" => isDescending 
                ? query.OrderByDescending(t => t.Category) 
                : query.OrderBy(t => t.Category),
            "duedate" => isDescending 
                ? query.OrderByDescending(t => t.DueDate) 
                : query.OrderBy(t => t.DueDate),
            "completed" => isDescending 
                ? query.OrderByDescending(t => t.Completed) 
                : query.OrderBy(t => t.Completed),
            "createdat" => isDescending 
                ? query.OrderByDescending(t => t.CreatedAt) 
                : query.OrderBy(t => t.CreatedAt),
            "updatedat" => isDescending 
                ? query.OrderByDescending(t => t.UpdatedAt) 
                : query.OrderBy(t => t.UpdatedAt),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };
    }

    public async Task<TaskItem?> GetByIdAsync(long id)
    {
        return await _context.Tasks.FindAsync(id);
    }

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<TaskItem?> UpdateAsync(TaskItem task)
    {
        var existing = await _context.Tasks.FindAsync(task.Id);
        if (existing == null)
            return null;

        _context.Entry(existing).CurrentValues.SetValues(task);
        
        try
        {
            await _context.SaveChangesAsync();
            return existing;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Erro de concorrência - a tarefa foi modificada por outro usuário
            throw;
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
            return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> CountAllAsync()
    {
        return await _context.Tasks.CountAsync();
    }

    public async Task<int> CountCompletedAsync()
    {
        return await _context.Tasks.CountAsync(t => t.Completed);
    }

    public async Task<int> CountPendingAsync()
    {
        return await _context.Tasks.CountAsync(t => !t.Completed);
    }

    public async Task<int> CountUrgentActiveAsync()
    {
        return await _context.Tasks.CountAsync(t => 
            t.Priority == Priority.Urgent && !t.Completed);
    }
}
