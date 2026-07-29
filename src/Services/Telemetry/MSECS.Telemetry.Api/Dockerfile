FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Services/Telemetry/MSECS.Telemetry.Api/MSECS.Telemetry.Api.csproj", "Services/Telemetry/MSECS.Telemetry.Api/"]
COPY ["Services/Telemetry/MSECS.Telemetry.Application/MSECS.Telemetry.Application.csproj", "Services/Telemetry/MSECS.Telemetry.Application/"]
COPY ["Services/Telemetry/MSECS.Telemetry.Infrastructure/MSECS.Telemetry.Infrastructure.csproj", "Services/Telemetry/MSECS.Telemetry.Infrastructure/"]
COPY ["Services/Telemetry/MSECS.Telemetry.ProtocolAdapters/MSECS.Telemetry.ProtocolAdapters.csproj", "Services/Telemetry/MSECS.Telemetry.ProtocolAdapters/"]
COPY ["Services/Telemetry/MSECS.Telemetry.Domain/MSECS.Telemetry.Domain.csproj", "Services/Telemetry/MSECS.Telemetry.Domain/"]
COPY ["Shared/MSECS.SharedKernel/MSECS.SharedKernel.csproj", "Shared/MSECS.SharedKernel/"]
COPY ["Shared/MSECS.BuildingBlocks/MSECS.BuildingBlocks.csproj", "Shared/MSECS.BuildingBlocks/"]
RUN dotnet restore "Services/Telemetry/MSECS.Telemetry.Api/MSECS.Telemetry.Api.csproj"
COPY . .
WORKDIR "/src/Services/Telemetry/MSECS.Telemetry.Api"
RUN dotnet build "MSECS.Telemetry.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MSECS.Telemetry.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MSECS.Telemetry.Api.dll"]
