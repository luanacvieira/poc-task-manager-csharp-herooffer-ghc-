FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8082

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Services/TaskManager.ApiGateway/TaskManager.ApiGateway.csproj", "Services/TaskManager.ApiGateway/"]
RUN dotnet restore "Services/TaskManager.ApiGateway/TaskManager.ApiGateway.csproj"
COPY . .
WORKDIR "/src/Services/TaskManager.ApiGateway"
RUN dotnet build "TaskManager.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManager.ApiGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.ApiGateway.dll"]

