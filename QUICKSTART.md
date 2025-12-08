# Quick Start Guide - Task Manager Microservices

## 🚀 Início Rápido (5 minutos)

### Pré-requisitos
- Docker Desktop instalado e em execução
- 4GB de RAM livres
- Portas 8080-8083 e 1433 disponíveis

### Passo 1: Clone o Repositório
```bash
git clone <repository-url>
cd poc-task-manager-csharp-herooffer-ghc
```

### Passo 2: Inicie a Aplicação

**Windows (PowerShell):**
```powershell
.\run-docker.ps1
```

**Linux/Mac:**
```bash
chmod +x run-docker.sh
./run-docker.sh
```

### Passo 3: Acesse a Aplicação

Aguarde ~20 segundos para todos os serviços iniciarem, depois acesse:

🌐 **Interface Web:** http://localhost:8083

### Passo 4: Teste a Aplicação

1. Clique em "Nova Tarefa"
2. Preencha:
   - **Título:** "Minha primeira tarefa"
   - **Descrição:** "Testar o sistema"
   - **Prioridade:** Alta
   - **Categoria:** Work
3. Clique em "Salvar"
4. Veja a tarefa criada e as estatísticas atualizadas!

## 📊 Endpoints Disponíveis

| Serviço | URL | Descrição |
|---------|-----|-----------|
| Frontend | http://localhost:8083 | Interface do usuário |
| API Gateway | http://localhost:8082/swagger | Documentação da API |
| Tasks API | http://localhost:8080/swagger | API de tarefas |
| Statistics API | http://localhost:8081/swagger | API de estatísticas |

## 🔍 Verificar Status

```powershell
# Ver logs de todos os serviços
docker-compose logs -f

# Ver logs de um serviço específico
docker-compose logs -f frontend

# Ver status dos containers
docker-compose ps
```

## 🛑 Parar a Aplicação

```powershell
docker-compose down
```

## 📚 Documentação Completa

- **Arquitetura:** [README-MICROSERVICES.md](README-MICROSERVICES.md)
- **Migração:** [MIGRATION-SUMMARY.md](MIGRATION-SUMMARY.md)

## ❓ Problemas Comuns

### Porta já em uso
```powershell
# Ver o que está usando a porta
netstat -ano | findstr :8083

# Parar containers antigos
docker-compose down
```

### Containers não iniciam
```powershell
# Limpar tudo e recomeçar
docker-compose down -v
docker system prune -f
.\run-docker.ps1
```

### Banco de dados não conecta
Aguarde mais tempo (~30 segundos) para o SQL Server inicializar completamente.

## 🎯 Próximos Passos

Depois de testar:
1. Leia [README-MICROSERVICES.md](README-MICROSERVICES.md) para entender a arquitetura
2. Explore as APIs via Swagger
3. Veja [MIGRATION-SUMMARY.md](MIGRATION-SUMMARY.md) para detalhes da migração

---

**Dúvidas?** Consulte a documentação completa ou os READMEs individuais de cada serviço.

