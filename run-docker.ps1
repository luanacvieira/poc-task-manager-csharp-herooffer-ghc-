Write-Host "==========================================" -ForegroundColor Green
Write-Host "Starting TaskManager with Docker Compose" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green

Write-Host "`nStopping any existing containers..." -ForegroundColor Yellow
docker-compose down

Write-Host "`nBuilding and starting all services..." -ForegroundColor Yellow
docker-compose up --build -d

Write-Host "`nWaiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

Write-Host "`n==========================================" -ForegroundColor Green
Write-Host "Services are starting up!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green

Write-Host "`nAvailable services:" -ForegroundColor Cyan
Write-Host "  - Frontend:          http://localhost:8083" -ForegroundColor White
Write-Host "  - API Gateway:       http://localhost:8082" -ForegroundColor White
Write-Host "  - Tasks Service:     http://localhost:8080" -ForegroundColor White
Write-Host "  - Statistics Service: http://localhost:8081" -ForegroundColor White
Write-Host "  - SQL Server:        localhost:1433" -ForegroundColor White

Write-Host "`nSwagger endpoints:" -ForegroundColor Cyan
Write-Host "  - Tasks API:         http://localhost:8080/swagger" -ForegroundColor White
Write-Host "  - Statistics API:    http://localhost:8081/swagger" -ForegroundColor White
Write-Host "  - API Gateway:       http://localhost:8082/swagger" -ForegroundColor White

Write-Host "`nTo view logs, run: docker-compose logs -f" -ForegroundColor Yellow
Write-Host "To stop services, run: docker-compose down" -ForegroundColor Yellow

