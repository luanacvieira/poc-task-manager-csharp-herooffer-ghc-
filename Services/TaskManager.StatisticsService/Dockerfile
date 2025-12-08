FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Services/TaskManager.StatisticsService/TaskManager.StatisticsService.csproj", "Services/TaskManager.StatisticsService/"]
RUN dotnet restore "Services/TaskManager.StatisticsService/TaskManager.StatisticsService.csproj"
COPY . .
WORKDIR "/src/Services/TaskManager.StatisticsService"
RUN dotnet build "TaskManager.StatisticsService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManager.StatisticsService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.StatisticsService.dll"]

