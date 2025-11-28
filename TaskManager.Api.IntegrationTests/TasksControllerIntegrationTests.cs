using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TaskManager.Web.Data;
using TaskManager.Web.DTOs;
using TaskManager.Web.Models;
using TaskManager.Web.Common;

namespace TaskManager.Api.IntegrationTests;

public class TasksControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IServiceScope _scope;
    private readonly TaskManagerDbContext _context;

    public TasksControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove o DbContext existente
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<TaskManagerDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Adiciona DbContext em memória
                services.AddDbContext<TaskManagerDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                });
            });
        });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<TaskManagerDbContext>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _scope.Dispose();
        _client.Dispose();
    }

    [Fact]
    public async Task GetTasks_ShouldReturnPaginatedTasks()
    {
        // Arrange
        await SeedTasksAsync();

        // Act
        var response = await _client.GetAsync("/api/tasks?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<TaskDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetTasks_WithFilters_ShouldReturnFilteredTasks()
    {
        // Arrange
        await SeedTasksAsync();

        // Act
        var response = await _client.GetAsync("/api/tasks?title=Task 1&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<TaskDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Be("Task 1");
    }

    [Fact]
    public async Task GetTask_WithValidId_ShouldReturnTask()
    {
        // Arrange
        var task = await CreateTaskInDbAsync("Test Task");

        // Act
        var response = await _client.GetAsync($"/api/tasks/{task.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(task.Id);
        result.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task GetTask_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTask_WithValidData_ShouldCreateTask()
    {
        // Arrange
        var createDto = new CreateTaskDto
        {
            Title = "New Task",
            Description = "Task description",
            Priority = Priority.High,
            Category = Category.Work,
            UserId = "user123",
            DueDate = DateTime.Today.AddDays(5)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        result.Should().NotBeNull();
        result!.Title.Should().Be("New Task");
        result.Id.Should().BeGreaterThan(0);

        // Verify in database
        var taskInDb = await _context.Tasks.FindAsync(result.Id);
        taskInDb.Should().NotBeNull();
        taskInDb!.Title.Should().Be("New Task");
    }

    [Fact]
    public async Task CreateTask_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateTaskDto
        {
            Title = "", // Invalid: empty title
            UserId = "user123",
            Priority = Priority.High,
            Category = Category.Work
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTask_WithValidData_ShouldUpdateTask()
    {
        // Arrange
        var task = await CreateTaskInDbAsync("Original Task");
        var updateDto = new UpdateTaskDto
        {
            Id = task.Id,
            Title = "Updated Task",
            Description = "Updated description",
            Priority = Priority.Low,
            Category = Category.Personal,
            Completed = true,
            RowVersion = task.RowVersion
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tasks/{task.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Task");
        result.Completed.Should().BeTrue();

        // Verify in database
        var taskInDb = await _context.Tasks.FindAsync(task.Id);
        taskInDb.Should().NotBeNull();
        taskInDb!.Title.Should().Be("Updated Task");
        taskInDb.Completed.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTask_WithInvalidId_ShouldReturnBadRequest()
    {
        // Arrange
        var task = await CreateTaskInDbAsync("Test Task");
        var updateDto = new UpdateTaskDto
        {
            Id = 999, // Different from URL
            Title = "Updated Task",
            Priority = Priority.High,
            Category = Category.Work
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tasks/{task.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTask_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var updateDto = new UpdateTaskDto
        {
            Id = 999,
            Title = "Updated Task",
            Priority = Priority.High,
            Category = Category.Work
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/tasks/999", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTask_WithValidId_ShouldDeleteTask()
    {
        // Arrange
        var task = await CreateTaskInDbAsync("Task to Delete");

        // Act
        var response = await _client.DeleteAsync($"/api/tasks/{task.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion in database
        var taskInDb = await _context.Tasks.FindAsync(task.Id);
        taskInDb.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTask_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/tasks/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetStatistics_ShouldReturnCorrectStatistics()
    {
        // Arrange
        await SeedTasksAsync();

        // Act
        var response = await _client.GetAsync("/api/tasks/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TaskStatisticsDto>();
        result.Should().NotBeNull();
        result!.Total.Should().Be(3);
        result.Completed.Should().Be(1);
        result.Pending.Should().Be(2);
    }

    [Fact]
    public async Task GetTasks_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        await SeedTasksAsync();

        // Act
        var response = await _client.GetAsync("/api/tasks?pageNumber=2&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<TaskDto>>();
        result.Should().NotBeNull();
        result!.PageNumber.Should().Be(2);
        result.Items.Should().HaveCount(1); // Only 1 item on page 2
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetTasks_WithSorting_ShouldReturnSortedTasks()
    {
        // Arrange
        await SeedTasksAsync();

        // Act
        var response = await _client.GetAsync("/api/tasks?sortBy=title&sortDirection=desc&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<TaskDto>>();
        result.Should().NotBeNull();
        result!.Items.First().Title.Should().Be("Task 3");
        result.Items.Last().Title.Should().Be("Task 1");
    }

    [Fact]
    public async Task CreateTask_WithDueDateInPast_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateTaskDto
        {
            Title = "Task with past date",
            UserId = "user123",
            Priority = Priority.High,
            Category = Category.Work,
            DueDate = DateTime.Today.AddDays(-1)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<TaskItem> CreateTaskInDbAsync(string title)
    {
        var task = new TaskItem
        {
            Title = title,
            Description = "Test description",
            UserId = "user123",
            Priority = Priority.Medium,
            Category = Category.Work,
            Completed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    private async Task SeedTasksAsync()
    {
        var tasks = new[]
        {
            new TaskItem
            {
                Title = "Task 1",
                UserId = "user1",
                Priority = Priority.High,
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
