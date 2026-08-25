-- API: DELETE media usage | RemoveAsync
SELECT u.* FROM media_usages u WHERE u.id=@usageId AND u.deleted_at IS NULL LIMIT 2;
SELECT EXISTS(SELECT 1 FROM media_usages u WHERE u.media_id=@mediaId AND u.deleted_at IS NULL AND u.id<>@usageId);
-- Only when no active usage remains.
SELECT m.* FROM media_objects m WHERE m.id=@mediaId LIMIT 2;
UPDATE media_usages SET deleted_at=@now WHERE id=@usageId;
UPDATE media_objects SET is_draft=1,drafted_at=@now WHERE id=@mediaId;
