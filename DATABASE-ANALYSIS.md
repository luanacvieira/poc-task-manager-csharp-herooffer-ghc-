# 🔍 ANÁLISE: Estratégia de Banco de Dados

## 📊 SITUAÇÃO ATUAL

### ❌ Banco de Dados Compartilhado (Não é o ideal para microserviços)

Atualmente, a aplicação está usando **UM ÚNICO BANCO DE DADOS** compartilhado entre os microserviços.

```
┌─────────────────────┐
│   SQL Server        │
│   (1 instância)     │
│                     │
│  ┌───────────────┐  │
│  │   TasksDb     │  │ ◄─── BANCO COMPARTILHADO
│  └───────────────┘  │
└─────────────────────┘
         ▲       ▲
         │       │
         │       │
    ┌────┴───┐  │
    │ Tasks  │  │
    │Service │  │
    └────────┘  │
                │
          ┌─────┴─────────┐
          │  Statistics   │
          │   Service     │
          └───────────────┘
```

### 🔎 Evidências:

**Docker Compose:**
- ✅ 1 container SQL Server
- ✅ Ambos serviços apontam para o mesmo banco

**Tasks Service:**
```yaml
ConnectionStrings__TasksConnection=Server=sqlserver;Database=TasksDb;...
```

**Statistics Service:**
```yaml
ConnectionStrings__StatisticsConnection=Server=sqlserver;Database=TasksDb;...
```

**Ambos usam:** `Database=TasksDb` ⚠️

---

## ⚠️ PROBLEMAS DA ABORDAGEM ATUAL

### 1. **Acoplamento de Dados**
- ❌ Serviços compartilham o mesmo esquema
- ❌ Mudança em uma tabela afeta ambos serviços
- ❌ Dificulta deploy independente

### 2. **Violação dos Princípios de Microserviços**
- ❌ Database-per-Service pattern não seguido
- ❌ Serviços não são verdadeiramente independentes
- ❌ Falha no isolamento de dados

### 3. **Escalabilidade Limitada**
- ❌ Não pode escalar bancos independentemente
- ❌ Bottleneck único no banco de dados
- ❌ Contenção de recursos

### 4. **Dificuldade de Manutenção**
- ❌ Schema migrations afetam múltiplos serviços
- ❌ Backup/restore acoplados
- ❌ Dificulta versionamento de schema

---

## ✅ SOLUÇÃO RECOMENDADA: Database-per-Service

### Arquitetura Ideal:

```
┌─────────────────────┐     ┌─────────────────────┐
│   SQL Server        │     │   SQL Server        │
│   (Tasks)           │     │   (Statistics)      │
│                     │     │                     │
│  ┌───────────────┐  │     │  ┌───────────────┐  │
│  │   TasksDb     │  │     │  │ StatisticsDb  │  │
│  └───────────────┘  │     │  └───────────────┘  │
└─────────────────────┘     └─────────────────────┘
         ▲                           ▲
         │                           │
         │                           │
    ┌────┴───────┐              ┌───┴──────────┐
    │   Tasks    │              │  Statistics  │
    │  Service   │──── HTTP ───►│   Service    │
    └────────────┘   (leitura)  └──────────────┘
```

### Vantagens:

1. ✅ **Independência Total**
   - Cada serviço possui seu próprio banco
   - Deploy independente
   - Schema evolution isolada

2. ✅ **Escalabilidade**
   - Escalar banco de dados por necessidade
   - Otimizar cada banco separadamente
   - Diferentes estratégias de backup

3. ✅ **Resiliência**
   - Falha em um banco não afeta o outro
   - Isolamento de problemas
   - Recuperação independente

4. ✅ **Flexibilidade Tecnológica**
   - Pode usar diferentes tipos de banco
   - Statistics poderia usar banco read-only
   - Tasks poderia usar banco otimizado para escrita

---

## 🔧 PLANO DE MIGRAÇÃO

### Opção 1: Bancos Separados na Mesma Instância (Mais Simples)

**Vantagens:**
- ✅ Fácil de implementar
- ✅ Usa mesma instância SQL Server
- ✅ Baixo custo de recursos

**Desvantagens:**
- ⚠️ Ainda compartilha a instância
- ⚠️ Contenção de recursos possível

**Implementação:**
```yaml
# docker-compose.yml - Mantém 1 SQL Server
sqlserver:
  image: mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04
  ...

# Mas usa bancos diferentes:
tasks-service:
  environment:
    - ConnectionStrings__TasksConnection=Server=sqlserver;Database=TasksDb;...

statistics-service:
  environment:
    - ConnectionStrings__StatisticsConnection=Server=sqlserver;Database=StatisticsDb;...
```

### Opção 2: Instâncias SQL Server Separadas (Ideal)

**Vantagens:**
- ✅ Isolamento completo
- ✅ Verdadeira independência
- ✅ Melhor para produção

**Desvantagens:**
- ⚠️ Mais recursos necessários (~3 GB RAM)
- ⚠️ Mais complexo de gerenciar

**Implementação:**
```yaml
# docker-compose.yml
services:
  sqlserver-tasks:
    image: mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04
    container_name: taskmanager-sqlserver-tasks
    ports:
      - "1433:1433"
    ...

  sqlserver-statistics:
    image: mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04
    container_name: taskmanager-sqlserver-statistics
    ports:
      - "1434:1433"  # Porta diferente no host
    ...
```

