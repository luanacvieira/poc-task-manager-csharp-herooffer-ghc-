using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Reflection;
using TaskManager.Web.Data;
using TaskManager.Web.Data.Interceptors;
using TaskManager.Web.Mappings;
using TaskManager.Web.Repositories;
using TaskManager.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(options =>
{
    // Limitar tamanho máximo de requisições para prevenir ataques DoS
    options.MaxModelBindingCollectionSize = 100;
});

// Configurar HttpContextAccessor para o interceptor de auditoria
builder.Services.AddHttpContextAccessor();

// Configurar interceptor de auditoria (apenas fora de testes de integração)
if (!builder.Configuration.GetValue<bool>("UseInMemoryDatabase"))
{
    builder.Services.AddSingleton<AuditInterceptor>();
}

// Configurar Entity Framework Core
builder.Services.AddDbContext<TaskManagerDbContext>((serviceProvider, options) =>
{
    // Verificar se deve usar InMemoryDatabase (para testes) ou SQL Server (produção/dev)
    var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");
    
    if (!useInMemory)
    {
        // Configuração para produção/desenvolvimento com SQL Server
        var auditInterceptor = serviceProvider.GetService<AuditInterceptor>();
        
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => 
            {
                sqlOptions.EnableRetryOnFailure();
                sqlOptions.CommandTimeout(30); // Timeout de 30 segundos para prevenir consultas longas
            });
        
        // Adicionar interceptor apenas se disponível (não nos testes)
        if (auditInterceptor != null)
        {
            options.AddInterceptors(auditInterceptor);
        }
    }
    // else: InMemory será configurado nos testes
    
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "IntegrationTest")
           .EnableDetailedErrors(builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "IntegrationTest");
});

// Configurar AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Configurar FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<MappingProfile>();

// Registrar repositórios e serviços
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

// Configurar OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Task Manager API",
        Description = "API REST para gerenciamento de tarefas com suporte a paginação, filtros e ordenação"
    });

    // Incluir comentários XML na documentação
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configurar CORS com política restritiva
builder.Services.AddCors(options =>
{
    options.AddPolicy("RestrictedPolicy", corsBuilder =>
    {
        // Permitir apenas origens específicas do appsettings
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
            ?? new[] { "http://localhost:5000", "https://localhost:5001" };
        
        corsBuilder.WithOrigins(allowedOrigins)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials(); // Necessário para cookies/autenticação
    });
});

var app = builder.Build();

// Aplicar migrations automaticamente no startup (apenas em desenvolvimento e não para InMemory)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskManagerDbContext>();
    
    try
    {
        // Apenas aplicar migrations se não estiver usando InMemory (testes)
        if (dbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            await dbContext.Database.MigrateAsync();
        }
    }
    catch (InvalidOperationException)
    {
        // Se houver erro ao acessar ProviderName (múltiplos providers registrados),
        // significa que estamos em um ambiente de teste - não fazer nada
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Manager API v1");
        options.RoutePrefix = string.Empty; // Swagger na raiz
        options.DocumentTitle = "Task Manager API Documentation";
    });
}

// Habilitar redirecionamento HTTPS para segurança
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("RestrictedPolicy");
app.UseAuthorization();
app.MapControllers();

app.Run();

// Tornar a classe Program acessível para testes de integração
public partial class Program { }
