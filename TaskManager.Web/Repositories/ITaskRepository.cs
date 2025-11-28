using TaskManager.Web.Common;
using TaskManager.Web.Models;

namespace TaskManager.Web.Repositories;

/// <summary>
/// Interface para o repositório de tarefas
/// </summary>
public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetAllAsync();
    Task<PaginatedResult<TaskItem>> GetPagedAsync(QueryParameters parameters);
    Task<TaskItem?> GetByIdAsync(long id);
    Task<TaskItem> CreateAsync(TaskItem task);
    Task<TaskItem?> UpdateAsync(TaskItem task);
    Task<bool> DeleteAsync(long id);
    Task<int> CountAllAsync();
    Task<int> CountCompletedAsync();
    Task<int> CountPendingAsync();
    Task<int> CountUrgentActiveAsync();
}
