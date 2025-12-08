# 🚀 GitHub Actions - CI/CD Pipeline

## 📋 Visão Geral

Este projeto possui **2 workflows** de CI/CD configurados:

1. **`build-and-test.yml`** - Pipeline completo com coverage (recomendado)
2. **`ci-quick.yml`** - Pipeline rápido e simplificado

---

## 🎯 Objetivo

Executar testes automaticamente e **permitir build SOMENTE se os testes passarem com 80% ou mais de sucesso**.

---

## 📊 Workflow Principal: build-and-test.yml

### ✨ Características

- ✅ Execução automática em **push**, **pull request** ou **manualmente**
- ✅ Build da aplicação .NET 9.0
- ✅ Execução de todos os testes unitários
- ✅ Coleta de cobertura de código
- ✅ Validação de taxa de sucesso mínima de **80%**
- ✅ Geração de relatórios HTML de coverage
- ✅ Comentário automático em Pull Requests
- ✅ Upload de artefatos (resultados e coverage)

### 🔄 Triggers

```yaml
on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]
  workflow_dispatch:  # Permite execução manual
```

**Quando é acionado:**
- ✅ Push para branch `main` ou `develop`
- ✅ Pull Request para `main` ou `develop`
- ✅ Manualmente via GitHub UI (Actions → workflow → Run workflow)

### 📝 Etapas do Pipeline

```
1. 📥 Checkout do código
2. 🔧 Setup do .NET 9.0
3. 📦 Restore de dependências
4. 🔨 Build da solution (Release)
5. 🧪 Execução dos testes com coverage
6. 📊 Parse dos resultados
7. ✅ Validação da taxa de sucesso (80%)
8. 📈 Geração de relatório de coverage
9. 📤 Upload de artefatos
10. 💬 Comentário no PR (se aplicável)
```

### 🎯 Validação de Sucesso

O pipeline **FALHA** se:
- ❌ Taxa de sucesso dos testes < 80%
- ❌ Algum passo do build falhar
- ❌ Nenhum teste for encontrado

O pipeline **PASSA** se:
- ✅ Taxa de sucesso dos testes ≥ 80%
- ✅ Todos os testes executaram
- ✅ Build concluído com sucesso

### 📊 Exemplo de Output

```
==================================
📊 TEST RESULTS SUMMARY
==================================
Total Tests: 36
Passed: ✅ 36
Failed: ❌ 0
Success Rate: 100%
Minimum Required: 80%
==================================
✅ SUCCESS: Test success rate (100%) meets minimum requirement (80%)
🎉 Build will proceed!
```

---

## ⚡ Workflow Rápido: ci-quick.yml

### ✨ Características

- ✅ Versão simplificada e mais rápida
- ✅ Mesma validação de 80% de sucesso
- ✅ Menos steps, execução mais rápida
- ✅ Ideal para branches de feature

### 🔄 Triggers

```yaml
on:
  push:
    branches: [ main, develop, feature/* ]
  pull_request:
    branches: [ main, develop ]
  workflow_dispatch:
```

---

## 🔧 Configuração

### Variáveis de Ambiente

Ambos workflows usam as seguintes variáveis:

```yaml
env:
  DOTNET_VERSION: '9.0.x'        # Versão do .NET
  MIN_TEST_SUCCESS_RATE: 80      # Taxa mínima de sucesso (%)
```

### Como Alterar a Taxa Mínima

Para mudar o percentual mínimo de 80% para outro valor:

1. Edite o arquivo `.github/workflows/build-and-test.yml`
2. Altere a linha:
   ```yaml
   MIN_TEST_SUCCESS_RATE: 80  # Altere para o valor desejado
   ```
3. Commit e push

---

## 🎮 Como Usar

### Execução Automática

Os workflows são executados automaticamente quando você:

```bash
# Push para branch protegida
git push origin main

# Criar Pull Request
# Via GitHub UI
```

### Execução Manual

1. Acesse GitHub → **Actions**
2. Selecione o workflow desejado
3. Clique em **Run workflow**
4. Escolha a branch
5. Clique em **Run workflow** (botão verde)

### Via Linha de Comando (GitHub CLI)

```bash
# Executar workflow principal
gh workflow run "Build and Test with Coverage"

# Executar workflow rápido
gh workflow run "CI/CD - Quick Test & Build"

# Ver status
gh run list

# Ver logs
gh run view
```

---

## 📦 Artefatos Gerados

Os workflows geram os seguintes artefatos:

### 1. Test Results (`test-results`)
- ✅ Arquivos `.trx` com resultados dos testes
- ✅ Mantido por 30 dias
- ✅ Download via GitHub Actions UI

### 2. Coverage Report (`coverage-report`)
- ✅ Relatório HTML interativo
- ✅ Relatório Markdown
- ✅ Arquivo Cobertura XML
- ✅ Mantido por 30 dias

### Como Baixar Artefatos

1. Acesse GitHub → **Actions**
2. Clique no workflow run desejado
3. Role até **Artifacts**
4. Clique para baixar

---

## 💬 Comentários em Pull Requests

Quando o workflow é executado em um PR, ele adiciona automaticamente um comentário:

### ✅ Exemplo de Sucesso

