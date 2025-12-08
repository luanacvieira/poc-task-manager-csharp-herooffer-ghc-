Write-Host "========================================" -ForegroundColor Green
Write-Host "Executando Todos os Testes - TaskManager" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

# Limpar e rebuild
Write-Host "`n1. Limpando solucao..." -ForegroundColor Yellow
dotnet clean --verbosity quiet

Write-Host "`n2. Restaurando pacotes..." -ForegroundColor Yellow
dotnet restore

Write-Host "`n3. Compilando TasksService..." -ForegroundColor Yellow
dotnet build Services/TaskManager.TasksService/TaskManager.TasksService.csproj --no-restore

Write-Host "`n4. Compilando StatisticsService..." -ForegroundColor Yellow
dotnet build Services/TaskManager.StatisticsService/TaskManager.StatisticsService.csproj --no-restore

Write-Host "`n5. Compilando Tests..." -ForegroundColor Yellow
dotnet build Tests/TaskManager.TasksService.Tests/TaskManager.TasksService.Tests.csproj --no-restore
dotnet build Tests/TaskManager.StatisticsService.Tests/TaskManager.StatisticsService.Tests.csproj --no-restore

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Executando Testes" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`n6. TasksService.Tests..." -ForegroundColor Yellow
dotnet test Tests/TaskManager.TasksService.Tests/TaskManager.TasksService.Tests.csproj `
    --no-build `
    --verbosity normal `
    --logger "console;verbosity=detailed" `
    -- RunConfiguration.CollectSourceInformation=true

Write-Host "`n7. StatisticsService.Tests..." -ForegroundColor Yellow
dotnet test Tests/TaskManager.StatisticsService.Tests/TaskManager.StatisticsService.Tests.csproj `
    --no-build `
    --verbosity normal `
    --logger "console;verbosity=detailed" `
    -- RunConfiguration.CollectSourceInformation=true

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "Testes Concluidos!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

