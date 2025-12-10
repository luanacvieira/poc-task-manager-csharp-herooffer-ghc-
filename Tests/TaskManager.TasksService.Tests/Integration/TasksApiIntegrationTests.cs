using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.TasksService.Data;
using TaskManager.TasksService.Models;
using Xunit;

namespace TaskManager.TasksService.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestDatabaseName = "TasksTestDb";
    
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:TasksConnection"] = $"Server=localhost,1433;Database={TestDatabaseName};User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"
            });
        });
    }

    public async Task CleanDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM Tasks");
    }

    public async Task EnsureDatabaseCreatedAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
}

public class TasksApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public TasksApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseCreatedAsync();
        await _factory.CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    #region GET Tests

    [Fact]
    public async Task GetAllTasks_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllTasks_WithTasks_ReturnsTaskList()
    {
        // Arrange
        await CreateTaskAsync("Task 1", Priority.Low, Category.Work);
        await CreateTaskAsync("Task 2", Priority.High, Category.Personal);

        // Act
        var response = await _client.GetAsync("/api/tasks");
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskItem>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        tasks.Should().NotBeNull();
        tasks.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetTaskById_WithValidId_ReturnsTask()
    {
        // Arrange
        var createdTask = await CreateTaskAsync("Test Task", Priority.High, Category.Work);

        // Act
        var response = await _client.GetAsync($"/api/tasks/{createdTask.Id}");
        var task = await response.Content.ReadFromJsonAsync<TaskItem>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        task.Should().NotBeNull();
        task!.Id.Should().Be(createdTask.Id);
        task.Title.Should().Be("Test Task");
        task.Priority.Should().Be(Priority.High);
        task.Category.Should().Be(Category.Work);
    }

    [Fact]
    public async Task GetTaskById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST Tests

    [Fact]
    public async Task CreateTask_WithValidData_ReturnsCreated()
    {
        // Arrange
        var newTask = new TaskItem
        {
            Title = "New Task",
            Description = "Task Description",
            Priority = Priority.High,
            Category = Category.Work,
            UserId = "test-user"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await response.Content.ReadFromJsonAsync<TaskItem>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        createdTask.Should().NotBeNull();
        createdTask!.Id.Should().BeGreaterThan(0);
        createdTask.Title.Should().Be("New Task");
        createdTask.Description.Should().Be("Task Description");
        createdTask.Priority.Should().Be(Priority.High);
        createdTask.Category.Should().Be(Category.Work);
        createdTask.UserId.Should().Be("test-user");
        createdTask.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateTask_WithNullTask_ReturnsBadRequest()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", (TaskItem?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTask_WithMinimalData_ReturnsCreated()
    {
        // Arrange
        var newTask = new TaskItem
        {
            Title = "Minimal Task",
            Priority = Priority.Medium,
            Category = Category.Personal,
            UserId = "user123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await response.Content.ReadFromJsonAsync<TaskItem>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        createdTask.Should().NotBeNull();
        createdTask!.Title.Should().Be("Minimal Task");
        createdTask.Completed.Should().BeFalse();
    }

    #endregion

    #region PUT Tests

    [Fact]
    public async Task UpdateTask_WithValidData_ReturnsOk()
    {
        // Arrange
        var createdTask = await CreateTaskAsync("Original Title", Priority.Low, Category.Work);
        
        createdTask.Title = "Updated Title";
        createdTask.Description = "Updated Description";
        createdTask.Priority = Priority.Urgent;
        createdTask.Completed = true;

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tasks/{createdTask.Id}", createdTask);
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskItem>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedTask.Should().NotBeNull();
        updatedTask!.Title.Should().Be("Updated Title");
        updatedTask.Completed.Should().BeTrue();
        updatedTask.Priority.Should().Be(Priority.Urgent);
    }

    [Fact]
    public async Task UpdateTask_WithMismatchedId_ReturnsBadRequest()
    {
        // Arrange
        var createdTask = await CreateTaskAsync("Test Task", Priority.Medium, Category.Work);
        createdTask.Id = 999;

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tasks/1", createdTask);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTask_MarkAsCompleted_Success()
    {
        // Arrange
        var createdTask = await CreateTaskAsync("Task to Complete", Priority.High, Category.Work);
        createdTask.Completed = true;

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tasks/{createdTask.Id}", createdTask);
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskItem>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedTask!.Completed.Should().BeTrue();
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task DeleteTask_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var createdTask = await CreateTaskAsync("Task to Delete", Priority.High, Category.Work);

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{createdTask.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTask_ThenGet_ReturnsNotFound()
    {
        // Arrange
        var createdTask = await CreateTaskAsync("Task to Delete", Priority.High, Category.Work);

        // Act
        await _client.DeleteAsync($"/api/tasks/{createdTask.Id}");
        var getResponse = await _client.GetAsync($"/api/tasks/{createdTask.Id}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTask_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/tasks/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Workflow Tests

    [Fact]
    public async Task CompleteWorkflow_CreateUpdateDelete_Success()
    {
        // Create
        var task = await CreateTaskAsync("Workflow Task", Priority.Medium, Category.Work);
        task.Completed.Should().BeFalse();

        // Update
        task.Completed = true;
        var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{task.Id}", task);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Delete
        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{task.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CreateMultipleTasks_AllPriorities_Success()
    {
        // Arrange & Act
        await CreateTaskAsync("Urgent Task", Priority.Urgent, Category.Work);
        await CreateTaskAsync("High Task", Priority.High, Category.Personal);
        await CreateTaskAsync("Medium Task", Priority.Medium, Category.Study);
        await CreateTaskAsync("Low Task", Priority.Low, Category.Health);

        // Assert
        var response = await _client.GetAsync("/api/tasks");
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskItem>>();
        
        tasks.Should().NotBeNull();
        tasks.Should().HaveCountGreaterThanOrEqualTo(4);
    }

    [Fact]
    public async Task CreateMultipleTasks_AllCategories_Success()
    {
        // Arrange & Act
        await CreateTaskAsync("Work Task", Priority.High, Category.Work);
        await CreateTaskAsync("Personal Task", Priority.Medium, Category.Personal);
        await CreateTaskAsync("Study Task", Priority.Low, Category.Study);
        await CreateTaskAsync("Health Task", Priority.Urgent, Category.Health);
        await CreateTaskAsync("Other Task", Priority.Medium, Category.Other);

        // Assert
        var response = await _client.GetAsync("/api/tasks");
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskItem>>();
        
        tasks.Should().NotBeNull();
        tasks.Should().HaveCountGreaterThanOrEqualTo(5);
    }

    #endregion

    #region Helper Methods

    private async Task<TaskItem> CreateTaskAsync(string title, Priority priority, Category category)
    {
        var task = new TaskItem
        {
            Title = title,
            Description = $"Description for {title}",
            Priority = priority,
            Category = category,
            UserId = "integration-test-user"
        };

        var response = await _client.PostAsJsonAsync("/api/tasks", task);
        response.EnsureSuccessStatusCode();
        
        var createdTask = await response.Content.ReadFromJsonAsync<TaskItem>();
        return createdTask!;
    }

    #endregion
}

