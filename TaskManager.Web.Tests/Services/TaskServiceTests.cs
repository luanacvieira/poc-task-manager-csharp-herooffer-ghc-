using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using TaskManager.Web.Services;
using TaskManager.Web.Repositories;
using TaskManager.Web.Models;
using TaskManager.Web.DTOs;
using TaskManager.Web.Common;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.Web.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateTaskDto>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateTaskDto>> _updateValidatorMock;
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _repositoryMock = new Mock<ITaskRepository>();
        _mapperMock = new Mock<IMapper>();
        _createValidatorMock = new Mock<IValidator<CreateTaskDto>>();
        _updateValidatorMock = new Mock<IValidator<UpdateTaskDto>>();
        
        _service = new TaskService(
            _repositoryMock.Object,
            _mapperMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object
        );
    }

    [Fact]
    public async Task GetAllTasksAsync_ShouldReturnSuccess_WhenTasksExist()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new() { Id = 1, Title = "Task 1", UserId = "user1" },
            new() { Id = 2, Title = "Task 2", UserId = "user1" }
        };
        var taskDtos = new List<TaskDto>
        {
            new() { Id = 1, Title = "Task 1", UserId = "user1" },
            new() { Id = 2, Title = "Task 2", UserId = "user1" }
        };

        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(tasks);
        _mapperMock.Setup(m => m.Map<IEnumerable<TaskDto>>(tasks)).Returns(taskDtos);

        // Act
        var result = await _service.GetAllTasksAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().BeEquivalentTo(taskDtos);
    }

    [Fact]
    public async Task GetAllTasksAsync_ShouldReturnFailure_WhenExceptionOccurs()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.GetAllTasksAsync();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Contain("Erro ao buscar tarefas");
    }

    [Fact]
    public async Task GetPagedTasksAsync_ShouldReturnPaginatedResult()
    {
        // Arrange
        var parameters = new QueryParameters { PageNumber = 1, PageSize = 10 };
        var tasks = new List<TaskItem>
        {
            new() { Id = 1, Title = "Task 1", UserId = "user1" }
        };
        var pagedResult = PaginatedResult<TaskItem>.Create(tasks, 1, 1, 10);
        var taskDtos = new List<TaskDto>
        {
            new() { Id = 1, Title = "Task 1", UserId = "user1" }
        };

        _repositoryMock.Setup(r => r.GetPagedAsync(parameters)).ReturnsAsync(pagedResult);
        _mapperMock.Setup(m => m.Map<List<TaskDto>>(tasks)).Returns(taskDtos);

        // Act
        var result = await _service.GetPagedTasksAsync(parameters);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetTaskByIdAsync_ShouldReturnTask_WhenTaskExists()
    {
        // Arrange
        var taskId = 1L;
        var task = new TaskItem { Id = taskId, Title = "Task 1", UserId = "user1" };
        var taskDto = new TaskDto { Id = taskId, Title = "Task 1", UserId = "user1" };

        _repositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);
        _mapperMock.Setup(m => m.Map<TaskDto>(task)).Returns(taskDto);

        // Act
        var result = await _service.GetTaskByIdAsync(taskId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(taskId);
    }

    [Fact]
    public async Task GetTaskByIdAsync_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var taskId = 999L;
        _repositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _service.GetTaskByIdAsync(taskId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCodes.NotFound);
    }

    [Fact]
    public async Task CreateTaskAsync_ShouldReturnSuccess_WhenValidationPasses()
    {
        // Arrange
        var createDto = new CreateTaskDto
        {
            Title = "New Task",
            UserId = "user1",
            Priority = Priority.High,
            Category = Category.Work
        };
        var task = new TaskItem { Title = "New Task", UserId = "user1" };
        var createdTask = new TaskItem { Id = 1, Title = "New Task", UserId = "user1" };
        var taskDto = new TaskDto { Id = 1, Title = "New Task", UserId = "user1" };

        _createValidatorMock.Setup(v => v.ValidateAsync(createDto, default))
            .ReturnsAsync(new ValidationResult());
        _mapperMock.Setup(m => m.Map<TaskItem>(createDto)).Returns(task);
        _repositoryMock.Setup(r => r.CreateAsync(task)).ReturnsAsync(createdTask);
        _mapperMock.Setup(m => m.Map<TaskDto>(createdTask)).Returns(taskDto);

        // Act
        var result = await _service.CreateTaskAsync(createDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(1);
    }

    [Fact]
    public async Task CreateTaskAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var createDto = new CreateTaskDto { Title = "", UserId = "user1" };
        var validationFailures = new List<ValidationFailure>
        {
            new("Title", "O título é obrigatório")
        };
        var validationResult = new ValidationResult(validationFailures);

        _createValidatorMock.Setup(v => v.ValidateAsync(createDto, default))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.CreateTaskAsync(createDto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCodes.ValidationError);
    }

    [Fact]
    public async Task UpdateTaskAsync_ShouldReturnSuccess_WhenValidationPassesAndTaskExists()
    {
        // Arrange
        var taskId = 1L;
        var updateDto = new UpdateTaskDto
        {
            Id = taskId,
            Title = "Updated Task",
            Priority = Priority.High,
            Category = Category.Work
        };
        var existingTask = new TaskItem { Id = taskId, Title = "Old Task", UserId = "user1" };
        var updatedTask = new TaskItem { Id = taskId, Title = "Updated Task", UserId = "user1" };
        var taskDto = new TaskDto { Id = taskId, Title = "Updated Task", UserId = "user1" };

        _updateValidatorMock.Setup(v => v.ValidateAsync(updateDto, default))
            .ReturnsAsync(new ValidationResult());
        _repositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(existingTask);
        _mapperMock.Setup(m => m.Map(updateDto, existingTask)).Returns(existingTask);
        _repositoryMock.Setup(r => r.UpdateAsync(existingTask)).ReturnsAsync(updatedTask);
        _mapperMock.Setup(m => m.Map<TaskDto>(updatedTask)).Returns(taskDto);

        // Act
        var result = await _service.UpdateTaskAsync(taskId, updateDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be("Updated Task");
    }

    [Fact]
    public async Task UpdateTaskAsync_ShouldReturnFailure_WhenIdMismatch()
    {
        // Arrange
        var taskId = 1L;
        var updateDto = new UpdateTaskDto 
        { 
            Id = 2L, 
            Title = "Task",
            Priority = Priority.High,
            Category = Category.Work
        };

        // Act
        var result = await _service.UpdateTaskAsync(taskId, updateDto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCodes.ValidationError);
    }

    [Fact]
    public async Task UpdateTaskAsync_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var taskId = 999L;
        var updateDto = new UpdateTaskDto
        {
            Id = taskId,
            Title = "Task",
            Priority = Priority.High,
            Category = Category.Work
        };

        _updateValidatorMock.Setup(v => v.ValidateAsync(updateDto, default))
            .ReturnsAsync(new ValidationResult());
        _repositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _service.UpdateTaskAsync(taskId, updateDto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCodes.NotFound);
    }

    [Fact]
    public async Task DeleteTaskAsync_ShouldReturnSuccess_WhenTaskExists()
    {
        // Arrange
        var taskId = 1L;
        _repositoryMock.Setup(r => r.DeleteAsync(taskId)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteTaskAsync(taskId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteTaskAsync_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var taskId = 999L;
        _repositoryMock.Setup(r => r.DeleteAsync(taskId)).ReturnsAsync(false);

        // Act
        var result = await _service.DeleteTaskAsync(taskId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCodes.NotFound);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnStatistics()
    {
        // Arrange
        _repositoryMock.Setup(r => r.CountAllAsync()).ReturnsAsync(10);
        _repositoryMock.Setup(r => r.CountCompletedAsync()).ReturnsAsync(5);
        _repositoryMock.Setup(r => r.CountPendingAsync()).ReturnsAsync(5);
        _repositoryMock.Setup(r => r.CountUrgentActiveAsync()).ReturnsAsync(2);

        var statisticsDto = new TaskStatisticsDto
        {
            Total = 10,
            Completed = 5,
            Pending = 5,
            UrgentActive = 2
        };

        _mapperMock.Setup(m => m.Map<TaskStatisticsDto>(It.IsAny<TaskStatistics>()))
            .Returns(statisticsDto);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Total.Should().Be(10);
        result.Value.Completed.Should().Be(5);
        result.Value.Pending.Should().Be(5);
        result.Value.UrgentActive.Should().Be(2);
    }
}
