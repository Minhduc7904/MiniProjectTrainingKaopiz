# Testcase — Media

Runtime/test evidence tham khảo:
[Media test catalog](../../../tests/media-service/).

| TC ID | Trace | Level/status | Setup / Test data | Action | Expected |
| --- | --- | --- | --- | --- | --- |
| TC-MEDIA-F09-001 | F09 AC, BR-MEDIA-01/02 | Integration / Existing | Ảnh hợp lệ, actor tồn tại, MinIO chạy | Upload | Media `PENDING → READY`; checksum/object tồn tại; response không lộ bucket/key. |
| TC-MEDIA-F09-002 | F09 alternative, BR-MEDIA-04 | Unit + integration / Designed | Media gốc upload được; thumbnail worker lỗi | Xử lý thumbnail | Media gốc vẫn `READY`/đọc được; thumbnail `FAILED`; retry không tạo derivation trùng. |
| TC-MEDIA-F09-003 | F09 boundary/negative | Unit + component / Existing một phần | File max size, quá cỡ, MIME giả, actor sai | Upload | Biên hợp lệ được nhận; input sai trả `400/413/415/404` trước side effect không phù hợp. |
| TC-MEDIA-F09-004 | F09 direct intent | Unit + component / Existing | Metadata hợp lệ, SHA-256 lowercase, actor tồn tại | Tạo upload intent | `201`, canonical `Location`, draft `PENDING`, policy exact key/type/size/checksum và expiry 900 giây; signed fields không được log. |
| TC-MEDIA-F09-005 | F09 direct complete | Unit + component / Existing | Staging object có size/type/checksum metadata hợp lệ | Complete lần đầu và retry | `READY` draft; replay `200` cùng logical media, tối đa một thumbnail reservation/job. |
| TC-MEDIA-F09-006 | F09 storage boundary | Integration / Existing một phần | Real MinIO source object rồi thay đổi ETag | Promote với ETag cũ | Promotion bị từ chối an toàn. Chưa có test kết hợp real MySQL+MinIO cho concurrent complete. |
| TC-MEDIA-F09-007 | F09 draft migration | Integration / Existing | Legacy media có/không có active usage | Apply V005 | Active usage thành non-draft/null; còn lại draft với `COALESCE(completed_at,created_at)`; index cleanup tồn tại. |
| TC-MEDIA-F09-008 | F09 frontend | Frontend unit / Existing | File hợp lệ/lỗi, runtime mới và reload | Hash/upload/finalize, reset runtime | Progress ba stage đúng; validation/redaction đúng; reload không restore file, intent hay signed fields. |
| TC-MEDIA-F09-009 | F09/F10 frontend preview | Frontend unit / Existing | Upload ảnh có `contentUrl`; thumbnail `QUEUED`/`PROCESSING`/`READY`/`FAILED` | Hiển thị result ở `/media/upload` và `/media/upload-direct` | Ảnh gốc tải qua `contentUrl`; polling chỉ chạy khi job active, dừng khi terminal; thumbnail READY được preview; FAILED gửi đúng retry actor. |
| TC-MEDIA-F10-001 | F10 AC | Component + integration / Existing | Media `READY`, caller được phép | GET content/metadata | Đúng bytes/metadata; không lộ storage location. |
| TC-MEDIA-F10-002 | F10 alternative | Component / Designed | Thumbnail `QUEUED` hoặc `FAILED`, media gốc `READY` | GET thumbnail rồi content gốc | Thumbnail trả trạng thái/lỗi đã chốt; content gốc vẫn truy cập được. |
| TC-MEDIA-F10-003 | F10 negative, NFR-SEC-01 | Component / Designed | ID sai, usage soft-delete hoặc caller không có quyền | GET | `404/403` an toàn; không lộ object key hoặc phân biệt resource của actor khác quá mức cần thiết. |
| TC-MEDIA-F11-001 | F11 AC | Integration / Existing | Avatar A active, B READY | Gán B | A soft-delete, B active; đúng một usage active. |
| TC-MEDIA-F11-002 | F11 alternative, BR-MEDIA-04 | Integration / Existing một phần | Command Notification usage được redeliver | Consume hai lần | Chỉ một active usage theo tuple; side effect count không tăng lần hai. |
| TC-MEDIA-F11-003 | F11 boundary/negative | Unit + integration / Existing một phần | 500 owner/1,000 rows; media not READY; tuple sai | Batch usage | Chunk biên hợp lệ; input sai không tạo partial/duplicate usage ngoài policy. |

MinIO integration test phải dùng bucket/container cô lập. Component test dùng
TestServer và test double, không được gọi MinIO thật.
