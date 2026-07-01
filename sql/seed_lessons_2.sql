INSERT INTO original_lessons (topic_id, title, summary, content, content_type, estimated_minutes, source_type, review_status, is_ai_generated, created_at, updated_at)
SELECT t.id, 
       'Bài học 2: Nâng cao ' + t.title, 
       'Nội dung nâng cao về ' + t.title, 
       '<p>Đây là bài học nâng cao cho chủ đề ' + t.title + '</p>', 
       'ARTICLE', 20, 'SYSTEM', 'APPROVED', 0, GETUTCDATE(), GETUTCDATE() 
FROM learning_topics t 
WHERE (SELECT COUNT(*) FROM original_lessons WHERE topic_id = t.id) < 2;

INSERT INTO original_lessons (topic_id, title, summary, content, content_type, estimated_minutes, source_type, review_status, is_ai_generated, created_at, updated_at)
SELECT t.id, 
       'Bài học 3: Thực hành ' + t.title, 
       'Thực hành các kiến thức về ' + t.title, 
       '<p>Bài tập thực hành cho chủ đề ' + t.title + '</p>', 
       'ARTICLE', 25, 'SYSTEM', 'APPROVED', 0, GETUTCDATE(), GETUTCDATE() 
FROM learning_topics t 
WHERE (SELECT COUNT(*) FROM original_lessons WHERE topic_id = t.id) < 3;
