#!/bin/bash

echo "=========================================="
echo "Starting TaskManager with Docker Compose"
echo "=========================================="

echo "Stopping any existing containers..."
docker-compose down

echo "Building and starting all services..."
docker-compose up --build -d

echo "Waiting for services to be ready..."
sleep 15

echo "=========================================="
echo "Services are starting up!"
echo "=========================================="

echo ""
echo "Available services:"
echo "  - Frontend:           http://localhost:8083"
echo "  - API Gateway:        http://localhost:8082"
echo "  - Tasks Service:      http://localhost:8080"
echo "  - Statistics Service: http://localhost:8081"
echo "  - SQL Server:         localhost:1433"

echo ""
echo "Swagger endpoints:"
echo "  - Tasks API:          http://localhost:8080/swagger"
echo "  - Statistics API:     http://localhost:8081/swagger"
echo "  - API Gateway:        http://localhost:8082/swagger"

echo ""
echo "To view logs, run: docker-compose logs -f"
echo "To stop services, run: docker-compose down"

