/* Seed prompt template for QUIZ_GENERATION_ENGLISH_V1
   Safe insert: only insert if not exists by prompt_code
*/

SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM dbo.ai_prompt_templates WHERE prompt_code = 'QUIZ_GENERATION_ENGLISH_V1')
BEGIN
    INSERT INTO dbo.ai_prompt_templates (prompt_code, prompt_name, system_prompt, output_schema, version_no, status, module_code, created_at)
    VALUES (
        'QUIZ_GENERATION_ENGLISH_V1',
        'Quiz generation (English) v1',
        N'System: You are an expert English teacher. Generate a single quiz item in JSON following the schema exactly. Provide clear correct answer index and brief explanation. Keep language appropriate for learners.',
        N'{
            "type": "object",
            "properties": {
                "question_text": { "type": "string" },
                "options": { "type": "array", "items": { "type": "string" } },
                "correct_answer_index": { "type": "integer" },
                "explanation": { "type": "string" },
                "difficulty": { "type": "string" },
                "skill_tags": { "type": "array", "items": { "type": "string" } }
            },
            "required": ["question_text","options","correct_answer_index"]
        }',
        1,
        'ACTIVE',
        'M14_QUIZ_GENERATION',
        SYSUTCDATETIME()
    );
END

PRINT 'Seeded QUIZ_GENERATION_ENGLISH_V1 if not exists.';
