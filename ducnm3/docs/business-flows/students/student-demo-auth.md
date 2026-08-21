# Demo Student identity không JWT

API contract: [register](../../api/student-service/endpoints/post-student-auth-register.md),
[login](../../api/student-service/endpoints/post-student-auth-login.md),
[me](../../api/student-service/endpoints/get-student-auth-me.md).

## Luồng

```mermaid
sequenceDiagram
    participant Browser as Student browser
    participant API as Student Service
    participant DB as MySQL Student

    Browser->>API: register(email, displayName) hoặc login(id)
    API->>DB: create hoặc lookup ACTIVE student
    DB-->>API: student
    API-->>Browser: actor=STUDENT, id
    Browser->>Browser: lưu Student local storage
    Browser->>API: GET me + actor headers
    API->>DB: validate Student tồn tại và ACTIVE
    API-->>Browser: profile hoặc lỗi
```

Frontend chỉ tự điều hướng vào trang loading khi đã có `actor=STUDENT` và UUID.
Loading gọi `me`; lỗi sẽ xóa Student local storage và đưa người dùng về login.
Admin storage độc lập, vì vậy logout Student không ảnh hưởng Admin.
