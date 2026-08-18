# System Handoff Task — Phase 5

## P5-21 — Hoàn thiện README và evidence handoff G4

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 4 giờ |
| Ticket | `Chưa tạo` |
| Loại | Documentation/system handoff |
| Dependency | P5-01..P5-20 |
| Requirement | `docs/development/readme-requirements.md`, G4 |

**Mục tiêu:** Tạo entry point đủ để reviewer dựng, hiểu, kiểm tra và demo hệ
thống từ evidence thật sau khi các implementation task hoàn thành.

**Phạm vi tài liệu:** `README.md`, tài liệu được link trong `docs/development/`,
`docs/guide/`, `docs/tests/` và evidence performance/demo. Không copy contract
chi tiết vào README; dùng link tới source of truth.

**Nội dung README bắt buộc:**

1. Tổng quan dự án.
2. Kiến trúc.
3. Yêu cầu hệ thống và dependency.
4. Cách chạy, trong đó có `docker compose up`.
5. Migration cơ sở dữ liệu.
6. Dữ liệu seed.
7. Các endpoint API.
8. Đánh giá hiệu năng dựa trên raw metrics của batch, CSV, query, index,
   pagination và API; không công bố số liệu chưa chạy.
9. Kịch bản demo.

**Handoff G4:**

- Chạy build/test toàn solution và các integration/E2E suite được bật cho môi
  trường handoff; lưu command, log và kết quả.
- Dựng stack từ trạng thái sạch, migrate, seed, chạy smoke/demo theo README và
  ghi lại lỗi/điều kiện môi trường.
- Kiểm tra mỗi F01–F18 trace được tới runtime source, test evidence và PR/ticket;
  trạng thái `Existing` chỉ được cập nhật khi evidence có thật.
- Tổng hợp coverage, benchmark, testcase checklist và known issue. G4 chỉ Ready
  for Verify khi không còn known bug và toàn bộ testcase approved đã pass.
