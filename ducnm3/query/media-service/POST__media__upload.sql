-- API: POST /api/media/upload | AddPendingAsync then finalizer
INSERT INTO media_objects (id,bucket,object_key,media_type,content_type,original_file_name,size_bytes,uploaded_by,uploaded_by_type,status,is_draft,created_at,updated_at) VALUES (@mediaId,@bucket,@objectKey,@mediaType,@contentType,@fileName,@size,@actorId,@actorType,'PENDING',1,@now,@now);
SELECT m.* FROM media_objects m WHERE m.id=@mediaId LIMIT 2;
UPDATE media_objects SET status='READY',checksum_sha256=@checksum,completed_at=@now,updated_at=@now,failure_reason=NULL WHERE id=@mediaId;
