FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Terminus.WebApi/Terminus.WebApi.csproj", "Terminus.WebApi/"]
COPY ["Terminus.Application/Terminus.Application.csproj", "Terminus.Application/"]
COPY ["Terminus.Domain/Terminus.Domain.csproj", "Terminus.Domain/"]
COPY ["Terminus.Infrastructure/Terminus.Infrastructure.csproj", "Terminus.Infrastructure/"]
RUN dotnet restore "Terminus.WebApi/Terminus.WebApi.csproj"
COPY . .
WORKDIR "/src/Terminus.WebApi"
RUN dotnet build "./Terminus.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Terminus.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Terminus.WebApi.dll"]
