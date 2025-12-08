using TaskManager.StatisticsService.Models;

namespace TaskManager.StatisticsService.Services;

public interface IStatisticsService
{
    Task<TaskStatistics> GetStatisticsAsync();
}

