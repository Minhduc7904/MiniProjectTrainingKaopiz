# Testcase — Student

Runtime/test evidence tham khảo:
[Student test catalog](../../../tests/student-service/). Case ghi `Existing`
chỉ khi catalog hiện tại đã mô tả coverage tương ứng.

| TC ID | Trace | Level/status | Setup / Test data | Action | Expected |
| --- | --- | --- | --- | --- | --- |
| TC-STUDENT-F07-001 | F07 AC, BR-STUDENT-02 | Component + integration / Existing | ACTIVE/INACTIVE Student; page 1 | GET list filter ACTIVE | Chỉ ACTIVE, stable order và offset metadata đúng; DB query áp dụng filter. |
| TC-STUDENT-F07-002 | F07 boundary | Component + integration / Existing | Trang đầu/cuối, page size tối đa | GET từng trang | Không trùng/mất record; trang cuối và metadata đúng. |
| TC-STUDENT-F07-003 | F07 negative | Unit + component / Existing | status/sort/page size sai | GET list | `400`; repository không chạy unbounded query. |
| TC-STUDENT-F08-001 | F08 AC | Unit / Existing | Student ID tồn tại | GET detail | `200`, đúng Student và chỉ field được phép lộ. |
| TC-STUDENT-F08-002 | F08 alternative | Component / Designed | Student có optional field null | GET detail | `200`; optional field null theo contract, envelope vẫn hợp lệ. |
| TC-STUDENT-F08-003 | F08 negative | Unit / Existing | UUID malformed và UUID không tồn tại | GET detail | `400` hoặc `404`; không trả profile khác. |

Ngoài kết quả HTTP, integration test phải query database/container riêng để xác
nhận filter/order/page boundary; không dùng database Docker Compose của developer.

