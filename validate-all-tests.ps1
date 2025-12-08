Write-Host "========================================" -ForegroundColor Green
Write-Host "Validação Completa de Testes" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

$ErrorActionPreference = "Continue"

Write-Host "`n1. Build TasksService..." -ForegroundColor Yellow
dotnet build Services/TaskManager.TasksService/TaskManager.TasksService.csproj --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ✗ Build falhou" -ForegroundColor Red
    exit 1
} else {
    Write-Host "  ✓ Build OK" -ForegroundColor Green
}

Write-Host "`n2. Build StatisticsService..." -ForegroundColor Yellow
dotnet build Services/TaskManager.StatisticsService/TaskManager.StatisticsService.csproj --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ✗ Build falhou" -ForegroundColor Red
    exit 1
} else {
    Write-Host "  ✓ Build OK" -ForegroundColor Green
}

Write-Host "`n3. Build TasksService.Tests..." -ForegroundColor Yellow
dotnet build Tests/TaskManager.TasksService.Tests/TaskManager.TasksService.Tests.csproj --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ✗ Build falhou" -ForegroundColor Red
    exit 1
} else {
    Write-Host "  ✓ Build OK" -ForegroundColor Green
}

Write-Host "`n4. Build StatisticsService.Tests..." -ForegroundColor Yellow
dotnet build Tests/TaskManager.StatisticsService.Tests/TaskManager.StatisticsService.Tests.csproj --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ✗ Build falhou" -ForegroundColor Red
    exit 1
} else {
    Write-Host "  ✓ Build OK" -ForegroundColor Green
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Executando Testes" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`n5. Testes TasksService..." -ForegroundColor Yellow
dotnet test Tests/TaskManager.TasksService.Tests/TaskManager.TasksService.Tests.csproj --no-build --verbosity normal
$tasksTestResult = $LASTEXITCODE

Write-Host "`n6. Testes StatisticsService..." -ForegroundColor Yellow
dotnet test Tests/TaskManager.StatisticsService.Tests/TaskManager.StatisticsService.Tests.csproj --no-build --verbosity normal
$statsTestResult = $LASTEXITCODE

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "Resumo Final" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

if ($tasksTestResult -eq 0) {
    Write-Host "✓ TasksService.Tests: PASSOU" -ForegroundColor Green
} else {
    Write-Host "✗ TasksService.Tests: FALHOU" -ForegroundColor Red
}

if ($statsTestResult -eq 0) {
    Write-Host "✓ StatisticsService.Tests: PASSOU" -ForegroundColor Green
} else {
    Write-Host "✗ StatisticsService.Tests: FALHOU" -ForegroundColor Red
}

if ($tasksTestResult -eq 0 -and $statsTestResult -eq 0) {
    Write-Host "`n🎉 TODOS OS TESTES PASSARAM!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n❌ ALGUNS TESTES FALHARAM" -ForegroundColor Red
    exit 1
}