```markdown
## ✅ Test Results - PASSED

### 📊 Summary

| Metric | Value |
|--------|-------|
| **Total Tests** | 36 |
| **Passed** | ✅ 36 |
| **Failed** | ❌ 0 |
| **Success Rate** | **100%** |
| **Minimum Required** | 80% |
| **Status** | ✅ PASSED |

✅ **Build Approved**: Test success rate meets the minimum requirement!
```

### ❌ Exemplo de Falha

```markdown
## ❌ Test Results - FAILED

### 📊 Summary

| Metric | Value |
|--------|-------|
| **Total Tests** | 36 |
| **Passed** | ✅ 28 |
| **Failed** | ❌ 8 |
| **Success Rate** | **77.78%** |
| **Minimum Required** | 80% |
| **Status** | ❌ FAILED |

❌ **Build Rejected**: Test success rate is below the minimum requirement!
```

---

## 🛡️ Branch Protection

### Recomendação de Configuração

Para forçar que todos os PRs passem nos testes antes de merge:

1. Acesse **Settings** → **Branches**
2. Adicione uma **Branch protection rule** para `main`
3. Ative:
   - ✅ Require status checks to pass before merging
   - ✅ Require branches to be up to date before merging
   - ✅ Selecione: `Test & Build .NET Application`
4. Salve

Agora **nenhum PR pode ser mergeado sem 80%+ de sucesso nos testes**!

---

## 📊 Relatório de Coverage

### Estrutura do Relatório

```
TestResults/CoverageReport/
├── index.html              # Relatório principal
├── Summary.md              # Resumo em Markdown
├── Cobertura.xml           # Dados de coverage
└── [outros arquivos HTML]
```

### Visualizar Coverage Localmente

```powershell
# Executar testes com coverage
dotnet test --collect:"XPlat Code Coverage"

# Instalar ReportGenerator
dotnet tool install --global dotnet-reportgenerator-globaltool

# Gerar relatório
reportgenerator `
  -reports:"TestResults/**/coverage.cobertura.xml" `
  -targetdir:"TestResults/CoverageReport" `
  -reporttypes:"Html"

# Abrir no navegador
start TestResults/CoverageReport/index.html
```

---

## 🎯 Métricas e Badges

### Adicionar Badge ao README

Adicione ao seu `README.md`:

```markdown
![Build and Test](https://github.com/SEU-USUARIO/SEU-REPO/actions/workflows/build-and-test.yml/badge.svg)
```

Substitua:
- `SEU-USUARIO` pelo seu username do GitHub
- `SEU-REPO` pelo nome do repositório

---

## 🔍 Troubleshooting

### Problema: "No tests found"

**Solução:**
```bash
# Verificar se os testes compilam
dotnet build Tests/

# Executar testes localmente
dotnet test --verbosity detailed
```

### Problema: "Success rate calculation failed"

**Solução:**
- Verificar se os arquivos `.trx` são gerados
- Conferir se o `xmlstarlet` está instalado no runner

### Problema: "Workflow não executa"

**Solução:**
- Verificar se os arquivos estão em `.github/workflows/`
- Conferir se o YAML está válido
- Verificar permissões do repositório

---

## 📝 Exemplo Completo de Uso

### Cenário: Feature Branch

```bash
# 1. Criar feature branch
git checkout -b feature/nova-funcionalidade

# 2. Fazer alterações e adicionar testes
# ... código ...

# 3. Commit
git add .
git commit -m "feat: Adiciona nova funcionalidade"

# 4. Push - workflow executa automaticamente
git push origin feature/nova-funcionalidade

# 5. Criar PR no GitHub
# Workflow executa novamente e comenta no PR

# 6. Se testes passarem (≥80%), PR pode ser mergeado
# Se falharem (<80%), PR é bloqueado
```

---

## ✅ Checklist de Implementação

- [x] Criar diretório `.github/workflows/`
- [x] Criar `build-and-test.yml` (completo)
- [x] Criar `ci-quick.yml` (simplificado)
- [x] Configurar taxa mínima de 80%
- [x] Habilitar execução manual
- [x] Configurar comentários em PRs
- [x] Upload de artefatos
- [x] Geração de coverage report
- [ ] Configurar branch protection (manual)
- [ ] Adicionar badge ao README (manual)

---

## 🚀 Próximos Passos

1. ✅ **Testar os workflows** - Faça um push para testar
2. ✅ **Ajustar taxa mínima** - Se necessário, altere de 80%
3. ✅ **Configurar branch protection** - Proteja `main` branch
4. ✅ **Adicionar mais testes** - Aumentar cobertura
5. ✅ **Monitorar resultados** - Acompanhe via GitHub Actions

---

## 📚 Recursos Adicionais

- [GitHub Actions Documentation](https://docs.github.com/actions)
- [.NET Testing Guide](https://docs.microsoft.com/dotnet/core/testing/)
- [Code Coverage Tools](https://github.com/danielpalme/ReportGenerator)

---

**Criado em:** 2025-12-08  
**Versão:** 1.0  
**Status:** ✅ Pronto para uso  

🎉 **Seus testes agora bloqueiam builds ruins automaticamente!**

