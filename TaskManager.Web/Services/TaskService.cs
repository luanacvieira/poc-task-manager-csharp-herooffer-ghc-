using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskManager.Web.Common;
using TaskManager.Web.DTOs;
using TaskManager.Web.Models;
using TaskManager.Web.Repositories;

namespace TaskManager.Web.Services;

/// <summary>
/// Implementação do serviço de tarefas com lógica de negócio
/// </summary>
public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateTaskDto> _createValidator;
    private readonly IValidator<UpdateTaskDto> _updateValidator;

    public TaskService(
        ITaskRepository repository,
        IMapper mapper,
        IValidator<CreateTaskDto> createValidator,
        IValidator<UpdateTaskDto> updateValidator)
    {
        _repository = repository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<IEnumerable<TaskDto>>> GetAllTasksAsync()
    {
        try
        {
            var tasks = await _repository.GetAllAsync();
            var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(tasks);
            return Result<IEnumerable<TaskDto>>.Success(taskDtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TaskDto>>.Failure(
                Error.Internal($"Erro ao buscar tarefas: {ex.Message}"));
        }
    }

    public async Task<Result<PaginatedResult<TaskDto>>> GetPagedTasksAsync(QueryParameters parameters)
    {
        try
        {
            var pagedTasks = await _repository.GetPagedAsync(parameters);
            var taskDtos = _mapper.Map<List<TaskDto>>(pagedTasks.Items);
            
            var result = new PaginatedResult<TaskDto>(
                taskDtos,
                pagedTasks.TotalCount,
                pagedTasks.PageNumber,
                pagedTasks.PageSize);

            return Result<PaginatedResult<TaskDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PaginatedResult<TaskDto>>.Failure(
                Error.Internal($"Erro ao buscar tarefas paginadas: {ex.Message}"));
        }
    }

    public async Task<Result<TaskDto>> GetTaskByIdAsync(long id)
    {
        try
        {
            var task = await _repository.GetByIdAsync(id);
            if (task == null)
            {
                return Result<TaskDto>.Failure(Error.NotFound("Tarefa", id));
            }

            var taskDto = _mapper.Map<TaskDto>(task);
            return Result<TaskDto>.Success(taskDto);
        }
        catch (Exception ex)
        {
            return Result<TaskDto>.Failure(
                Error.Internal($"Erro ao buscar tarefa: {ex.Message}"));
        }
    }

    public async Task<Result<TaskDto>> CreateTaskAsync(CreateTaskDto createDto)
    {
        try
        {
            // Validar DTO
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());

                return Result<TaskDto>.Failure(
                    Error.Validation("Erro de validação", errors));
            }

            // Mapear e criar
            var task = _mapper.Map<TaskItem>(createDto);
            var createdTask = await _repository.CreateAsync(task);
            var taskDto = _mapper.Map<TaskDto>(createdTask);

            return Result<TaskDto>.Success(taskDto);
        }
        catch (Exception ex)
        {
            return Result<TaskDto>.Failure(
                Error.Internal($"Erro ao criar tarefa: {ex.Message}"));
        }
    }

    public async Task<Result<TaskDto>> UpdateTaskAsync(long id, UpdateTaskDto updateDto)
    {
        try
        {
            // Verificar se IDs correspondem
            if (id != updateDto.Id)
            {
                return Result<TaskDto>.Failure(
                    Error.Validation("ID da URL não corresponde ao ID do corpo da requisição"));
            }

            // Validar DTO
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());

                return Result<TaskDto>.Failure(
                    Error.Validation("Erro de validação", errors));
            }

            // Verificar se existe
            var existingTask = await _repository.GetByIdAsync(id);
            if (existingTask == null)
            {
                return Result<TaskDto>.Failure(Error.NotFound("Tarefa", id));
            }

            // Mapear e atualizar
            _mapper.Map(updateDto, existingTask);
            
            var updatedTask = await _repository.UpdateAsync(existingTask);
            if (updatedTask == null)
            {
                return Result<TaskDto>.Failure(Error.NotFound("Tarefa", id));
            }

            var taskDto = _mapper.Map<TaskDto>(updatedTask);
            return Result<TaskDto>.Success(taskDto);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<TaskDto>.Failure(
                new Error(ErrorCodes.ConcurrencyError, 
                    "A tarefa foi modificada por outro usuário. Por favor, recarregue e tente novamente."));
        }
        catch (Exception ex)
        {
            return Result<TaskDto>.Failure(
                Error.Internal($"Erro ao atualizar tarefa: {ex.Message}"));
        }
    }

    public async Task<Result> DeleteTaskAsync(long id)
    {
        try
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted)
            {
                return Result.Failure(Error.NotFound("Tarefa", id));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(
                Error.Internal($"Erro ao deletar tarefa: {ex.Message}"));
        }
    }

    public async Task<Result<TaskStatisticsDto>> GetStatisticsAsync()
    {
        try
        {
            var total = await _repository.CountAllAsync();
            var completed = await _repository.CountCompletedAsync();
            var pending = await _repository.CountPendingAsync();
            var urgentActive = await _repository.CountUrgentActiveAsync();

            var statistics = new TaskStatistics
            {
                Total = total,
                Completed = completed,
                Pending = pending,
                UrgentActive = urgentActive
            };

            var statisticsDto = _mapper.Map<TaskStatisticsDto>(statistics);
            return Result<TaskStatisticsDto>.Success(statisticsDto);
        }
        catch (Exception ex)
        {
            return Result<TaskStatisticsDto>.Failure(
                Error.Internal($"Erro ao buscar estatísticas: {ex.Message}"));
        }
    }
}
