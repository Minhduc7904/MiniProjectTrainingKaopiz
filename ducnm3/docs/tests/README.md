# Test Documentation

Tài liệu này mô tả các test đang được triển khai, theo cùng cách tổ chức với
API documentation:

```text
docs/tests/
├── shared-presentation/
│   ├── unit.md
│   └── component.md
├── api-gateway/
│   └── component.md
└── <service>/
    ├── unit.md
    ├── component.md
    └── integration.md
```

Mỗi tài liệu chỉ liệt kê test hiện có trong source code, mục đích, dữ liệu hoặc
dependency được dùng, và điều kiện pass. Không suy diễn rằng loại test chưa có
đã được triển khai.

## Chạy test

```bash
dotnet test backend/Lms.sln -m:1
```

Chạy một project cụ thể theo đường dẫn được ghi trong tài liệu của service. Test
integration dùng Testcontainers cần Docker Engine đang chạy. Xem
[`../development/testing.md`](../development/testing.md) để biết quy ước chung
và cách thêm test mới.
