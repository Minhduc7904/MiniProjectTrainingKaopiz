-- API: GET /api/media/{mediaId}/content | GetByIdAsync
SELECT m.id,m.bucket,m.object_key,m.media_type,m.content_type,m.original_file_name,m.size_bytes,m.status,m.created_at,m.deleted_at FROM media_objects m WHERE m.id=@mediaId LIMIT 2;
