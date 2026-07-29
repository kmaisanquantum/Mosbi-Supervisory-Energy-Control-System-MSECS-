namespace MSECS.DeviceRegistry.Application.DTOs;

public record DeviceDto(
    Guid Id, Guid OrganizationId, Guid SiteId, Guid AssetId, string SerialNumber, string Protocol,
    string? IpAddress, int? Port, int? ModbusUnitId, string? MqttTopic,
    string ProvisioningStatus, string HealthStatus, DateTimeOffset? LastSeenAtUtc);

public record ProvisionDeviceResultDto(DeviceDto Device, string PlaintextSecret);
