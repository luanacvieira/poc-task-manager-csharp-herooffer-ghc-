# TaskManager.StatisticsService

## Descrição
Microserviço responsável por cálculo e fornecimento de estatísticas sobre tarefas.

## Responsabilidades
- Calcular estatísticas gerais (total, completadas, pendentes, urgentes)
- Fornecer estatísticas por categoria
- Fornecer estatísticas por prioridade
- Leitura do mesmo banco de dados de tarefas (read-only operations)

## Endpoints
- `GET /api/statistics` - Obter todas as estatísticas

## Banco de Dados
- **Nome**: TasksDb (compartilhado com TasksService - somente leitura)
- **Servidor**: SQL Server
- **Conexão**: Configurada via ConnectionString no appsettings.json

## Como Executar

### Local
```bash
cd Services/TaskManager.StatisticsService
dotnet restore
dotnet run
```

### Docker
```bash
docker build -t taskmanager-statistics-service -f Services/TaskManager.StatisticsService/Dockerfile .
docker run -p 8081:8081 taskmanager-statistics-service
```

## Porta
- **Padrão**: 8081
- **Swagger**: http://localhost:8081/swagger

