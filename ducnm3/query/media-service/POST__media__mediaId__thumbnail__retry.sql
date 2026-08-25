-- API: POST thumbnail retry | RetryAsync
SELECT j.* FROM media_background_jobs j WHERE j.job_type='THUMBNAIL_DERIVATION' AND j.subject_id=@sourceMediaId LIMIT 2;
SELECT m.* FROM media_objects m WHERE m.id=@sourceMediaId LIMIT 2;
UPDATE media_background_jobs SET status='QUEUED',failed_item_count=0,last_error=NULL,completed_at=NULL,updated_at=@now WHERE id=@jobId;
