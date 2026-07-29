using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.DeviceRegistry.Application.Common.Interfaces;
using MSECS.DeviceRegistry.Domain.Enums;
using MSECS.SharedKernel.Exceptions;

namespace MSECS.DeviceRegistry.Application.Devices.Commands.UpdateHealthStatus;

public class UpdateHealthStatusCommandHandler : IRequestHandler<UpdateHealthStatusCommand>
{
    private readonly IDeviceDbContext _db;
    public UpdateHealthStatusCommandHandler(IDeviceDbContext db) => _db = db;

    public async Task Handle(UpdateHealthStatusCommand request, CancellationToken cancellationToken)
    {
        var device = await _db.Devices.FirstOrDefaultAsync(d => d.Id == request.DeviceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Device), request.DeviceId);

        if (Enum.TryParse<DeviceHealthStatus>(request.HealthStatus, true, out var status))
        {
            if (status == DeviceHealthStatus.Online) device.RecordHeartbeat();
            else device.UpdateHealth(status);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
