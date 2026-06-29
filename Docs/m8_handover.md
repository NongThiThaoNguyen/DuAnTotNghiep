# M8 Handover — AI Learning Path Engine

## Routes

- Student: `GET /Student/LearningPath/Generate`, `POST /Student/LearningPath/Generate`, `GET /Student/LearningPath/Summary`, `GET /Student/LearningPath/Detail/{id}`, `POST /Student/LearningPath/Regenerate`.
- Admin: `GET /Admin/LearningPaths/PathHistory`, `GET /Admin/LearningPaths/PathDetail/{id}`, `GET /Admin/LearningPaths/GenerationLogs`.
- Admin templates: `GET/POST /Admin/PathTemplates/Create`, `GET/POST /Admin/PathTemplates/Edit/{id}`, `GET /Admin/PathTemplates/Details/{id}`, `POST /Admin/PathTemplates/Publish/{id}`, `POST /Admin/PathTemplates/Archive/{id}`.

## Controllers

- `Areas/Student/Controllers/LearningPathController.cs`: generation readiness, initial generation, summary/detail, regenerate, and ownership-safe node opening.
- `Areas/Admin/Controllers/LearningPathsController.cs`: path history, path detail, AI generation logs.
- `Areas/Admin/Controllers/PathTemplatesController.cs`: fallback template CRUD, publish, archive.

## Services

- `ILearningPathEngineService` / `LearningPathEngineService`: builds AI input, generates paths, maps nodes, archives/regenerates, unlocks next node, enforces ownership.
- `ILearningPathAiService` / `LearningPathAiService`: mock AI generation and output validation, ready to replace with real Gemini/API call.
- `ILearningPathComplianceService` / `LearningPathComplianceService`: checks active/published/approved content and blocks `REFERENCE_ONLY` content.
- `ILearningPathRepository` / `LearningPathRepository`: active path lookup, path detail/history, paged admin list, add/update path/nodes.

## Models

- `StudentLearningPath`: stores student path metadata, status, version, archive/replacement links, AI summary.
- `LearningPathNode`: stores ordered path nodes, typed content references, status, AI reason, prerequisite link.
- `LearningPathTemplate` / `LearningPathTemplateNode`: admin-managed fallback templates used when AI generation fails.
- `AiUsageLog`: records `LEARNING_PATH` AI attempts with prompt template, model, token estimates, status, and errors.
- `AiReplanningEvent`: records regenerate reason and old/new summaries.

## ViewModels

- Student: `LearningPathGenerateViewModel`, `LearningPathSummaryViewModel`, `LearningPathDetailViewModel`.
- Admin path: `PathHistoryViewModel`, `PathDetailAdminViewModel`, `GenerationLogViewModel`.
- Admin template: `PathTemplateViewModel`, `CreatePathTemplateViewModel`, `EditPathTemplateViewModel`.

## AI Output Schema

`LearningPathOutputDto` contains `PathTitle`, `Summary`, `TotalWeeks`, and `Phases`.

Each phase contains `PhaseName`, `Weeks`, and `Nodes`.

Each node contains title, description, `ActionType`, optional `TopicId` / `LessonId` / `QuizId` / `PracticeTaskId`, estimated minutes, AI reason, scheduled day, and phase.

## DB Mapping

- Output title and summary map to `student_learning_paths.title`, `description`, and `ai_plan_summary`.
- Output total weeks maps to `target_end_date`.
- Output nodes map to `learning_path_nodes` in sequential `order_index`.
- First node becomes `AVAILABLE`; later nodes become `LOCKED`.
- Node `AiReason` maps to `learning_path_nodes.ai_reason`.
- Regenerate archives the previous path, increments `path_version`, and writes `replaced_by_path_id`.

## Seed Data

`DatabaseSeeder.SeedM8LearningPathAssetsAsync()` creates:

- AI prompt template `M8_LEARNING_PATH_V1` for module `LEARNING_PATH`.
- Published fallback templates `M8 Foundation Template` and `M8 Practice Template`.

## Downstream Usage

- M9 consumes active path/nodes for the student learning path UI and node opening.
- M10 can use completed/available nodes as study activity targets.
- M12 can use `LearningPathNode` status and `AiReason` for progress/recommendation displays.
- M16 can use AI usage logs, compliance status, and path history for admin analytics/audit.