### Opção 3: Databases Separados + Event Sourcing (Avançado)

**Como funciona:**
- Tasks Service: Escreve em TasksDb
- Publica eventos quando dados mudam
- Statistics Service: Escuta eventos e popula StatisticsDb

**Vantagens:**
- ✅ Desacoplamento total
- ✅ Statistics pode ter schema otimizado
- ✅ Padrão recomendado para microserviços

**Desvantagens:**
- ⚠️ Mais complexo
- ⚠️ Requer message broker (RabbitMQ, Kafka)
- ⚠️ Eventual consistency

---

## 🎯 RECOMENDAÇÃO

### Para DESENVOLVIMENTO (Atual): ✅ Opção 1
**Bancos separados na mesma instância SQL Server**

- Fácil de implementar
- Baixo consumo de recursos
- Mantém simplicidade
- Já separa os dados logicamente

### Para PRODUÇÃO (Futuro): ✅ Opção 2 ou 3
**Instâncias separadas ou Event Sourcing**

- Isolamento completo
- Alta disponibilidade
- Escalabilidade real
- Resiliência

---

## 📝 IMPLEMENTAÇÃO RÁPIDA (Opção 1)

### Mudanças Necessárias:

**1. docker-compose.yml:**
```yaml
# Mantém o mesmo - 1 SQL Server
sqlserver:
  image: mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04
  ...

# Tasks Service - Usa TasksDb
tasks-service:
  environment:
    - ConnectionStrings__TasksConnection=Server=sqlserver;Database=TasksDb;...

# Statistics Service - Usa StatisticsDb (MUDANÇA AQUI)
statistics-service:
  environment:
    - ConnectionStrings__StatisticsConnection=Server=sqlserver;Database=StatisticsDb;...
```

**2. StatisticsService/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "StatisticsConnection": "Server=sqlserver;Database=StatisticsDb;..."
  }
}
```

**3. Criar script de inicialização:**
```sql
-- init-databases.sql
USE master;
GO

-- Criar TasksDb se não existir
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TasksDb')
BEGIN
    CREATE DATABASE TasksDb;
END
GO

-- Criar StatisticsDb se não existir
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'StatisticsDb')
BEGIN
    CREATE DATABASE StatisticsDb;
END
GO
```

**4. Adicionar script ao docker-compose:**
```yaml
sqlserver:
  volumes:
    - sqlserver-data:/var/opt/mssql
    - ./init-databases.sql:/docker-entrypoint-initdb.d/init.sql
```

---

## 🤔 DECISÃO: O QUE FAZER AGORA?

### Status Atual:
⚠️ **Banco Compartilhado** - Funciona mas não é ideal

### Opções:

**A) Manter Como Está (Para POC/Demo)**
- ✅ Rápido para demonstração
- ✅ Já funciona
- ❌ Não segue best practices

**B) Migrar para Bancos Separados (Recomendado)**
- ✅ Melhor arquitetura
- ✅ Mais próximo do ideal
- ✅ Fácil de implementar (Opção 1)
- ⏱️ Leva ~30 minutos

**C) Aguardar e Migrar Para Produção**
- ✅ Foco em funcionalidades primeiro
- ✅ Refatorar depois
- ⚠️ Mais trabalho depois

---

## 📊 COMPARAÇÃO

| Aspecto | Atual | Opção 1 | Opção 2 | Opção 3 |
|---------|-------|---------|---------|---------|
| **Isolamento** | ❌ Baixo | ⚠️ Médio | ✅ Alto | ✅ Muito Alto |
| **Recursos** | ✅ Baixo | ✅ Baixo | ⚠️ Alto | ⚠️ Alto |
| **Complexidade** | ✅ Simples | ✅ Simples | ⚠️ Média | ❌ Alta |
| **Produção** | ❌ Não | ⚠️ Sim | ✅ Sim | ✅ Ideal |
| **Tempo Impl.** | - | 30 min | 2h | 8h+ |

---

## ✅ CONCLUSÃO

### Situação Atual:
🔴 **Banco de Dados Compartilhado (TasksDb)**
- Tasks Service → TasksDb
- Statistics Service → TasksDb (mesmo banco!)

### Impacto:
- ⚠️ Funciona mas não é arquitetura ideal de microserviços
- ⚠️ Serviços acoplados pelo banco de dados
- ⚠️ Dificulta escalabilidade e manutenção futura

### Recomendação:
✅ **Migrar para Opção 1** (bancos separados, mesma instância)
- Baixo esforço
- Melhora significativa na arquitetura
- Mantém simplicidade

---

## 🚀 Quer que eu implemente a separação de bancos?

Posso fazer as mudanças necessárias para:
1. Criar StatisticsDb separado
2. Atualizar configurações
3. Adicionar scripts de inicialização
4. Documentar as mudanças

**Você gostaria que eu prossiga com a implementação?**

---

**Análise realizada em:** 2025-12-08  
**Status:** ⚠️ **BANCO COMPARTILHADO IDENTIFICADO**  
**Recomendação:** 🔄 **SEPARAR BANCOS DE DADOS**

