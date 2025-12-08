# 🔧 SOLUÇÃO: Containers Não Acessíveis Localmente

## ❌ Problema Identificado

Os containers Docker subiram mas não estão acessíveis via localhost.

## ✅ SOLUÇÕES IMPLEMENTADAS

### 1. Atualização do docker-compose.yml

Alterei todas as portas para bind explícito em `0.0.0.0`:

**Antes:**
```yaml
ports:
  - "8080:8080"  # Pode não funcionar no Windows/WSL2
```

**Depois:**
```yaml
ports:
  - "0.0.0.0:8080:8080"  # Bind explícito em todas as interfaces
```

**Também mudei:**
```yaml
# Antes
ASPNETCORE_URLS=http://+:8080

# Depois
ASPNETCORE_URLS=http://0.0.0.0:8080
```

---

## 🔄 COMO APLICAR AS CORREÇÕES

### Método 1: Reiniciar Containers (RECOMENDADO)

```powershell
# Parar containers atuais
docker-compose down

# Iniciar com novas configurações
docker-compose up -d

# Aguardar 30 segundos
Start-Sleep -Seconds 30

# Verificar status
docker ps
```

### Método 2: Rebuild Completo

```powershell
# Parar e remover tudo
docker-compose down -v

# Rebuild e start
docker-compose up -d --build

# Aguardar
Start-Sleep -Seconds 60
```

---

## 🧪 TESTE DE CONECTIVIDADE

### Teste 1: Verificar Portas Abertas

```powershell
Test-NetConnection -ComputerName localhost -Port 8083
Test-NetConnection -ComputerName localhost -Port 8082
Test-NetConnection -ComputerName localhost -Port 8080
Test-NetConnection -ComputerName localhost -Port 8081
```

**Resultado esperado:** `TcpTestSucceeded: True` para todas

### Teste 2: Verificar Containers Rodando

```powershell
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
```

**Você deve ver:**
- taskmanager-sqlserver (porta 1433)
- taskmanager-tasks-service (porta 8080)
- taskmanager-statistics-service (porta 8081)
- taskmanager-api-gateway (porta 8082)
- taskmanager-frontend (porta 8083)

### Teste 3: Teste HTTP Direto

```powershell
# Frontend
Invoke-WebRequest -Uri "http://localhost:8083" -Method Get

# Tasks API
Invoke-WebRequest -Uri "http://localhost:8080/swagger" -Method Get

# Statistics API
Invoke-WebRequest -Uri "http://localhost:8081/swagger" -Method Get
```

---

## 🌐 ALTERNATIVAS DE ACESSO

Se ainda não funcionar com `localhost`, tente:

### Opção 1: Use 127.0.0.1
```
http://127.0.0.1:8083
http://127.0.0.1:8082/swagger
http://127.0.0.1:8080/swagger
http://127.0.0.1:8081/swagger
```

### Opção 2: Use IP da Máquina

```powershell
# Descobrir seu IP local
Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.InterfaceAlias -like "*Ethernet*" -or $_.InterfaceAlias -like "*Wi-Fi*"} | Select-Object IPAddress
```

Depois acesse: `http://[SEU-IP]:8083`

### Opção 3: Use Docker Internal Network

```powershell
# Entrar no container
docker exec -it taskmanager-frontend sh

# Testar de dentro do container
curl http://api-gateway:8082/swagger
```

---

## 🐛 TROUBLESHOOTING AVANÇADO

### Problema: Docker Desktop WSL2

Se você está usando Docker Desktop com WSL2, pode haver problemas de rede.

**Solução:**

1. Abra Docker Desktop
2. Settings → General
3. Desmarque "Use the WSL 2 based engine" (se disponível)
4. Aplicar e Reiniciar Docker
5. Ou mantenha WSL2 mas configure:
   - Settings → Resources → Network
   - Marque "Enable host networking"

### Problema: Firewall do Windows

```powershell
# Temporariamente desabilitar firewall para teste
Set-NetFirewallProfile -Profile Domain,Public,Private -Enabled False

# Testar acesso
Invoke-WebRequest -Uri "http://localhost:8083"

# Reabilitar firewall
Set-NetFirewallProfile -Profile Domain,Public,Private -Enabled True
```

