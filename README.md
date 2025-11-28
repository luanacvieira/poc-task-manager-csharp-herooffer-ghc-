# Resumo da Modernização - Task Manager

## 📋 Índice
- [Visão Geral](#visão-geral)
- [Arquivos Criados e Modificados](#arquivos-criados-e-modificados)
- [Serviços Extraídos e Padrões Implementados](#serviços-extraídos-e-padrões-implementados)
- [Como Rodar a Aplicação](#como-rodar-a-aplicação)
- [Endpoints da API REST](#endpoints-da-api-rest)
- [Exemplos de Uso](#exemplos-de-uso)
- [Próximos Passos](#próximos-passos)

---

## 🎯 Visão Geral

Esta aplicação ASP.NET Core MVC Task Manager foi modernizada seguindo as melhores práticas atuais de desenvolvimento. As principais melhorias incluem:

### ✨ Principais Melhorias
- **Separação de Concerns**: Projeto API REST separado do MVC
- **Result Pattern**: Tratamento de erros consistente em toda a aplicação
- **Validação Robusta**: FluentValidation para validação de entrada
- **Documentação API**: Swagger/OpenAPI integrado
- **Auditoria Automática**: Interceptor para campos CreatedBy/UpdatedBy
- **Concorrência Otimista**: RowVersion para prevenir conflitos
- **Performance**: Índices de banco de dados otimizados
- **Migrations**: Substituição de EnsureCreated por EF Core Migrations
- **Paginação e Filtros**: Suporte completo para paginação, filtros e ordenação

---

## 📁 Arquivos Criados e Modificados

### 🆕 Novos Arquivos Criados

#### **TaskManager.Web/Common/**
```
├── Result.cs                    # Result Pattern genérico
├── Error.cs                     # Representação padronizada de erros
├── ErrorCodes.cs               # Códigos de erro centralizados
├── QueryParameters.cs          # Parâmetros de query (paginação, filtros, ordenação)
└── PaginatedResult.cs          # Resultado paginado genérico
```

#### **TaskManager.Web/DTOs/**
```
└── TaskDtos.cs
    ├── CreateTaskDto           # DTO para criação de tarefas
    ├── UpdateTaskDto           # DTO para atualização de tarefas
    ├── TaskDto                 # DTO de resposta com dados completos
    └── TaskStatisticsDto       # DTO para estatísticas de tarefas
```

#### **TaskManager.Web/Validators/**
```
├── TaskItemValidator.cs        # Validador para entidade TaskItem
└── TaskDtoValidators.cs
    ├── CreateTaskDtoValidator  # Validador para criação
    └── UpdateTaskDtoValidator  # Validador para atualização
```

#### **TaskManager.Web/Mappings/**
```
└── MappingProfile.cs           # Perfil AutoMapper para conversões
```

#### **TaskManager.Web/Data/Interceptors/**
```
└── AuditInterceptor.cs         # Interceptor para auditoria automática
```

#### **TaskManager.Web/Migrations/**
```
└── 20251128171723_CurrentSnapshot.cs  # Migration baseline
```

#### **TaskManager.Api/** (Novo Projeto)
```
├── TaskManager.Api.csproj      # Projeto da API REST
├── Program.cs                  # Configuração da API
├── appsettings.json            # Configurações da API
├── Properties/
│   └── launchSettings.json     # Configurações de execução
└── Controllers/
    └── TasksController.cs      # Controlador REST com 6 endpoints
```

### 📝 Arquivos Modificados

#### **TaskManager.Web/**
```
├── Models/
│   └── TaskItem.cs             # Adicionados: CreatedBy, UpdatedBy, RowVersion
├── Data/
│   └── TaskManagerDbContext.cs # Adicionados: 7 índices, removida lógica de timestamp manual
├── Repositories/
│   ├── ITaskRepository.cs      # Adicionado: GetPagedAsync com filtros
│   └── TaskRepository.cs       # Implementação de paginação, filtros e ordenação
├── Services/
│   ├── ITaskService.cs         # Refatorado para usar Result Pattern e DTOs
│   └── TaskService.cs          # Implementação com validação e Result Pattern
├── Controllers/
│   └── TasksController.cs      # Atualizado para usar DTOs e Result Pattern
└── Program.cs                  # Adicionados: AutoMapper, FluentValidation, AuditInterceptor, Migrations
```

---

## 🏗️ Serviços Extraídos e Padrões Implementados

### 1️⃣ **Result Pattern**
**Localização**: `TaskManager.Web/Common/Result.cs`

Padrão para tratamento consistente de erros sem usar exceções para controle de fluxo:

```csharp
// Exemplo de uso
public async Task<Result<TaskDto>> GetTaskByIdAsync(int id)
{
    var task = await _repository.GetByIdAsync(id);
    if (task == null)
        return Result<TaskDto>.Failure(Error.NotFound("Task.NotFound", $"Task with ID {id} not found"));
    
    var taskDto = _mapper.Map<TaskDto>(task);
    return Result<TaskDto>.Success(taskDto);
}
```

**Benefícios**:
- Erros explícitos no tipo de retorno
- Código mais limpo e legível
- Evita exceções desnecessárias
- Tratamento de erro previsível

### 2️⃣ **Data Transfer Objects (DTOs)**
**Localização**: `TaskManager.Web/DTOs/TaskDtos.cs`

Separação entre modelo de domínio e modelo de API:

- **CreateTaskDto**: Apenas campos necessários para criação
- **UpdateTaskDto**: Campos editáveis com RowVersion para concorrência
- **TaskDto**: Resposta completa incluindo campos de auditoria
- **TaskStatisticsDto**: Estatísticas agregadas

**Benefícios**:
- Controle fino sobre o que é exposto na API
- Validação específica por operação
- Versionamento de API facilitado
- Segurança (não expor campos internos)

### 3️⃣ **FluentValidation**
**Localização**: `TaskManager.Web/Validators/`

Validação declarativa e reutilizável:

```csharp
public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("O título é obrigatório")
            .MaximumLength(200).WithMessage("O título deve ter no máximo 200 caracteres");
        
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .When(x => x.DueDate.HasValue)
            .WithMessage("A data de vencimento não pode estar no passado");
    }
}
```

**Benefícios**:
- Validações complexas e legíveis
- Mensagens de erro personalizadas
- Validações reutilizáveis
- Integração com ASP.NET Core

### 4️⃣ **AutoMapper**
**Localização**: `TaskManager.Web/Mappings/MappingProfile.cs`

Mapeamento automático entre entidades e DTOs:

```csharp
CreateMap<CreateTaskDto, TaskItem>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());
```

**Benefícios**:
- Menos código boilerplate
- Conversões consistentes
- Fácil manutenção
- Testável

### 5️⃣ **Audit Interceptor**
**Localização**: `TaskManager.Web/Data/Interceptors/AuditInterceptor.cs`

Auditoria automática de entidades:

```csharp
public override ValueTask<InterceptionResult<int>> SavingChangesAsync(...)
{
    var entries = DbContext.ChangeTracker.Entries()
        .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
    
    foreach (var entry in entries)
    {
        if (entry.State == EntityState.Added)
        {
            entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
            entry.Property("CreatedBy").CurrentValue = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
        }
        // ...
    }
}
```

**Benefícios**:
- Auditoria automática e consistente
- Sem código duplicado nos serviços
- Cross-cutting concern centralizado
- Rastreabilidade completa

### 6️⃣ **Concorrência Otimista**
**Localização**: `TaskManager.Web/Models/TaskItem.cs`

Prevenção de conflitos de atualização concorrente:

```csharp
[Timestamp]
public byte[] RowVersion { get; set; } = null!;
```

Tratamento no repositório:
```csharp
catch (DbUpdateConcurrencyException ex)
{
    throw new InvalidOperationException(
        "The task was modified by another user. Please refresh and try again.", ex);
}
```

**Benefícios**:
- Previne perda de dados
- Detecta modificações concorrentes
- Feedback claro ao usuário
- Performance (sem locks no banco)

### 7️⃣ **Índices de Performance**
**Localização**: `TaskManager.Web/Data/TaskManagerDbContext.cs`

Otimização de queries frequentes:

```csharp
modelBuilder.Entity<TaskItem>()
    .HasIndex(t => t.UserId);
    
modelBuilder.Entity<TaskItem>()
    .HasIndex(t => new { t.UserId, t.Completed });
    
modelBuilder.Entity<TaskItem>()
    .HasIndex(t => t.DueDate);
```

**Índices criados**:
1. `IX_Tasks_UserId` - Filtro por usuário
2. `IX_Tasks_Completed` - Filtro por status
3. `IX_Tasks_Priority` - Ordenação por prioridade
4. `IX_Tasks_Category` - Filtro por categoria
5. `IX_Tasks_DueDate` - Filtro e ordenação por data
6. `IX_Tasks_CreatedAt` - Ordenação por data de criação
7. `IX_Tasks_UserId_Completed` - Índice composto para queries combinadas

### 8️⃣ **Paginação, Filtros e Ordenação**
**Localização**: `TaskManager.Web/Common/QueryParameters.cs` e `TaskManager.Web/Repositories/TaskRepository.cs`

Sistema completo de query:

**Filtros suportados**:
- Título (busca parcial)
- Prioridade (Low, Medium, High)
- Categoria (Work, Personal, Shopping, Health, Other)
- Completado (true/false)
- UserId
- AssignedTo
- DueDateFrom / DueDateTo (range de datas)
- Tag (busca em array)

**Ordenação suportada**:
- Title, DueDate, Priority, Category, CreatedAt, UpdatedAt, Completed
- Direção: asc/desc

**Paginação**:
- PageNumber (padrão: 1)
- PageSize (padrão: 10, máximo: 100)

---

## 🚀 Como Rodar a Aplicação

### Pré-requisitos
- .NET 9.0 SDK
- SQL Server LocalDB (mssqllocaldb)
- Visual Studio Code ou Visual Studio 2022

### 1. Restaurar Pacotes
```powershell
cd c:\Users\leonardo.vizagre\source\repos\poc-task-manager-csharp-herooffer-ghc
dotnet restore
```

### 2. Aplicar Migrations (Primeira Execução)
```powershell
cd TaskManager.Web
dotnet ef database update
```

Ou simplesmente rode a aplicação - as migrations são aplicadas automaticamente no startup.

### 3. Rodar a API REST
```powershell
dotnet run --project TaskManager.Api\TaskManager.Api.csproj
```

A API estará disponível em:
- **HTTP**: http://localhost:5001
- **Swagger UI**: http://localhost:5001/swagger

### 4. Rodar a Aplicação MVC (Interface Web)
```powershell
dotnet run --project TaskManager.Web\TaskManager.Web.csproj
```

A aplicação web estará disponível em:
- **HTTP**: https://localhost:5001 ou http://localhost:5000

### 5. Verificar o Banco de Dados
```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -d TaskManagerDB -Q "SELECT * FROM Tasks"
```

---

## 🌐 Endpoints da API REST

Base URL: `http://localhost:5001/api`

### 📋 1. Listar Tarefas (Paginado)
**Endpoint**: `GET /api/tasks`

**Query Parameters**:
```
pageNumber: int (default: 1)
pageSize: int (default: 10, max: 100)
sortBy: string (Title, DueDate, Priority, Category, CreatedAt, UpdatedAt, Completed)
sortDirection: string (asc, desc)
title: string (filtro parcial)
priority: int (0=Low, 1=Medium, 2=High)
category: int (0=Work, 1=Personal, 2=Shopping, 3=Health, 4=Other)
completed: bool
userId: string
assignedTo: string
dueDateFrom: datetime
dueDateTo: datetime
tag: string
```

**Resposta 200 OK**:
```json
{
  "items": [
    {
      "id": 1,
      "title": "Implementar API REST",
      "description": "Criar endpoints para CRUD de tarefas",
      "completed": false,
      "priority": 2,
      "category": 0,
      "dueDate": "2025-12-31T00:00:00Z",
      "userId": "user123",
      "assignedTo": "developer@example.com",
      "tags": ["backend", "api"],
      "createdAt": "2025-11-28T10:00:00Z",
      "createdBy": "System",
      "updatedAt": "2025-11-28T10:00:00Z",
      "updatedBy": "System",
      "rowVersion": "AAAAAAAAB9E="
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5,
  "totalCount": 42,
  "hasPrevious": false,
  "hasNext": true
}
```

### 🔍 2. Obter Tarefa por ID
**Endpoint**: `GET /api/tasks/{id}`

**Path Parameters**:
- `id`: int (ID da tarefa)

**Resposta 200 OK**:
```json
{
  "id": 1,
  "title": "Implementar API REST",
  "description": "Criar endpoints para CRUD de tarefas",
  "completed": false,
  "priority": 2,
  "category": 0,
  "dueDate": "2025-12-31T00:00:00Z",
  "userId": "user123",
  "assignedTo": "developer@example.com",
  "tags": ["backend", "api"],
  "createdAt": "2025-11-28T10:00:00Z",
  "createdBy": "System",
  "updatedAt": "2025-11-28T10:00:00Z",
  "updatedBy": "System",
  "rowVersion": "AAAAAAAAB9E="
}
```

**Resposta 404 Not Found**:
```json
{
  "code": "Task.NotFound",
  "message": "Task with ID 999 not found"
}
```

### ✏️ 3. Criar Tarefa
**Endpoint**: `POST /api/tasks`

**Request Body**:
```json
{
  "title": "Nova tarefa",
  "description": "Descrição detalhada da tarefa",
  "priority": 1,
  "category": 0,
  "dueDate": "2025-12-31T23:59:59Z",
  "userId": "user123",
  "assignedTo": "developer@example.com",
  "tags": ["backend", "urgent"]
}
```

**Resposta 201 Created**:
```json
{
  "id": 42,
  "title": "Nova tarefa",
  "description": "Descrição detalhada da tarefa",
  "completed": false,
  "priority": 1,
  "category": 0,
  "dueDate": "2025-12-31T23:59:59Z",
  "userId": "user123",
  "assignedTo": "developer@example.com",
  "tags": ["backend", "urgent"],
  "createdAt": "2025-11-28T17:30:00Z",
  "createdBy": "user123",
  "updatedAt": "2025-11-28T17:30:00Z",
  "updatedBy": "user123",
  "rowVersion": "AAAAAAAACDE="
}
```

**Resposta 400 Bad Request** (Validação):
```json
{
  "code": "Validation.Failed",
  "message": "One or more validation errors occurred",
  "validationErrors": {
    "Title": ["O título é obrigatório"],
    "DueDate": ["A data de vencimento não pode estar no passado"]
  }
}
```

### 🔄 4. Atualizar Tarefa
**Endpoint**: `PUT /api/tasks/{id}`

**Path Parameters**:
- `id`: int (ID da tarefa)

**Request Body**:
```json
{
  "title": "Tarefa atualizada",
  "description": "Nova descrição",
  "completed": true,
  "priority": 2,
  "category": 0,
  "dueDate": "2025-12-31T23:59:59Z",
  "assignedTo": "another@example.com",
  "tags": ["backend", "completed"],
  "rowVersion": "AAAAAAAAB9E="
}
```

**Resposta 200 OK**:
```json
{
  "id": 1,
  "title": "Tarefa atualizada",
  "description": "Nova descrição",
  "completed": true,
  "priority": 2,
  "category": 0,
  "dueDate": "2025-12-31T23:59:59Z",
  "userId": "user123",
  "assignedTo": "another@example.com",
  "tags": ["backend", "completed"],
  "createdAt": "2025-11-28T10:00:00Z",
  "createdBy": "System",
  "updatedAt": "2025-11-28T17:45:00Z",
  "updatedBy": "user123",
  "rowVersion": "AAAAAAAACDF="
}
```

**Resposta 404 Not Found**:
```json
{
  "code": "Task.NotFound",
  "message": "Task with ID 999 not found"
}
```

**Resposta 409 Conflict** (Concorrência):
```json
{
  "code": "Task.Conflict",
  "message": "The task was modified by another user. Please refresh and try again."
}
```

### ❌ 5. Excluir Tarefa
**Endpoint**: `DELETE /api/tasks/{id}`

**Path Parameters**:
- `id`: int (ID da tarefa)

**Resposta 204 No Content**:
(Sem corpo de resposta)

**Resposta 404 Not Found**:
```json
{
  "code": "Task.NotFound",
  "message": "Task with ID 999 not found"
}
```

### 📊 6. Obter Estatísticas
**Endpoint**: `GET /api/tasks/statistics`

**Query Parameters**:
```
userId: string (opcional - filtrar por usuário)
```

**Resposta 200 OK**:
```json
{
  "totalTasks": 42,
  "completedTasks": 18,
  "pendingTasks": 24,
  "overdueTasks": 5,
  "tasksByPriority": {
    "Low": 10,
    "Medium": 20,
    "High": 12
  },
  "tasksByCategory": {
    "Work": 25,
    "Personal": 10,
    "Shopping": 3,
    "Health": 2,
    "Other": 2
  }
}
```

---

## 📝 Exemplos de Uso

### Exemplo 1: Criar uma Tarefa
```powershell
$body = @{
    title = "Revisar código da API"
    description = "Fazer code review do PR #123"
    priority = 2
    category = 0
    dueDate = "2025-12-01T18:00:00Z"
    userId = "dev001"
    assignedTo = "senior.dev@company.com"
    tags = @("code-review", "urgent")
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5001/api/tasks" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"
```

### Exemplo 2: Listar Tarefas Pendentes de Alta Prioridade
```powershell
Invoke-RestMethod -Uri "http://localhost:5001/api/tasks?completed=false&priority=2&pageSize=20&sortBy=DueDate&sortDirection=asc"
```

### Exemplo 3: Atualizar uma Tarefa
```powershell
$task = Invoke-RestMethod -Uri "http://localhost:5001/api/tasks/1"

$updateBody = @{
    title = $task.title
    description = $task.description
    completed = $true
    priority = $task.priority
    category = $task.category
    dueDate = $task.dueDate
    assignedTo = $task.assignedTo
    tags = $task.tags
    rowVersion = $task.rowVersion
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5001/api/tasks/1" `
    -Method Put `
    -Body $updateBody `
    -ContentType "application/json"
```

### Exemplo 4: Buscar Tarefas com Filtros Múltiplos
```powershell
# Tarefas de trabalho pendentes com vencimento esta semana
$hoje = Get-Date -Format "yyyy-MM-dd"
$proximaSemana = (Get-Date).AddDays(7).ToString("yyyy-MM-dd")

Invoke-RestMethod -Uri "http://localhost:5001/api/tasks?category=0&completed=false&dueDateFrom=$hoje&dueDateTo=$proximaSemana&sortBy=DueDate"
```

### Exemplo 5: Obter Estatísticas por Usuário
```powershell
Invoke-RestMethod -Uri "http://localhost:5001/api/tasks/statistics?userId=dev001"
```

### Exemplo 6: Excluir uma Tarefa
```powershell
Invoke-RestMethod -Uri "http://localhost:5001/api/tasks/1" -Method Delete
```

### Exemplo 7: Filtrar por Tags
```powershell
Invoke-RestMethod -Uri "http://localhost:5001/api/tasks?tag=urgent&pageSize=50"
```

---

## 🎯 Próximos Passos

### 1️⃣ **Segurança e Autenticação**
- [ ] Implementar autenticação JWT
- [ ] Adicionar autorização baseada em roles (Admin, User)
- [ ] Implementar políticas de acesso (usuários só veem suas próprias tarefas)
- [ ] Rate limiting na API
- [ ] Validação de origem (CORS configurado corretamente)

**Sugestão de implementação**:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* configuração */ });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TaskOwner", policy =>
        policy.Requirements.Add(new TaskOwnerRequirement()));
});
```

### 2️⃣ **Observabilidade**
- [ ] Logging estruturado com Serilog
- [ ] Application Insights para telemetria
- [ ] Health checks (banco de dados, dependências externas)
- [ ] Métricas customizadas (ex: taxa de conclusão de tarefas)
- [ ] Distributed tracing

**Sugestão de implementação**:
```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TaskManagerDbContext>()
    .AddCheck<CustomHealthCheck>("custom");

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
```

### 3️⃣ **CI/CD**
- [ ] Pipeline GitHub Actions ou Azure DevOps
  - Build automatizado
  - Testes unitários e de integração
  - Análise de código (SonarQube)
  - Deploy automático
- [ ] Versionamento semântico
- [ ] Changelog automatizado
- [ ] Docker containerization

**Exemplo de pipeline**:
```yaml
# .github/workflows/ci.yml
name: CI/CD
on: [push, pull_request]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      - name: Restore
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal
```

### 4️⃣ **Testes**
- [ ] Testes unitários para serviços
- [ ] Testes de integração para repositórios
- [ ] Testes de API (endpoints)
- [ ] Testes de validação
- [ ] Testes de concorrência
- [ ] Code coverage > 80%

**Sugestão de estrutura**:
```
TaskManager.Tests/
├── Unit/
│   ├── Services/
│   │   └── TaskServiceTests.cs
│   └── Validators/
│       └── CreateTaskDtoValidatorTests.cs
├── Integration/
│   ├── Repositories/
│   │   └── TaskRepositoryTests.cs
│   └── Api/
│       └── TasksControllerTests.cs
└── TestFixtures/
    └── DatabaseFixture.cs
```

### 5️⃣ **Performance e Escalabilidade**
- [ ] Caching (Redis) para queries frequentes
- [ ] Paginação cursor-based para grandes volumes
- [ ] Compressão de resposta (Gzip/Brotli)
- [ ] Background jobs (Hangfire) para tarefas assíncronas
- [ ] Read replicas para queries pesadas

**Sugestão de implementação**:
```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration.GetConnectionString("Redis");
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
});
```

### 6️⃣ **Funcionalidades Adicionais**
- [ ] Notificações (email, push) para tarefas vencendo
- [ ] Anexos de arquivos (Azure Blob Storage)
- [ ] Comentários e histórico de mudanças
- [ ] Compartilhamento de tarefas entre usuários
- [ ] Subtarefas (relacionamento hierárquico)
- [ ] Recorrência de tarefas
- [ ] Exportação (CSV, PDF)
- [ ] Webhooks para integrações

### 7️⃣ **Documentação**
- [ ] Exemplos de requisição/resposta no Swagger
- [ ] OpenAPI annotations detalhadas
- [ ] README com guia de início rápido
- [ ] Postman collection
- [ ] Documentação de arquitetura (diagramas)

### 8️⃣ **DevOps e Infraestrutura**
- [ ] Docker Compose para ambiente local completo
- [ ] Kubernetes manifests para orquestração
- [ ] Terraform para infraestrutura como código
- [ ] Secrets management (Azure Key Vault)
- [ ] Backup automático do banco de dados

### 9️⃣ **Qualidade de Código**
- [ ] Análise estática (Roslyn analyzers)
- [ ] EditorConfig para consistência
- [ ] Pre-commit hooks (Husky)
- [ ] Conventional commits
- [ ] Pull request templates

### 🔟 **Monitoramento e Alertas**
- [ ] Dashboard de métricas em tempo real
- [ ] Alertas para erros críticos
- [ ] Monitoramento de performance (APM)
- [ ] Logs centralizados (ELK Stack ou Azure Monitor)

---

## 🔧 Troubleshooting

### Problema: Migration não é aplicada
**Solução**:
```powershell
cd TaskManager.Web
dotnet ef database drop --force
dotnet ef database update
```

### Problema: Erro de conexão com o banco
**Solução**: Verificar se SQL Server LocalDB está instalado e rodando:
```powershell
sqllocaldb info
sqllocaldb start MSSQLLocalDB
```

### Problema: Porta já em uso
**Solução**: Alterar a porta em `launchSettings.json` ou matar o processo:
```powershell
# Encontrar processo usando a porta 5001
Get-Process -Id (Get-NetTCPConnection -LocalPort 5001).OwningProcess | Stop-Process
```

### Problema: Warning sobre versão do AutoMapper
**Solução**: Este warning é não-bloqueante e ocorre porque `AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1` requer `AutoMapper 12.0.1`, mas a versão `15.1.0` foi resolvida. Para resolver:
```powershell
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection --version 13.0.1
```

---

## 📚 Recursos e Referências

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [FluentValidation](https://docs.fluentvalidation.net)
- [AutoMapper](https://docs.automapper.org)
- [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [Result Pattern](https://enterprisecraftsmanship.com/posts/error-handling-exception-or-result/)

---

## 🤝 Contribuição

Para contribuir com o projeto:

1. Fork o repositório
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

---

## 📄 Licença

Este projeto é privado e proprietário.

---

**Última atualização**: 28/11/2025  
**Versão**: 1.0.0  
**Autor**: Modernização realizada por GitHub Copilot
