using MediatR;
using MSECS.DeviceRegistry.Application.DTOs;

namespace MSECS.DeviceRegistry.Application.Devices.Commands.ProvisionDevice;

public record ProvisionDeviceCommand(
    Guid OrganizationId,
    Guid SiteId,
    Guid AssetId,
    string SerialNumber,
    string Protocol,
    string? IpAddress,
    int? Port,
    int? ModbusUnitId,
    string? MqttTopic) : IRequest<ProvisionDeviceResultDto>;
