BEGIN TRANSACTION;

-- Add rejected_by column if it does not exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('reference_sources') AND name = 'rejected_by')
BEGIN
    ALTER TABLE reference_sources ADD rejected_by INT NULL;
    ALTER TABLE reference_sources ADD CONSTRAINT FK_reference_sources_users_rejected_by FOREIGN KEY (rejected_by) REFERENCES users(id);
END

-- Add rejected_at column if it does not exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('reference_sources') AND name = 'rejected_at')
BEGIN
    ALTER TABLE reference_sources ADD rejected_at DATETIME2 NULL;
END

-- Add rejection_reason column if it does not exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('reference_sources') AND name = 'rejection_reason')
BEGIN
    ALTER TABLE reference_sources ADD rejection_reason NVARCHAR(MAX) NULL;
END

COMMIT;
GO
