# TaskManager.ApiGateway

## Descrição
API Gateway que roteia requisições externas para os microserviços internos usando YARP (Yet Another Reverse Proxy).

## Responsabilidades
- Roteamento de requisições para microserviços
- Ponto de entrada único para clientes externos
- Load balancing (futuramente)
- Autenticação e autorização (futuramente)

## Rotas Configuradas
- `/api/tasks/**` → TasksService (porta 8080)
- `/api/statistics/**` → StatisticsService (porta 8081)

## Como Executar

### Local
```bash
cd Services/TaskManager.ApiGateway
dotnet restore
dotnet run
```

### Docker
```bash
docker build -t taskmanager-api-gateway -f Services/TaskManager.ApiGateway/Dockerfile .
docker run -p 8082:8082 taskmanager-api-gateway
```

## Porta
- **Padrão**: 8082
- **Swagger**: http://localhost:8082/swagger

## Tecnologias
- YARP (Yet Another Reverse Proxy)
- ASP.NET Core 9.0

