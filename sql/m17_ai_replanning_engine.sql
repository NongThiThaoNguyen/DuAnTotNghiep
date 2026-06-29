/*
================================================================================
SQL MIGRATION SCRIPT: MODULE M17 - AI REPLANNING ENGINE
================================================================================
Mục đích:
  - Kiểm tra và khởi tạo hoặc bổ sung cấu trúc bảng `ai_replanning_events` phục vụ
    cho module cơ chế lập lại lộ trình học (AI Replanning Engine).
  - Tích hợp các ràng buộc CHECK CONSTRAINT và INDEX tối ưu hóa truy vấn tìm kiếm.
  - Phù hợp chạy trên SQL Server, bảo toàn dữ liệu hiện có (idempotent).

Lịch sử thay đổi:
  - 2026-06-28: Senior Backend Developer - Khởi tạo kịch bản migration idempotent.
================================================================================
*/

SET NOCOUNT ON;

-- ============================================================================
-- 1. KHỞI TẠO BẢNG HOẶC CẬP NHẬT CẤU TRÚC BẢNG (IDEMPOTENT)
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables t WHERE t.name = 'ai_replanning_events' AND SCHEMA_NAME(t.schema_id) = 'dbo')
BEGIN
    -- Trường hợp bảng chưa tồn tại: Tạo mới bảng với đầy đủ cấu trúc & khóa ngoại
    CREATE TABLE dbo.ai_replanning_events (
        -- [id]: Khoá chính tự tăng, định danh duy nhất cho mỗi sự kiện đề xuất lộ trình mới.
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,

        -- [student_id]: Khóa ngoại tham chiếu đến bảng users(id), xác định học viên nhận đề xuất.
        student_id INT NOT NULL,

        -- [learning_path_id]: Khóa ngoại tham chiếu đến student_learning_paths(id), xác định lộ trình học tập hiện tại cần điều chỉnh.
        learning_path_id INT NOT NULL,

        -- [trigger_type]: Tác nhân/Nguyên nhân kích hoạt sự kiện replan (ví dụ: LOW_SCORE, INACTIVE,...).
        trigger_type NVARCHAR(50) NOT NULL,

        -- [old_plan_summary]: Tóm tắt thông tin của lộ trình học tập cũ trước khi điều chỉnh (chuỗi mô tả hoặc JSON).
        old_plan_summary NVARCHAR(MAX) NULL,

        -- [new_plan_summary]: Tóm tắt thông tin của lộ trình học tập mới được đề xuất (chuỗi mô tả hoặc JSON).
        new_plan_summary NVARCHAR(MAX) NULL,

        -- [reason]: Lý do cụ thể kích hoạt sự kiện (ví dụ: 'Học viên có 3 bài kiểm tra dưới 50%').
        reason NVARCHAR(MAX) NOT NULL,

        -- [status]: Trạng thái của đề xuất (mặc định ban đầu là 'SUGGESTED').
        status NVARCHAR(30) NOT NULL DEFAULT 'SUGGESTED',

        -- [path_version_no]: Số phiên bản của lộ trình mới sau khi được áp dụng (nullable, cập nhật khi chuyển trạng thái APPLIED).
        path_version_no INT NULL,

        -- [accepted_by_user]: Đánh dấu sự đồng ý của học viên. NULL/0: chưa đồng ý/từ chối, 1: đồng ý tiếp nhận lộ trình mới.
        accepted_by_user BIT NULL,

        -- [accepted_at]: Thời điểm học viên đồng ý áp dụng đề xuất lộ trình học mới.
        accepted_at DATETIME NULL,

        -- [applied_at]: Thời điểm hệ thống hoàn thành việc cập nhật lộ trình học tập mới vào thực tế.
        applied_at DATETIME NULL,

        -- [error_message]: Chi tiết thông điệp lỗi nếu quá trình áp dụng lộ trình mới bị thất bại.
        error_message NVARCHAR(1000) NULL,

        -- [changed_nodes_json]: Danh sách các node/topic thay đổi dưới dạng JSON (mô tả thay đổi chi tiết).
        changed_nodes_json NVARCHAR(MAX) NULL,

        -- [created_at]: Thời điểm bản ghi được tạo ra (tự động điền theo thời gian UTC của hệ thống).
        created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        -- Các ràng buộc khóa ngoại (Foreign Keys)
        CONSTRAINT FK_replan_student FOREIGN KEY (student_id) REFERENCES dbo.users(id),
        CONSTRAINT FK_replan_path FOREIGN KEY (learning_path_id) REFERENCES dbo.student_learning_paths(id)
    );

    PRINT 'Đã khởi tạo thành công bảng dbo.ai_replanning_events.';
