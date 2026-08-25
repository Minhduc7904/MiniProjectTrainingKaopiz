-- API: GET notification media usage job | GetByIdAsync
SELECT j.* FROM notification_media_usage_jobs j WHERE j.id=@jobId LIMIT 2;
