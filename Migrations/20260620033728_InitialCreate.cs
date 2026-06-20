using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuAnTotNghiep.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
            migrationBuilder.CreateTable(
                name: "english_proficiency_levels",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    order_index = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__english___3213E83F203E1EC7", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "english_skills",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    skill_code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    skill_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    order_index = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__english___3213E83F663F25FF", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "learning_goals",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    goal_code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    goal_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__learning__3213E83FD1E17598", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    role_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__roles__3213E83FC530F3DF", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false, defaultValue: "ACTIVE"),
                    avatar_url = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    last_login_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__users__3213E83F43CE90C9", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_roles",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ai_prompt_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    prompt_code = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    prompt_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    module_code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    system_prompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    output_schema = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "ACTIVE"),
                    version_no = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ai_promp__3213E83F34DDDEE9", x => x.id);
                    table.ForeignKey(
                        name: "FK_prompt_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    action = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    entity_name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    entity_id = table.Column<int>(type: "int", nullable: true),
                    old_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ip_address = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__audit_lo__3213E83F13E45708", x => x.id);
                    table.ForeignKey(
                        name: "FK_audit_logs_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "content_compliance_reviews",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    content_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    content_id = table.Column<int>(type: "int", nullable: false),
                    reviewer_id = table.Column<int>(type: "int", nullable: false),
                    copyright_check = table.Column<bool>(type: "bit", nullable: false),
                    plagiarism_risk = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    review_status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    review_note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__content___3213E83FABD8D38F", x => x.id);
                    table.ForeignKey(
                        name: "FK_content_reviews_reviewer",
                        column: x => x.reviewer_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "learning_path_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    template_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    goal_id = table.Column<int>(type: "int", nullable: true),
                    start_level_id = table.Column<int>(type: "int", nullable: true),
                    target_level_id = table.Column<int>(type: "int", nullable: true),
                    duration_weeks = table.Column<int>(type: "int", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "DRAFT"),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__learning__3213E83F1B0089FE", x => x.id);
                    table.ForeignKey(
                        name: "FK_lpt_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_lpt_goal",
                        column: x => x.goal_id,
                        principalTable: "learning_goals",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_lpt_start_level",
                        column: x => x.start_level_id,
                        principalTable: "english_proficiency_levels",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_lpt_target_level",
                        column: x => x.target_level_id,
                        principalTable: "english_proficiency_levels",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "learning_topics",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    skill_id = table.Column<int>(type: "int", nullable: false),
                    parent_topic_id = table.Column<int>(type: "int", nullable: true),
                    level_id = table.Column<int>(type: "int", nullable: true),
                    topic_code = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    difficulty_level = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false, defaultValue: "BASIC"),
                    estimated_minutes = table.Column<int>(type: "int", nullable: true),
                    order_index = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false, defaultValue: "ACTIVE"),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__learning__3213E83F8C330B01", x => x.id);
                    table.ForeignKey(
                        name: "FK_topics_level",
                        column: x => x.level_id,
                        principalTable: "english_proficiency_levels",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_topics_parent",
                        column: x => x.parent_topic_id,
                        principalTable: "learning_topics",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_topics_skills",
                        column: x => x.skill_id,
                        principalTable: "english_skills",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_topics_users",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    notification_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    target_user_id = table.Column<int>(type: "int", nullable: true),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__notifica__3213E83FE246EE88", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_notifications_target",
                        column: x => x.target_user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "placement_tests",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    target_level_id = table.Column<int>(type: "int", nullable: true),
                    time_limit_minutes = table.Column<int>(type: "int", nullable: true),
                    total_score = table.Column<decimal>(type: "decimal(6,2)", nullable: false, defaultValue: 100m),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "DRAFT"),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__placemen__3213E83F955AE2ED", x => x.id);
                    table.ForeignKey(
                        name: "FK_pt_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_pt_level",
                        column: x => x.target_level_id,
                        principalTable: "english_proficiency_levels",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "reference_sources",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    source_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    source_url = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    source_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    license_note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    usage_policy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "PENDING"),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    approved_by = table.Column<int>(type: "int", nullable: true),
                    approved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__referenc__3213E83F85C4EC5F", x => x.id);
                    table.ForeignKey(
                        name: "FK_ref_approved_by",
                        column: x => x.approved_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ref_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    token_hash = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__refresh___3213E83FF99585F1", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "report_snapshots",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    report_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    report_date = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(CONVERT([date],sysutcdatetime()))"),
                    generated_by = table.Column<int>(type: "int", nullable: true),
                    report_data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__report_s__3213E83F404FE2DE", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_generated_by",
                        column: x => x.generated_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "search_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    keyword = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    search_scope = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "ALL"),
                    result_count = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__search_l__3213E83FBB7ECF99", x => x.id);
                    table.ForeignKey(
                        name: "FK_search_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "student_learning_profiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    current_level_id = table.Column<int>(type: "int", nullable: true),
                    target_level_id = table.Column<int>(type: "int", nullable: true),
                    main_goal_id = table.Column<int>(type: "int", nullable: true),
                    target_score = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    daily_study_minutes = table.Column<int>(type: "int", nullable: true),
                    weekly_study_days = table.Column<int>(type: "int", nullable: true),
                    preferred_study_time = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    learning_note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    onboarding_status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false, defaultValue: "NOT_STARTED"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__student___3213E83FB021A51B", x => x.id);
                    table.ForeignKey(
                        name: "FK_slp_current_level",
                        column: x => x.current_level_id,
                        principalTable: "english_proficiency_levels",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_slp_goal",
                        column: x => x.main_goal_id,
                        principalTable: "learning_goals",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_slp_target_level",
                        column: x => x.target_level_id,
                        principalTable: "english_proficiency_levels",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_slp_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "system_settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    setting_key = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    setting_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    updated_by = table.Column<int>(type: "int", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__system_s__3213E83FCC2454AB", x => x.id);
                    table.ForeignKey(
                        name: "FK_settings_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, defaultValue: "Viá»‡t Nam"),
                    bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__user_pro__3213E83FD33481F9", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_profiles_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_sessions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    session_token = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    ip_address = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    device_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    last_activity_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__user_ses__3213E83FA162BF6F", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_sessions_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ai_usage_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    module_code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    prompt_template_id = table.Column<int>(type: "int", nullable: true),
                    ai_model = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    input_tokens = table.Column<int>(type: "int", nullable: true),
                    output_tokens = table.Column<int>(type: "int", nullable: true),
                    cost_estimate = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    request_status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "SUCCESS"),
                    error_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ai_usage__3213E83F2E522233", x => x.id);
                    table.ForeignKey(
                        name: "FK_ai_usage_prompt",
                        column: x => x.prompt_template_id,
                        principalTable: "ai_prompt_templates",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ai_usage_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ai_generated_contents",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    requested_by = table.Column<int>(type: "int", nullable: true),
                    content_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    related_topic_id = table.Column<int>(type: "int", nullable: true),
                    prompt_text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    generated_content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ai_model = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    review_status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "PENDING"),
                    reviewed_by = table.Column<int>(type: "int", nullable: true),
                    review_note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ai_gener__3213E83F42F7F589", x => x.id);
                    table.ForeignKey(
                        name: "FK_aig_requested_by",
                        column: x => x.requested_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_aig_reviewed_by",
                        column: x => x.reviewed_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_aig_topic",
                        column: x => x.related_topic_id,
                        principalTable: "learning_topics",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "learning_objectives",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    topic_id = table.Column<int>(type: "int", nullable: false),
                    objective_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cognitive_level = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "UNDERSTAND"),
                    order_index = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__learning__3213E83F6239F152", x => x.id);
                    table.ForeignKey(
                        name: "FK_objectives_topics",
                        column: x => x.topic_id,
                        principalTable: "learning_topics",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "learning_path_template_nodes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    template_id = table.Column<int>(type: "int", nullable: false),
                    topic_id = table.Column<int>(type: "int", nullable: true),
                    skill_id = table.Column<int>(type: "int", nullable: true),
                    node_title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    node_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    estimated_minutes = table.Column<int>(type: "int", nullable: true),
                    order_index = table.Column<int>(type: "int", nullable: false),
                    unlock_condition = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__learning__3213E83F13D5DDD4", x => x.id);
                    table.ForeignKey(
                        name: "FK_lptn_skill",
                        column: x => x.skill_id,
                        principalTable: "english_skills",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_lptn_template",
                        column: x => x.template_id,
                        principalTable: "learning_path_templates",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_lptn_topic",
                        column: x => x.topic_id,
                        principalTable: "learning_topics",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "original_lessons",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    topic_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    content_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "TEXT"),
                    estimated_minutes = table.Column<int>(type: "int", nullable: true),
                    source_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "SELF_CREATED"),
                    review_status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "DRAFT"),
                    is_ai_generated = table.Column<bool>(type: "bit", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    approved_by = table.Column<int>(type: "int", nullable: true),
                    approved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__original__3213E83FFCDBE3D5", x => x.id);
                    table.ForeignKey(
                        name: "FK_lessons_approved_by",
                        column: x => x.approved_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_lessons_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_lessons_topics",
                        column: x => x.topic_id,
                        principalTable: "learning_topics",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "practice_tasks",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    topic_id = table.Column<int>(type: "int", nullable: true),
                    skill_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    instruction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    task_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    difficulty_level = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false, defaultValue: "BASIC"),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "PUBLISHED"),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__practice__3213E83F9C46F05B", x => x.id);
                    table.ForeignKey(
                        name: "FK_practice_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_practice_skill",
                        column: x => x.skill_id,
                        principalTable: "english_skills",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_practice_topic",
                        column: x => x.topic_id,
                        principalTable: "learning_topics",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "question_bank",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    topic_id = table.Column<int>(type: "int", nullable: true),
                    skill_id = table.Column<int>(type: "int", nullable: false),
                    level_id = table.Column<int>(type: "int", nullable: true),
                    question_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    question_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    audio_url = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    image_url = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    correct_answer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    difficulty_level = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false, defaultValue: "BASIC"),
                    source_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "SELF_CREATED"),
                    review_status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "DRAFT"),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    approved_by = table.Column<int>(type: "int", nullable: true),
                    approved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__question__3213E83F73ECF016", x => x.id);
                    table.ForeignKey(
                        name: "FK_qb_approved_by",
                        column: x => x.approved_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_qb_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_qb_level",
                        column: x => x.level_id,
                        principalTable: "english_proficiency_levels",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_qb_skill",
                        column: x => x.skill_id,
                        principalTable: "english_skills",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_qb_topic",
                        column: x => x.topic_id,
                        principalTable: "learning_topics",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "quizzes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    topic_id = table.Column<int>(type: "int", nullable: true),
                    skill_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    quiz_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "PRACTICE"),
                    time_limit_minutes = table.Column<int>(type: "int", nullable: true),
                    passing_score = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "DRAFT"),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__quizzes__3213E83F0BD02077", x => x.id);
                    table.ForeignKey(
                        name: "FK_quiz_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_quiz_skill",
                        column: x => x.skill_id,
                        principalTable: "english_skills",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_quiz_topic",
                        column: x => x.topic_id,
                        principalTable: "learning_topics",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "student_progress_snapshots",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    skill_id = table.Column<int>(type: "int", nullable: true),
                    topic_id = table.Column<int>(type: "int", nullable: true),
                    progress_percent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    average_score = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    total_study_minutes = table.Column<int>(type: "int", nullable: false),
                    completed_nodes = table.Column<int>(type: "int", nullable: false),
                    weak_points = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    snapshot_date = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(CONVERT([date],sysutcdatetime()))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__student___3213E83F6D6FC72B", x => x.id);
                    table.ForeignKey(
                        name: "FK_sps_skill",
                        column: x => x.skill_id,
                        principalTable: "english_skills",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_sps_student",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_sps_topic",
                        column: x => x.topic_id,
                        principalTable: "learning_topics",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "notification_reads",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    notification_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    read_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__notifica__3213E83FE8DFDE34", x => x.id);
                    table.ForeignKey(
                        name: "FK_nr_notification",
                        column: x => x.notification_id,
                        principalTable: "notifications",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_nr_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "placement_test_sections",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    placement_test_id = table.Column<int>(type: "int", nullable: false),
                    skill_id = table.Column<int>(type: "int", nullable: false),
                    section_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    instruction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    order_index = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    max_score = table.Column<decimal>(type: "decimal(6,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__placemen__3213E83FC01FC668", x => x.id);
                    table.ForeignKey(
                        name: "FK_pts_skill",
                        column: x => x.skill_id,
                        principalTable: "english_skills",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_pts_test",
                        column: x => x.placement_test_id,
                        principalTable: "placement_tests",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "test_attempts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    placement_test_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    started_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    total_score = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    estimated_level_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "IN_PROGRESS")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__test_att__3213E83FE21DABC0", x => x.id);
                    table.ForeignKey(
                        name: "FK_attempt_level",
                        column: x => x.estimated_level_id,
                        principalTable: "english_proficiency_levels",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_attempt_student",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_attempt_test",
                        column: x => x.placement_test_id,
                        principalTable: "placement_tests",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "topic_references",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    topic_id = table.Column<int>(type: "int", nullable: false),
                    reference_source_id = table.Column<int>(type: "int", nullable: false),
                    note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__topic_re__3213E83FAB67747E", x => x.id);
                    table.ForeignKey(
                        name: "FK_topic_ref_source",
                        column: x => x.reference_source_id,
                        principalTable: "reference_sources",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_topic_ref_topic",
                        column: x => x.topic_id,
                        principalTable: "learning_topics",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "student_skill_preferences",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_profile_id = table.Column<int>(type: "int", nullable: false),
                    skill_code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    priority_level = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__student___3213E83F3D22755E", x => x.id);
                    table.ForeignKey(
                        name: "FK_ssp_profile",
                        column: x => x.student_profile_id,
                        principalTable: "student_learning_profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "practice_submissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    practice_task_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    submission_text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    file_url = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    audio_url = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    score = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    ai_feedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    teacher_feedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "SUBMITTED")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__practice__3213E83FFB71DA05", x => x.id);
                    table.ForeignKey(
                        name: "FK_ps_student",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ps_task",
                        column: x => x.practice_task_id,
                        principalTable: "practice_tasks",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "question_options",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    question_id = table.Column<int>(type: "int", nullable: false),
                    option_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_correct = table.Column<bool>(type: "bit", nullable: false),
                    order_index = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__question__3213E83FC1CBAC30", x => x.id);
                    table.ForeignKey(
                        name: "FK_options_question",
                        column: x => x.question_id,
                        principalTable: "question_bank",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "quiz_attempts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    quiz_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    started_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    score = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "IN_PROGRESS")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__quiz_att__3213E83F46B04B87", x => x.id);
                    table.ForeignKey(
                        name: "FK_qa_quiz",
                        column: x => x.quiz_id,
                        principalTable: "quizzes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_qa_student",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "quiz_questions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    quiz_id = table.Column<int>(type: "int", nullable: false),
                    question_id = table.Column<int>(type: "int", nullable: false),
                    points = table.Column<decimal>(type: "decimal(6,2)", nullable: false, defaultValue: 1m),
                    order_index = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__quiz_que__3213E83FAD1F10DF", x => x.id);
                    table.ForeignKey(
                        name: "FK_qq_question",
                        column: x => x.question_id,
                        principalTable: "question_bank",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_qq_quiz",
                        column: x => x.quiz_id,
                        principalTable: "quizzes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "placement_test_questions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    section_id = table.Column<int>(type: "int", nullable: false),
                    question_id = table.Column<int>(type: "int", nullable: false),
                    points = table.Column<decimal>(type: "decimal(6,2)", nullable: false, defaultValue: 1m),
                    order_index = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__placemen__3213E83FBEEA4D47", x => x.id);
                    table.ForeignKey(
                        name: "FK_ptq_question",
                        column: x => x.question_id,
                        principalTable: "question_bank",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ptq_section",
                        column: x => x.section_id,
                        principalTable: "placement_test_sections",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "competency_analyses",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    test_attempt_id = table.Column<int>(type: "int", nullable: true),
                    summary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    current_level_id = table.Column<int>(type: "int", nullable: true),
                    recommended_level_id = table.Column<int>(type: "int", nullable: true),
                    strengths = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    weaknesses = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gap_analysis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ai_model = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    confidence_score = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__competen__3213E83FBF5D890C", x => x.id);
                    table.ForeignKey(
                        name: "FK_ca_attempt",
                        column: x => x.test_attempt_id,
                        principalTable: "test_attempts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ca_current_level",
                        column: x => x.current_level_id,
                        principalTable: "english_proficiency_levels",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ca_recommended_level",
                        column: x => x.recommended_level_id,
                        principalTable: "english_proficiency_levels",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ca_student",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "test_answers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    attempt_id = table.Column<int>(type: "int", nullable: false),
                    question_id = table.Column<int>(type: "int", nullable: false),
                    selected_option_id = table.Column<int>(type: "int", nullable: true),
                    answer_text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_correct = table.Column<bool>(type: "bit", nullable: true),
                    score = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    answered_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__test_ans__3213E83F49C84548", x => x.id);
                    table.ForeignKey(
                        name: "FK_ta_attempt",
                        column: x => x.attempt_id,
                        principalTable: "test_attempts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ta_option",
                        column: x => x.selected_option_id,
                        principalTable: "question_options",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ta_question",
                        column: x => x.question_id,
                        principalTable: "question_bank",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ai_feedbacks",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    quiz_attempt_id = table.Column<int>(type: "int", nullable: true),
                    practice_submission_id = table.Column<int>(type: "int", nullable: true),
                    feedback_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    feedback_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mistake_analysis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    recommended_action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ai_feedb__3213E83F659EE2FA", x => x.id);
                    table.ForeignKey(
                        name: "FK_feedback_practice",
                        column: x => x.practice_submission_id,
                        principalTable: "practice_submissions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_feedback_quiz_attempt",
                        column: x => x.quiz_attempt_id,
                        principalTable: "quiz_attempts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_feedback_student",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "quiz_answers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    quiz_attempt_id = table.Column<int>(type: "int", nullable: false),
                    question_id = table.Column<int>(type: "int", nullable: false),
                    selected_option_id = table.Column<int>(type: "int", nullable: true),
                    answer_text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_correct = table.Column<bool>(type: "bit", nullable: true),
                    score = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    ai_explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    answered_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__quiz_ans__3213E83F04A1567E", x => x.id);
                    table.ForeignKey(
                        name: "FK_qans_attempt",
                        column: x => x.quiz_attempt_id,
                        principalTable: "quiz_attempts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_qans_option",
                        column: x => x.selected_option_id,
                        principalTable: "question_options",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_qans_question",
                        column: x => x.question_id,
                        principalTable: "question_bank",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "competency_skill_scores",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    competency_analysis_id = table.Column<int>(type: "int", nullable: false),
                    skill_id = table.Column<int>(type: "int", nullable: false),
                    score = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    level_id = table.Column<int>(type: "int", nullable: true),
                    weakness_note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    priority_level = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__competen__3213E83FC90ADCC0", x => x.id);
                    table.ForeignKey(
                        name: "FK_css_analysis",
                        column: x => x.competency_analysis_id,
                        principalTable: "competency_analyses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_css_level",
                        column: x => x.level_id,
                        principalTable: "english_proficiency_levels",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_css_skill",
                        column: x => x.skill_id,
                        principalTable: "english_skills",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "student_learning_paths",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    template_id = table.Column<int>(type: "int", nullable: true),
                    competency_analysis_id = table.Column<int>(type: "int", nullable: true),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    goal_id = table.Column<int>(type: "int", nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    target_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    ai_plan_summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "ACTIVE"),
                    generated_by_ai = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__student___3213E83F4EC62BD7", x => x.id);
                    table.ForeignKey(
                        name: "FK_slpath_analysis",
                        column: x => x.competency_analysis_id,
                        principalTable: "competency_analyses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_slpath_goal",
                        column: x => x.goal_id,
                        principalTable: "learning_goals",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_slpath_student",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_slpath_template",
                        column: x => x.template_id,
                        principalTable: "learning_path_templates",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ai_replanning_events",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    learning_path_id = table.Column<int>(type: "int", nullable: false),
                    trigger_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    old_plan_summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_plan_summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "APPLIED"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ai_repla__3213E83F1F00B02E", x => x.id);
                    table.ForeignKey(
                        name: "FK_replan_path",
                        column: x => x.learning_path_id,
                        principalTable: "student_learning_paths",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_replan_student",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "learning_path_nodes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    learning_path_id = table.Column<int>(type: "int", nullable: false),
                    topic_id = table.Column<int>(type: "int", nullable: true),
                    lesson_id = table.Column<int>(type: "int", nullable: true),
                    quiz_id = table.Column<int>(type: "int", nullable: true),
                    practice_task_id = table.Column<int>(type: "int", nullable: true),
                    node_title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    node_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    node_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    path_phase = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    scheduled_date = table.Column<DateOnly>(type: "date", nullable: true),
                    estimated_minutes = table.Column<int>(type: "int", nullable: true),
                    order_index = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "LOCKED"),
                    ai_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__learning__3213E83FDB07DA35", x => x.id);
                    table.ForeignKey(
                        name: "FK_lpn_lesson",
                        column: x => x.lesson_id,
                        principalTable: "original_lessons",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_lpn_path",
                        column: x => x.learning_path_id,
                        principalTable: "student_learning_paths",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_lpn_quiz",
                        column: x => x.quiz_id,
                        principalTable: "quizzes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_lpn_task",
                        column: x => x.practice_task_id,
                        principalTable: "practice_tasks",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_lpn_topic",
                        column: x => x.topic_id,
                        principalTable: "learning_topics",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ai_tutor_conversations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    topic_id = table.Column<int>(type: "int", nullable: true),
                    learning_path_node_id = table.Column<int>(type: "int", nullable: true),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "ACTIVE"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ai_tutor__3213E83F49FD9E0A", x => x.id);
                    table.ForeignKey(
                        name: "FK_conv_node",
                        column: x => x.learning_path_node_id,
                        principalTable: "learning_path_nodes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_conv_student",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_conv_topic",
                        column: x => x.topic_id,
                        principalTable: "learning_topics",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "study_activity_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    activity_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    topic_id = table.Column<int>(type: "int", nullable: true),
                    learning_path_node_id = table.Column<int>(type: "int", nullable: true),
                    duration_minutes = table.Column<int>(type: "int", nullable: true),
                    score = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__study_ac__3213E83F34CF263F", x => x.id);
                    table.ForeignKey(
                        name: "FK_sal_node",
                        column: x => x.learning_path_node_id,
                        principalTable: "learning_path_nodes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_sal_student",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_sal_topic",
                        column: x => x.topic_id,
                        principalTable: "learning_topics",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ai_tutor_messages",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    conversation_id = table.Column<int>(type: "int", nullable: false),
                    sender_type = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    message_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ai_model = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    token_usage = table.Column<int>(type: "int", nullable: true),
                    safety_flag = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ai_tutor__3213E83FA2FBD9A7", x => x.id);
                    table.ForeignKey(
                        name: "FK_msg_conversation",
                        column: x => x.conversation_id,
                        principalTable: "ai_tutor_conversations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_feedbacks_practice_submission_id",
                table: "ai_feedbacks",
                column: "practice_submission_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_feedbacks_quiz_attempt_id",
                table: "ai_feedbacks",
                column: "quiz_attempt_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_feedbacks_student_id",
                table: "ai_feedbacks",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_generated_contents_related_topic_id",
                table: "ai_generated_contents",
                column: "related_topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_generated_contents_requested_by",
                table: "ai_generated_contents",
                column: "requested_by");

            migrationBuilder.CreateIndex(
                name: "IX_ai_generated_contents_reviewed_by",
                table: "ai_generated_contents",
                column: "reviewed_by");

            migrationBuilder.CreateIndex(
                name: "IX_ai_prompt_templates_created_by",
                table: "ai_prompt_templates",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "UQ__ai_promp__3BD932B213642BD3",
                table: "ai_prompt_templates",
                column: "prompt_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ai_replanning_events_learning_path_id",
                table: "ai_replanning_events",
                column: "learning_path_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_replanning_events_student_id",
                table: "ai_replanning_events",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_tutor_conversations_learning_path_node_id",
                table: "ai_tutor_conversations",
                column: "learning_path_node_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_tutor_conversations_student_id",
                table: "ai_tutor_conversations",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_tutor_conversations_topic_id",
                table: "ai_tutor_conversations",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_tutor_messages_conversation_id",
                table: "ai_tutor_messages",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_usage_logs_prompt_template_id",
                table: "ai_usage_logs",
                column: "prompt_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_usage_logs_user_id",
                table: "ai_usage_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_usage_module_date",
                table: "ai_usage_logs",
                columns: new[] { "module_code", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_user_id",
                table: "audit_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_competency_analyses_current_level_id",
                table: "competency_analyses",
                column: "current_level_id");

            migrationBuilder.CreateIndex(
                name: "IX_competency_analyses_recommended_level_id",
                table: "competency_analyses",
                column: "recommended_level_id");

            migrationBuilder.CreateIndex(
                name: "IX_competency_analyses_student_id",
                table: "competency_analyses",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_competency_analyses_test_attempt_id",
                table: "competency_analyses",
                column: "test_attempt_id");

            migrationBuilder.CreateIndex(
                name: "IX_competency_skill_scores_competency_analysis_id",
                table: "competency_skill_scores",
                column: "competency_analysis_id");

            migrationBuilder.CreateIndex(
                name: "IX_competency_skill_scores_level_id",
                table: "competency_skill_scores",
                column: "level_id");

            migrationBuilder.CreateIndex(
                name: "IX_competency_skill_scores_skill_id",
                table: "competency_skill_scores",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_compliance_reviews_reviewer_id",
                table: "content_compliance_reviews",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "UQ__english___357D4CF9AA0D901F",
                table: "english_proficiency_levels",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__english___03ED21D844E200DF",
                table: "english_skills",
                column: "skill_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__learning__A2EA35BF031D9129",
                table: "learning_goals",
                column: "goal_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_learning_objectives_topic_id",
                table: "learning_objectives",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_learning_nodes_path_status",
                table: "learning_path_nodes",
                columns: new[] { "learning_path_id", "status", "order_index" });

            migrationBuilder.CreateIndex(
                name: "IX_learning_path_nodes_lesson_id",
                table: "learning_path_nodes",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "IX_learning_path_nodes_practice_task_id",
                table: "learning_path_nodes",
                column: "practice_task_id");

            migrationBuilder.CreateIndex(
                name: "IX_learning_path_nodes_quiz_id",
                table: "learning_path_nodes",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_learning_path_nodes_topic_id",
                table: "learning_path_nodes",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_learning_path_template_nodes_skill_id",
                table: "learning_path_template_nodes",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_learning_path_template_nodes_template_id",
                table: "learning_path_template_nodes",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_learning_path_template_nodes_topic_id",
                table: "learning_path_template_nodes",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_learning_path_templates_created_by",
                table: "learning_path_templates",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_learning_path_templates_goal_id",
                table: "learning_path_templates",
                column: "goal_id");

            migrationBuilder.CreateIndex(
                name: "IX_learning_path_templates_start_level_id",
                table: "learning_path_templates",
                column: "start_level_id");

            migrationBuilder.CreateIndex(
                name: "IX_learning_path_templates_target_level_id",
                table: "learning_path_templates",
                column: "target_level_id");

            migrationBuilder.CreateIndex(
                name: "IX_learning_topics_created_by",
                table: "learning_topics",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_learning_topics_level_id",
                table: "learning_topics",
                column: "level_id");

            migrationBuilder.CreateIndex(
                name: "IX_learning_topics_parent_topic_id",
                table: "learning_topics",
                column: "parent_topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_topics_skill_level",
                table: "learning_topics",
                columns: new[] { "skill_id", "level_id" });

            migrationBuilder.CreateIndex(
                name: "UQ__learning__DDA414C51EC96AFC",
                table: "learning_topics",
                column: "topic_code",
                unique: true,
                filter: "[topic_code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_notification_reads_user_id",
                table: "notification_reads",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ_notification_read",
                table: "notification_reads",
                columns: new[] { "notification_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_created_by",
                table: "notifications",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_target",
                table: "notifications",
                columns: new[] { "target_user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_original_lessons_approved_by",
                table: "original_lessons",
                column: "approved_by");

            migrationBuilder.CreateIndex(
                name: "IX_original_lessons_created_by",
                table: "original_lessons",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_original_lessons_topic_id",
                table: "original_lessons",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_placement_test_questions_question_id",
                table: "placement_test_questions",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "UQ_ptq",
                table: "placement_test_questions",
                columns: new[] { "section_id", "question_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_placement_test_sections_placement_test_id",
                table: "placement_test_sections",
                column: "placement_test_id");

            migrationBuilder.CreateIndex(
                name: "IX_placement_test_sections_skill_id",
                table: "placement_test_sections",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_placement_tests_created_by",
                table: "placement_tests",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_placement_tests_target_level_id",
                table: "placement_tests",
                column: "target_level_id");

            migrationBuilder.CreateIndex(
                name: "IX_practice_submissions_practice_task_id",
                table: "practice_submissions",
                column: "practice_task_id");

            migrationBuilder.CreateIndex(
                name: "IX_practice_submissions_student_id",
                table: "practice_submissions",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_practice_tasks_created_by",
                table: "practice_tasks",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_practice_tasks_skill_id",
                table: "practice_tasks",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_practice_tasks_topic_id",
                table: "practice_tasks",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_question_bank_approved_by",
                table: "question_bank",
                column: "approved_by");

            migrationBuilder.CreateIndex(
                name: "IX_question_bank_created_by",
                table: "question_bank",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_question_bank_level_id",
                table: "question_bank",
                column: "level_id");

            migrationBuilder.CreateIndex(
                name: "IX_question_bank_topic_id",
                table: "question_bank",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_questions_skill_topic",
                table: "question_bank",
                columns: new[] { "skill_id", "topic_id", "difficulty_level" });

            migrationBuilder.CreateIndex(
                name: "IX_question_options_question_id",
                table: "question_options",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_answers_question_id",
                table: "quiz_answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_answers_quiz_attempt_id",
                table: "quiz_answers",
                column: "quiz_attempt_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_answers_selected_option_id",
                table: "quiz_answers",
                column: "selected_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempt_student",
                table: "quiz_attempts",
                columns: new[] { "student_id", "submitted_at" });

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempts_quiz_id",
                table: "quiz_attempts",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_questions_question_id",
                table: "quiz_questions",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "UQ_quiz_question",
                table: "quiz_questions",
                columns: new[] { "quiz_id", "question_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quizzes_created_by",
                table: "quizzes",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_quizzes_skill_id",
                table: "quizzes",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_quizzes_topic_id",
                table: "quizzes",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_reference_sources_approved_by",
                table: "reference_sources",
                column: "approved_by");

            migrationBuilder.CreateIndex(
                name: "IX_reference_sources_created_by",
                table: "reference_sources",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_snapshots_generated_by",
                table: "report_snapshots",
                column: "generated_by");

            migrationBuilder.CreateIndex(
                name: "UQ__roles__BAE630753730BBBE",
                table: "roles",
                column: "role_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_search_logs_user_id",
                table: "search_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_learning_paths_student",
                table: "student_learning_paths",
                columns: new[] { "student_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_student_learning_paths_competency_analysis_id",
                table: "student_learning_paths",
                column: "competency_analysis_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_learning_paths_goal_id",
                table: "student_learning_paths",
                column: "goal_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_learning_paths_template_id",
                table: "student_learning_paths",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_learning_profiles_current_level_id",
                table: "student_learning_profiles",
                column: "current_level_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_learning_profiles_main_goal_id",
                table: "student_learning_profiles",
                column: "main_goal_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_learning_profiles_target_level_id",
                table: "student_learning_profiles",
                column: "target_level_id");

            migrationBuilder.CreateIndex(
                name: "UQ__student___B9BE370EE2CB829C",
                table: "student_learning_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_progress_snapshots_skill_id",
                table: "student_progress_snapshots",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_progress_snapshots_student_id",
                table: "student_progress_snapshots",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_progress_snapshots_topic_id",
                table: "student_progress_snapshots",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "UQ_ssp_profile_skill",
                table: "student_skill_preferences",
                columns: new[] { "student_profile_id", "skill_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_activity_student_date",
                table: "study_activity_logs",
                columns: new[] { "student_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_study_activity_logs_learning_path_node_id",
                table: "study_activity_logs",
                column: "learning_path_node_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_activity_logs_topic_id",
                table: "study_activity_logs",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_system_settings_updated_by",
                table: "system_settings",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "UQ__system_s__0DFAC42717432674",
                table: "system_settings",
                column: "setting_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_test_answers_attempt_id",
                table: "test_answers",
                column: "attempt_id");

            migrationBuilder.CreateIndex(
                name: "IX_test_answers_question_id",
                table: "test_answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_test_answers_selected_option_id",
                table: "test_answers",
                column: "selected_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_test_attempt_student",
                table: "test_attempts",
                columns: new[] { "student_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_test_attempts_estimated_level_id",
                table: "test_attempts",
                column: "estimated_level_id");

            migrationBuilder.CreateIndex(
                name: "IX_test_attempts_placement_test_id",
                table: "test_attempts",
                column: "placement_test_id");

            migrationBuilder.CreateIndex(
                name: "IX_topic_references_reference_source_id",
                table: "topic_references",
                column: "reference_source_id");

            migrationBuilder.CreateIndex(
                name: "UQ_topic_reference",
                table: "topic_references",
                columns: new[] { "topic_id", "reference_source_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__user_pro__B9BE370E7D6D2F0C",
                table: "user_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_sessions_user_id",
                table: "user_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_role_status",
                table: "users",
                columns: new[] { "role_id", "status" });

            migrationBuilder.CreateIndex(
                name: "UQ__users__AB6E6164B7147E19",
                table: "users",
                column: "email",
                unique: true);
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*
            migrationBuilder.DropTable(
                name: "ai_feedbacks");

            migrationBuilder.DropTable(
                name: "ai_generated_contents");

            migrationBuilder.DropTable(
                name: "ai_replanning_events");

            migrationBuilder.DropTable(
                name: "ai_tutor_messages");

            migrationBuilder.DropTable(
                name: "ai_usage_logs");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "competency_skill_scores");

            migrationBuilder.DropTable(
                name: "content_compliance_reviews");

            migrationBuilder.DropTable(
                name: "learning_objectives");

            migrationBuilder.DropTable(
                name: "learning_path_template_nodes");

            migrationBuilder.DropTable(
                name: "notification_reads");

            migrationBuilder.DropTable(
                name: "placement_test_questions");

            migrationBuilder.DropTable(
                name: "quiz_answers");

            migrationBuilder.DropTable(
                name: "quiz_questions");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "report_snapshots");

            migrationBuilder.DropTable(
                name: "search_logs");

            migrationBuilder.DropTable(
                name: "student_progress_snapshots");

            migrationBuilder.DropTable(
                name: "student_skill_preferences");

            migrationBuilder.DropTable(
                name: "study_activity_logs");

            migrationBuilder.DropTable(
                name: "system_settings");

            migrationBuilder.DropTable(
                name: "test_answers");

            migrationBuilder.DropTable(
                name: "topic_references");

            migrationBuilder.DropTable(
                name: "user_profiles");

            migrationBuilder.DropTable(
                name: "user_sessions");

            migrationBuilder.DropTable(
                name: "practice_submissions");

            migrationBuilder.DropTable(
                name: "ai_tutor_conversations");

            migrationBuilder.DropTable(
                name: "ai_prompt_templates");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "placement_test_sections");

            migrationBuilder.DropTable(
                name: "quiz_attempts");

            migrationBuilder.DropTable(
                name: "student_learning_profiles");

            migrationBuilder.DropTable(
                name: "question_options");

            migrationBuilder.DropTable(
                name: "reference_sources");

            migrationBuilder.DropTable(
                name: "learning_path_nodes");

            migrationBuilder.DropTable(
                name: "question_bank");

            migrationBuilder.DropTable(
                name: "original_lessons");

            migrationBuilder.DropTable(
                name: "student_learning_paths");

            migrationBuilder.DropTable(
                name: "quizzes");

            migrationBuilder.DropTable(
                name: "practice_tasks");

            migrationBuilder.DropTable(
                name: "competency_analyses");

            migrationBuilder.DropTable(
                name: "learning_path_templates");

            migrationBuilder.DropTable(
                name: "learning_topics");

            migrationBuilder.DropTable(
                name: "test_attempts");

            migrationBuilder.DropTable(
                name: "learning_goals");

            migrationBuilder.DropTable(
                name: "english_skills");

            migrationBuilder.DropTable(
                name: "placement_tests");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "english_proficiency_levels");

            migrationBuilder.DropTable(
                name: "roles");
            */
        }
    }
}
