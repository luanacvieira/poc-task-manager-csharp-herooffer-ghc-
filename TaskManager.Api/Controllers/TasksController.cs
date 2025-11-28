using Microsoft.AspNetCore.Mvc;
using TaskManager.Web.Common;
using TaskManager.Web.DTOs;
using TaskManager.Web.Services;

namespace TaskManager.Api.Controllers;

/// <summary>
/// Controller REST para gerenciamento de tarefas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todas as tarefas com suporte a paginação, filtros e ordenação
    /// </summary>
    /// <param name="parameters">Parâmetros de consulta</param>
    /// <returns>Lista paginada de tarefas</returns>
    /// <response code="200">Retorna a lista paginada de tarefas</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResult<TaskDto>>> GetTasks([FromQuery] QueryParameters parameters)
    {
        // Validar parâmetros de entrada para prevenir ataques
        if (parameters.PageSize > 100)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Parâmetro inválido",
                Detail = "O tamanho da página não pode exceder 100 itens",
                Status = 400
            });
        }

        if (parameters.PageNumber < 1)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Parâmetro inválido",
                Detail = "O número da página deve ser maior que zero",
                Status = 400
            });
        }

        var result = await _taskService.GetPagedTasksAsync(parameters);
        
        if (result.IsFailure)
        {
            // Log sem informações sensíveis
            _logger.LogError("Erro ao buscar tarefas. Código: {ErrorCode}", result.Error?.Code);
            
            return StatusCode(500, new ProblemDetails
            {
                Title = "Erro ao buscar tarefas",
                Detail = "Ocorreu um erro ao processar sua solicitação",
                Status = 500
            });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Obtém uma tarefa específica por ID
    /// </summary>
    /// <param name="id">ID da tarefa</param>
    /// <returns>Detalhes da tarefa</returns>
    /// <response code="200">Retorna a tarefa encontrada</response>
    /// <response code="404">Tarefa não encontrada</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskDto>> GetTask(long id)
    {
        // Validar ID para prevenir ataques
        if (id <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "ID inválido",
                Detail = "O ID da tarefa deve ser um número positivo",
                Status = 400
            });
        }

        var result = await _taskService.GetTaskByIdAsync(id);
        
        if (result.IsFailure)
        {
            if (result.Error?.Code == ErrorCodes.NotFound)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Tarefa não encontrada",
                    Detail = "A tarefa solicitada não existe",
                    Status = 404
                });
            }

            // Log sanitizado sem informações sensíveis
            _logger.LogError("Erro ao buscar tarefa. ID: {TaskId}, Código: {ErrorCode}", id, result.Error?.Code);
            
            return StatusCode(500, new ProblemDetails
            {
                Title = "Erro ao buscar tarefa",
                Detail = "Ocorreu um erro ao processar sua solicitação",
                Status = 500
            });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Cria uma nova tarefa
    /// </summary>
    /// <param name="createDto">Dados da tarefa a ser criada</param>
    /// <returns>Tarefa criada</returns>
    /// <response code="201">Tarefa criada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto createDto)
    {
        // Validações adicionais de segurança
        if (createDto == null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Dados inválidos",
                Detail = "O corpo da requisição não pode estar vazio",
                Status = 400
            });
        }

        // Sanitizar entrada: limitar tamanho de strings
        if (!string.IsNullOrEmpty(createDto.Title) && createDto.Title.Length > 200)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Título muito longo",
                Detail = "O título não pode exceder 200 caracteres",
                Status = 400
            });
        }

        if (!string.IsNullOrEmpty(createDto.Description) && createDto.Description.Length > 2000)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Descrição muito longa",
                Detail = "A descrição não pode exceder 2000 caracteres",
                Status = 400
            });
        }

        // Por enquanto, usar ID de usuário padrão (sem autenticação)
        // TODO: Implementar autenticação JWT e obter usuário do token
        createDto.UserId = HttpContext.User.Identity?.Name ?? "api-user";

        var result = await _taskService.CreateTaskAsync(createDto);
        
        if (result.IsFailure)
        {
            if (result.Error?.Code == ErrorCodes.ValidationError)
            {
                var validationProblem = new ValidationProblemDetails(result.Error.ValidationErrors ?? new Dictionary<string, string[]>())
                {
                    Title = "Erro de validação",
                    Detail = "Os dados fornecidos são inválidos",
                    Status = 400
                };
                return BadRequest(validationProblem);
            }

            // Log sanitizado
            _logger.LogError("Erro ao criar tarefa. Código: {ErrorCode}", result.Error?.Code);
            
            return StatusCode(500, new ProblemDetails
            {
                Title = "Erro ao criar tarefa",
                Detail = "Ocorreu um erro ao processar sua solicitação",
                Status = 500
            });
        }

        return CreatedAtAction(nameof(GetTask), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Atualiza uma tarefa existente
    /// </summary>
    /// <param name="id">ID da tarefa</param>
    /// <param name="updateDto">Dados atualizados da tarefa</param>
    /// <returns>Tarefa atualizada</returns>
    /// <response code="200">Tarefa atualizada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="404">Tarefa não encontrada</response>
    /// <response code="409">Conflito de concorrência</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskDto>> UpdateTask(long id, [FromBody] UpdateTaskDto updateDto)
    {
        // Validar ID
        if (id <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "ID inválido",
                Detail = "O ID da tarefa deve ser um número positivo",
                Status = 400
            });
        }

        // Validar corpo da requisição
        if (updateDto == null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Dados inválidos",
                Detail = "O corpo da requisição não pode estar vazio",
                Status = 400
            });
        }

        var result = await _taskService.UpdateTaskAsync(id, updateDto);
        
        if (result.IsFailure)
        {
            if (result.Error?.Code == ErrorCodes.ValidationError)
            {
                var validationProblem = new ValidationProblemDetails(result.Error.ValidationErrors ?? new Dictionary<string, string[]>())
                {
                    Title = "Erro de validação",
                    Detail = "Os dados fornecidos são inválidos",
                    Status = 400
                };
                return BadRequest(validationProblem);
            }

            if (result.Error?.Code == ErrorCodes.NotFound)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Tarefa não encontrada",
                    Detail = "A tarefa solicitada não existe",
                    Status = 404
                });
            }

            if (result.Error?.Code == ErrorCodes.ConcurrencyError)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Conflito de concorrência",
                    Detail = result.Error.Message,
                    Status = 409
                });
            }

            // Log sanitizado
            _logger.LogError("Erro ao atualizar tarefa. ID: {TaskId}, Código: {ErrorCode}", id, result.Error?.Code);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Erro ao atualizar tarefa",
                Detail = "Ocorreu um erro ao processar sua solicitação",
                Status = 500
            });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Exclui uma tarefa
    /// </summary>
    /// <param name="id">ID da tarefa</param>
    /// <returns>Sem conteúdo</returns>
    /// <response code="204">Tarefa excluída com sucesso</response>
    /// <response code="404">Tarefa não encontrada</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTask(long id)
    {
        // Validar ID
        if (id <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "ID inválido",
                Detail = "O ID da tarefa deve ser um número positivo",
                Status = 400
            });
        }

        var result = await _taskService.DeleteTaskAsync(id);
        
        if (result.IsFailure)
        {
            if (result.Error?.Code == ErrorCodes.NotFound)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Tarefa não encontrada",
                    Detail = "A tarefa solicitada não existe",
                    Status = 404
                });
            }

            // Log sanitizado
            _logger.LogError("Erro ao excluir tarefa. ID: {TaskId}, Código: {ErrorCode}", id, result.Error?.Code);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Erro ao excluir tarefa",
                Detail = "Ocorreu um erro ao processar sua solicitação",
                Status = 500
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Obtém estatísticas das tarefas
    /// </summary>
    /// <returns>Estatísticas das tarefas</returns>
    /// <response code="200">Retorna as estatísticas</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(TaskStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskStatisticsDto>> GetStatistics()
    {
        var result = await _taskService.GetStatisticsAsync();
        
        if (result.IsFailure)
        {
            _logger.LogError("Erro ao buscar estatísticas: {Error}", result.Error?.Message);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Erro ao buscar estatísticas",
                Detail = result.Error?.Message,
                Status = 500
            });
        }

        return Ok(result.Value);
    }
}
