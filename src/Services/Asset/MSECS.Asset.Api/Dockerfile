FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Services/Asset/MSECS.Asset.Api/MSECS.Asset.Api.csproj", "Services/Asset/MSECS.Asset.Api/"]
COPY ["Services/Asset/MSECS.Asset.Application/MSECS.Asset.Application.csproj", "Services/Asset/MSECS.Asset.Application/"]
COPY ["Services/Asset/MSECS.Asset.Infrastructure/MSECS.Asset.Infrastructure.csproj", "Services/Asset/MSECS.Asset.Infrastructure/"]
COPY ["Services/Asset/MSECS.Asset.Domain/MSECS.Asset.Domain.csproj", "Services/Asset/MSECS.Asset.Domain/"]
COPY ["Shared/MSECS.SharedKernel/MSECS.SharedKernel.csproj", "Shared/MSECS.SharedKernel/"]
COPY ["Shared/MSECS.BuildingBlocks/MSECS.BuildingBlocks.csproj", "Shared/MSECS.BuildingBlocks/"]
RUN dotnet restore "Services/Asset/MSECS.Asset.Api/MSECS.Asset.Api.csproj"
COPY . .
WORKDIR "/src/Services/Asset/MSECS.Asset.Api"
RUN dotnet build "MSECS.Asset.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MSECS.Asset.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MSECS.Asset.Api.dll"]
