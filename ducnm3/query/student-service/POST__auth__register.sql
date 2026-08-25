-- API: POST /api/auth/register | GetByNormalizedEmailAsync then CreateAsync
SELECT s.* FROM students s WHERE s.email = @normalizedEmail LIMIT 2;
INSERT INTO students (id, email, display_name, status, created_at, updated_at) VALUES (@studentId, @normalizedEmail, @displayName, 'ACTIVE', @now, @now);
