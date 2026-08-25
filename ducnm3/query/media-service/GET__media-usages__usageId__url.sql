-- API: GET usage URL | ActiveImageUsages projection
SELECT u.*,m.*,t.id AS thumbnail_id,t.bucket AS thumbnail_bucket,t.object_key AS thumbnail_object_key FROM media_usages u INNER JOIN media_objects m ON m.id=u.media_id LEFT JOIN media_objects t ON t.source_media_id=m.id AND t.derivation_type='THUMBNAIL' AND t.deleted_at IS NULL WHERE u.id=@usageId AND u.deleted_at IS NULL LIMIT 2;
