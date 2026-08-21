-- Tối ưu danh sách vận hành Media background job theo thứ tự cập nhật ổn định và filter loại job.
CREATE INDEX ix_media_background_jobs_updated_at_id
    ON media_background_jobs (updated_at DESC, id DESC);

CREATE INDEX ix_media_background_jobs_type_status_updated_at_id
    ON media_background_jobs (job_type, status, updated_at DESC, id DESC);
