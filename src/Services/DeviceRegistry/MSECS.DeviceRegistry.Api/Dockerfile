FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Services/DeviceRegistry/MSECS.DeviceRegistry.Api/MSECS.DeviceRegistry.Api.csproj", "Services/DeviceRegistry/MSECS.DeviceRegistry.Api/"]
COPY ["Services/DeviceRegistry/MSECS.DeviceRegistry.Application/MSECS.DeviceRegistry.Application.csproj", "Services/DeviceRegistry/MSECS.DeviceRegistry.Application/"]
COPY ["Services/DeviceRegistry/MSECS.DeviceRegistry.Infrastructure/MSECS.DeviceRegistry.Infrastructure.csproj", "Services/DeviceRegistry/MSECS.DeviceRegistry.Infrastructure/"]
COPY ["Services/DeviceRegistry/MSECS.DeviceRegistry.Domain/MSECS.DeviceRegistry.Domain.csproj", "Services/DeviceRegistry/MSECS.DeviceRegistry.Domain/"]
COPY ["Shared/MSECS.SharedKernel/MSECS.SharedKernel.csproj", "Shared/MSECS.SharedKernel/"]
COPY ["Shared/MSECS.BuildingBlocks/MSECS.BuildingBlocks.csproj", "Shared/MSECS.BuildingBlocks/"]
RUN dotnet restore "Services/DeviceRegistry/MSECS.DeviceRegistry.Api/MSECS.DeviceRegistry.Api.csproj"
COPY . .
WORKDIR "/src/Services/DeviceRegistry/MSECS.DeviceRegistry.Api"
RUN dotnet build "MSECS.DeviceRegistry.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MSECS.DeviceRegistry.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MSECS.DeviceRegistry.Api.dll"]