END
ELSE
BEGIN
    -- Trường hợp bảng đã tồn tại: Tiến hành cập nhật cột & kiểu dữ liệu một cách an toàn
    PRINT 'Bảng dbo.ai_replanning_events đã tồn tại. Bắt đầu kiểm tra và cập nhật cấu trúc...';

    -- 1.1 Loại bỏ các CHECK CONSTRAINT cũ nếu có để tránh xung đột khi sửa kiểu dữ liệu cột
    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_replan_trigger' AND parent_object_id = OBJECT_ID('dbo.ai_replanning_events'))
    BEGIN
        ALTER TABLE dbo.ai_replanning_events DROP CONSTRAINT CK_replan_trigger;
        PRINT 'Đã loại bỏ CHECK constraint cũ: CK_replan_trigger';
    END

    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_ai_replanning_events_trigger_type' AND parent_object_id = OBJECT_ID('dbo.ai_replanning_events'))
    BEGIN
        ALTER TABLE dbo.ai_replanning_events DROP CONSTRAINT CK_ai_replanning_events_trigger_type;
        PRINT 'Đã loại bỏ CHECK constraint cũ: CK_ai_replanning_events_trigger_type';
    END

    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_replan_status' AND parent_object_id = OBJECT_ID('dbo.ai_replanning_events'))
    BEGIN
        ALTER TABLE dbo.ai_replanning_events DROP CONSTRAINT CK_replan_status;
        PRINT 'Đã loại bỏ CHECK constraint cũ: CK_replan_status';
    END

    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_ai_replanning_events_status' AND parent_object_id = OBJECT_ID('dbo.ai_replanning_events'))
    BEGIN
        ALTER TABLE dbo.ai_replanning_events DROP CONSTRAINT CK_ai_replanning_events_status;
        PRINT 'Đã loại bỏ CHECK constraint cũ: CK_ai_replanning_events_status';
    END

    -- 1.2 Thay đổi kiểu dữ liệu cho trigger_type sang NVARCHAR(50) NOT NULL
    ALTER TABLE dbo.ai_replanning_events ALTER COLUMN trigger_type NVARCHAR(50) NOT NULL;
    PRINT 'Đã cập nhật kiểu dữ liệu cột trigger_type thành NVARCHAR(50) NOT NULL.';

    -- 1.3 Thay đổi kiểu dữ liệu cột status sang NVARCHAR(30) NOT NULL và cập nhật Default Constraint
    -- Tìm tên Default Constraint ngẫu nhiên của cột status (nếu có) để DROP trước khi thay đổi cột
    DECLARE @DefaultConstraintName NVARCHAR(256);
    SELECT @DefaultConstraintName = dc.name
    FROM sys.default_constraints dc
    JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    WHERE dc.parent_object_id = OBJECT_ID('dbo.ai_replanning_events') AND c.name = 'status';

    IF @DefaultConstraintName IS NOT NULL
    BEGIN
        EXEC('ALTER TABLE dbo.ai_replanning_events DROP CONSTRAINT ' + @DefaultConstraintName);
        PRINT 'Đã hủy Default Constraint cũ của cột status: ' + @DefaultConstraintName;
    END

    -- Thay đổi kiểu dữ liệu cột status
    ALTER TABLE dbo.ai_replanning_events ALTER COLUMN status NVARCHAR(30) NOT NULL;
    PRINT 'Đã cập nhật kiểu dữ liệu cột status thành NVARCHAR(30) NOT NULL.';

    -- Thêm lại Default Constraint mặc định mới là 'SUGGESTED'
    ALTER TABLE dbo.ai_replanning_events ADD CONSTRAINT DF_ai_replanning_events_status DEFAULT 'SUGGESTED' FOR status;
    PRINT 'Đã thiết lập giá trị mặc định DEFAULT ''SUGGESTED'' cho cột status.';

    -- 1.4 Bổ sung các cột mới nếu chưa tồn tại
    -- Cột path_version_no
    IF COL_LENGTH('dbo.ai_replanning_events', 'path_version_no') IS NULL
    BEGIN
        ALTER TABLE dbo.ai_replanning_events ADD path_version_no INT NULL;
        PRINT 'Đã bổ sung cột path_version_no (INT NULL).';
    END

    -- Cột accepted_by_user
    IF COL_LENGTH('dbo.ai_replanning_events', 'accepted_by_user') IS NULL
    BEGIN
        ALTER TABLE dbo.ai_replanning_events ADD accepted_by_user BIT NULL;
        PRINT 'Đã bổ sung cột accepted_by_user (BIT NULL).';
    END

    -- Cột accepted_at
    IF COL_LENGTH('dbo.ai_replanning_events', 'accepted_at') IS NULL
    BEGIN
        ALTER TABLE dbo.ai_replanning_events ADD accepted_at DATETIME NULL;
        PRINT 'Đã bổ sung cột accepted_at (DATETIME NULL).';
    END

    -- Cột applied_at
    IF COL_LENGTH('dbo.ai_replanning_events', 'applied_at') IS NULL
    BEGIN
        ALTER TABLE dbo.ai_replanning_events ADD applied_at DATETIME NULL;
        PRINT 'Đã bổ sung cột applied_at (DATETIME NULL).';
    END

    -- Cột error_message
    IF COL_LENGTH('dbo.ai_replanning_events', 'error_message') IS NULL
    BEGIN
        ALTER TABLE dbo.ai_replanning_events ADD error_message NVARCHAR(1000) NULL;
        PRINT 'Đã bổ sung cột error_message (NVARCHAR(1000) NULL).';
    END

    -- Cột changed_nodes_json
    IF COL_LENGTH('dbo.ai_replanning_events', 'changed_nodes_json') IS NULL
    BEGIN
        ALTER TABLE dbo.ai_replanning_events ADD changed_nodes_json NVARCHAR(MAX) NULL;
        PRINT 'Đã bổ sung cột changed_nodes_json (NVARCHAR(MAX) NULL).';
    END
