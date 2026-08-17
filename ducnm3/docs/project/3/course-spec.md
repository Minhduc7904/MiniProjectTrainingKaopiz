# Requirements — Course

## Quy tắc chung

- `BR-COURSE-01`: Chỉ quản trị viên được quản lý Khóa học/Bài học; Học viên chỉ
  truy cập nội dung đủ điều kiện theo trạng thái và ghi danh.
- `BR-COURSE-02`: Khóa học chỉ được ghi danh khi ở trạng thái `PUBLISHED`.
- `BR-COURSE-03`: `display_order` của Bài học là duy nhất trong một Khóa học.
- `BR-COURSE-04`: Dữ liệu Course không tạo foreign key hay query trực tiếp sang
  database service khác.

## F01 — Quản lý Khóa học

| Nội dung | Yêu cầu |
| --- | --- |
| Purpose / Actor | Quản trị viên tạo, cập nhật và xuất bản Khóa học. |
| Preconditions | Actor có quyền quản lý; tên và Markdown hợp lệ. |
| Input / Output | Thông tin Khóa học và thao tác trạng thái / bản ghi Khóa học hợp lệ hoặc lỗi nghiệp vụ. |
| Main flow | Validate → tạo/sửa bản nháp → bổ sung Bài học → kiểm tra điều kiện → xuất bản. |
| Error cases | Thiếu dữ liệu; không có quyền; Khóa học không tồn tại; không đủ điều kiện xuất bản. |
| AC | Given admin hợp lệ, when tạo Khóa học hợp lệ, then có Khóa học `DRAFT`; when xuất bản thiếu điều kiện, then bị từ chối với lỗi có thể xử lý. |
| Cases | Happy: tạo và xuất bản hợp lệ. Boundary: tên/Markdown tại giới hạn. Negative: tên rỗng, Markdown không an toàn, actor không có quyền. |

## F02 — Quản lý Bài học

| Nội dung | Yêu cầu |
| --- | --- |
| Purpose / Actor | Quản trị viên thêm và duy trì Bài học thuộc một Khóa học. |
| Preconditions | Khóa học tồn tại; actor có quyền; thứ tự hiển thị hợp lệ. |
| Input / Output | Nội dung Bài học, `display_order` / Bài học được lưu hoặc lỗi. |
| Main flow | Validate → kiểm tra Khóa học và thứ tự → lưu Bài học → phản hồi kết quả. |
| Error cases | Khóa học không tồn tại; thứ tự trùng; dữ liệu Bài học không hợp lệ. |
| AC | Given Khóa học hợp lệ, when thêm Bài học với thứ tự chưa dùng, then Bài học được gắn đúng Khóa học; when thứ tự trùng, then không tạo dữ liệu trùng. |
| Cases | Happy: thêm Bài học. Boundary: Bài học đầu/cuối. Negative: courseId sai, order trùng, không có quyền. |

## F03 — Tra cứu Khóa học

| Nội dung | Yêu cầu |
| --- | --- |
| Purpose / Actor | Client tra cứu danh sách, chi tiết và thông tin tổng hợp Khóa học. |
| Preconditions | Query hợp lệ; Học viên chỉ được thấy nội dung được phép. |
| Input / Output | Tiêu chí lọc/phân trang hoặc định danh / danh sách, chi tiết hoặc không tìm thấy. |
| Main flow | Validate query → đọc dữ liệu cần thiết → áp dụng quyền hiển thị → trả kết quả. |
| Error cases | Định danh không tồn tại; query không hợp lệ; truy cập nội dung không được phép. |
| AC | Given Khóa học tồn tại và actor đủ quyền, when tra cứu, then trả đúng dữ liệu; when không tồn tại, then không trả dữ liệu của Khóa học khác. |
| Cases | Happy: list/detail. Boundary: trang rỗng, cursor cuối. Negative: filter/cursor sai, resource không tồn tại. |

## F04 — Ghi danh Khóa học

| Nội dung | Yêu cầu |
| --- | --- |
| Purpose / Actor | Học viên `ACTIVE` ghi danh một Khóa học `PUBLISHED`. |
| Preconditions | Học viên xác thực và `ACTIVE`; Khóa học tồn tại, được xuất bản. |
| Input / Output | Danh tính Học viên và Khóa học / lượt ghi danh hoặc lý do bị từ chối. |
| Main flow | Xác minh Học viên → kiểm tra điều kiện → tạo lượt ghi danh hoạt động. |
| Error cases | Chưa xác thực; không đủ điều kiện; Khóa học không tồn tại; đã ghi danh. |
| AC | Given Học viên ACTIVE chưa ghi danh, when ghi danh Khóa học PUBLISHED, then có đúng một lượt ghi danh; when gửi lại, then không tạo bản ghi thứ hai. |
| Cases | Happy: ghi danh mới. Boundary: Khóa học vừa đổi trạng thái. Negative: student inactive, course DRAFT, ghi danh trùng. |

## F05 — Cập nhật tiến độ Bài học

| Nội dung | Yêu cầu |
| --- | --- |
| Purpose / Actor | Học viên ghi nhận tiến độ Bài học thuộc lượt ghi danh của mình. |
| Preconditions | Học viên đã ghi danh; Bài học thuộc Khóa học đó. |
| Input / Output | Lesson, Học viên, phần trăm tiến độ / tiến độ đã cập nhật. |
| Main flow | Kiểm tra quyền → tạo/cập nhật tiến độ → đặt thời điểm hoàn thành ở 100%. |
| Error cases | Không có quyền; Bài học/ghi danh không tồn tại; giá trị tiến độ không hợp lệ. |
| AC | Given Học viên đã ghi danh, when gửi 100%, then tiến độ hoàn thành có thời điểm hoàn thành; when Học viên khác gửi, then không được sửa dữ liệu. |
| Cases | Happy: upsert tiến độ. Boundary: 0% và 100%. Negative: ngoài khoảng, lesson ngoài Khóa học, chưa ghi danh. |

## F06 — Xuất danh sách Khóa học CSV

| Nội dung | Yêu cầu |
| --- | --- |
| Purpose / Actor | Quản trị viên xuất danh sách Khóa học phục vụ quản lý/benchmark. |
| Preconditions | Actor có quyền; tiêu chí xuất hợp lệ. |
| Input / Output | Bộ lọc xuất / tệp CSV hợp lệ hoặc lỗi. |
| Main flow | Validate → truy xuất tập dữ liệu → tạo response CSV → hoàn tất hoặc hủy theo request. |
| Error cases | Không có quyền; filter không hợp lệ; request bị hủy; dependency lỗi. |
| AC | Given dữ liệu hợp lệ, when xuất CSV, then tệp chứa header và đúng dữ liệu theo filter; when dataset lớn, then không yêu cầu giữ toàn bộ record và chuỗi CSV trong memory. |
| Cases | Happy: xuất dữ liệu nhỏ. Boundary: 100k+ record. Negative: filter sai, hủy request, lỗi truy xuất. |
