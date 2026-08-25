-- API: GET thumbnail status | GetThumbnailBySourceAsync then active thumbnail usage
SELECT j.* FROM media_background_jobs j WHERE j.job_type='THUMBNAIL_DERIVATION' AND j.subject_id=@sourceMediaId LIMIT 2;
SELECT u.media_id FROM media_usages u WHERE u.owner_service='MEDIA' AND u.owner_type='MEDIA_THUMBNAIL' AND u.owner_id=@sourceMediaId AND u.usage_type='THUMBNAIL' AND u.deleted_at IS NULL LIMIT 2;
