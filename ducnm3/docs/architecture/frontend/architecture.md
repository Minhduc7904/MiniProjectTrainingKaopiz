# Kiến trúc Frontend

Frontend là React SPA gọi Backend qua YARP Gateway; không gọi microservice trực tiếp.
Route gốc `/` chuyển tới `/student/login`; các route quản trị và Student còn lại
được React Router xử lý ở client. Khi deploy Vercel, `frontend/lms-web/vercel.json`
rewrite mọi deep link về `index.html` để reload không nhận `404` từ host.

~~~mermaid
flowchart LR
  Page --> Hook --> Redux[Redux Toolkit]
  Hook --> Api[Axios client]
  Api --> Gateway[YARP Gateway]
  Redux --> Components
~~~

## Các tầng

- **Page:** composition Workbench Input/Output, không gọi Axios.
- **Hook:** entry point load/mutate của Page.
- **Redux feature:** data, pagination, loading, success, error và trace ID.
- **API:** Axios client, route, envelope mapping và interceptor.
- **UI/layout:** chỉ nhận props, không biết Redux/API.

Với result Media, `useMediaThumbnail` sở hữu vòng đời polling ngắn hạn và hủy
timer/request khi media đổi, Reset hoặc unmount. API layer đọc `thumbnailStatusUrl`
do backend trả về và gọi retry bằng `mediaId`; `MediaImagePreview` lấy binary
qua `GET contentUrl` của `httpClient`, tạo object URL tạm thời và thu hồi khi
unmount. Component chỉ nhận state để hiển thị preview, metadata và thumbnail
status.

## Đã triển khai hiện tại

Vite + React, Tailwind, React Router, Redux Toolkit, Axios, toast interceptor, trang Student, Media và Notification Batch. Student có `studentHttpClient` độc lập gửi Student actor, feature Redux `studentLearning` cho enrollment list, catalog, detail, progress và enrollment mutation, cùng UI namespace `components/ui/student`. Home chỉ hiển thị các Course đã ghi danh, nạp progress theo từng Course, hỗ trợ offset pagination và đi tới Course detail chỉ đọc. Menu Khóa học hiển thị Course `PUBLISHED` chưa ghi danh; POST enrollment thành công thay route sang Course detail. Notification có ba menu tách biệt: tạo batch, quản lý list offset có action retry failed và tiến trình theo URL chứa `batchId`. Trang tiến trình polling tuần tự snapshot status, delivery status rồi Media Usage job status; bước sau không request trước khi bước trước terminal. Hai trang Media dùng chung preview ảnh và polling/retry thumbnail; trang direct upload hiển thị progress XHR ở Output panel. Xem [Backend overview](../backend/overview.md).

Media có menu Quản lý job tương ứng duy nhất `GET /media/api/media/jobs`. Trang
dùng Redux list state, Workbench Mẫu/Thủ công và table vận hành; không gộp retry
hoặc detail payload vào trang list.

Trang chi tiết Course của Student truyền `scrollable` cho `StudentShell` để
cuộn độc lập trong viewport (root không cuộn). Gallery Course là carousel điều
hướng bằng nút trước/sau hoặc chỉ mục ảnh; ảnh vào bằng transition transform và
opacity, đồng thời tắt chuyển động khi người dùng bật reduced motion.

Dashboard quản trị ở `/admin/dashboard` dùng shell riêng không có sidebar.
Dashboard là màn hình tổng hợp nhiều API nên không dùng Workbench một-API: hook
điều phối ba summary request và sáu health request song song, Redux giữ trạng
thái từng card để một lỗi không chặn phần còn lại.

Player Student tại `/student/courses/:courseId/learn/:lessonId` không dùng
`StudentShell`: header, sidebar Lesson và media rail đứng yên trong viewport;
chỉ list Lesson và content panel cuộn độc lập. Redux giữ Course preview, Lesson
detail và completion state; HTML Lesson luôn do Course Service render/sanitize.
`RenderedMarkdown` dùng chung cho Admin và Student, chỉ typeset các placeholder
`course-math` do server tạo (`$...$`, `$$...$$`, `\\[...\\]`) bằng KaTeX với
`trust: false`; công thức block có vùng cuộn ngang trên màn hình hẹp.

Lesson detail của Admin tách thành hai tab trong `LessonDetailTabs`: tab **Nội
dung** là mặc định và chỉ hiển thị HTML đã sanitize; tab **Tài liệu** hiển thị
attachment, preview/gỡ media và mở `MediaLibraryModal` ở Thư viện hoặc Upload.
Upload hoàn tất trở về thư viện để Admin chọn rồi gắn media vào Lesson; không
thêm request trực tiếp từ component UI.

`MediaLibraryModal` dùng chung cho Course, Lesson và Markdown có filter loại ở
panel trái, dropdown `status` (`PENDING`/`READY`/`FAILED`) và hai chế độ xem
Grid/List. Redux cache tách theo cặp loại + trạng thái, nên đổi filter không
hiển thị lại dữ liệu của filter trước.

Menu **Thư viện Media** tại `/admin/media/library` là màn hình duyệt độc lập:
vẫn dùng endpoint thư viện và cache Redux theo filter, nhưng bố cục master-detail
với danh sách Grid/List ở panel trái và preview cùng metadata ở panel phải. Chọn
một media không phát sinh request thứ hai; chỉ media `READY` mới có thể đọc
content để preview. Preview Grid/List của ảnh, video, PDF dùng thumbnail WebP
derivative khi `thumbnail.status = READY`, không tải original để dựng cover;
panel phải xem file original qua `contentUrl` và hiển thị metadata thumbnail độc
lập với file gốc.
Màn hình này không có chức năng upload.

## Định hướng/chưa triển khai

Trang mới tuân theo Page → Hook → Redux → API; không thêm Axios call trực tiếp trong UI component.
