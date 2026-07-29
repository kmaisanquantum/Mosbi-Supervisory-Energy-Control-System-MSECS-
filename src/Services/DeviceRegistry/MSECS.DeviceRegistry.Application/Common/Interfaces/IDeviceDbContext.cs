using Microsoft.EntityFrameworkCore;
using MSECS.DeviceRegistry.Domain.Entities;

namespace MSECS.DeviceRegistry.Application.Common.Interfaces;

public interface IDeviceDbContext
{
    DbSet<Device> Devices { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
