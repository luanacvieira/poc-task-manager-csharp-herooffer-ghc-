FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Services/TaskManager.TasksService/TaskManager.TasksService.csproj", "Services/TaskManager.TasksService/"]
RUN dotnet restore "Services/TaskManager.TasksService/TaskManager.TasksService.csproj"
COPY . .
WORKDIR "/src/Services/TaskManager.TasksService"
RUN dotnet build "TaskManager.TasksService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManager.TasksService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.TasksService.dll"]

