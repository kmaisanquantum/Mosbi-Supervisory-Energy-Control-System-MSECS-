using MediatR;
using MSECS.DeviceRegistry.Application.DTOs;

namespace MSECS.DeviceRegistry.Application.Devices.Queries.ListDevicesBySite;

public record ListDevicesBySiteQuery(Guid SiteId) : IRequest<IReadOnlyList<DeviceDto>>;
