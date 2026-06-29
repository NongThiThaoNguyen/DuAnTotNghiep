BEGIN TRANSACTION;

-- Add author column if it does not exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('reference_sources') AND name = 'author')
BEGIN
    ALTER TABLE reference_sources ADD author NVARCHAR(255) NULL;
END

-- Add organization column if it does not exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('reference_sources') AND name = 'organization')
BEGIN
    ALTER TABLE reference_sources ADD organization NVARCHAR(255) NULL;
END

-- Add description column if it does not exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('reference_sources') AND name = 'description')
BEGIN
    ALTER TABLE reference_sources ADD description NVARCHAR(MAX) NULL;
END

COMMIT;
GO
