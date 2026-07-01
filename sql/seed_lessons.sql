INSERT INTO original_lessons (topic_id, title, summary, content, content_type, estimated_minutes, source_type, review_status, is_ai_generated, created_at, updated_at)
SELECT t.id, 
       'Bài học 1: ' + t.title, 
       'Nội dung cơ bản về ' + t.title, 
       '<p>Đây là nội dung cho bài học ' + t.title + '</p>', 
       'ARTICLE', 15, 'SYSTEM', 'APPROVED', 0, GETUTCDATE(), GETUTCDATE() 
FROM learning_topics t 
LEFT JOIN original_lessons l ON t.id = l.topic_id 
WHERE l.id IS NULL;