Se funcionar, adicione regras:

```powershell
New-NetFirewallRule -DisplayName "TaskManager Frontend" -Direction Inbound -LocalPort 8083 -Protocol TCP -Action Allow
New-NetFirewallRule -DisplayName "TaskManager Gateway" -Direction Inbound -LocalPort 8082 -Protocol TCP -Action Allow
New-NetFirewallRule -DisplayName "TaskManager Tasks" -Direction Inbound -LocalPort 8080 -Protocol TCP -Action Allow
New-NetFirewallRule -DisplayName "TaskManager Statistics" -Direction Inbound -LocalPort 8081 -Protocol TCP -Action Allow
```

### Problema: Containers Crashando

```powershell
# Ver logs detalhados
docker-compose logs --tail=100

# Ver logs de um serviço específico
docker-compose logs frontend --tail=50

# Ver por que container parou
docker inspect taskmanager-frontend
```

### Problema: SQL Server Demorando Muito

```powershell
# Monitorar logs do SQL Server
docker-compose logs -f sqlserver

# Aguardar até ver:
# "SQL Server is now ready for client connections"
```

---

## 📋 CHECKLIST DE DIAGNÓSTICO

Execute este script para diagnóstico completo:

```powershell
# Salve como check-docker.ps1 e execute
Write-Host "=== DIAGNÓSTICO DOCKER ===" -ForegroundColor Cyan

# 1. Docker rodando?
Write-Host "`n1. Docker Service:" -ForegroundColor Yellow
docker version

# 2. Containers ativos?
Write-Host "`n2. Containers:" -ForegroundColor Yellow
docker ps

# 3. Portas abertas?
Write-Host "`n3. Testando Portas:" -ForegroundColor Yellow
@(8083, 8082, 8080, 8081, 1433) | ForEach-Object {
    $result = Test-NetConnection -ComputerName localhost -Port $_ -WarningAction SilentlyContinue
    if ($result.TcpTestSucceeded) {
        Write-Host "   ✓ Porta $_ ABERTA" -ForegroundColor Green
    } else {
        Write-Host "   ✗ Porta $_ FECHADA" -ForegroundColor Red
    }
}

# 4. HTTP funcionando?
Write-Host "`n4. Testando HTTP:" -ForegroundColor Yellow
try {
    Invoke-WebRequest -Uri "http://localhost:8083" -TimeoutSec 5 -ErrorAction Stop | Out-Null
    Write-Host "   ✓ Frontend acessível" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Frontend NÃO acessível" -ForegroundColor Red
}

Write-Host "`n=== FIM DO DIAGNÓSTICO ===" -ForegroundColor Cyan
```

---

## ✅ CONFIRMAÇÃO DE SUCESSO

Você saberá que está funcionando quando:

1. **Portas Test**Net retornam `TcpTestSucceeded: True`
2. **docker ps** mostra 5 containers rodando
3. **Navegador** abre http://localhost:8083 sem erro
4. **Swagger** abre em http://localhost:8080/swagger

---

## 🚀 PRÓXIMOS PASSOS

Após resolver o problema de conectividade:

1. Acesse http://localhost:8083
2. Crie uma tarefa de teste
3. Verifique estatísticas
4. Teste Swagger APIs
5. Confirme CRUD completo funciona

---

## 📞 AINDA COM PROBLEMAS?

### Execute diagnóstico:
```powershell
cd C:\dev\poc-task-manager-csharp-herooffer-ghc
.\diagnose.ps1
```

### Coletar informações:
```powershell
# Exportar logs
docker-compose logs > docker-logs.txt

# Info do sistema
docker info > docker-info.txt

# Status dos containers
docker ps -a > containers-status.txt
```

---

**Atualizado:** 2025-12-08  
**Status:** ✅ **CORREÇÕES APLICADAS - TESTE NOVAMENTE**  

🔧 **Execute: `docker-compose down && docker-compose up -d` e aguarde 60 segundos**

