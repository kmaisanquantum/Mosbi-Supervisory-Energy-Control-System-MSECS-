using System.Security.Cryptography;
using System.Text;
using MSECS.SharedKernel.Common;
using MSECS.SharedKernel.Multitenancy;
using MSECS.DeviceRegistry.Domain.Enums;
using MSECS.DeviceRegistry.Domain.Events;

namespace MSECS.DeviceRegistry.Domain.Entities;

/// <summary>
/// The network-addressable endpoint for a physical Asset — what the Telemetry Service
/// and Command Service actually talk to. One Device maps 1:1 to one Asset (SiteService's
/// Asset.DeviceId), but is provisioned and authenticated independently so credentials can
/// be rotated without touching equipment inventory records.
/// </summary>
public class Device : AggregateRoot<Guid>, ITenantAware
{
    public Guid OrganizationId { get; private set; }
    public Guid SiteId { get; private set; }
    public Guid AssetId { get; private set; }
    public string SerialNumber { get; private set; } = string.Empty;
    public DeviceProtocol Protocol { get; private set; }
    public string? IpAddress { get; private set; }
    public int? Port { get; private set; }
    public int? ModbusUnitId { get; private set; }
    public string? MqttTopic { get; private set; }
    public string CredentialHash { get; private set; } = string.Empty;
    public ProvisioningStatus ProvisioningStatus { get; private set; } = ProvisioningStatus.PendingProvisioning;
    public DeviceHealthStatus HealthStatus { get; private set; } = DeviceHealthStatus.Unknown;
    public DateTimeOffset? LastSeenAtUtc { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; } = new();

    private Device() { }

    private Device(Guid id, Guid organizationId, Guid siteId, Guid assetId, string serialNumber, DeviceProtocol protocol)
        : base(id)
    {
        OrganizationId = organizationId;
        SiteId = siteId;
        AssetId = assetId;
        SerialNumber = serialNumber;
        Protocol = protocol;
    }

    /// <summary>Registers the device shell; returns the plaintext provisioning secret exactly
    /// once (only its hash is persisted), mirroring the Identity Service's API key pattern.</summary>
    public static (Device Device, string PlaintextSecret) Register(
        Guid organizationId, Guid siteId, Guid assetId, string serialNumber, DeviceProtocol protocol)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            throw new ArgumentException("Serial number is required.", nameof(serialNumber));

        var device = new Device(Guid.NewGuid(), organizationId, siteId, assetId, serialNumber.Trim(), protocol);

        var secretBytes = RandomNumberGenerator.GetBytes(32);
        var plaintextSecret = "dev_" + Convert.ToHexString(secretBytes).ToLowerInvariant();
        device.CredentialHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plaintextSecret))).ToLowerInvariant();

        return (device, plaintextSecret);
    }

    public void ConfigureModbusTcp(string ipAddress, int port, int unitId)
    {
        if (Protocol != DeviceProtocol.ModbusTcp)
            throw new InvalidOperationException("Device protocol is not ModbusTcp.");
        IpAddress = ipAddress;
        Port = port;
        ModbusUnitId = unitId;
    }

    public void ConfigureMqtt(string topic)
    {
        if (Protocol != DeviceProtocol.Mqtt)
            throw new InvalidOperationException("Device protocol is not Mqtt.");
        MqttTopic = topic;
    }

    public void CompleteProvisioning()
    {
        ProvisioningStatus = ProvisioningStatus.Provisioned;
        RaiseDomainEvent(new DeviceProvisionedEvent(Id, OrganizationId, SiteId, AssetId, Protocol.ToString()));
    }

    public void Revoke()
    {
        ProvisioningStatus = ProvisioningStatus.Revoked;
        RaiseDomainEvent(new DeviceRevokedEvent(Id));
    }

    public void RecordHeartbeat()
    {
        LastSeenAtUtc = DateTimeOffset.UtcNow;
        UpdateHealth(DeviceHealthStatus.Online);
    }

    public void UpdateHealth(DeviceHealthStatus status)
    {
        if (HealthStatus == status) return;
        var old = HealthStatus;
        HealthStatus = status;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new DeviceHealthChangedEvent(Id, old.ToString(), status.ToString(), DateTimeOffset.UtcNow));
    }

    public bool VerifySecret(string plaintextSecret) =>
        CredentialHash == Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plaintextSecret))).ToLowerInvariant();

    public void SetMetadata(string key, string value) => Metadata[key] = value;
}
