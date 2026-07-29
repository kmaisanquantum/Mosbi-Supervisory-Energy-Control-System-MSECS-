using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.DeviceRegistry.Application.Common.Interfaces;
using MSECS.DeviceRegistry.Application.Devices.Commands.ProvisionDevice;
using MSECS.DeviceRegistry.Application.DTOs;
using MSECS.SharedKernel.Exceptions;

namespace MSECS.DeviceRegistry.Application.Devices.Queries.GetDevice;

public class GetDeviceQueryHandler : IRequestHandler<GetDeviceQuery, DeviceDto>
{
    private readonly IDeviceDbContext _db;
    public GetDeviceQueryHandler(IDeviceDbContext db) => _db = db;

    public async Task<DeviceDto> Handle(GetDeviceQuery request, CancellationToken cancellationToken)
    {
        var device = await _db.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.Id == request.DeviceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Device), request.DeviceId);
        return ProvisionDeviceCommandHandler.Map(device);
    }
}
