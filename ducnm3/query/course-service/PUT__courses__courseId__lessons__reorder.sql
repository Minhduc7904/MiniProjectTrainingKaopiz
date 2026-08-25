-- API: PUT /api/courses/{courseId}/lessons/reorder | ReorderAsync
START TRANSACTION;
SELECT l.id, l.course_id, l.display_order FROM lessons l WHERE l.course_id = @courseId;
-- SaveChangesAsync #1 emits one UPDATE per tracked lesson.
UPDATE lessons SET display_order = display_order + 1000000 WHERE course_id = @courseId;
-- SaveChangesAsync #2 emits one UPDATE per tracked lesson; CASE is a readable batch equivalent.
UPDATE lessons SET display_order = CASE id WHEN @lessonId1 THEN 1 WHEN @lessonId2 THEN 2 END WHERE id IN (@lessonIds);
COMMIT;
