# Task Manager - C# Implementation

## Descrição

Réplica em C# do projeto **poc-task-manager-java-herooffer-ghc**, mantendo equivalência funcional entre as branches com diferentes arquiteturas.

Este repositório contém duas branches principais que demonstram diferentes abordagens arquiteturais:

## 📌 Branches

### Branch: main (Monolith)
Aplicação monolítica de gerenciamento de tarefas desenvolvida em C# usando ASP.NET Core MVC.

**Características:**
- ✅ Arquitetura monolítica
- ✅ ASP.NET Core 9.0 MVC
- ✅ Entity Framework Core com SQL Server LocalDB
- ✅ UI básica com Razor Pages e Bootstrap
- ❌ **Sem** health checks
- ❌ **Sem** containerização
- ❌ **Sem** observabilidade avançada

**Stack:**
- ASP.NET Core 9.0 MVC
- Entity Framework Core 9.0
- SQL Server LocalDB
- Bootstrap 5
- Razor Pages

### Branch: feature/melhorias (Microservices) ⬅️ Você está aqui
Arquitetura de microserviços com serviços desacoplados e melhorias de observabilidade.

**Características:**
- ✅ Arquitetura de microserviços
- ✅ Task Service (CRUD de tarefas)
- ✅ Statistics Service (agregação de dados)
- ✅ API Gateway (roteamento)
- ✅ **Health checks habilitados**
- ✅ **Docker Compose**
- ✅ CORS configurado
- ✅ UI moderna (planejada: React/Blazor)
- ✅ Observabilidade e logging

**Stack:**
- ASP.NET Core 9.0 Web API
- Entity Framework Core 9.0
- SQL Server (containerizado)
- Docker & Docker Compose
- Health Checks
- CORS

## 🏗️ Arquitetura (Branch feature/melhorias)

```
┌─────────────────┐
│   API Gateway   │ :5000
│  (Planejado)    │
└────────┬────────┘
         │
    ┌────┴────┬──────────────┐
    │         │              │
┌───▼────┐ ┌─▼──────────┐  │
│  Task  │ │ Statistics │  │
│Service │ │  Service   │  │
│  :5001 │ │  (Planned) │  │
└───┬────┘ └────────────┘  │
    │                       │
┌───▼──────────────────────▼──┐
│   SQL Server Container      │
│          :1433              │
└─────────────────────────────┘
```

## 🚀 Microserviços

### 1. TaskService.API (porta 5001)
Serviço principal responsável pelo CRUD de tarefas.

**Endpoints:**
- `GET /api/tasks` - Lista todas as tarefas
- `GET /api/tasks/{id}` - Busca tarefa por ID
- `POST /api/tasks` - Cria nova tarefa
- `PUT /api/tasks/{id}` - Atualiza tarefa
- `DELETE /api/tasks/{id}` - Exclui tarefa
- `GET /api/tasks/stats` - Estatísticas
- `GET /health` - Health check

**Funcionalidades:**
- ✅ CRUD completo
- ✅ Validações de entrada
- ✅ Health check com verificação de banco de dados
- ✅ CORS habilitado
- ✅ Logging estruturado
- ✅ Docker support

### 2. StatisticsService.API (planejado)
Serviço de agregação que consome o TaskService para gerar estatísticas avançadas.

### 3. API Gateway (planejado)
Gateway usando Ocelot ou YARP para roteamento de requisições.

## 📋 Modelo de Domínio

### TaskItem
- **Id**: long (PK, auto-gerado)
- **Title**: string (obrigatório, máx 200 caracteres)
- **Description**: string (opcional, máx 2000 caracteres)
- **Priority**: enum (Low, Medium, High, Urgent) - padrão: Medium
- **Category**: enum (Work, Personal, Study, Health, Other) - padrão: Other
- **DueDate**: DateTime? (data de vencimento)
- **Tags**: List<string> (lista de tags)
- **AssignedTo**: string (pessoa atribuída)
- **UserId**: string (obrigatório)
- **Completed**: bool (status de conclusão)
- **CreatedAt**: DateTime (timestamp de criação)
- **UpdatedAt**: DateTime (timestamp de atualização)

## 🔧 Requisitos

- .NET 9.0 SDK
- Docker Desktop (para executar com Docker Compose)
- SQL Server LocalDB ou SQL Server (para execução local sem Docker)

## 🏃 Como Executar

### Opção 1: Com Docker Compose (Recomendado)

```bash
# Clone o repositório
git clone https://github.com/luanacvieira/poc-task-manager-csharp-herooffer-ghc-.git
cd poc-task-manager-csharp-herooffer-ghc-

# Checkout na branch de microserviços
git checkout feature/melhorias

# Executar com Docker Compose
docker-compose up -d
```

