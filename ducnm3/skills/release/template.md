# Release Template

```text
Branch/commit/tag:
Release scope:
Artifacts:
Migration order:
Dependency order:
Configuration changes:
Health checks:
Monitoring signals:
Rollback trigger:
Rollback steps:
Owner:
```

## Checklist

```text
- [ ] Đã đọc release skill/reference/template và runbook.
- [ ] Build Release pass.
- [ ] Toàn bộ test liên quan pass.
- [ ] Formatter/lint/whitespace pass.
- [ ] Docker Compose config hợp lệ.
- [ ] Migration history/checksum hợp lệ.
- [ ] Không có secret trong diff.
- [ ] Deployment order và rollback đã được review.
- [ ] Chỉ thực hiện external action đã được user cho phép.
```
