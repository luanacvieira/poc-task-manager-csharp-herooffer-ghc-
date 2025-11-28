using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskManager.Web.Data;
using TaskManager.Web.Data.Interceptors;
using TaskManager.Web.Mappings;
using TaskManager.Web.Repositories;
using TaskManager.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurar HttpContextAccessor para o interceptor de auditoria
builder.Services.AddHttpContextAccessor();

// Configurar interceptor de auditoria
builder.Services.AddSingleton<AuditInterceptor>();

// Configurar Entity Framework Core com SQL Server LocalDB e interceptor
builder.Services.AddDbContext<TaskManagerDbContext>((serviceProvider, options) =>
{
    var auditInterceptor = serviceProvider.GetRequiredService<AuditInterceptor>();
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure())
    .AddInterceptors(auditInterceptor);
});

// Configurar AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Configurar FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Registrar repositórios e serviços
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

var app = builder.Build();

// Aplicar migrations automaticamente no startup (apenas em desenvolvimento)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskManagerDbContext>();
    
    // Usar migrations ao invés de EnsureCreated
    await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tasks}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
