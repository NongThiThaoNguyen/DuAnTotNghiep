BEGIN TRANSACTION;

-- Create indexes on reference_sources if they do not exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_reference_sources_source_name' AND object_id = OBJECT_ID('reference_sources'))
BEGIN
    CREATE INDEX IX_reference_sources_source_name ON reference_sources(source_name);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_reference_sources_source_url' AND object_id = OBJECT_ID('reference_sources'))
BEGIN
    CREATE INDEX IX_reference_sources_source_url ON reference_sources(source_url);
END

COMMIT;
GO
