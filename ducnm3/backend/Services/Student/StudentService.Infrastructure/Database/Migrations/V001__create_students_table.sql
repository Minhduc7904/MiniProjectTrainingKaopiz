CREATE TABLE students (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh Student',
    email VARCHAR(320) NOT NULL COMMENT 'Email đăng nhập hoặc liên hệ; unique',
    display_name VARCHAR(200) NOT NULL COMMENT 'Tên hiển thị của Student',
    status VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'ACTIVE | INACTIVE | BLOCKED',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm tạo Student, UTC',
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm cập nhật Student gần nhất, UTC',
    CONSTRAINT pk_students PRIMARY KEY (id),
    CONSTRAINT uq_students_email UNIQUE (email),
    CONSTRAINT chk_students_status CHECK (status IN ('ACTIVE', 'INACTIVE', 'BLOCKED')),
    INDEX ix_students_status_created_at (status, created_at DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
