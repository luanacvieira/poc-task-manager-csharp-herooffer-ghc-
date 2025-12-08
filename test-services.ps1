Write-Host "========================================" -ForegroundColor Green
Write-Host "TaskManager Microservices - Test Status" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

Write-Host "`n Verificando Docker..." -ForegroundColor Yellow
docker --version

Write-Host "`n Containers em execução:" -ForegroundColor Cyan
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

Write-Host "`n`n Testando serviços..." -ForegroundColor Yellow

$services = @(
    @{Name="SQL Server"; Url="localhost:1433"; Description="Database"},
    @{Name="Tasks Service"; Url="http://localhost:8080/swagger"; Description="CRUD API"},
    @{Name="Statistics Service"; Url="http://localhost:8081/swagger"; Description="Stats API"},
    @{Name="API Gateway"; Url="http://localhost:8082/swagger"; Description="Gateway"},
    @{Name="Frontend"; Url="http://localhost:8083"; Description="Web UI"}
)

foreach ($service in $services) {
    Write-Host "`n ✓ $($service.Name) - $($service.Description)" -ForegroundColor White
    Write-Host "   URL: $($service.Url)" -ForegroundColor Gray
}

Write-Host "`n`n========================================" -ForegroundColor Green
Write-Host "Aguarde ~30 segundos para SQL Server iniciar" -ForegroundColor Yellow
Write-Host "Depois acesse: http://localhost:8083" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Green

