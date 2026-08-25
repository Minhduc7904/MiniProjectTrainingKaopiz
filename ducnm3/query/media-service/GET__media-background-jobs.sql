-- API: GET media background jobs | ListAsync
SELECT COUNT(*) FROM media_background_jobs j WHERE (@jobType IS NULL OR j.job_type=@jobType) AND (@status IS NULL OR j.status=@status) AND (@correlationId IS NULL OR j.correlation_id=@correlationId);
SELECT j.* FROM media_background_jobs j WHERE (@jobType IS NULL OR j.job_type=@jobType) AND (@status IS NULL OR j.status=@status) AND (@correlationId IS NULL OR j.correlation_id=@correlationId) ORDER BY j.updated_at DESC,j.id DESC LIMIT @pageSize OFFSET @offset;
