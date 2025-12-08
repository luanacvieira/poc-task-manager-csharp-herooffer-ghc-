# TaskManager.Frontend

## Descrição
Interface web frontend que consome os microserviços via API Gateway.

## Responsabilidades
- Interface de usuário para gerenciamento de tarefas
- Comunicação com API Gateway
- Exibição de estatísticas
- Formulários de criação/edição de tarefas

## Como Executar

### Local
```bash
cd Services/TaskManager.Frontend
dotnet restore
dotnet run
```

### Docker
```bash
docker build -t taskmanager-frontend -f Services/TaskManager.Frontend/Dockerfile .
docker run -p 8083:8083 taskmanager-frontend
```

## Porta
- **Padrão**: 8083
- **URL**: http://localhost:8083

## Dependências
- API Gateway (http://api-gateway:8082)

## Tecnologias
- ASP.NET Core MVC 9.0
- Bootstrap 5
- Razor Views

