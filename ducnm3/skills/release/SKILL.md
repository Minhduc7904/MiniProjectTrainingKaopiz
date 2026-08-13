---
name: release
description: Chuẩn bị và kiểm tra một release/deployment của MiniProject. Dùng khi thay đổi release procedure, build artifact, deployment order hoặc rollback runbook.
---

# Release

## Bắt buộc đọc

1. [`reference.md`](reference.md).
2. [`template.md`](template.md).
3. Runbook liên quan trong `docs/runbooks/`.
4. Development/setup, migration và Docker Compose docs liên quan.

## Workflow

1. Chốt commit/branch/tag và phạm vi artifact.
2. Xác minh migration order, configuration và dependency readiness.
3. Không commit secret, `.env`, token hoặc credential.
4. Chạy build, toàn bộ tests, formatter, Compose validation và security checks
   được project yêu cầu.
5. Ghi deployment order, health checks, monitoring signal và rollback trigger.
6. Cập nhật runbook nếu procedure hoặc recovery behavior thay đổi.
7. Chỉ push/tag/deploy khi user yêu cầu rõ.

## Definition of done

- Verification pass và kết quả được ghi lại.
- Migration/dependency order rõ ràng.
- Rollback procedure khả thi.
- Không có secret hoặc artifact ngoài phạm vi trong commit.