END

-- ============================================================================
-- 2. THIẾT LẬP CHECK CONSTRAINTS ĐẢM BẢO TOÀN VẸN DỮ LIỆU
-- ============================================================================

-- Ràng buộc CHECK cho trigger_type: LOW_SCORE, INACTIVE, FAST_PROGRESS, MANUAL_REQUEST, AI_RECOMMENDATION, GOAL_CHANGED
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_ai_replanning_events_trigger_type' AND parent_object_id = OBJECT_ID('dbo.ai_replanning_events'))
BEGIN
    ALTER TABLE dbo.ai_replanning_events
    ADD CONSTRAINT CK_ai_replanning_events_trigger_type CHECK (
        trigger_type IN ('LOW_SCORE', 'INACTIVE', 'FAST_PROGRESS', 'MANUAL_REQUEST', 'AI_RECOMMENDATION', 'GOAL_CHANGED')
    );
    PRINT 'Đã thêm ràng buộc CHECK CONSTRAINT: CK_ai_replanning_events_trigger_type.';
END

-- Ràng buộc CHECK cho status: SUGGESTED, APPLIED, REJECTED, EXPIRED, FAILED
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_ai_replanning_events_status' AND parent_object_id = OBJECT_ID('dbo.ai_replanning_events'))
BEGIN
    ALTER TABLE dbo.ai_replanning_events
    ADD CONSTRAINT CK_ai_replanning_events_status CHECK (
        status IN ('SUGGESTED', 'APPLIED', 'REJECTED', 'EXPIRED', 'FAILED')
    );
    PRINT 'Đã thêm ràng buộc CHECK CONSTRAINT: CK_ai_replanning_events_status.';