**Serviços disponíveis:**
- Task Service API: http://localhost:5001/api/tasks
- Health Check: http://localhost:5001/health
- SQL Server: localhost:1433

### Opção 2: Execução Local

```bash
# Restaurar dependências
dotnet restore

# Executar Task Service
cd src/TaskService.API
dotnet run
```

A API estará disponível em:
- HTTPS: https://localhost:5001
- HTTP: http://localhost:5000

### Testando a API

```bash
# Listar todas as tarefas
curl http://localhost:5001/api/tasks

# Criar uma tarefa
curl -X POST http://localhost:5001/api/tasks \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Implementar microserviços",
    "description": "Criar arquitetura de microserviços",
    "priority": "High",
    "category": "Work",
    "dueDate": "2025-12-31",
    "completed": false
  }'

# Verificar health
curl http://localhost:5001/health
```

## 🏥 Health Checks

O TaskService.API possui health checks configurados que verificam:
- ✅ Disponibilidade da aplicação
- ✅ Conectividade com o banco de dados
- ✅ Estado do DbContext

**Endpoint:** `GET /health`

**Respostas:**
- `200 OK` - Todos os serviços saudáveis
- `503 Service Unavailable` - Algum serviço com problema

## 🐳 Docker

### Build da imagem

```bash
docker build -t taskservice-api:latest -f src/TaskService.API/Dockerfile .
```

### Executar container

```bash
docker run -d \
  -p 5001:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal,1433;Database=TaskServiceDb;User=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True" \
  taskservice-api:latest
```

## 📊 Comparação entre Branches

| Característica | main (Monolith) | feature/melhorias (Microservices) |
|----------------|-----------------|-----------------------------------|
| Arquitetura | Monolítica MVC | Microserviços API |
| UI | Razor Pages básico | API REST (UI moderna planejada) |
| Health Checks | ❌ Não | ✅ Sim |
| Docker | ❌ Não | ✅ Sim |
| CORS | ❌ Não | ✅ Sim |
| Observabilidade | Básica | Avançada |
| Escalabilidade | Limitada | Alta |
| Complexidade | Baixa | Média/Alta |

## 🔄 Comparação com Java

Este projeto replica a funcionalidade do **poc-task-manager-java-herooffer-ghc**:

| Java | C# | Equivalência |
|------|-----|--------------|
| Spring Boot | ASP.NET Core | ✅ Framework web |
| Vaadin | Razor Pages/Blazor | ✅ UI framework |
| JPA/Hibernate | Entity Framework Core | ✅ ORM |
| H2 Database | SQL Server LocalDB | ✅ Banco de dados |
| Spring Cloud Gateway | Ocelot/YARP | ✅ API Gateway |
| Docker Compose | Docker Compose | ✅ Orquestração |
| Spring Boot Actuator | Health Checks | ✅ Monitoramento |

## 📁 Estrutura do Projeto (feature/melhorias)

```
csharp-task-manager/
├── src/
│   ├── TaskManager.Web/        # Monolito (branch main)
│   └── TaskService.API/         # Microserviço Task Service
│       ├── Controllers/
│       ├── Models/
│       ├── Data/
│       ├── Repositories/
│       ├── Services/
│       ├── Program.cs
│       ├── appsettings.json
│       └── Dockerfile
├── docker-compose.yml
├── TaskManager.sln
└── README.md
```

## 🎯 Próximos Passos

- [ ] Implementar StatisticsService.API
- [ ] Implementar API Gateway (Ocelot ou YARP)
- [ ] Criar frontend moderno (React ou Blazor WebAssembly)
- [ ] Adicionar autenticação JWT
- [ ] Implementar cache distribuído (Redis)
- [ ] Adicionar message broker (RabbitMQ ou Azure Service Bus)
- [ ] Implementar observabilidade completa (Application Insights, Prometheus)
- [ ] Adicionar testes unitários e de integração
- [ ] Configurar CI/CD (GitHub Actions)

## 📝 Notas Importantes

### Branch main
- Ideal para aprendizado e projetos pequenos
- Baixa complexidade de deploy
- Todos os componentes em um único processo

### Branch feature/melhorias
- Ideal para produção e projetos escaláveis
- Requer infraestrutura mais complexa
- Serviços independentes e escaláveis
- Melhor observabilidade e monitoramento

## 🤝 Contribuindo

Este é um projeto educacional que demonstra diferentes arquiteturas. Contribuições são bem-vindas!

## 📄 Licença

Este é um projeto de exemplo para fins educacionais.

## 🔗 Links Relacionados

- Repositório Java original: [poc-task-manager-java-herooffer-ghc](https://github.com/luanacvieira/poc-task-manager-java-herooffer-ghc)
- ASP.NET Core Documentation: https://docs.microsoft.com/aspnet/core
- Docker Documentation: https://docs.docker.com
- Entity Framework Core: https://docs.microsoft.com/ef/core

