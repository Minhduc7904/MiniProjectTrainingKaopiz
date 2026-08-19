using System;
using System.Collections.Generic;

namespace StudentService.Infrastructure.Persistence.Scaffolded;

public partial class Student
{
    /// <summary>
    /// UUID định danh Student
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Email đăng nhập hoặc liên hệ; unique
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Tên hiển thị của Student
    /// </summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// ACTIVE | INACTIVE | BLOCKED
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Thời điểm tạo Student, UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm cập nhật Student gần nhất, UTC
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
