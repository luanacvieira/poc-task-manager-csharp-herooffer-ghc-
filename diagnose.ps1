Write-Host "========================================" -ForegroundColor Green
Write-Host "Diagnóstico dos Containers Docker" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

Write-Host "`n1. Verificando Docker Desktop..." -ForegroundColor Yellow
try {
    $dockerVersion = docker version --format '{{.Server.Version}}' 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ Docker Server: $dockerVersion" -ForegroundColor Green
    } else {
        Write-Host "   ✗ Docker Server não está respondendo" -ForegroundColor Red
        Write-Host "   Inicie o Docker Desktop e tente novamente" -ForegroundColor Yellow
        exit
    }
} catch {
    Write-Host "   ✗ Erro ao verificar Docker: $_" -ForegroundColor Red
    exit
}

Write-Host "`n2. Listando containers..." -ForegroundColor Yellow
$containers = docker ps --format "{{.Names}}" 2>&1
if ($LASTEXITCODE -eq 0) {
    $containerList = $containers -split "`n" | Where-Object { $_ -ne "" }
    if ($containerList.Count -gt 0) {
        Write-Host "   ✓ Containers rodando: $($containerList.Count)" -ForegroundColor Green
        foreach ($container in $containerList) {
            Write-Host "     - $container" -ForegroundColor White
        }
    } else {
        Write-Host "   ✗ Nenhum container rodando" -ForegroundColor Red
        Write-Host "   Execute: docker-compose up -d" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ✗ Erro ao listar containers" -ForegroundColor Red
}

Write-Host "`n3. Verificando portas..." -ForegroundColor Yellow
$ports = @(1433, 8080, 8081, 8082, 8083)
foreach ($port in $ports) {
    $connection = Test-NetConnection -ComputerName localhost -Port $port -WarningAction SilentlyContinue -ErrorAction SilentlyContinue
    if ($connection.TcpTestSucceeded) {
        Write-Host "   ✓ Porta $port está ABERTA" -ForegroundColor Green
    } else {
        Write-Host "   ✗ Porta $port está FECHADA" -ForegroundColor Red
    }
}

Write-Host "`n4. Testando endpoints HTTP..." -ForegroundColor Yellow
$endpoints = @(
    @{Url="http://localhost:8083"; Name="Frontend"},
    @{Url="http://localhost:8082/swagger"; Name="API Gateway"},
    @{Url="http://localhost:8080/swagger"; Name="Tasks Service"},
    @{Url="http://localhost:8081/swagger"; Name="Statistics Service"}
)

foreach ($endpoint in $endpoints) {
    try {
        $response = Invoke-WebRequest -Uri $endpoint.Url -Method Get -TimeoutSec 2 -ErrorAction Stop
        Write-Host "   ✓ $($endpoint.Name) - HTTP $($response.StatusCode)" -ForegroundColor Green
    } catch {
        Write-Host "   ✗ $($endpoint.Name) - Não acessível" -ForegroundColor Red
    }
}

Write-Host "`n5. Logs recentes dos containers..." -ForegroundColor Yellow
docker-compose logs --tail=5 2>&1 | Out-String | ForEach-Object { 
    if ($_ -ne "") { 
        Write-Host $_ -ForegroundColor Gray 
    } 
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "Diagnóstico completo" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

Write-Host "`nSe as portas estão FECHADAS:" -ForegroundColor Yellow
Write-Host "1. Verifique se containers estão rodando: docker ps" -ForegroundColor White
Write-Host "2. Veja logs dos containers: docker-compose logs" -ForegroundColor White
Write-Host "3. Reinicie os containers: docker-compose restart" -ForegroundColor White

Write-Host "`nSe as portas estão ABERTAS mas não acessíveis:" -ForegroundColor Yellow
Write-Host "1. Verifique firewall do Windows" -ForegroundColor White
Write-Host "2. Tente: http://127.0.0.1:8083 ao invés de localhost" -ForegroundColor White
Write-Host "3. Reinicie Docker Desktop" -ForegroundColor White

