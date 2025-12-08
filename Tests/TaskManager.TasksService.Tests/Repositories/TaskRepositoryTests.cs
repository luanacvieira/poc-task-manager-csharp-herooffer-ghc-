using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManager.TasksService.Data;
using TaskManager.TasksService.Models;
using TaskManager.TasksService.Repositories;
using Xunit;

namespace TaskManager.TasksService.Tests.Repositories;

public class TaskRepositoryTests : IDisposable
{
    private readonly TasksDbContext _context;
    private readonly TaskRepository _repository;

    public TaskRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TasksDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TasksDbContext(options);
        _repository = new TaskRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_WithNoTasks_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithTasks_ReturnsAllTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new() { Title = "Task 1", Description = "Desc 1", Priority = Priority.High, Category = Category.Work, UserId = "user1" },
            new() { Title = "Task 2", Description = "Desc 2", Priority = Priority.Medium, Category = Category.Personal, UserId = "user2" },
            new() { Title = "Task 3", Description = "Desc 3", Priority = Priority.Low, Category = Category.Study, UserId = "user3" }
        };
        
        await _context.Tasks.AddRangeAsync(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(t => t.Title == "Task 1");
        result.Should().Contain(t => t.Title == "Task 2");
        result.Should().Contain(t => t.Title == "Task 3");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsTask()
    {
        // Arrange
        var task = new TaskItem 
        { 
            Title = "Test Task", 
            Description = "Test Description",
            Priority = Priority.Urgent,
            Category = Category.Work,
            UserId = "test-user"
        };
        
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(task.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Task");
        result.Description.Should().Be("Test Description");
        result.Priority.Should().Be(Priority.Urgent);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_CreatesNewTask()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "New Task",
            Description = "New Description",
            Priority = Priority.High,
            Category = Category.Health,
            UserId = "test-user"
        };

        // Act
        var result = await _repository.CreateAsync(task);
        var allTasks = await _context.Tasks.ToListAsync();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        allTasks.Should().HaveCount(1);
        allTasks.First().Title.Should().Be("New Task");
    }

    [Fact]
    public async Task UpdateAsync_WithValidTask_UpdatesTask()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Original Title",
            Description = "Original Description",
            Priority = Priority.Low,
            Category = Category.Personal,
            UserId = "test-user"
        };
        
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();

        // Act
        task.Title = "Updated Title";
        task.Description = "Updated Description";
        task.Priority = Priority.Urgent;
        task.Completed = true;
        
        var result = await _repository.UpdateAsync(task);
        var updatedTask = await _context.Tasks.FindAsync(task.Id);

        // Assert
        result.Should().NotBeNull();
        updatedTask.Should().NotBeNull();
        updatedTask!.Title.Should().Be("Updated Title");
        updatedTask.Description.Should().Be("Updated Description");
        updatedTask.Priority.Should().Be(Priority.Urgent);
        updatedTask.Completed.Should().BeTrue();
        updatedTask.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_RemovesTask()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Task to Delete",
            Description = "Will be deleted",
            Priority = Priority.Medium,
            Category = Category.Other,
            UserId = "test-user"
        };
        
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();
        var taskId = task.Id;

        // Act
        var result = await _repository.DeleteAsync(taskId);
        var deletedTask = await _context.Tasks.FindAsync(taskId);
        var allTasks = await _context.Tasks.ToListAsync();

        // Assert
        result.Should().BeTrue();
        deletedTask.Should().BeNull();
        allTasks.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_OrdersByCreatedAtDescending()
    {
        // Arrange
        var task1 = new TaskItem { Title = "First", Priority = Priority.Low, Category = Category.Work, UserId = "user1" };
        var task2 = new TaskItem { Title = "Second", Priority = Priority.Medium, Category = Category.Work, UserId = "user2" };
        var task3 = new TaskItem { Title = "Third", Priority = Priority.High, Category = Category.Work, UserId = "user3" };
        
        await _context.Tasks.AddAsync(task1);
        await _context.SaveChangesAsync();
        await Task.Delay(10);
        
        await _context.Tasks.AddAsync(task2);
        await _context.SaveChangesAsync();
        await Task.Delay(10);
        
        await _context.Tasks.AddAsync(task3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.First().Title.Should().Be("Third");
        result.Last().Title.Should().Be("First");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

