FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/TaskService.API/TaskService.API.csproj", "TaskService.API/"]
RUN dotnet restore "TaskService.API/TaskService.API.csproj"
COPY src/TaskService.API/ TaskService.API/
WORKDIR "/src/TaskService.API"
RUN dotnet build "TaskService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskService.API.dll"]
