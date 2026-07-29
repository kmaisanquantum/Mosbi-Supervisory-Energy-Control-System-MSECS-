namespace MSECS.Site.Application.DTOs;

public record SiteDto(
    Guid Id,
    Guid OrganizationId,
    string Name,
    double Latitude,
    double Longitude,
    string WeatherZone,
    string Timezone,
    decimal InstalledCapacityKw,
    DateOnly InstallationDate,
    string? Address,
    string Status);
