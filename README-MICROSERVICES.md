# Task Manager - Arquitetura de Microserviços

Sistema de gerenciamento de tarefas desenvolvido em .NET 9.0 com arquitetura de microserviços.

## 🏗️ Arquitetura

O sistema foi transformado de um monolito em uma arquitetura de microserviços com os seguintes componentes:

### Microserviços

1. **Tasks Service** (Porta 8080)
   - Responsável por operações CRUD de tarefas
   - Banco de dados: TasksDb
   - API REST com Swagger

2. **Statistics Service** (Porta 8081)
   - Calcula e fornece estatísticas sobre tarefas
   - Leitura do banco TasksDb (read-only)
   - API REST com Swagger

3. **API Gateway** (Porta 8082)
   - Ponto de entrada único para requisições externas
   - Roteamento inteligente usando YARP
   - Gerenciamento de rotas

4. **Frontend** (Porta 8083)
   - Interface web MVC
   - Consome serviços via API Gateway
   - Views Razor + Bootstrap

5. **SQL Server** (Porta 1433)
   - Banco de dados compartilhado
   - TasksDb para dados de tarefas

## 🚀 Como Executar

### Pré-requisitos
- Docker e Docker Compose instalados
- OU .NET 9.0 SDK para execução local

### Executar com Docker Compose (Recomendado)

**Windows (PowerShell):**
```powershell
.\run-docker.ps1
```

**Linux/Mac:**
```bash
chmod +x run-docker.sh
./run-docker.sh
```

**Ou manualmente:**
```bash
docker-compose up --build
```

### Executar Localmente (Desenvolvimento)

**Build todos os serviços:**
```powershell
.\build-all.ps1
```

**Executar cada serviço em terminais separados:**

```bash
# Terminal 1 - Tasks Service
cd Services/TaskManager.TasksService
dotnet run

# Terminal 2 - Statistics Service
cd Services/TaskManager.StatisticsService
dotnet run

# Terminal 3 - API Gateway
cd Services/TaskManager.ApiGateway
dotnet run

# Terminal 4 - Frontend
cd Services/TaskManager.Frontend
dotnet run
```

## 🌐 Endpoints

### Interface do Usuário
- **Frontend**: http://localhost:8083

### APIs
- **API Gateway**: http://localhost:8082
- **Tasks Service**: http://localhost:8080
- **Statistics Service**: http://localhost:8081

### Swagger (Documentação API)
- **Tasks API**: http://localhost:8080/swagger
- **Statistics API**: http://localhost:8081/swagger
- **API Gateway**: http://localhost:8082/swagger

## 📂 Estrutura do Projeto

```
.
├── Services/
│   ├── TaskManager.TasksService/          # Microserviço de tarefas
│   │   ├── Controllers/
│   │   ├── Data/
│   │   ├── Models/
│   │   ├── Repositories/
│   │   ├── Dockerfile
│   │   └── README.md
│   │
│   ├── TaskManager.StatisticsService/     # Microserviço de estatísticas
│   │   ├── Controllers/
│   │   ├── Data/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Dockerfile
│   │   └── README.md
│   │
│   ├── TaskManager.ApiGateway/            # Gateway de API
│   │   ├── appsettings.json (rotas YARP)
│   │   ├── Dockerfile
│   │   └── README.md
│   │
│   └── TaskManager.Frontend/              # Interface Web
│       ├── Controllers/
│       ├── Models/
│       ├── Services/
│       ├── Views/
│       ├── wwwroot/
│       ├── Dockerfile
│       └── README.md
│
├── TaskManager.Web/                       # Monolito original (legado)
├── docker-compose.yml                     # Orquestração de serviços
├── build-all.ps1                          # Script de build (Windows)
├── build-all.sh                           # Script de build (Linux/Mac)
├── run-docker.ps1                         # Script de execução (Windows)
├── run-docker.sh                          # Script de execução (Linux/Mac)
└── README.md                              # Este arquivo
```

## 🔄 Comunicação Entre Serviços

```
[Usuário] 
    ↓
[Frontend:8083] 
    ↓ (HTTP)
[API Gateway:8082]
    ↓
    ├─→ [Tasks Service:8080] ─→ [SQL Server:1433]
    └─→ [Statistics Service:8081] ─→ [SQL Server:1433]
```

