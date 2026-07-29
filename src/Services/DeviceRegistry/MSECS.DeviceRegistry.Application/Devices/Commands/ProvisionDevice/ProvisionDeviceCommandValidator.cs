using FluentValidation;
using MSECS.DeviceRegistry.Domain.Enums;

namespace MSECS.DeviceRegistry.Application.Devices.Commands.ProvisionDevice;

public class ProvisionDeviceCommandValidator : AbstractValidator<ProvisionDeviceCommand>
{
    public ProvisionDeviceCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.AssetId).NotEmpty();
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Protocol).NotEmpty().Must(p => Enum.TryParse<DeviceProtocol>(p, true, out _))
            .WithMessage("Protocol must be one of: ModbusTcp, Mqtt, Rest.");

        RuleFor(x => x.IpAddress).NotEmpty()
            .When(x => string.Equals(x.Protocol, nameof(DeviceProtocol.ModbusTcp), StringComparison.OrdinalIgnoreCase));
        RuleFor(x => x.Port).NotNull().InclusiveBetween(1, 65535)
            .When(x => string.Equals(x.Protocol, nameof(DeviceProtocol.ModbusTcp), StringComparison.OrdinalIgnoreCase));
        RuleFor(x => x.ModbusUnitId).NotNull().InclusiveBetween(0, 247)
            .When(x => string.Equals(x.Protocol, nameof(DeviceProtocol.ModbusTcp), StringComparison.OrdinalIgnoreCase));

        RuleFor(x => x.MqttTopic).NotEmpty()
            .When(x => string.Equals(x.Protocol, nameof(DeviceProtocol.Mqtt), StringComparison.OrdinalIgnoreCase));
    }
}
