# 🔄 ATUALIZAÇÃO: Imagem SQL Server

## ✅ Mudança Realizada

A imagem do SQL Server foi atualizada para uma versão mais recente e específica.

### **Antes:**
```yaml
image: mcr.microsoft.com/mssql/server:2022-latest
```

### **Depois:**
```yaml
image: mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04
```

---

## 📋 Detalhes da Mudança

### Por Que Mudar?

1. **`2022-latest`** - Tag genérica que pode mudar
   - ❌ Pode pegar versões instáveis
   - ❌ Menos controle sobre a versão exata
   - ❌ Potenciais breaking changes

2. **`2022-CU14-ubuntu-22.04`** - Versão específica
   - ✅ SQL Server 2022 Cumulative Update 14
   - ✅ Base Ubuntu 22.04 LTS (estável)
   - ✅ Versão testada e estável
   - ✅ Melhor para produção

### Vantagens da Nova Imagem

- ✅ **Estabilidade**: CU14 é uma versão testada
- ✅ **Compatibilidade**: Ubuntu 22.04 LTS tem suporte de longo prazo
- ✅ **Segurança**: Inclui patches de segurança mais recentes
- ✅ **Performance**: Otimizações da CU14
- ✅ **Reproduzibilidade**: Mesma imagem sempre

---

## 🚀 Como Aplicar a Mudança

### Passo 1: Parar Containers Atuais

```powershell
cd C:\dev\poc-task-manager-csharp-herooffer-ghc
docker-compose down
```

### Passo 2: Remover Imagem Antiga (Opcional)

```powershell
# Ver imagens atuais
docker images | Select-String "mssql"

# Remover imagem antiga
docker rmi mcr.microsoft.com/mssql/server:2022-latest
```

### Passo 3: Pull Nova Imagem

```powershell
docker pull mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04
```

### Passo 4: Iniciar com Nova Imagem

```powershell
docker-compose up -d
```

### Passo 5: Aguardar Inicialização

```powershell
# Aguardar 60 segundos
Start-Sleep -Seconds 60

# Verificar logs
docker-compose logs sqlserver
```

---

## 🔍 Verificação

### Verificar Versão do SQL Server

```powershell
# Conectar ao container
docker exec -it taskmanager-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "SELECT @@VERSION"
```

**Saída esperada:**
```
Microsoft SQL Server 2022 (RTM-CU14) (KB5038325) - 16.0.4125.3 (X64)
```

### Verificar Container Rodando

```powershell
docker ps | Select-String "taskmanager-sqlserver"
```

### Verificar Health Check

```powershell
docker inspect taskmanager-sqlserver | Select-String "Health"
```

---

## 📊 Comparação de Versões

| Aspecto | 2022-latest | 2022-CU14-ubuntu-22.04 |
|---------|-------------|------------------------|
| **Estabilidade** | Variável | ✅ Estável |
| **Previsibilidade** | ❌ Baixa | ✅ Alta |
| **Produção** | ❌ Não recomendado | ✅ Recomendado |
| **Tamanho** | ~1.5 GB | ~1.5 GB |
| **Suporte** | Latest | LTS (Long Term Support) |
| **Patches Segurança** | Automático | CU14 (testado) |

---

## 🐛 Problemas Conhecidos e Soluções

### Problema: "Image not found"

**Solução:**
```powershell
# Tentar tag alternativa
docker pull mcr.microsoft.com/mssql/server:2022-latest

# Ou versão anterior estável
docker pull mcr.microsoft.com/mssql/server:2022-CU13-ubuntu-22.04
```

### Problema: Dados do volume antigo

**Solução - Preservar Dados:**
```powershell
# Parar sem remover volumes
docker-compose down

# Iniciar com nova imagem
docker-compose up -d
```

**Solução - Limpar Dados:**
```powershell
# Remover tudo incluindo volumes
docker-compose down -v

# Iniciar limpo
docker-compose up -d
```

### Problema: Container não inicia

**Solução:**
```powershell
# Ver logs detalhados
docker-compose logs sqlserver --tail=100

# Verificar se porta está livre
Test-NetConnection -ComputerName localhost -Port 1433

# Remover container problemático
docker rm -f taskmanager-sqlserver

# Recriar
docker-compose up -d sqlserver
```

---

## 🔄 Outras Opções de Imagem

Se tiver problemas com CU14, tente estas alternativas:

### Opção 1: Última CU Estável
```yaml
image: mcr.microsoft.com/mssql/server:2022-CU13-ubuntu-22.04
```

### Opção 2: SQL Server 2019 (Mais Estável)
```yaml
image: mcr.microsoft.com/mssql/server:2019-CU28-ubuntu-20.04
```

### Opção 3: SQL Server 2022 RTM
```yaml
image: mcr.microsoft.com/mssql/server:2022-RTM-ubuntu-22.04
```

---

## ✅ Checklist de Atualização

- [ ] Parar containers: `docker-compose down`
- [ ] (Opcional) Remover imagem antiga
- [ ] Pull nova imagem
- [ ] Iniciar containers: `docker-compose up -d`
- [ ] Aguardar 60 segundos
- [ ] Verificar logs: `docker-compose logs sqlserver`
- [ ] Testar conexão ao banco
- [ ] Verificar aplicação funcionando
- [ ] Testar criar/ler tarefas

---

## 📝 Commit da Mudança

```bash
git add docker-compose.yml
git commit -m "chore: Update SQL Server to specific CU14 image

- Changed from 2022-latest to 2022-CU14-ubuntu-22.04
- Provides better stability and reproducibility
- Ubuntu 22.04 LTS base for long-term support"
```

---

## 🎯 Impacto na Aplicação

✅ **Nenhum impacto esperado** - A mudança é transparente para a aplicação.

Os microserviços continuam se conectando da mesma forma:
```
Server=sqlserver;Database=TasksDb;User Id=sa;Password=YourStrong@Passw0rd
```

---

## 📚 Referências

- [SQL Server Docker Images](https://hub.docker.com/_/microsoft-mssql-server)
- [SQL Server 2022 Release Notes](https://learn.microsoft.com/en-us/sql/sql-server/sql-server-2022-release-notes)
- [Ubuntu 22.04 LTS](https://ubuntu.com/blog/ubuntu-22-04-lts-released)

---

## 🆘 Precisa Reverter?

Se tiver problemas, volte para a versão anterior:

```yaml
# docker-compose.yml
image: mcr.microsoft.com/mssql/server:2022-latest
```

Depois:
```powershell
docker-compose down
docker-compose up -d
```

---

**Data da Atualização**: 2025-12-08  
**Versão Nova**: SQL Server 2022 CU14 (Ubuntu 22.04)  
**Status**: ✅ **ATUALIZADO - REINICIE OS CONTAINERS**  

🔄 **Execute: `docker-compose down && docker-compose up -d`**

