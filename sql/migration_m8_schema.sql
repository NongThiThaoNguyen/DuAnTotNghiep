SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('dbo.student_learning_paths', 'path_version') IS NULL
BEGIN
    ALTER TABLE dbo.student_learning_paths
    ADD path_version INT NOT NULL
        CONSTRAINT DF_student_learning_paths_path_version DEFAULT (1);
END;

IF COL_LENGTH('dbo.student_learning_paths', 'archived_at') IS NULL
BEGIN
    ALTER TABLE dbo.student_learning_paths
    ADD archived_at DATETIME NULL;
END;

IF COL_LENGTH('dbo.student_learning_paths', 'replaced_by_path_id') IS NULL
BEGIN
    ALTER TABLE dbo.student_learning_paths
    ADD replaced_by_path_id INT NULL;
END;

IF COL_LENGTH('dbo.learning_path_nodes', 'required_node_id') IS NULL
BEGIN
    ALTER TABLE dbo.learning_path_nodes
    ADD required_node_id INT NULL;
END;

IF COL_LENGTH('dbo.student_learning_paths', 'replaced_by_path_id') IS NOT NULL
    AND NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = 'FK_student_learning_paths_replaced_by_path'
          AND parent_object_id = OBJECT_ID('dbo.student_learning_paths')
    )
BEGIN
    ALTER TABLE dbo.student_learning_paths
    ADD CONSTRAINT FK_student_learning_paths_replaced_by_path
        FOREIGN KEY (replaced_by_path_id)
        REFERENCES dbo.student_learning_paths(id);
END;

IF COL_LENGTH('dbo.learning_path_nodes', 'required_node_id') IS NOT NULL
    AND NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = 'FK_learning_path_nodes_required_node'
          AND parent_object_id = OBJECT_ID('dbo.learning_path_nodes')
    )
BEGIN
    ALTER TABLE dbo.learning_path_nodes
    ADD CONSTRAINT FK_learning_path_nodes_required_node
        FOREIGN KEY (required_node_id)
        REFERENCES dbo.learning_path_nodes(id);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_student_learning_paths_student_id_status'
      AND object_id = OBJECT_ID('dbo.student_learning_paths')
)
BEGIN
    CREATE INDEX IX_student_learning_paths_student_id_status
        ON dbo.student_learning_paths(student_id, status);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_learning_path_nodes_learning_path_id_order_index'
      AND object_id = OBJECT_ID('dbo.learning_path_nodes')
)
BEGIN
    CREATE INDEX IX_learning_path_nodes_learning_path_id_order_index
        ON dbo.learning_path_nodes(learning_path_id, order_index);
END;

COMMIT TRANSACTION;
