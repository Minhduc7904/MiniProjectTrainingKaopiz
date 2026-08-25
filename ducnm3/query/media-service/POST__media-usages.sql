-- API: POST /api/media-usages | CreateMediaUsageHandler; actor validation is external
SELECT m.* FROM media_objects m WHERE m.id=@mediaId LIMIT 2;
-- EnsureCourseOriginalImage/EnsureCourseLessonMedia branch may issue this readiness query.
SELECT COUNT(*) FROM media_objects m WHERE m.id IN (@mediaIds) AND m.status='READY' AND m.deleted_at IS NULL AND m.source_media_id IS NULL;
START TRANSACTION;
SELECT m.id FROM media_objects m WHERE m.id IN (@mediaIds) AND m.status='READY' AND m.deleted_at IS NULL;
SELECT u.media_id,u.owner_service,u.owner_type,u.owner_id,u.usage_type FROM media_usages u WHERE u.deleted_at IS NULL AND u.media_id IN (@mediaIds) AND u.owner_id IN (@ownerIds);
SELECT m.* FROM media_objects m WHERE m.id IN (@mediaIds) AND m.deleted_at IS NULL;
INSERT INTO media_usages (id,media_id,owner_service,owner_type,owner_id,usage_type,display_order,created_by,created_by_type,created_at) VALUES (@usageId,@mediaId,@ownerService,@ownerType,@ownerId,@usageType,@displayOrder,@actorId,@actorType,@now);
UPDATE media_objects SET is_draft=0,drafted_at=NULL WHERE id IN (@mediaIds);
COMMIT;
