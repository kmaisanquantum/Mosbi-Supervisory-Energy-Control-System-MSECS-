-- Reference / manual-run version of what
-- MSECS.Telemetry.Infrastructure.DependencyInjection.EnsureTimescaleHypertableAsync()
-- does automatically at Telemetry Service startup. Kept here for operators who want
-- to inspect or re-run it by hand (e.g. after a manual restore).

SELECT create_hypertable('telemetry.readings', 'recorded_at_utc',
    if_not_exists => TRUE, migrate_data => TRUE);

ALTER TABLE telemetry.readings SET (
    timescaledb.compress,
    timescaledb.compress_segmentby = 'device_id, metric_type'
);

-- Compress chunks older than 7 days to save storage; readings are still queryable,
-- just stored more efficiently.
SELECT add_compression_policy('telemetry.readings', INTERVAL '7 days', if_not_exists => TRUE);

-- Drop chunks older than 2 years. Adjust per your regulatory/reporting retention needs.
SELECT add_retention_policy('telemetry.readings', INTERVAL '2 years', if_not_exists => TRUE);
