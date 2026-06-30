IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='learning_path_nodes' AND COLUMN_NAME='rescheduled_from')
    ALTER TABLE learning_path_nodes ADD rescheduled_from DATE NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='learning_path_nodes' AND COLUMN_NAME='skipped_reason')
    ALTER TABLE learning_path_nodes ADD skipped_reason NVARCHAR(500) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LPN_PathId_ScheduledDate_Status' AND object_id = OBJECT_ID('learning_path_nodes'))
    CREATE NONCLUSTERED INDEX IX_LPN_PathId_ScheduledDate_Status ON learning_path_nodes (learning_path_id, scheduled_date, [status]);
