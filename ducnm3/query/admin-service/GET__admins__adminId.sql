-- API: GET /api/admins/{adminId}
-- ConfigurationAdminRepository không dùng ORM/database; dữ liệu admin lấy từ configuration lúc khởi động.
SELECT 'Admin Service uses configuration; no database query exists.' AS note;
