using MediatR;
using MSECS.DeviceRegistry.Application.DTOs;

namespace MSECS.DeviceRegistry.Application.Devices.Queries.GetDevice;

public record GetDeviceQuery(Guid DeviceId) : IRequest<DeviceDto>;
