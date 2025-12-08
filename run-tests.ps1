Write-Host "========================================" -ForegroundColor Green
Write-Host "Executando Testes com Cobertura" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

Write-Host "`n1. Building test projects..." -ForegroundColor Yellow
dotnet build Tests/TaskManager.TasksService.Tests/TaskManager.TasksService.Tests.csproj
dotnet build Tests/TaskManager.StatisticsService.Tests/TaskManager.StatisticsService.Tests.csproj

Write-Host "`n2. Running TasksService tests..." -ForegroundColor Yellow
dotnet test Tests/TaskManager.TasksService.Tests/TaskManager.TasksService.Tests.csproj `
    --collect:"XPlat Code Coverage" `
    --results-directory:"./TestResults" `
    --logger:"console;verbosity=detailed"

Write-Host "`n3. Running StatisticsService tests..." -ForegroundColor Yellow
dotnet test Tests/TaskManager.StatisticsService.Tests/TaskManager.StatisticsService.Tests.csproj `
    --collect:"XPlat Code Coverage" `
    --results-directory:"./TestResults" `
    --logger:"console;verbosity=detailed"

Write-Host "`n4. Test Summary..." -ForegroundColor Cyan
Get-ChildItem -Path ./TestResults -Filter coverage.cobertura.xml -Recurse | ForEach-Object {
    Write-Host "Coverage file: $($_.FullName)" -ForegroundColor Gray
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "Tests completed!" -ForegroundColor Green  
Write-Host "========================================" -ForegroundColor Green

