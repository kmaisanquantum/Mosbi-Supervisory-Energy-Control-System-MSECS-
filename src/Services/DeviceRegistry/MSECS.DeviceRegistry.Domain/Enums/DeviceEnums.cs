namespace MSECS.DeviceRegistry.Domain.Enums;

public enum DeviceProtocol
{
    ModbusTcp = 1,
    Mqtt = 2,
    Rest = 3
}

public enum DeviceHealthStatus
{
    Unknown = 0,
    Online = 1,
    Offline = 2,
    Degraded = 3
}

public enum ProvisioningStatus
{
    PendingProvisioning = 1,
    Provisioned = 2,
    Revoked = 3
}
