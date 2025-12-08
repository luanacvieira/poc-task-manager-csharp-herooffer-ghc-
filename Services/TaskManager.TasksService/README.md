# TaskManager.TasksService

## Descrição
Microserviço responsável por gerenciar as operações CRUD de tarefas.

## Responsabilidades
- Criar, ler, atualizar e deletar tarefas
- Armazenar tarefas no banco de dados TasksDb
- Expor API REST para operações de tarefas

## Endpoints
- `GET /api/tasks` - Listar todas as tarefas
- `GET /api/tasks/{id}` - Obter tarefa por ID
- `POST /api/tasks` - Criar nova tarefa
- `PUT /api/tasks/{id}` - Atualizar tarefa
- `DELETE /api/tasks/{id}` - Deletar tarefa

## Banco de Dados
- **Nome**: TasksDb
- **Servidor**: SQL Server
- **Conexão**: Configurada via ConnectionString no appsettings.json

## Como Executar

### Local
```bash
cd Services/TaskManager.TasksService
dotnet restore
dotnet run
```

### Docker
```bash
docker build -t taskmanager-tasks-service -f Services/TaskManager.TasksService/Dockerfile .
docker run -p 8080:8080 taskmanager-tasks-service
```

## Porta
- **Padrão**: 8080
- **Swagger**: http://localhost:8080/swagger

