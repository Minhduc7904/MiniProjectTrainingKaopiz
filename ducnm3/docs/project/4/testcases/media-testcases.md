# Testcase — Media

Runtime/test evidence tham khảo:
[Media test catalog](../../../tests/media-service/).

| TC ID | Trace | Level/status | Setup / Test data | Action | Expected |
| --- | --- | --- | --- | --- | --- |
| TC-MEDIA-F09-001 | F09 AC, BR-MEDIA-01/02 | Integration / Existing | Ảnh hợp lệ, actor tồn tại, MinIO chạy | Upload | Media `PENDING → READY`; checksum/object tồn tại; response không lộ bucket/key. |
| TC-MEDIA-F09-002 | F09 alternative, BR-MEDIA-04 | Unit + integration / Designed | Media gốc upload được; thumbnail worker lỗi | Xử lý thumbnail | Media gốc vẫn `READY`/đọc được; thumbnail `FAILED`; retry không tạo derivation trùng. |
| TC-MEDIA-F09-003 | F09 boundary/negative | Unit + component / Existing một phần | File max size, quá cỡ, MIME giả, actor sai | Upload | Biên hợp lệ được nhận; input sai trả `400/413/415/404` trước side effect không phù hợp. |
| TC-MEDIA-F10-001 | F10 AC | Component + integration / Existing | Media `READY`, caller được phép | GET content/metadata | Đúng bytes/metadata; không lộ storage location. |
| TC-MEDIA-F10-002 | F10 alternative | Component / Designed | Thumbnail `QUEUED` hoặc `FAILED`, media gốc `READY` | GET thumbnail rồi content gốc | Thumbnail trả trạng thái/lỗi đã chốt; content gốc vẫn truy cập được. |
| TC-MEDIA-F10-003 | F10 negative, NFR-SEC-01 | Component / Designed | ID sai, usage soft-delete hoặc caller không có quyền | GET | `404/403` an toàn; không lộ object key hoặc phân biệt resource của actor khác quá mức cần thiết. |
| TC-MEDIA-F11-001 | F11 AC | Integration / Existing | Avatar A active, B READY | Gán B | A soft-delete, B active; đúng một usage active. |
| TC-MEDIA-F11-002 | F11 alternative, BR-MEDIA-04 | Integration / Existing một phần | Command Notification usage được redeliver | Consume hai lần | Chỉ một active usage theo tuple; side effect count không tăng lần hai. |
| TC-MEDIA-F11-003 | F11 boundary/negative | Unit + integration / Existing một phần | 500 owner/1,000 rows; media not READY; tuple sai | Batch usage | Chunk biên hợp lệ; input sai không tạo partial/duplicate usage ngoài policy. |

MinIO integration test phải dùng bucket/container cô lập. Component test dùng
TestServer và test double, không được gọi MinIO thật.

