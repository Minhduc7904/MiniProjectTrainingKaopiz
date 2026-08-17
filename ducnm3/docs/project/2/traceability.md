# Lead-level Requirement Traceability

| Need cấp lead | Evidence/artefact hiện có | Phase tiếp theo |
| --- | --- | --- |
| Service boundary và ownership | `docs/architecture/`, `docs/database/` | Requirements/Design xác nhận AC. |
| API/business flow | `docs/api/`, `docs/business-flows/` | Requirements hoàn thiện spec chi tiết. |
| Environment | `docker-compose.yml`, `docs/development/docker.md` | Design/Test xác nhận config/case. |
| Git/workspace | `rules/git.md`, `docs/guide/GIT_GUIDE.md` | Development thực thi ticket/PR. |
| Quality/testing | `docs/development/testing.md`, `docs/tests/` | Testcase/Verify bổ sung test case cụ thể. |
| Performance | `docs/development/performance.md` | Design/Development triển khai benchmark và ghi số đo. |

## Definition of Done Phase 0–2

- Scope, constraint, assumption, dependency và risk đã có owner/evidence.
- Architecture direction chỉ ở high-level; không thay Basic Design/API sequence.
- Capability Planned được tách khỏi runtime hiện tại.
- Các phase sau có link artifact và milestone rõ ràng.
- Không tạo Spec Function hoặc testcase chi tiết trong Phase 0–2.
