FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Services/Identity/MSECS.Identity.Api/MSECS.Identity.Api.csproj", "Services/Identity/MSECS.Identity.Api/"]
COPY ["Services/Identity/MSECS.Identity.Application/MSECS.Identity.Application.csproj", "Services/Identity/MSECS.Identity.Application/"]
COPY ["Services/Identity/MSECS.Identity.Infrastructure/MSECS.Identity.Infrastructure.csproj", "Services/Identity/MSECS.Identity.Infrastructure/"]
COPY ["Services/Identity/MSECS.Identity.Domain/MSECS.Identity.Domain.csproj", "Services/Identity/MSECS.Identity.Domain/"]
COPY ["Shared/MSECS.SharedKernel/MSECS.SharedKernel.csproj", "Shared/MSECS.SharedKernel/"]
COPY ["Shared/MSECS.BuildingBlocks/MSECS.BuildingBlocks.csproj", "Shared/MSECS.BuildingBlocks/"]
RUN dotnet restore "Services/Identity/MSECS.Identity.Api/MSECS.Identity.Api.csproj"

COPY . .
WORKDIR "/src/Services/Identity/MSECS.Identity.Api"
RUN dotnet build "MSECS.Identity.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MSECS.Identity.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MSECS.Identity.Api.dll"]
