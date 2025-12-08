using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.TasksService.Controllers;
using TaskManager.TasksService.Models;
using TaskManager.TasksService.Repositories;
using Xunit;

namespace TaskManager.TasksService.Tests.Controllers;

public class TasksControllerTests
{
    private readonly Mock<ITaskRepository> _mockRepository;
    private readonly Mock<ILogger<TasksController>> _mockLogger;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _mockRepository = new Mock<ITaskRepository>();
        _mockLogger = new Mock<ILogger<TasksController>>();
        _controller = new TasksController(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new() { Id = 1, Title = "Task 1", Priority = Priority.High, Category = Category.Work },
            new() { Id = 2, Title = "Task 2", Priority = Priority.Medium, Category = Category.Personal }
        };
        
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTasks = okResult.Value.Should().BeAssignableTo<IEnumerable<TaskItem>>().Subject;
        returnedTasks.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithNoTasks_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTasks = okResult.Value.Should().BeAssignableTo<IEnumerable<TaskItem>>().Subject;
        returnedTasks.Should().BeEmpty();
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithTask()
    {
        // Arrange
        var task = new TaskItem 
        { 
            Id = 1, 
            Title = "Test Task", 
            Priority = Priority.Urgent,
            Category = Category.Work 
        };
        
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTask = okResult.Value.Should().BeOfType<TaskItem>().Subject;
        returnedTask.Id.Should().Be(1);
        returnedTask.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_WithValidTask_ReturnsCreatedAtAction()
    {
        // Arrange
        var newTask = new TaskItem
        {
            Title = "New Task",
            Description = "New Description",
            Priority = Priority.High,
            Category = Category.Study,
            UserId = "test-user"
        };
        
        var createdTask = new TaskItem
        {
            Id = 1,
            Title = newTask.Title,
            Description = newTask.Description,
            Priority = newTask.Priority,
            Category = newTask.Category,
            UserId = newTask.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<TaskItem>())).ReturnsAsync(createdTask);

        // Act
        var result = await _controller.Create(newTask);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(TasksController.GetById));
        var returnedTask = createdResult.Value.Should().BeOfType<TaskItem>().Subject;
        returnedTask.Id.Should().Be(1);
        returnedTask.Title.Should().Be("New Task");
    }

    [Fact]
    public async Task Create_WithNullTask_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Create(null!);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WithValidTask_ReturnsOk()
    {
        // Arrange
        var updatedTask = new TaskItem
        {
            Id = 1,
            Title = "Updated",
            Priority = Priority.High,
            Category = Category.Work,
            UserId = "test-user"
        };
        
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>())).ReturnsAsync(updatedTask);

        // Act
        var result = await _controller.Update(1, updatedTask);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTask = okResult.Value.Should().BeOfType<TaskItem>().Subject;
        returnedTask.Title.Should().Be("Updated");
    }

    [Fact]
    public async Task Update_WithMismatchedId_ReturnsBadRequest()
    {
        // Arrange
        var task = new TaskItem 
        { 
            Id = 1, 
            Title = "Task", 
            Priority = Priority.Medium, 
            Category = Category.Work,
            UserId = "test-user"
        };

        // Act
        var result = await _controller.Update(2, task);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WithNonExistentTask_ReturnsNotFound()
    {
        // Arrange
        var task = new TaskItem 
        { 
            Id = 999, 
            Title = "Task", 
            Priority = Priority.Medium, 
            Category = Category.Work,
            UserId = "test-user"
        };
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>())).ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _controller.Update(999, task);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _mockRepository.Verify(r => r.DeleteAsync(999), Times.Once);
    }
}

