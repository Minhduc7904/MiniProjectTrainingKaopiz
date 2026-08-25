-- API: POST /api/media/direct-upload/intents | AddPendingAsync
INSERT INTO media_objects (id,bucket,object_key,media_type,content_type,original_file_name,size_bytes,uploaded_by,uploaded_by_type,status,is_draft,created_at,updated_at) VALUES (@mediaId,@bucket,@objectKey,@mediaType,@contentType,@fileName,@size,@actorId,@actorType,'PENDING',1,@now,@now);
