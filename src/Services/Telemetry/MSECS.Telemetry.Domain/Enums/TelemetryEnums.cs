namespace MSECS.Telemetry.Domain.Enums;

/// <summary>
/// Every measurement type MSECS Phase 1 devices can report. This is the enum the AI-ready
/// forecasting/anomaly-detection data models (Analytics Service, future ML) key off of, so
/// new device types should map onto these rather than inventing ad hoc field names.
/// </summary>
public enum TelemetryMetricType
{
    VoltageV = 1,
    CurrentA = 2,
    PowerKw = 3,
    FrequencyHz = 4,
    EnergyKwh = 5,
    TemperatureC = 6,
    BatterySocPercent = 7,
    BatterySohPercent = 8,
    InverterStatusCode = 9,
    PanelTemperatureC = 10,
    SolarIrradianceWm2 = 11
}

public enum InverterOperatingStatus
{
    Off = 0,
    Standby = 1,
    Producing = 2,
    Fault = 3,
    Curtailed = 4
}
