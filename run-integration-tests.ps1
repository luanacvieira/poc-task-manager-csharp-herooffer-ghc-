Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Testes de Integracao - SQL Server" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/5] Verificando Docker..." -ForegroundColor Yellow
try {
    $dockerVersion = docker version --format '{{.Server.Version}}' 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "Docker nao esta rodando"
    }
    Write-Host "  Docker esta rodando (versao $dockerVersion)" -ForegroundColor Green
} catch {
    Write-Host "  Erro: Docker nao encontrado ou nao esta rodando" -ForegroundColor Red
    Write-Host "  Instale o Docker Desktop e inicie-o antes de executar os testes" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "[2/5] Iniciando SQL Server container..." -ForegroundColor Yellow
docker-compose -f docker-compose.test.yml up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "  Erro ao iniciar SQL Server container" -ForegroundColor Red
    exit 1
}

Write-Host "  Container iniciado" -ForegroundColor Green
Write-Host ""

Write-Host "[3/5] Aguardando SQL Server ficar pronto..." -ForegroundColor Yellow
$maxAttempts = 30
$attempt = 0
$ready = $false

while (-not $ready -and $attempt -lt $maxAttempts) {
    $attempt++
    Write-Host "  Tentativa $attempt/$maxAttempts..." -ForegroundColor Gray
    
    $healthStatus = docker inspect --format='{{.State.Health.Status}}' sqlserver-test 2>$null
    
    if ($healthStatus -eq "healthy") {
        $ready = $true
        Write-Host "  SQL Server esta pronto!" -ForegroundColor Green
    } else {
        Start-Sleep -Seconds 2
    }
}

if (-not $ready) {
    Write-Host "  Timeout: SQL Server nao ficou pronto em tempo habil" -ForegroundColor Red
    Write-Host "  Parando containers..." -ForegroundColor Yellow
    docker-compose -f docker-compose.test.yml down
    exit 1
}

Write-Host ""
Write-Host "[4/5] Executando testes de integracao..." -ForegroundColor Yellow
Write-Host ""

dotnet test --filter "FullyQualifiedName~Integration" --verbosity normal --logger "console;verbosity=detailed"

$testExitCode = $LASTEXITCODE

Write-Host ""
Write-Host "[5/5] Limpando ambiente de teste..." -ForegroundColor Yellow
docker-compose -f docker-compose.test.yml down -v

if ($LASTEXITCODE -ne 0) {
    Write-Host "  Aviso: Erro ao parar containers" -ForegroundColor Yellow
} else {
    Write-Host "  Containers parados e volumes removidos" -ForegroundColor Green
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan

if ($testExitCode -eq 0) {
    Write-Host "  SUCESSO: Todos os testes passaram!" -ForegroundColor Green
} else {
    Write-Host "  FALHA: Alguns testes falharam" -ForegroundColor Red
}

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

exit $testExitCode