### Fluxo de Requisições

1. Usuário acessa o Frontend (porta 8083)
2. Frontend faz requisições HTTP para o API Gateway (porta 8082)
3. API Gateway roteia para o microserviço apropriado:
   - `/api/tasks/*` → Tasks Service (porta 8080)
   - `/api/statistics/*` → Statistics Service (porta 8081)
4. Cada microserviço acessa seu banco de dados

## 🗄️ Banco de Dados

- **Servidor**: SQL Server 2022
- **Database**: TasksDb (compartilhado entre Tasks e Statistics)
- **Connection String**: `Server=sqlserver;Database=TasksDb;User Id=sa;Password=YourStrong@Passw0rd`

### Isolamento de Dados
Embora compartilhem o mesmo banco inicialmente, os serviços estão preparados para migração para bancos separados:
- Tasks Service: operações de escrita e leitura
- Statistics Service: apenas leitura

## 🛠️ Tecnologias Utilizadas

- **.NET 9.0**: Framework base
- **ASP.NET Core MVC**: Frontend
- **ASP.NET Core Web API**: Microserviços
- **Entity Framework Core**: ORM
- **YARP (Yet Another Reverse Proxy)**: API Gateway
- **SQL Server**: Banco de dados
- **Docker & Docker Compose**: Containerização
- **Swagger/OpenAPI**: Documentação de API

## 📊 Funcionalidades

- ✅ Criar, editar e excluir tarefas
- ✅ Categorizar tarefas (Work, Personal, Study, Health, Other)
- ✅ Definir prioridades (Low, Medium, High, Urgent)
- ✅ Atribuir tarefas a usuários
- ✅ Definir datas de vencimento
- ✅ Marcar tarefas como concluídas
- ✅ Visualizar estatísticas em tempo real
- ✅ Dashboard com métricas

## 🧪 Testando a Aplicação

### 1. Criar uma Tarefa
- Acesse http://localhost:8083
- Clique em "Nova Tarefa"
- Preencha os dados e salve

### 2. Ver Estatísticas
- As estatísticas aparecem automaticamente na página principal
- Mostra: Total, Concluídas, Pendentes, Urgentes Ativas

### 3. Testar APIs Diretamente
- Acesse http://localhost:8080/swagger (Tasks API)
- Acesse http://localhost:8081/swagger (Statistics API)

## 🔧 Comandos Úteis do Docker

```bash
# Ver logs de todos os serviços
docker-compose logs -f

# Ver logs de um serviço específico
docker-compose logs -f frontend
docker-compose logs -f tasks-service

# Parar todos os serviços
docker-compose down

# Parar e remover volumes (limpar banco de dados)
docker-compose down -v

# Reconstruir e iniciar
docker-compose up --build

# Ver status dos containers
docker-compose ps
```

## 🎯 Benefícios da Arquitetura de Microserviços

1. **Escalabilidade Independente**: Cada serviço pode ser escalado separadamente
2. **Desenvolvimento Independente**: Equipes podem trabalhar em paralelo
3. **Deployment Independente**: Atualizar um serviço sem afetar outros
4. **Tecnologia Flexível**: Cada serviço pode usar diferentes tecnologias
5. **Resiliência**: Falha em um serviço não derruba todo o sistema
6. **Manutenibilidade**: Código menor e mais focado em cada serviço

## 📝 Próximos Passos

- [ ] Implementar autenticação e autorização (JWT)
- [ ] Separar bancos de dados por serviço
- [ ] Implementar Event-Driven Architecture (RabbitMQ/Kafka)
- [ ] Adicionar Service Discovery (Consul)
- [ ] Implementar Circuit Breaker (Polly)
- [ ] Adicionar Distributed Tracing (Jaeger)
- [ ] Implementar Health Checks
- [ ] Adicionar testes de integração
- [ ] Configurar CI/CD pipeline
- [ ] Implementar rate limiting

## 📄 Licença

Este projeto é um POC (Proof of Concept) para demonstração de arquitetura de microserviços.

## 👥 Contribuindo

Veja nossos templates de PR e Issues para contribuir com o projeto!

