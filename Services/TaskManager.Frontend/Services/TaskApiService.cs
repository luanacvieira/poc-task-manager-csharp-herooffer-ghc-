using System.Text;
using System.Text.Json;
using TaskManager.Frontend.Models;

namespace TaskManager.Frontend.Services;

public class TaskApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TaskApiService> _logger;
    private readonly string _apiBaseUrl;

    public TaskApiService(HttpClient httpClient, IConfiguration configuration, ILogger<TaskApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiBaseUrl = configuration["ApiGateway:BaseUrl"] ?? "http://api-gateway:8082";
        _httpClient.BaseAddress = new Uri(_apiBaseUrl);
    }

    public async Task<List<TaskItem>> GetAllTasksAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/tasks");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TaskItem>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<TaskItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tasks");
            return new List<TaskItem>();
        }
    }

    public async Task<TaskItem?> GetTaskByIdAsync(long id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/tasks/{id}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TaskItem>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching task {TaskId}", id);
            return null;
        }
    }

    public async Task<TaskItem?> CreateTaskAsync(TaskItem task)
    {
        try
        {
            var json = JsonSerializer.Serialize(task);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/tasks", content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TaskItem>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return null;
        }
    }

    public async Task<TaskItem?> UpdateTaskAsync(TaskItem task)
    {
        try
        {
            var json = JsonSerializer.Serialize(task);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"/api/tasks/{task.Id}", content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TaskItem>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task");
            return null;
        }
    }

    public async Task<bool> DeleteTaskAsync(long id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/tasks/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            return false;
        }
    }

    public async Task<TaskStatistics?> GetStatisticsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/statistics");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TaskStatistics>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching statistics");
            return null;
        }
    }
}

