# 🚀 Quick Start - Testes de Integração

## ⚠️ Pré-requisito: Docker Desktop

**ANTES de executar os testes, você DEVE:**

### 1. Instalar Docker Desktop (se ainda não tiver)

📥 [Download Docker Desktop para Windows](https://www.docker.com/products/docker-desktop/)

### 2. Iniciar Docker Desktop

- Abra o Docker Desktop
- Aguarde o ícone 🐋 ficar verde na bandeja do sistema
- Pode levar 1-2 minutos para inicializar completamente

### 3. Verificar que Docker está rodando

```powershell
docker version
```

✅ Deve mostrar informações do Client E Server (sem erros)

## 🎯 Execução Rápida

Depois que o Docker estiver rodando:

```powershell
# Navegar até a pasta do projeto
cd c:\Projetos\Patria\poc-task-manager-csharp-herooffer-ghc

# Executar script automatizado
.\run-integration-tests.ps1
```

O script faz TUDO automaticamente:
1. ✓ Verifica Docker
2. ✓ Inicia SQL Server
3. ✓ Aguarda ficar pronto
4. ✓ Roda testes
5. ✓ Limpa containers

## 📊 O que Esperar

```
════════════════════════════════════════════════════════
  🧪 Testes de Integração - TaskManager API
════════════════════════════════════════════════════════

[1/5] ✓ Docker está rodando
[2/5] ✓ Iniciando SQL Server container...
[3/5] ✓ Aguardando SQL Server ficar pronto...
[4/5] ✓ Executando testes de integração...

  Descobrindo testes...
  Executando testes...
  
  Passed! - 22 tests, 0 falhas
  
[5/5] ✓ Limpando containers...

════════════════════════════════════════════════════════
  ✨ Testes completados com sucesso!
════════════════════════════════════════════════════════
```

## ❌ Problemas Comuns

### Erro: "Docker não está rodando"

**Causa**: Docker Desktop não foi iniciado

**Solução**: 
1. Abra Docker Desktop
2. Aguarde ícone 🐋 ficar verde
3. Execute o script novamente

### Erro: "Porta 1433 já está em uso"

**Causa**: SQL Server local já está usando a porta

**Solução**: 
1. Pare SQL Server local temporariamente
2. OU edite `docker-compose.test.yml` para usar outra porta:
   ```yaml
   ports:
     - "1434:1433"  # Muda de 1433 para 1434
   ```
3. Execute o script novamente

### Erro: "Timeout ao conectar"

**Causa**: SQL Server demorou muito para inicializar

**Solução**: 
```powershell
# Parar e limpar
docker-compose -f docker-compose.test.yml down -v

# Tentar novamente
.\run-integration-tests.ps1
```

## 🎓 Primeira Execução

Na primeira vez, pode demorar mais (~2-3 minutos) porque:
- Docker precisa baixar a imagem do SQL Server (~700MB)
- SQL Server precisa inicializar pela primeira vez

Execuções seguintes são muito mais rápidas (~30-45 segundos).

## 💡 Dicas

✅ **Deixe Docker Desktop rodando** enquanto desenvolve

✅ **Execute antes de commits** para evitar regressões

✅ **Monitore a saída** - mensagens coloridas indicam o progresso

✅ **Consulte logs** se algo der errado:
```powershell
docker logs sqlserver-test
```

## 📚 Documentação Completa

Para detalhes completos sobre arquitetura, troubleshooting e CI/CD, consulte:

📖 [INTEGRATION-TESTS-GUIDE.md](./INTEGRATION-TESTS-GUIDE.md)

## 🆘 Ajuda Rápida

```powershell
# Ver containers rodando
docker ps

# Ver logs do SQL Server
docker logs sqlserver-test

# Parar manualmente
docker-compose -f docker-compose.test.yml down -v

# Limpar volumes antigos
docker volume prune -f

# Verificar saúde do container
docker inspect --format='{{.State.Health.Status}}' sqlserver-test
```

---

**Pronto para começar?** Inicie o Docker Desktop e rode `.\run-integration-tests.ps1`! 🚀
