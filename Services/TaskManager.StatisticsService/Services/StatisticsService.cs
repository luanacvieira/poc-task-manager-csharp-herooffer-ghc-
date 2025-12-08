using Microsoft.EntityFrameworkCore;
using TaskManager.StatisticsService.Data;
using TaskManager.StatisticsService.Models;

namespace TaskManager.StatisticsService.Services;

public class StatisticsService : IStatisticsService
{
    private readonly StatisticsDbContext _context;

    public StatisticsService(StatisticsDbContext context)
    {
        _context = context;
    }

    public async Task<TaskStatistics> GetStatisticsAsync()
    {
        var total = await _context.Tasks.CountAsync();
        var completed = await _context.Tasks.CountAsync(t => t.Completed);
        var pending = await _context.Tasks.CountAsync(t => !t.Completed);
        var urgentActive = await _context.Tasks.CountAsync(t => 
            t.Priority == Priority.Urgent && !t.Completed);

        var byCategory = await _context.Tasks
            .GroupBy(t => t.Category)
            .Select(g => new { Category = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Category, x => x.Count);

        var byPriority = await _context.Tasks
            .GroupBy(t => t.Priority)
            .Select(g => new { Priority = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Priority, x => x.Count);

        return new TaskStatistics
        {
            Total = total,
            Completed = completed,
            Pending = pending,
            UrgentActive = urgentActive,
            ByCategory = byCategory,
            ByPriority = byPriority
        };
    }
}

