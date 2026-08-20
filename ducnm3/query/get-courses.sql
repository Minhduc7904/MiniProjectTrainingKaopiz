SELECT
    status,
    COUNT(*) AS row_count,
    ROUND(
        COUNT(*) * 100.0 / SUM(COUNT(*)) OVER (),
        2
    ) AS percentage
FROM courses
GROUP BY status
ORDER BY row_count DESC;

EXPLAIN ANALYZE
SELECT
    id,
    name,
    status,
    created_at
FROM courses
WHERE status = 'PUBLISHED'
ORDER BY created_at DESC
LIMIT 20 OFFSET 0;

EXPLAIN ANALYZE
SELECT
    id,
    name,
    status,
    created_at
FROM courses
WHERE status = 'ARCHIVED'
ORDER BY created_at DESC
LIMIT 20 OFFSET 0;

EXPLAIN ANALYZE
SELECT
    id,
    name,
    status,
    created_at
FROM courses IGNORE INDEX (ix_courses_status_created_at)
WHERE status = 'ARCHIVED'
ORDER BY created_at DESC
LIMIT 20;

EXPLAIN ANALYZE
SELECT
    id,
    name,
    status,
    created_at
FROM courses
WHERE status = 'ARCHIVED'
ORDER BY created_at DESC
LIMIT 20;

EXPLAIN ANALYZE
SELECT
    id,
    name,
    status,
    created_at
FROM courses IGNORE INDEX (ix_courses_status_created_at)
WHERE status = 'PUBLISHED'
ORDER BY created_at DESC
LIMIT 20;

EXPLAIN ANALYZE
SELECT
    id,
    name,
    status,
    created_at
FROM courses
WHERE status = 'PUBLISHED'
ORDER BY created_at DESC
LIMIT 20;



