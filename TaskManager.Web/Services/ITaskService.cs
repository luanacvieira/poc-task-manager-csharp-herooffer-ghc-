using TaskManager.Web.Common;
using TaskManager.Web.DTOs;
using TaskManager.Web.Models;

namespace TaskManager.Web.Services;

/// <summary>
/// Interface para o serviço de tarefas
/// </summary>
public interface ITaskService
{
    Task<Result<IEnumerable<TaskDto>>> GetAllTasksAsync();
    Task<Result<PaginatedResult<TaskDto>>> GetPagedTasksAsync(QueryParameters parameters);
    Task<Result<TaskDto>> GetTaskByIdAsync(long id);
    Task<Result<TaskDto>> CreateTaskAsync(CreateTaskDto createDto);
    Task<Result<TaskDto>> UpdateTaskAsync(long id, UpdateTaskDto updateDto);
    Task<Result> DeleteTaskAsync(long id);
    Task<Result<TaskStatisticsDto>> GetStatisticsAsync();
}

/// <summary>
/// DTO para estatísticas de tarefas (modelo interno)
/// </summary>
public class TaskStatistics
{
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Pending { get; set; }
    public int UrgentActive { get; set; }
}
