-- API: POST /api/media/direct-upload/{mediaId}/complete | GetByIdAsync then MarkReadyAsync
SELECT m.* FROM media_objects m WHERE m.id=@mediaId LIMIT 2;
SELECT m.* FROM media_objects m WHERE m.id=@mediaId LIMIT 2;
UPDATE media_objects SET status='READY',checksum_sha256=@checksum,completed_at=@now,updated_at=@now,failure_reason=NULL WHERE id=@mediaId;
