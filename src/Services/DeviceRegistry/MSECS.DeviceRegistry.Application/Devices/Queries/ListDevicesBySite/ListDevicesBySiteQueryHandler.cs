using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.DeviceRegistry.Application.Common.Interfaces;
using MSECS.DeviceRegistry.Application.Devices.Commands.ProvisionDevice;
using MSECS.DeviceRegistry.Application.DTOs;

namespace MSECS.DeviceRegistry.Application.Devices.Queries.ListDevicesBySite;

public class ListDevicesBySiteQueryHandler : IRequestHandler<ListDevicesBySiteQuery, IReadOnlyList<DeviceDto>>
{
    private readonly IDeviceDbContext _db;
    public ListDevicesBySiteQueryHandler(IDeviceDbContext db) => _db = db;

    public async Task<IReadOnlyList<DeviceDto>> Handle(ListDevicesBySiteQuery request, CancellationToken cancellationToken)
    {
        var devices = await _db.Devices.AsNoTracking().Where(d => d.SiteId == request.SiteId)
            .OrderBy(d => d.SerialNumber).ToListAsync(cancellationToken);
        return devices.Select(ProvisionDeviceCommandHandler.Map).ToList();
    }
}
