FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["LiteQueue.API/LiteQueue.API.csproj", "LiteQueue.API/"]
COPY ["LiteQueue.Application/LiteQueue.Application.csproj", "LiteQueue.Application/"]
COPY ["LiteQueue.Background/LiteQueue.Background.csproj", "LiteQueue.Background/"]
COPY ["LiteQueue.Contracts/LiteQueue.Contracts.csproj", "LiteQueue.Contracts/"]
COPY ["LiteQueue.Domain/LiteQueue.Domain.csproj", "LiteQueue.Domain/"]
COPY ["LiteQueue.Infrastructure/LiteQueue.Infrastructure.csproj", "LiteQueue.Infrastructure/"]
RUN dotnet restore "LiteQueue.API/LiteQueue.API.csproj"
COPY . .
RUN dotnet build "LiteQueue.API/LiteQueue.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LiteQueue.API/LiteQueue.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiteQueue.API.dll"]
