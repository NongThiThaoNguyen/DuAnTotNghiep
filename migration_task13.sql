BEGIN TRANSACTION;

-- Drop constraint if exists first, so we can update the policy names
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'CK_ref_usage_policy' AND type = 'C')
BEGIN
    ALTER TABLE reference_sources DROP CONSTRAINT CK_ref_usage_policy;
END

-- Update existing usage policy values in DB
UPDATE reference_sources SET usage_policy = 'OPEN_LICENSE' WHERE usage_policy = 'OPEN_USE';
UPDATE reference_sources SET usage_policy = 'RESTRICTED' WHERE usage_policy = 'INTERNAL_ONLY';

-- Add updated constraint
ALTER TABLE reference_sources ADD CONSTRAINT CK_ref_usage_policy CHECK ([usage_policy] IS NULL OR [usage_policy] IN ('REFERENCE_ONLY', 'OPEN_LICENSE', 'RESTRICTED'));

-- Add compliance_evidence_url column if it does not exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('reference_sources') AND name = 'compliance_evidence_url')
BEGIN
    ALTER TABLE reference_sources ADD compliance_evidence_url NVARCHAR(1000) NULL;
END

COMMIT;
GO
