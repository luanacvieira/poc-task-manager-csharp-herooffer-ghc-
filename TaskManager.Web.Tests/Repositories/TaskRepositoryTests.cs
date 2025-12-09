using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManager.Web.Data;
using TaskManager.Web.Repositories;
using TaskManager.Web.Models;
using TaskManager.Web.Common;

namespace TaskManager.Web.Tests.Repositories;

public class TaskRepositoryTests : IDisposable
{
    private readonly TaskManagerDbContext _context;
    private readonly TaskRepository _repository;

    public TaskRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TaskManagerDbContext(options);
        _repository = new TaskRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTasks()
    {
        // Arrange
        await SeedTasksAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTask_WhenExists()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Test Task",
            UserId = "user1",
            Priority = Priority.High,
            Category = Category.Work
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(task.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(999L);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddTask()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "New Task",
            UserId = "user1",
            Priority = Priority.Medium,
            Category = Category.Personal,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(task);

        // Assert
        result.Id.Should().BeGreaterThan(0);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        var savedTask = await _context.Tasks.FindAsync(result.Id);
        savedTask.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTask()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Original Title",
            UserId = "user1",
            Priority = Priority.Low,
            Category = Category.Work,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        task.Title = "Updated Title";
        task.Priority = Priority.High;
        task.UpdatedAt = DateTime.UtcNow;
        var result = await _repository.UpdateAsync(task);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
        result.Priority.Should().Be(Priority.High);
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveTask_WhenExists()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Task to Delete",
            UserId = "user1",
            Priority = Priority.Medium,
            Category = Category.Work
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(task.Id);

        // Assert
        result.Should().BeTrue();
        var deletedTask = await _context.Tasks.FindAsync(task.Id);
        deletedTask.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
    {
        // Act
        var result = await _repository.DeleteAsync(999L);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPaginatedResults()
    {
        // Arrange
        await SeedTasksAsync();
        var parameters = new QueryParameters
        {
            PageNumber = 1,
            PageSize = 2
        };

        // Act
        var result = await _repository.GetPagedAsync(parameters);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task GetPagedAsync_WithTitleFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        await SeedTasksAsync();
        var parameters = new QueryParameters
        {
            PageNumber = 1,
            PageSize = 10,
            Title = "Task 1"
        };

        // Act
        var result = await _repository.GetPagedAsync(parameters);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Be("Task 1");
    }

    [Fact]
    public async Task GetPagedAsync_WithPriorityFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        await SeedTasksAsync();
        var parameters = new QueryParameters
        {
            PageNumber = 1,
            PageSize = 10,
            Priority = "Urgent"
        };

        // Act
        var result = await _repository.GetPagedAsync(parameters);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Priority.Should().Be(Priority.Urgent);
    }

    [Fact]
    public async Task GetPagedAsync_WithCompletedFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        await SeedTasksAsync();
        var parameters = new QueryParameters
        {
            PageNumber = 1,
            PageSize = 10,
            Completed = true
        };

        // Act
        var result = await _repository.GetPagedAsync(parameters);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Completed.Should().BeTrue();
    }

    [Fact]
    public async Task CountAllAsync_ShouldReturnTotalCount()
    {
        // Arrange
        await SeedTasksAsync();

        // Act
        var result = await _repository.CountAllAsync();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task CountCompletedAsync_ShouldReturnCompletedCount()
    {
        // Arrange
        await SeedTasksAsync();

        // Act
        var result = await _repository.CountCompletedAsync();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task CountPendingAsync_ShouldReturnPendingCount()
    {
        // Arrange
        await SeedTasksAsync();

        // Act
        var result = await _repository.CountPendingAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task CountUrgentActiveAsync_ShouldReturnUrgentActiveCount()
    {
        // Arrange
        await SeedTasksAsync();

        // Act
        var result = await _repository.CountUrgentActiveAsync();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedAsync_WithSorting_ShouldReturnSortedResults()
    {
        // Arrange
        await SeedTasksAsync();
        var parameters = new QueryParameters
        {
            PageNumber = 1,
            PageSize = 10,
            SortBy = "title",
            SortDirection = "desc"
        };

        // Act
        var result = await _repository.GetPagedAsync(parameters);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items.First().Title.Should().Be("Task 3");
        result.Items.Last().Title.Should().Be("Task 1");
    }

    private async Task SeedTasksAsync()
    {
        var tasks = new[]
        {
            new TaskItem
            {
                Title = "Task 1",
                UserId = "user1",
                Priority = Priority.Urgent,
                Category = Category.Work,
                Completed = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new TaskItem
            {
                Title = "Task 2",
                UserId = "user1",
                Priority = Priority.Medium,
                Category = Category.Personal,
                Completed = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new TaskItem
            {
                Title = "Task 3",
                UserId = "user2",
                Priority = Priority.Low,
                Category = Category.Other,
                Completed = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();
    }
}
