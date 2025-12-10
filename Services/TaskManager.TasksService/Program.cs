using Microsoft.EntityFrameworkCore;
using TaskManager.TasksService.Data;
using TaskManager.TasksService.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Database
builder.Services.AddDbContext<TasksDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("TasksConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

// Register repositories
builder.Services.AddScoped<ITaskRepository, TaskRepository>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
