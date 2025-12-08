using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskManager.TasksService.Data;
using TaskManager.TasksService.Models;
using Xunit;

namespace TaskManager.TasksService.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestDatabaseName = "InMemoryTestDb";
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureServices(services =>
        {
            // Remove the default DbContext registration
            services.RemoveAll<DbContextOptions<TasksDbContext>>();
            services.RemoveAll<TasksDbContext>();
            
            // Add DbContext using InMemory database for testing with fixed name
            services.AddDbContext<TasksDbContext>(options =>
            {
                options.UseInMemoryDatabase(TestDatabaseName);
            });
            
            // Ensure database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
            db.Database.EnsureCreated();
        });
    }
}

public class TasksApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public TasksApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // Clean database after each test
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
        context.Tasks.RemoveRange(context.Tasks);
        await context.SaveChangesAsync();
    }

    #region GET /api/tasks - List All Tasks

    [Fact]
    public async Task GetAllTasks_WhenNoTasks_ReturnsEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks");
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskItem>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        tasks.Should().NotBeNull();
        tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllTasks_WhenTasksExist_ReturnsAllTasks()
    {
        // Arrange
        var task1 = await CreateTaskAsync("Task 1", Priority.High);
        var task2 = await CreateTaskAsync("Task 2", Priority.Low);
        var task3 = await CreateTaskAsync("Task 3", Priority.Medium);

        // Act
        var response = await _client.GetAsync("/api/tasks");
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskItem>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        tasks.Should().NotBeNull();
        tasks.Should().HaveCount(3);
        tasks.Should().Contain(t => t.Title == "Task 1");
        tasks.Should().Contain(t => t.Title == "Task 2");
        tasks.Should().Contain(t => t.Title == "Task 3");
    }

    [Fact]
    public async Task GetAllTasks_ReturnsTasksInDescendingOrderByCreatedAt()
    {
        // Arrange
        var task1 = await CreateTaskAsync("First Task", Priority.Low);
        await Task.Delay(10);
        var task2 = await CreateTaskAsync("Second Task", Priority.Medium);
        await Task.Delay(10);
        var task3 = await CreateTaskAsync("Third Task", Priority.High);

        // Act
        var response = await _client.GetAsync("/api/tasks");
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskItem>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        tasks.Should().NotBeNull();
        tasks.Should().HaveCount(3);
        tasks![0].Title.Should().Be("Third Task");
        tasks[1].Title.Should().Be("Second Task");
        tasks[2].Title.Should().Be("First Task");
    }

    #endregion

    #region GET /api/tasks/{id} - Get Task By ID

    [Fact]
    public async Task GetTaskById_WhenTaskExists_ReturnsTask()
    {
        // Arrange
        var createdTask = await CreateTaskAsync("Test Task", Priority.High);

        // Act
        var response = await _client.GetAsync($"/api/tasks/{createdTask.Id}");
        var task = await response.Content.ReadFromJsonAsync<TaskItem>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        task.Should().NotBeNull();
        task!.Id.Should().Be(createdTask.Id);
        task.Title.Should().Be("Test Task");
        task.Priority.Should().Be(Priority.High);
    }

    [Fact]
    public async Task GetTaskById_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTaskById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks/-1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/tasks - Create Task

    [Fact]
    public async Task CreateTask_WithValidData_ReturnsCreatedTask()
    {
        // Arrange
        var newTask = new TaskItem
        {
            Title = "New Task",
            Description = "Task description",
            Priority = Priority.High,
            Category = Category.Work,
            DueDate = DateTime.UtcNow.AddDays(7),
            Tags = new List<string> { "tag1", "tag2" },
            UserId = "user123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await response.Content.ReadFromJsonAsync<TaskItem>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        createdTask.Should().NotBeNull();
        createdTask!.Id.Should().BeGreaterThan(0);
        createdTask.Title.Should().Be("New Task");
        createdTask.Description.Should().Be("Task description");
        createdTask.Priority.Should().Be(Priority.High);
        createdTask.Category.Should().Be(Category.Work);
        createdTask.Tags.Should().HaveCount(2);
        createdTask.Tags.Should().Contain("tag1");
        createdTask.Tags.Should().Contain("tag2");
        createdTask.UserId.Should().Be("user123");
        createdTask.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        createdTask.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateTask_WithMinimalData_ReturnsCreatedTask()
    {
        // Arrange
        var newTask = new TaskItem
        {
            Title = "Minimal Task",
            Priority = Priority.Low,
            Category = Category.Personal,
            UserId = "test-user",
            Tags = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await response.Content.ReadFromJsonAsync<TaskItem>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        createdTask.Should().NotBeNull();
        createdTask!.Title.Should().Be("Minimal Task");
        createdTask.UserId.Should().Be("test-user");
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
    public async Task CreateTask_WithEmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        var newTask = new TaskItem
        {
            Title = "",
            Priority = Priority.Low,
            Category = Category.Personal,
            UserId = "test-user",
            Tags = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", newTask);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTask_WithAllPriorities_CreatesSuccessfully()
    {
        // Act & Assert
        foreach (Priority priority in Enum.GetValues(typeof(Priority)))
        {
            var task = new TaskItem
            {
                Title = $"Task with {priority} priority",
                Priority = priority,
                Category = Category.Work,
                UserId = "test-user",
                Tags = new List<string>()
            };

            var response = await _client.PostAsJsonAsync("/api/tasks", task);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }

    [Fact]
    public async Task CreateTask_WithAllCategories_CreatesSuccessfully()
    {
        // Act & Assert
        foreach (Category category in Enum.GetValues(typeof(Category)))
        {
            var task = new TaskItem
            {
                Title = $"Task in {category} category",
                Priority = Priority.Medium,
                Category = category,
                UserId = "test-user",
                Tags = new List<string>()
            };

            var response = await _client.PostAsJsonAsync("/api/tasks", task);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }

    #endregion

    #region PUT /api/tasks/{id} - Update Task

    [Fact]
    public async Task UpdateTask_WithValidData_ReturnsUpdatedTask()
    {
        // Arrange
        var createdTask = await CreateTaskAsync("Original Title", Priority.Low);
        createdTask.Title = "Updated Title";
        createdTask.Description = "Updated description";
        createdTask.Priority = Priority.Urgent;
        createdTask.Category = Category.Personal;
        createdTask.Completed = true;

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tasks/{createdTask.Id}", createdTask);
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskItem>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedTask.Should().NotBeNull();
        updatedTask!.Id.Should().Be(createdTask.Id);
        updatedTask.Title.Should().Be("Updated Title");
        updatedTask.Description.Should().Be("Updated description");
        updatedTask.Priority.Should().Be(Priority.Urgent);
        updatedTask.Category.Should().Be(Category.Personal);
        updatedTask.Completed.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTask_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentTask = new TaskItem
        {
            Id = 99999,
            Title = "Non-existent Task",
            Priority = Priority.Low,
            Category = Category.Work,
            UserId = "test-user",
            Tags = new List<string>()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tasks/{nonExistentTask.Id}", nonExistentTask);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTask_WithMismatchedId_ReturnsBadRequest()
    {
        // Arrange
        var createdTask = await CreateTaskAsync("Test Task", Priority.Low);
        createdTask.Title = "Updated Title";

        // Act - Send to different ID
        var response = await _client.PutAsJsonAsync($"/api/tasks/{createdTask.Id + 1}", createdTask);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTask_MarkAsCompleted_UpdatesSuccessfully()
    {
        // Arrange
        var createdTask = await CreateTaskAsync("Task to Complete", Priority.Medium);

        // Act
        createdTask.Completed = true;
        var response = await _client.PutAsJsonAsync($"/api/tasks/{createdTask.Id}", createdTask);
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskItem>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedTask!.Completed.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTask_ChangePriority_UpdatesSuccessfully()
    {
        // Arrange
        var createdTask = await CreateTaskAsync("Priority Test", Priority.Low);

        // Act & Assert - Test all priority transitions
        var priorities = new[] { Priority.Medium, Priority.High, Priority.Urgent, Priority.Low };
        foreach (var priority in priorities)
        {
            createdTask.Priority = priority;
            var response = await _client.PutAsJsonAsync($"/api/tasks/{createdTask.Id}", createdTask);
            var updatedTask = await response.Content.ReadFromJsonAsync<TaskItem>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            updatedTask!.Priority.Should().Be(priority);
        }
    }

    #endregion

    #region DELETE /api/tasks/{id} - Delete Task

    [Fact]
    public async Task DeleteTask_WhenTaskExists_ReturnsNoContent()
    {
        // Arrange
        var createdTask = await CreateTaskAsync("Task to Delete", Priority.Low);

        // Act
        var response = await _client.DeleteAsync($"/api/tasks/{createdTask.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTask_WhenTaskExists_RemovesTaskFromDatabase()
    {
        // Arrange
        var createdTask = await CreateTaskAsync("Task to Delete", Priority.Low);

        // Act
        await _client.DeleteAsync($"/api/tasks/{createdTask.Id}");

        // Verify task is deleted
        var getResponse = await _client.GetAsync($"/api/tasks/{createdTask.Id}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTask_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/tasks/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTask_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/tasks/-1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Integration Scenarios - Complex Workflows

    [Fact]
    public async Task CompleteWorkflow_CreateReadUpdateDelete_WorksCorrectly()
    {
        // 1. Create
        var newTask = new TaskItem
        {
            Title = "Workflow Test Task",
            Description = "Testing complete workflow",
            Priority = Priority.High,
            Category = Category.Work,
            UserId = "test-user",
            Tags = new List<string>()
        };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItem>();
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // 2. Read
        var getResponse = await _client.GetAsync($"/api/tasks/{createdTask!.Id}");
        var retrievedTask = await getResponse.Content.ReadFromJsonAsync<TaskItem>();
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        retrievedTask!.Title.Should().Be("Workflow Test Task");

        // 3. Update
        retrievedTask.Title = "Updated Workflow Task";
        retrievedTask.Priority = Priority.Urgent;
        var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{retrievedTask.Id}", retrievedTask);
        var updatedTask = await updateResponse.Content.ReadFromJsonAsync<TaskItem>();
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedTask!.Title.Should().Be("Updated Workflow Task");

        // 4. Delete
        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{updatedTask.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 5. Verify deletion
        var verifyResponse = await _client.GetAsync($"/api/tasks/{updatedTask.Id}");
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MultipleTasksWorkflow_CreatesAndManagesMultipleTasks()
    {
        // Create multiple tasks
        var task1 = await CreateTaskAsync("Task 1", Priority.High);
        var task2 = await CreateTaskAsync("Task 2", Priority.Medium);
        var task3 = await CreateTaskAsync("Task 3", Priority.Low);

        // Verify all exist
        var allTasksResponse = await _client.GetAsync("/api/tasks");
        var allTasks = await allTasksResponse.Content.ReadFromJsonAsync<List<TaskItem>>();
        allTasks.Should().HaveCount(3);

        // Update one
        task2.Completed = true;
        await _client.PutAsJsonAsync($"/api/tasks/{task2.Id}", task2);

        // Delete one
        await _client.DeleteAsync($"/api/tasks/{task3.Id}");

        // Verify final state
        var finalResponse = await _client.GetAsync("/api/tasks");
        var finalTasks = await finalResponse.Content.ReadFromJsonAsync<List<TaskItem>>();
        finalTasks.Should().HaveCount(2);
        finalTasks.Should().Contain(t => t.Id == task1.Id);
        finalTasks.Should().Contain(t => t.Id == task2.Id && t.Completed);
        finalTasks.Should().NotContain(t => t.Id == task3.Id);
    }

    [Fact]
    public async Task TaskWithDueDate_CreatesAndUpdatesCorrectly()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(30);
        var task = new TaskItem
        {
            Title = "Task with Due Date",
            Priority = Priority.High,
            Category = Category.Work,
            DueDate = futureDate,
            UserId = "test-user",
            Tags = new List<string>()
        };

        // Act - Create
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", task);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItem>();

        // Assert - Verify due date
        createdTask!.DueDate.Should().NotBeNull();
        createdTask.DueDate!.Value.Should().BeCloseTo(futureDate, TimeSpan.FromSeconds(1));

        // Act - Update due date
        var newDueDate = DateTime.UtcNow.AddDays(15);
        createdTask.DueDate = newDueDate;
        var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{createdTask.Id}", createdTask);
        var updatedTask = await updateResponse.Content.ReadFromJsonAsync<TaskItem>();

        // Assert - Verify updated due date
        updatedTask!.DueDate.Should().NotBeNull();
        updatedTask.DueDate!.Value.Should().BeCloseTo(newDueDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task TaskWithTags_CreatesAndUpdatesCorrectly()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Task with Tags",
            Priority = Priority.Medium,
            Category = Category.Work,
            Tags = new List<string> { "urgent", "review", "backend" },
            UserId = "test-user"
        };

        // Act - Create
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", task);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItem>();

        // Assert - Verify tags
        createdTask!.Tags.Should().HaveCount(3);
        createdTask.Tags.Should().Contain("urgent");
        createdTask.Tags.Should().Contain("review");
        createdTask.Tags.Should().Contain("backend");

        // Act - Update tags
        createdTask.Tags = new List<string> { "completed", "archived" };
        var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{createdTask.Id}", createdTask);
        var updatedTask = await updateResponse.Content.ReadFromJsonAsync<TaskItem>();

        // Assert - Verify updated tags
        updatedTask!.Tags.Should().HaveCount(2);
        updatedTask.Tags.Should().Contain("completed");
        updatedTask.Tags.Should().Contain("archived");
    }

    [Fact]
    public async Task ConcurrentTaskCreation_HandlesMultipleRequests()
    {
        // Arrange
        var tasks = Enumerable.Range(1, 10).Select(i => new TaskItem
        {
            Title = $"Concurrent Task {i}",
            Priority = Priority.Medium,
            Category = Category.Work,
            UserId = "test-user",
            Tags = new List<string>()
        }).ToList();

        // Act - Create tasks concurrently
        var createTasks = tasks.Select(task => _client.PostAsJsonAsync("/api/tasks", task)).ToList();
        var responses = await Task.WhenAll(createTasks);

        // Assert - All should succeed
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.Created));

        // Verify all tasks were created
        var allTasksResponse = await _client.GetAsync("/api/tasks");
        var allTasks = await allTasksResponse.Content.ReadFromJsonAsync<List<TaskItem>>();
        allTasks.Should().HaveCountGreaterThanOrEqualTo(10);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task CreateTask_WithVeryLongTitle_HandlesCorrectly()
    {
        // Arrange
        var longTitle = new string('A', 500); // Very long title
        var task = new TaskItem
        {
            Title = longTitle,
            Priority = Priority.Low,
            Category = Category.Personal,
            UserId = "test-user",
            Tags = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", task);

        // Assert - Should handle long titles
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var createdTask = await response.Content.ReadFromJsonAsync<TaskItem>();
            createdTask!.Title.Should().HaveLength(longTitle.Length);
        }
        else
        {
            // Or return BadRequest if there's a length limit
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task UpdateTask_WithPartialData_UpdatesOnlySpecifiedFields()
    {
        // Arrange
        var createdTask = await CreateTaskAsync("Original Task", Priority.Low);
        var originalDescription = createdTask.Description;
        var originalCategory = createdTask.Category;

        // Act - Update only title and priority
        createdTask.Title = "New Title";
        createdTask.Priority = Priority.High;
        var response = await _client.PutAsJsonAsync($"/api/tasks/{createdTask.Id}", createdTask);
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskItem>();

        // Assert
        updatedTask!.Title.Should().Be("New Title");
        updatedTask.Priority.Should().Be(Priority.High);
        updatedTask.Description.Should().Be(originalDescription);
        updatedTask.Category.Should().Be(originalCategory);
    }

    [Fact]
    public async Task DeleteTask_MultipleTimes_ReturnsNotFoundOnSecondAttempt()
    {
        // Arrange
        var createdTask = await CreateTaskAsync("Task to Delete Twice", Priority.Low);

        // Act - First deletion
        var firstDeleteResponse = await _client.DeleteAsync($"/api/tasks/{createdTask.Id}");
        firstDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act - Second deletion attempt
        var secondDeleteResponse = await _client.DeleteAsync($"/api/tasks/{createdTask.Id}");

        // Assert
        secondDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Helper Methods

    private async Task<TaskItem> CreateTaskAsync(string title, Priority priority, Category category = Category.Work, string? userId = null)
    {
        var task = new TaskItem
        {
            Title = title,
            Description = $"Description for {title}",
            Priority = priority,
            Category = category,
            UserId = userId ?? "test-user",
            Tags = new List<string>()
        };

        var response = await _client.PostAsJsonAsync("/api/tasks", task);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create task. Status: {response.StatusCode}, Error: {error}");
        }
        var createdTask = await response.Content.ReadFromJsonAsync<TaskItem>();
        return createdTask!;
    }

    #endregion
}

