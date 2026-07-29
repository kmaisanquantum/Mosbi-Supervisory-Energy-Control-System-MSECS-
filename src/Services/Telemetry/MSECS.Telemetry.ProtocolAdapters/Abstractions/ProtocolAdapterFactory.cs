namespace MSECS.Telemetry.ProtocolAdapters.Abstractions;

public class ProtocolAdapterFactory
{
    private readonly IReadOnlyDictionary<string, IProtocolAdapter> _adapters;

    public ProtocolAdapterFactory(IEnumerable<IProtocolAdapter> adapters)
    {
        _adapters = adapters.ToDictionary(a => a.ProtocolName, StringComparer.OrdinalIgnoreCase);
    }

    public IProtocolAdapter Resolve(string protocolName) =>
        _adapters.TryGetValue(protocolName, out var adapter)
            ? adapter
            : throw new NotSupportedException($"No protocol adapter is registered for '{protocolName}'. " +
                $"Supported protocols: {string.Join(", ", _adapters.Keys)}.");
}
