using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.DeviceRegistry.Application.Common.Interfaces;
using MSECS.DeviceRegistry.Application.DTOs;
using MSECS.DeviceRegistry.Domain.Entities;
using MSECS.DeviceRegistry.Domain.Enums;
using MSECS.SharedKernel.Exceptions;

namespace MSECS.DeviceRegistry.Application.Devices.Commands.ProvisionDevice;

public class ProvisionDeviceCommandHandler : IRequestHandler<ProvisionDeviceCommand, ProvisionDeviceResultDto>
{
    private readonly IDeviceDbContext _db;

    public ProvisionDeviceCommandHandler(IDeviceDbContext db) => _db = db;

    public async Task<ProvisionDeviceResultDto> Handle(ProvisionDeviceCommand request, CancellationToken cancellationToken)
    {
        var duplicateSerial = await _db.Devices.AnyAsync(d => d.SerialNumber == request.SerialNumber, cancellationToken);
        if (duplicateSerial)
            throw new ConflictException($"A device with serial number '{request.SerialNumber}' is already registered.");

        var protocol = Enum.Parse<DeviceProtocol>(request.Protocol, true);
        var (device, plaintextSecret) = Device.Register(
            request.OrganizationId, request.SiteId, request.AssetId, request.SerialNumber, protocol);

        switch (protocol)
        {
            case DeviceProtocol.ModbusTcp:
                device.ConfigureModbusTcp(request.IpAddress!, request.Port!.Value, request.ModbusUnitId!.Value);
                break;
            case DeviceProtocol.Mqtt:
                device.ConfigureMqtt(request.MqttTopic!);
                break;
            case DeviceProtocol.Rest:
                break; // REST devices push directly to the Telemetry ingestion endpoint using their API key.
        }

        device.CompleteProvisioning();

        await _db.Devices.AddAsync(device, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return new ProvisionDeviceResultDto(Map(device), plaintextSecret);
    }

    public static DeviceDto Map(Device device) => new(
        device.Id, device.OrganizationId, device.SiteId, device.AssetId, device.SerialNumber,
        device.Protocol.ToString(), device.IpAddress, device.Port, device.ModbusUnitId, device.MqttTopic,
        device.ProvisioningStatus.ToString(), device.HealthStatus.ToString(), device.LastSeenAtUtc);
}
