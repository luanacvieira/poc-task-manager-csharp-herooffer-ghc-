using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.TasksService.Data;
using TaskManager.TasksService.Models;
using Xunit;

namespace TaskManager.TasksService.Tests.Integration;

public class TasksApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IServiceScope _scope;

    public TasksApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<TasksDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<TasksDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
                });
            });
        });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
    }

    [Fact]
    public async Task GetAllTasks_ReturnsOkResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateTask_ThenGet_ReturnsCreatedTask()
    {
        // Arrange
        var newTask = new TaskItem
        {
            Title = "Integration Test Task",
            Description = "Testing end-to-end",
            Priority = Priority.High,
            Category = Category.Work,
            UserId = "integration-test-user"
        };

        // Act - Create
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItem>();
        createdTask.Should().NotBeNull();

        // Act - Get
        var getResponse = await _client.GetAsync($"/api/tasks/{createdTask!.Id}");
        var retrievedTask = await getResponse.Content.ReadFromJsonAsync<TaskItem>();

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        retrievedTask.Should().NotBeNull();
        retrievedTask!.Title.Should().Be("Integration Test Task");
        retrievedTask.Description.Should().Be("Testing end-to-end");
    }

    [Fact]
    public async Task UpdateTask_UpdatesSuccessfully()
    {
        // Arrange - Create a task first
        var newTask = new TaskItem
        {
            Title = "Original Title",
            Priority = Priority.Low,
            Category = Category.Personal,
            UserId = "integration-test-user"
        };
        
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItem>();

        // Act - Update
        createdTask!.Title = "Updated Title";
        createdTask.Priority = Priority.Urgent;
        
        var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{createdTask.Id}", createdTask);
        
        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var updatedTask = await updateResponse.Content.ReadFromJsonAsync<TaskItem>();
        updatedTask!.Title.Should().Be("Updated Title");
        updatedTask.Priority.Should().Be(Priority.Urgent);
    }

    [Fact]
    public async Task DeleteTask_RemovesTask()
    {
        // Arrange - Create a task first
        var newTask = new TaskItem
        {
            Title = "Task to Delete",
            Priority = Priority.Medium,
            Category = Category.Other,
            UserId = "integration-test-user"
        };
        
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItem>();

        // Act - Delete
        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{createdTask!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert - Try to get deleted task
        var getResponse = await _client.GetAsync($"/api/tasks/{createdTask.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTask_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public void Dispose()
    {
        _scope?.Dispose();
        _client?.Dispose();
    }
}

