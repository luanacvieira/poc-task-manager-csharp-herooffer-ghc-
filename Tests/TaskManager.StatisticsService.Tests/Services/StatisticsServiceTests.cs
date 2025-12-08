using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManager.StatisticsService.Data;
using TaskManager.StatisticsService.Models;
using TaskManager.StatisticsService.Services;
using Xunit;

namespace TaskManager.StatisticsService.Tests.Services;

public class StatisticsServiceTests : IDisposable
{
    private readonly StatisticsDbContext _context;
    private readonly IStatisticsService _service;

    public StatisticsServiceTests()
    {
        var options = new DbContextOptionsBuilder<StatisticsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StatisticsDbContext(options);
        _service = new TaskManager.StatisticsService.Services.StatisticsService(_context);
    }

    [Fact]
    public async Task GetStatisticsAsync_WithNoTasks_ReturnsZeroStatistics()
    {
        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Total.Should().Be(0);
        result.Completed.Should().Be(0);
        result.Pending.Should().Be(0);
        result.UrgentActive.Should().Be(0);
        result.ByCategory.Should().BeEmpty();
        result.ByPriority.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStatisticsAsync_CalculatesTotalCorrectly()
    {
        // Arrange
        await AddTasksToDatabase(
            new TaskItem { Title = "Task 1", Priority = Priority.High, Category = Category.Work, Completed = false },
            new TaskItem { Title = "Task 2", Priority = Priority.Medium, Category = Category.Personal, Completed = true },
            new TaskItem { Title = "Task 3", Priority = Priority.Low, Category = Category.Study, Completed = false }
        );

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.Total.Should().Be(3);
    }

    [Fact]
    public async Task GetStatisticsAsync_CalculatesCompletedCorrectly()
    {
        // Arrange
        await AddTasksToDatabase(
            new TaskItem { Title = "Task 1", Priority = Priority.High, Category = Category.Work, Completed = true },
            new TaskItem { Title = "Task 2", Priority = Priority.Medium, Category = Category.Personal, Completed = true },
            new TaskItem { Title = "Task 3", Priority = Priority.Low, Category = Category.Study, Completed = false }
        );

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.Completed.Should().Be(2);
    }

    [Fact]
    public async Task GetStatisticsAsync_CalculatesPendingCorrectly()
    {
        // Arrange
        await AddTasksToDatabase(
            new TaskItem { Title = "Task 1", Priority = Priority.High, Category = Category.Work, Completed = false },
            new TaskItem { Title = "Task 2", Priority = Priority.Medium, Category = Category.Personal, Completed = false },
            new TaskItem { Title = "Task 3", Priority = Priority.Low, Category = Category.Study, Completed = true }
        );

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.Pending.Should().Be(2);
    }

    [Fact]
    public async Task GetStatisticsAsync_CalculatesUrgentActiveCorrectly()
    {
        // Arrange
        await AddTasksToDatabase(
            new TaskItem { Title = "Task 1", Priority = Priority.Urgent, Category = Category.Work, Completed = false },
            new TaskItem { Title = "Task 2", Priority = Priority.Urgent, Category = Category.Personal, Completed = false },
            new TaskItem { Title = "Task 3", Priority = Priority.Urgent, Category = Category.Study, Completed = true },
            new TaskItem { Title = "Task 4", Priority = Priority.High, Category = Category.Health, Completed = false }
        );

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.UrgentActive.Should().Be(2); // Only urgent AND not completed
    }

    [Fact]
    public async Task GetStatisticsAsync_GroupsByCategoryCorrectly()
    {
        // Arrange
        await AddTasksToDatabase(
            new TaskItem { Title = "Task 1", Priority = Priority.High, Category = Category.Work, Completed = false },
            new TaskItem { Title = "Task 2", Priority = Priority.Medium, Category = Category.Work, Completed = false },
            new TaskItem { Title = "Task 3", Priority = Priority.Low, Category = Category.Personal, Completed = false },
            new TaskItem { Title = "Task 4", Priority = Priority.High, Category = Category.Study, Completed = false },
            new TaskItem { Title = "Task 5", Priority = Priority.Medium, Category = Category.Study, Completed = false },
            new TaskItem { Title = "Task 6", Priority = Priority.Low, Category = Category.Study, Completed = false }
        );

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.ByCategory.Should().HaveCount(3);
        result.ByCategory[Category.Work.ToString()].Should().Be(2);
        result.ByCategory[Category.Personal.ToString()].Should().Be(1);
        result.ByCategory[Category.Study.ToString()].Should().Be(3);
    }

    [Fact]
    public async Task GetStatisticsAsync_GroupsByPriorityCorrectly()
    {
        // Arrange
        await AddTasksToDatabase(
            new TaskItem { Title = "Task 1", Priority = Priority.Urgent, Category = Category.Work, Completed = false },
            new TaskItem { Title = "Task 2", Priority = Priority.Urgent, Category = Category.Work, Completed = false },
            new TaskItem { Title = "Task 3", Priority = Priority.High, Category = Category.Personal, Completed = false },
            new TaskItem { Title = "Task 4", Priority = Priority.Medium, Category = Category.Study, Completed = false },
            new TaskItem { Title = "Task 5", Priority = Priority.Low, Category = Category.Health, Completed = false }
        );

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.ByPriority.Should().HaveCount(4);
        result.ByPriority[Priority.Urgent.ToString()].Should().Be(2);
        result.ByPriority[Priority.High.ToString()].Should().Be(1);
        result.ByPriority[Priority.Medium.ToString()].Should().Be(1);
        result.ByPriority[Priority.Low.ToString()].Should().Be(1);
    }

    [Fact]
    public async Task GetStatisticsAsync_HandlesAllTasksCompleted()
    {
        // Arrange
        await AddTasksToDatabase(
            new TaskItem { Title = "Task 1", Priority = Priority.High, Category = Category.Work, Completed = true },
            new TaskItem { Title = "Task 2", Priority = Priority.Medium, Category = Category.Personal, Completed = true },
            new TaskItem { Title = "Task 3", Priority = Priority.Urgent, Category = Category.Study, Completed = true }
        );

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.Total.Should().Be(3);
        result.Completed.Should().Be(3);
        result.Pending.Should().Be(0);
        result.UrgentActive.Should().Be(0);
    }

    [Fact]
    public async Task GetStatisticsAsync_HandlesAllTasksPending()
    {
        // Arrange
        await AddTasksToDatabase(
            new TaskItem { Title = "Task 1", Priority = Priority.High, Category = Category.Work, Completed = false },
            new TaskItem { Title = "Task 2", Priority = Priority.Medium, Category = Category.Personal, Completed = false },
            new TaskItem { Title = "Task 3", Priority = Priority.Low, Category = Category.Study, Completed = false }
        );

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.Total.Should().Be(3);
        result.Completed.Should().Be(0);
        result.Pending.Should().Be(3);
    }

    [Fact]
    public async Task GetStatisticsAsync_IncludesAllCategories()
    {
        // Arrange
        await AddTasksToDatabase(
            new TaskItem { Title = "Task 1", Priority = Priority.High, Category = Category.Work, Completed = false },
            new TaskItem { Title = "Task 2", Priority = Priority.Medium, Category = Category.Personal, Completed = false },
            new TaskItem { Title = "Task 3", Priority = Priority.Low, Category = Category.Study, Completed = false },
            new TaskItem { Title = "Task 4", Priority = Priority.High, Category = Category.Health, Completed = false },
            new TaskItem { Title = "Task 5", Priority = Priority.Medium, Category = Category.Other, Completed = false }
        );

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.ByCategory.Should().HaveCount(5);
        result.ByCategory.Should().ContainKey(Category.Work.ToString());
        result.ByCategory.Should().ContainKey(Category.Personal.ToString());
        result.ByCategory.Should().ContainKey(Category.Study.ToString());
        result.ByCategory.Should().ContainKey(Category.Health.ToString());
        result.ByCategory.Should().ContainKey(Category.Other.ToString());
    }

    private async Task AddTasksToDatabase(params TaskItem[] tasks)
    {
        foreach (var task in tasks)
        {
            // Set default values for required fields if not provided
            if (string.IsNullOrEmpty(task.UserId))
                task.UserId = "test-user";
            
            if (task.CreatedAt == default)
                task.CreatedAt = DateTime.UtcNow;
            
            if (task.UpdatedAt == null)
                task.UpdatedAt = DateTime.UtcNow;
            
            await _context.Tasks.AddAsync(task);
        }
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

