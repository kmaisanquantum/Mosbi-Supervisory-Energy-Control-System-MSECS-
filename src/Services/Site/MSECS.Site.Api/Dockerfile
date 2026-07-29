FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Services/Site/MSECS.Site.Api/MSECS.Site.Api.csproj", "Services/Site/MSECS.Site.Api/"]
COPY ["Services/Site/MSECS.Site.Application/MSECS.Site.Application.csproj", "Services/Site/MSECS.Site.Application/"]
COPY ["Services/Site/MSECS.Site.Infrastructure/MSECS.Site.Infrastructure.csproj", "Services/Site/MSECS.Site.Infrastructure/"]
COPY ["Services/Site/MSECS.Site.Domain/MSECS.Site.Domain.csproj", "Services/Site/MSECS.Site.Domain/"]
COPY ["Shared/MSECS.SharedKernel/MSECS.SharedKernel.csproj", "Shared/MSECS.SharedKernel/"]
COPY ["Shared/MSECS.BuildingBlocks/MSECS.BuildingBlocks.csproj", "Shared/MSECS.BuildingBlocks/"]
RUN dotnet restore "Services/Site/MSECS.Site.Api/MSECS.Site.Api.csproj"
COPY . .
WORKDIR "/src/Services/Site/MSECS.Site.Api"
RUN dotnet build "MSECS.Site.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MSECS.Site.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MSECS.Site.Api.dll"]
