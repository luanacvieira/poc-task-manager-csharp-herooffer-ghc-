#!/bin/bash
echo "=========================================="
echo "Build completed successfully!"
echo "=========================================="

cd ../..
dotnet build -c Release
cd Services/TaskManager.Frontend
echo "Building Frontend..."
# Build Frontend

cd ../..
dotnet build -c Release
cd Services/TaskManager.ApiGateway
echo "Building API Gateway..."
# Build API Gateway

cd ../..
dotnet build -c Release
cd Services/TaskManager.StatisticsService
echo "Building Statistics Service..."
# Build Statistics Service

cd ../..
dotnet build -c Release
cd Services/TaskManager.TasksService
echo "Building Tasks Service..."
# Build Tasks Service

echo "=========================================="
echo "Building TaskManager Microservices"
echo "=========================================="


