# 🎯 SERVIÇOS INICIADOS - COMO TESTAR

## ✅ Status da Inicialização

Os microserviços foram construídos e estão sendo iniciados via Docker Compose. 

**Imagens Docker criadas com sucesso:**
- ✅ poc-task-manager-csharp-herooffer-ghc-tasks-service
- ✅ poc-task-manager-csharp-herooffer-ghc-statistics-service  
- ✅ poc-task-manager-csharp-herooffer-ghc-api-gateway
- ✅ poc-task-manager-csharp-herooffer-ghc-frontend
- ✅ SQL Server 2022 (mcr.microsoft.com/mssql/server:2022-latest)

---

## 🕐 Tempo de Inicialização

**Aguarde aproximadamente 30-60 segundos** para todos os serviços estarem prontos:

1. **SQL Server** - ~20-30 segundos (o mais demorado)
2. **Tasks Service** - ~5-10 segundos  
3. **Statistics Service** - ~5-10 segundos
4. **API Gateway** - ~3-5 segundos
5. **Frontend** - ~3-5 segundos

---

## 🔍 Como Verificar o Status

### Método 1: Ver Containers Rodando

Abra um novo terminal PowerShell e execute:

```powershell
docker ps
```

**Você deve ver 5 containers:**
- taskmanager-sqlserver
- taskmanager-tasks-service
- taskmanager-statistics-service  
- taskmanager-api-gateway
- taskmanager-frontend

### Método 2: Ver Logs

```powershell
# Todos os serviços
docker-compose logs -f

# Serviço específico
docker-compose logs -f frontend
docker-compose logs -f tasks-service
docker-compose logs -f sqlserver
```

### Método 3: Ver Status do Compose

```powershell
docker-compose ps
```

---

## 🌐 URLs de Acesso

Após os serviços iniciarem, acesse:

### **Frontend (Interface do Usuário)**
```
http://localhost:8083
```
**O QUE FAZER:**
1. Acesse a URL no navegador
2. Você verá o dashboard de tarefas
3. Clique em "Nova Tarefa" para criar
4. Teste CRUD completo

### **API Gateway (Swagger)**
```
http://localhost:8082/swagger
```
**O QUE FAZER:**
1. Ver documentação dos endpoints
2. Testar APIs diretamente

### **Tasks API (Swagger)**
```
http://localhost:8080/swagger
```
**O QUE FAZER:**
1. Ver endpoints de tarefas
2. Testar GET /api/tasks
3. Testar POST /api/tasks

### **Statistics API (Swagger)**
```
http://localhost:8081/swagger
```
**O QUE FAZER:**
1. Ver endpoint de estatísticas
2. Testar GET /api/statistics

---

## 📝 Testando o Sistema Completo

### Teste 1: Criar uma Tarefa via Frontend

1. Acesse: http://localhost:8083
2. Clique em **"Nova Tarefa"**
3. Preencha:
   - **Título:** "Minha primeira tarefa no microserviço"
   - **Descrição:** "Testando a arquitetura"
   - **Prioridade:** Alta
   - **Categoria:** Work
4. Clique em **"Salvar"**
5. Verifique se aparece na lista

### Teste 2: Ver Estatísticas

