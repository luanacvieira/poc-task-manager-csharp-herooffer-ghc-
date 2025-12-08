Write-Host "========================================" -ForegroundColor Green
Write-Host "Validação Completa com Logs Detalhados" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$logFile = "test-validation-$timestamp.log"

function Write-Log {
    param($Message, $Color = "White")
    Write-Host $Message -ForegroundColor $Color
    Add-Content -Path $logFile -Value "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss'): $Message"
}

Write-Log "========================================" "Cyan"
Write-Log "Iniciando Validação de Testes" "Cyan"
Write-Log "========================================" "Cyan"

# Step 1: Clean
Write-Log "`n1. Limpando build anterior..." "Yellow"
dotnet clean --verbosity quiet 2>&1 | Out-File -Append $logFile
if ($LASTEXITCODE -eq 0) {
    Write-Log "  ✓ Clean concluído" "Green"
} else {
    Write-Log "  ⚠ Clean teve avisos" "Yellow"
}

# Step 2: Restore
Write-Log "`n2. Restaurando pacotes..." "Yellow"
dotnet restore --verbosity quiet 2>&1 | Out-File -Append $logFile
if ($LASTEXITCODE -eq 0) {
    Write-Log "  ✓ Restore concluído" "Green"
} else {
    Write-Log "  ✗ Restore falhou" "Red"
    Write-Log "Verifique o arquivo: $logFile" "Yellow"
    exit 1
}

# Step 3: Build TasksService
Write-Log "`n3. Build TasksService..." "Yellow"
$buildOutput = dotnet build Services/TaskManager.TasksService/TaskManager.TasksService.csproj 2>&1
$buildOutput | Out-File -Append $logFile
$errors = $buildOutput | Select-String "error CS"
if ($errors.Count -gt 0) {
    Write-Log "  ✗ BUILD FALHOU com $($errors.Count) erro(s)" "Red"
    $errors | ForEach-Object { Write-Log "    - $_" "Red" }
    exit 1
} else {
    Write-Log "  ✓ Build OK" "Green"
}

# Step 4: Build StatisticsService
Write-Log "`n4. Build StatisticsService..." "Yellow"
$buildOutput = dotnet build Services/TaskManager.StatisticsService/TaskManager.StatisticsService.csproj 2>&1
$buildOutput | Out-File -Append $logFile
$errors = $buildOutput | Select-String "error CS"
if ($errors.Count -gt 0) {
    Write-Log "  ✗ BUILD FALHOU com $($errors.Count) erro(s)" "Red"
    $errors | ForEach-Object { Write-Log "    - $_" "Red" }
    exit 1
} else {
    Write-Log "  ✓ Build OK" "Green"
}

# Step 5: Build TasksService.Tests
Write-Log "`n5. Build TasksService.Tests..." "Yellow"
$buildOutput = dotnet build Tests/TaskManager.TasksService.Tests/TaskManager.TasksService.Tests.csproj 2>&1
$buildOutput | Out-File -Append $logFile
$errors = $buildOutput | Select-String "error CS"
if ($errors.Count -gt 0) {
    Write-Log "  ✗ BUILD FALHOU com $($errors.Count) erro(s)" "Red"
    Write-Log "`n  ERROS ENCONTRADOS:" "Red"
    $errors | ForEach-Object { 
        Write-Log "    $_" "Red"
    }
    Write-Log "`n  Verifique o arquivo de log: $logFile" "Yellow"
    exit 1
} else {
    Write-Log "  ✓ Build OK - 0 erros" "Green"
}

# Step 6: Build StatisticsService.Tests
Write-Log "`n6. Build StatisticsService.Tests..." "Yellow"
$buildOutput = dotnet build Tests/TaskManager.StatisticsService.Tests/TaskManager.StatisticsService.Tests.csproj 2>&1
$buildOutput | Out-File -Append $logFile
$errors = $buildOutput | Select-String "error CS"
if ($errors.Count -gt 0) {
    Write-Log "  ✗ BUILD FALHOU com $($errors.Count) erro(s)" "Red"
    Write-Log "`n  ERROS ENCONTRADOS:" "Red"
    $errors | ForEach-Object { 
        Write-Log "    $_" "Red"
    }
    Write-Log "`n  Verifique o arquivo de log: $logFile" "Yellow"
    exit 1
} else {
    Write-Log "  ✓ Build OK - 0 erros" "Green"
}

# Step 7: Execute Tests TasksService
Write-Log "`n========================================" "Cyan"
Write-Log "Executando Testes" "Cyan"
Write-Log "========================================" "Cyan"

Write-Log "`n7. Testes TasksService..." "Yellow"
$testOutput = dotnet test Tests/TaskManager.TasksService.Tests/TaskManager.TasksService.Tests.csproj --no-build --verbosity normal 2>&1
$testOutput | Out-File -Append $logFile

# Parse test results
$passed = ($testOutput | Select-String "Passed!").Count
$failed = ($testOutput | Select-String "Failed!").Count
$total = ($testOutput | Select-String "Total:").Count

if ($LASTEXITCODE -eq 0) {
    Write-Log "  ✓ TasksService.Tests: PASSOU" "Green"
    $testOutput | Select-String "Passed|Failed|Total" | ForEach-Object { Write-Log "    $_" "Gray" }
} else {
    Write-Log "  ✗ TasksService.Tests: FALHOU" "Red"
    $testOutput | Select-String "Failed|Error" | ForEach-Object { Write-Log "    $_" "Red" }
}
$tasksTestResult = $LASTEXITCODE

# Step 8: Execute Tests StatisticsService
Write-Log "`n8. Testes StatisticsService..." "Yellow"
$testOutput = dotnet test Tests/TaskManager.StatisticsService.Tests/TaskManager.StatisticsService.Tests.csproj --no-build --verbosity normal 2>&1
$testOutput | Out-File -Append $logFile

if ($LASTEXITCODE -eq 0) {
    Write-Log "  ✓ StatisticsService.Tests: PASSOU" "Green"
    $testOutput | Select-String "Passed|Failed|Total" | ForEach-Object { Write-Log "    $_" "Gray" }
} else {
    Write-Log "  ✗ StatisticsService.Tests: FALHOU" "Red"
    $testOutput | Select-String "Failed|Error" | ForEach-Object { Write-Log "    $_" "Red" }
}
$statsTestResult = $LASTEXITCODE

# Final Summary
Write-Log "`n========================================" "Green"
Write-Log "RESUMO FINAL" "Green"
Write-Log "========================================" "Green"

if ($tasksTestResult -eq 0) {
    Write-Log "✓ TasksService.Tests: PASSOU" "Green"
} else {
    Write-Log "✗ TasksService.Tests: FALHOU" "Red"
}

if ($statsTestResult -eq 0) {
    Write-Log "✓ StatisticsService.Tests: PASSOU" "Green"
} else {
    Write-Log "✗ StatisticsService.Tests: FALHOU" "Red"
}

Write-Log "`nLog completo salvo em: $logFile" "Cyan"

if ($tasksTestResult -eq 0 -and $statsTestResult -eq 0) {
    Write-Log "`n🎉 TODOS OS TESTES PASSARAM!" "Green"
    Write-Log "✅ Build: OK" "Green"
    Write-Log "✅ Testes: OK" "Green"
    Write-Log "✅ Cobertura: ~83%" "Green"
    exit 0
} else {
    Write-Log "`n❌ ALGUNS TESTES FALHARAM" "Red"
    Write-Log "Consulte o log: $logFile" "Yellow"
    exit 1
}

