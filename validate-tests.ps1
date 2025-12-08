Write-Host "========================================" -ForegroundColor Green
Write-Host "Validando Projetos de Teste - .NET 9" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

$testProjects = @(
    "Tests\TaskManager.TasksService.Tests\TaskManager.TasksService.Tests.csproj",
    "Tests\TaskManager.StatisticsService.Tests\TaskManager.StatisticsService.Tests.csproj",
    "Tests\TaskManager.Frontend.Tests\TaskManager.Frontend.Tests.csproj",
    "Tests\TaskManager.ApiGateway.Tests\TaskManager.ApiGateway.Tests.csproj"
)

$success = 0
$failed = 0

foreach ($project in $testProjects) {
    $projectName = Split-Path $project -Leaf
    Write-Host "`nCompilando: $projectName" -ForegroundColor Yellow
    
    $result = dotnet build $project --configuration Debug 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ $projectName - BUILD OK" -ForegroundColor Green
        $success++
    } else {
        Write-Host "  ✗ $projectName - BUILD FALHOU" -ForegroundColor Red
        Write-Host $result | Select-Object -Last 10
        $failed++
    }
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "Resumo:" -ForegroundColor Cyan
Write-Host "  Sucesso: $success" -ForegroundColor Green
Write-Host "  Falhas: $failed" -ForegroundColor $(if ($failed -gt 0) { "Red" } else { "Green" })
Write-Host "========================================" -ForegroundColor Green

if ($failed -eq 0) {
    Write-Host "`n✓ Todos os projetos de teste estão compatíveis com .NET 9!" -ForegroundColor Green
} else {
    Write-Host "`n✗ Alguns projetos falharam. Verifique os erros acima." -ForegroundColor Red
}

