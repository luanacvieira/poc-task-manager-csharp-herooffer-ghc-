FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8083

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Services/TaskManager.Frontend/TaskManager.Frontend.csproj", "Services/TaskManager.Frontend/"]
RUN dotnet restore "Services/TaskManager.Frontend/TaskManager.Frontend.csproj"
COPY . .
WORKDIR "/src/Services/TaskManager.Frontend"
RUN dotnet build "TaskManager.Frontend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManager.Frontend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.Frontend.dll"]

