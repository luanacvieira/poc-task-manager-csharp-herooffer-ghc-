Write-Host "==========================================" -ForegroundColor Green
Write-Host "Building TaskManager Microservices" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green

# Build Tasks Service
Write-Host "Building Tasks Service..." -ForegroundColor Yellow
Set-Location Services\TaskManager.TasksService
dotnet build -c Release
Set-Location ..\..

# Build Statistics Service
Write-Host "Building Statistics Service..." -ForegroundColor Yellow
Set-Location Services\TaskManager.StatisticsService
dotnet build -c Release
Set-Location ..\..

# Build API Gateway
Write-Host "Building API Gateway..." -ForegroundColor Yellow
Set-Location Services\TaskManager.ApiGateway
dotnet build -c Release
Set-Location ..\..

# Build Frontend
Write-Host "Building Frontend..." -ForegroundColor Yellow
Set-Location Services\TaskManager.Frontend
dotnet build -c Release
Set-Location ..\..

Write-Host "==========================================" -ForegroundColor Green
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green

