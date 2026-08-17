# Requirements — Student

## Quy tắc chung

- `BR-STUDENT-01`: Student Service là nguồn dữ liệu Học viên; service khác chỉ
  tra cứu qua boundary đã công bố.
- `BR-STUDENT-02`: List query phải validate trạng thái, sort và pagination trước
  khi truy xuất.
- `BR-STUDENT-03`: Dữ liệu seed chỉ phục vụ môi trường phát triển và không là
  chức năng quản trị production.

## F07 — Tra cứu danh sách Học viên

| Nội dung | Yêu cầu |
| --- | --- |
| Purpose / Actor | Admin/service nội bộ tra cứu Học viên theo tiêu chí được phép. |
| Preconditions | Caller được xác thực theo boundary; query hợp lệ. |
| Input / Output | Filter trạng thái, sort, pagination / danh sách nhất quán và metadata trang. |
| Main flow | Validate → lọc/sắp xếp → phân trang → trả danh sách. |
| Alternative flow | Query hợp lệ không có Học viên phù hợp hoặc đi đến trang cuối trả danh sách rỗng cùng metadata nhất quán. |
| Error cases | Filter, sort hoặc pagination không hợp lệ; dependency/database lỗi. |
| AC | Given tập Học viên, when query `ACTIVE` theo trang, then chỉ trả record ACTIVE và metadata đúng; when query sai, then không chạy truy vấn không giới hạn. |
| Cases | Happy: list ACTIVE. Boundary: trang đầu/cuối, page size tối đa. Negative: status/sort/page size sai. |

## F08 — Tra cứu chi tiết Học viên

| Nội dung | Yêu cầu |
| --- | --- |
| Purpose / Actor | Client/service nội bộ lấy dữ liệu Học viên theo định danh. |
| Preconditions | Định danh hợp lệ; caller có quyền theo boundary. |
| Input / Output | Student ID / hồ sơ Học viên hoặc không tìm thấy. |
| Main flow | Validate ID → đọc Học viên → trả dữ liệu được phép lộ. |
| Alternative flow | Học viên tồn tại nhưng trường hồ sơ tùy chọn chưa có dữ liệu vẫn trả hồ sơ hợp lệ với các trường đó để trống/null theo contract. |
| Error cases | ID không hợp lệ; không tồn tại; không có quyền; dependency lỗi. |
| AC | Given Học viên tồn tại, when caller hợp lệ tra cứu, then trả đúng Học viên; when ID không tồn tại, then không trả profile khác. |
| Cases | Happy: lấy một Học viên. Boundary: UUID hợp lệ biên. Negative: UUID sai, không tồn tại, caller không hợp lệ. |