END

-- ============================================================================
-- 3. KHỞI TẠO CHỈ MỤC INDEX ĐỂ TỐI ƯU HÓA TRUY VẤN
-- ============================================================================

-- Tạo chỉ mục (Index) trên bộ 4 cột thường dùng để lọc và truy vấn: (student_id, learning_path_id, created_at, status)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes i
    JOIN sys.objects o ON i.object_id = o.object_id
    WHERE i.name = 'IX_ai_replanning_events_student_path_created_status'
      AND o.name = 'ai_replanning_events'
)
BEGIN
    CREATE INDEX IX_ai_replanning_events_student_path_created_status
    ON dbo.ai_replanning_events (student_id, learning_path_id, created_at, status);
    PRINT 'Đã tạo INDEX IX_ai_replanning_events_student_path_created_status thành công.';
END
ELSE
BEGIN
    PRINT 'INDEX IX_ai_replanning_events_student_path_created_status đã tồn tại từ trước.';
END

-- ============================================================================
-- 4. KHỞI TẠO BẢNG REPLANNING RULES & SEED DỮ LIỆU MẪU (IDEMPOTENT)
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables t WHERE t.name = 'replanning_rules' AND SCHEMA_NAME(t.schema_id) = 'dbo')
BEGIN
    CREATE TABLE dbo.replanning_rules (
        -- [id]: Khoá chính tự tăng cho cấu hình luật replanning.
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,

        -- [low_score_threshold]: Ngưỡng điểm thấp để kích hoạt replan (mặc định 60).
        low_score_threshold DECIMAL(5,2) NOT NULL DEFAULT 60.00,

        -- [missed_days_threshold]: Số ngày không hoạt động tối đa trước khi kích hoạt replan (mặc định 3).
        missed_days_threshold INT NOT NULL DEFAULT 3,

        -- [fast_progress_score_threshold]: Ngưỡng điểm trung bình cao để kích hoạt replan nhanh (mặc định 85).
        fast_progress_score_threshold DECIMAL(5,2) NOT NULL DEFAULT 85.00,

        -- [auto_apply_enabled]: Cho phép tự động áp dụng lộ trình mới mà không cần học viên đồng ý (mặc định 0 - False).
        auto_apply_enabled BIT NOT NULL DEFAULT 0,

        -- [suggestion_expiry_days]: Số ngày hết hạn của đề xuất lộ trình mới (mặc định 7).
        suggestion_expiry_days INT NOT NULL DEFAULT 7,

        -- [debounce_hours]: Khoảng thời gian giãn cách tối thiểu (giờ) giữa hai lần replan liên tiếp cùng một loại (mặc định 24).
        debounce_hours INT NOT NULL DEFAULT 24,

        -- [is_active]: Đánh dấu luật này có đang hoạt động hay không.
        is_active BIT NOT NULL DEFAULT 1,

        -- [created_at]: Thời điểm cấu hình luật này được tạo.
        created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        -- [updated_at]: Thời điểm cấu hình luật được cập nhật lần cuối.
        updated_at DATETIME2 NULL
    );
    PRINT 'Đã khởi tạo thành công bảng dbo.replanning_rules.';
END
ELSE
BEGIN
    PRINT 'Bảng dbo.replanning_rules đã tồn tại. Bỏ qua khởi tạo.';
END

-- Seed dữ liệu mẫu cho luật hoạt động ban đầu nếu chưa có luật nào hoạt động
IF NOT EXISTS (SELECT 1 FROM dbo.replanning_rules WHERE is_active = 1)
BEGIN
    INSERT INTO dbo.replanning_rules (
        low_score_threshold,
        missed_days_threshold,
        fast_progress_score_threshold,
        auto_apply_enabled,
        suggestion_expiry_days,
        debounce_hours,
        is_active
    )
    VALUES (60.00, 3, 85.00, 0, 7, 24, 1);
    PRINT 'Đã seed cấu hình luật replanning mặc định.';
END

PRINT 'MIGRATION MODULE M17 - AI REPLANNING ENGINE HOÀN THÀNH THÀNH CÔNG.';
