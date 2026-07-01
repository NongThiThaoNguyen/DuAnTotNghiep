DELETE FROM original_lessons WHERE source_type = 'SYSTEM' AND is_ai_generated = 0 AND created_by IS NULL;

INSERT INTO original_lessons (topic_id, title, summary, content, content_type, estimated_minutes, source_type, review_status, is_ai_generated, created_at, updated_at)
SELECT t.id, 
       N'Bài học 1: ' + t.title, 
       N'Nội dung cơ bản về ' + t.title, 
       N'<p>Đây là nội dung cho bài học ' + t.title + N'</p>', 
       'ARTICLE', 15, 'SYSTEM', 'APPROVED', 0, GETUTCDATE(), GETUTCDATE() 
FROM learning_topics t 
WHERE (SELECT COUNT(*) FROM original_lessons WHERE topic_id = t.id) = 0;

INSERT INTO original_lessons (topic_id, title, summary, content, content_type, estimated_minutes, source_type, review_status, is_ai_generated, created_at, updated_at)
SELECT t.id, 
       N'Bài học 2: Nâng cao ' + t.title, 
       N'Nội dung nâng cao về ' + t.title, 
       N'<p>Đây là bài học nâng cao cho chủ đề ' + t.title + N'</p>', 
       'ARTICLE', 20, 'SYSTEM', 'APPROVED', 0, GETUTCDATE(), GETUTCDATE() 
FROM learning_topics t 
WHERE (SELECT COUNT(*) FROM original_lessons WHERE topic_id = t.id) = 1;

INSERT INTO original_lessons (topic_id, title, summary, content, content_type, estimated_minutes, source_type, review_status, is_ai_generated, created_at, updated_at)
SELECT t.id, 
       N'Bài học 3: Thực hành ' + t.title, 
       N'Thực hành các kiến thức về ' + t.title, 
       N'<p>Bài tập thực hành cho chủ đề ' + t.title + N'</p>', 
       'ARTICLE', 25, 'SYSTEM', 'APPROVED', 0, GETUTCDATE(), GETUTCDATE() 
FROM learning_topics t 
WHERE (SELECT COUNT(*) FROM original_lessons WHERE topic_id = t.id) = 2;
