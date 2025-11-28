# Task Manager - POC C# Hero Offer

Sistema de gerenciamento de tarefas desenvolvido em .NET 9.0 com ASP.NET Core MVC.

## Tecnologias

- .NET 9.0
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server

## Estrutura do Projeto

```text
TaskManager.Web/
├── Controllers/      # Controladores MVC
├── Models/          # Modelos de dados
├── Services/        # Camada de serviços
├── Repositories/    # Camada de acesso a dados
├── Data/            # Contexto do banco de dados
└── Views/           # Views Razor
```

## Configuração

1. Clone o repositório
2. Configure a connection string no `appsettings.json`
3. Execute as migrations: `dotnet ef database update`
4. Execute o projeto: `dotnet run`

## Testes Automatizados

Este projeto utiliza GitHub Actions para CI/CD:

- Testes automatizados em cada push/PR
- Análise de segurança com CodeQL
- Linting de código
- Labels automáticos em PRs

## Funcionalidades

- Criar, editar e excluir tarefas
- Categorizar tarefas
- Definir prioridades
- Visualizar lista de tarefas

## Contribuindo

Veja nossos templates de PR e Issues para contribuir com o projeto!
