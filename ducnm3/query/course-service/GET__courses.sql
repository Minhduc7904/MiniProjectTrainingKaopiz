-- API: GET /api/courses | EfCourseListRepository.GetPagedAsync
-- Query 1: LongCountAsync after status/search filter.
SELECT COUNT(*) FROM courses AS c
WHERE (@status IS NULL OR c.status = @status)
  AND (@search IS NULL OR c.name LIKE CONCAT('%', @search, '%'));
-- Query 2: ToListAsync. ORDER BY changes with the allowlisted sortBy/direction.
SELECT c.id, c.name, c.status, c.created_at FROM courses AS c
WHERE (@status IS NULL OR c.status = @status)
  AND (@search IS NULL OR c.name LIKE CONCAT('%', @search, '%'))
ORDER BY c.created_at DESC, c.id DESC LIMIT @pageSize OFFSET @offset;