1. Na página principal (http://localhost:8083)
2. Observe os **cards coloridos no topo:**
   - Total de tarefas
   - Tarefas concluídas
   - Tarefas pendentes
   - Urgentes ativas

### Teste 3: Testar API Diretamente

**Via Swagger:**
1. Acesse http://localhost:8080/swagger
2. Expanda **GET /api/tasks**
3. Clique em **"Try it out"**
4. Clique em **"Execute"**
5. Veja o JSON retornado

**Via PowerShell:**
```powershell
# Listar todas as tarefas
Invoke-RestMethod -Uri "http://localhost:8080/api/tasks" -Method Get

# Ver estatísticas
Invoke-RestMethod -Uri "http://localhost:8081/api/statistics" -Method Get
```

### Teste 4: Fluxo Completo (CRUD)

1. **CREATE** - Criar tarefa via frontend ou Swagger
2. **READ** - Ver lista de tarefas
3. **UPDATE** - Editar uma tarefa existente
4. **DELETE** - Deletar uma tarefa

---

## 🐛 Problemas Comuns

### "Cannot connect" ou "Connection refused"

**Solução:** Aguarde mais tempo. SQL Server leva ~30 segundos para iniciar.

```powershell
# Verifique logs do SQL Server
docker-compose logs sqlserver

# Aguarde até ver: "SQL Server is now ready for client connections"
```

### Container reiniciando constantemente

**Solução:** Veja os logs para identificar o erro

```powershell
docker-compose logs tasks-service
```

### Porta já em uso

**Solução:** Pare outros serviços usando essas portas

```powershell
# Ver o que está usando a porta
netstat -ano | findstr :8083

# Parar e reiniciar
docker-compose down
docker-compose up -d
```

---

## 🛑 Comandos Úteis

### Parar Todos os Serviços
```powershell
docker-compose down
```

### Parar e Remover Volumes (Reset Completo)
```powershell
docker-compose down -v
```

### Reiniciar um Serviço Específico
```powershell
docker-compose restart frontend
```

### Ver Logs em Tempo Real
```powershell
docker-compose logs -f --tail=50
```

### Executar Comando Dentro do Container
```powershell
docker exec -it taskmanager-frontend /bin/bash
```

---

## ✅ Checklist de Teste

- [ ] Acessar Frontend (http://localhost:8083)
- [ ] Criar nova tarefa
- [ ] Ver tarefa na lista
- [ ] Editar tarefa
- [ ] Marcar como concluída
- [ ] Ver estatísticas atualizadas
- [ ] Deletar tarefa
- [ ] Acessar Swagger do Tasks Service
- [ ] Testar GET /api/tasks via Swagger
- [ ] Acessar Swagger do Statistics Service
- [ ] Testar GET /api/statistics via Swagger
- [ ] Verificar que todos 5 containers estão rodando

---

## 📊 O Que Esperar

### Frontend (http://localhost:8083)

**Você verá:**
- Dashboard com 4 cards de estatísticas (azul, verde, amarelo, vermelho)
- Botão "Nova Tarefa"
- Tabela com lista de tarefas
- Colunas: Título, Prioridade, Categoria, Data, Atribuída a, Status, Ações

### Swagger APIs

**Tasks Service (8080):**
- GET /api/tasks - Listar
- GET /api/tasks/{id} - Obter
- POST /api/tasks - Criar
- PUT /api/tasks/{id} - Atualizar
- DELETE /api/tasks/{id} - Deletar

**Statistics Service (8081):**
- GET /api/statistics - Estatísticas

---

## 🎉 Sistema Funcionando Perfeitamente Se:

✅ Frontend carrega sem erros  
✅ Dashboard mostra cards de estatísticas  
✅ Consegue criar uma nova tarefa  
✅ Tarefa aparece na lista  
✅ Estatísticas atualizam em tempo real  
✅ Swagger das APIs abre  
✅ APIs respondem corretamente  
✅ Todos 5 containers estão "healthy" ou "running"  

---

## 📚 Documentação Adicional

- **[README-MICROSERVICES.md](README-MICROSERVICES.md)** - Arquitetura completa
- **[QUICKSTART.md](QUICKSTART.md)** - Guia rápido
- **[CHANGES-EXPLAINED.md](CHANGES-EXPLAINED.md)** - O que foi mudado

---

## 🆘 Precisa de Ajuda?

1. Verifique os logs: `docker-compose logs -f`
2. Verifique status: `docker-compose ps`
3. Reinicie: `docker-compose restart`
4. Reset completo: `docker-compose down -v && docker-compose up -d`

---

**Data:** 2025-12-08  
**Status:** ✅ **SERVIÇOS INICIADOS - AGUARDANDO SQL SERVER**  
**Próximo Passo:** Aguarde 30-60 segundos e acesse http://localhost:8083

🚀 **Boa sorte testando os microserviços!** 🚀

