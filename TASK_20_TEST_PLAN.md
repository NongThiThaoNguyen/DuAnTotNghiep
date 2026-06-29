# TÀI LIỆU BÀN GIAO KỸ THUẬT & KỊCH BẢN KIỂM THỬ (TASK 20)
## MODULE M7 - AI PHÂN TÍCH NĂNG LỰC

Tài liệu này đặc tả toàn bộ quy trình kiểm thử chất lượng, kịch bản Demo liên thông End-to-End và tiêu chí hoàn thành nghiệm thu (Definition of Done) cho Module M7 thuộc hệ thống DuAnTotNghiep.

---

## 1. MA TRẬN KỊCH BẢN KIỂM THỬ CHỨC NĂNG (CORE FUNCTIONAL TEST CASES)

### TC-F-01: Học sinh hoàn thành bài kiểm tra trắc nghiệm, sinh báo cáo năng lực
*   **Mô tả:** Đảm bảo hệ thống tự động tính toán điểm, gọi AI phân tích và lưu báo cáo ở trạng thái hoàn tất sau khi nộp bài.
*   **Dữ liệu đầu vào (Input):**
    *   Tài khoản học sinh đăng nhập hợp lệ.
    *   Bài làm trắc nghiệm hoàn thành của Module M6 (TestAttemptId hợp lệ).
*   **Các bước thực hiện:**
    1. Đăng nhập vào tài khoản học sinh.
    2. Thực hiện làm bài kiểm tra trắc nghiệm đầu vào.
    3. Nhấn nút "Nộp bài" (Submit).
    4. Theo dõi màn hình chờ tải thông tin (`PendingView`).
    5. Đợi hệ thống xử lý phân tích và tự động chuyển sang trang kết quả (`Result`).
*   **Kết quả mong đợi (Expected Results):**
    *   Bảng `competency_analyses` xuất hiện bản ghi mới với `status = 'COMPLETED'` và `is_latest = 1`.
    *   Bảng `competency_skill_scores` ghi nhận đầy đủ điểm số chi tiết cho từng kỹ năng và chủ đề ưu tiên.
    *   Trang kết quả Razor View hiển thị đúng trình độ CEFR (ví dụ: B1 - Intermediate) kèm biểu đồ phần trăm kỹ năng.
    *   Bảng `audit_logs` xuất hiện 1 bản ghi mới có hành động `generate`.

### TC-F-02: Yêu cầu tạo lại phân tích năng lực (Regenerate)
*   **Mô tả:** Đảm bảo hệ thống lưu trữ lịch sử báo cáo cũ (không xóa log) và tạo bản phân tích mới.
*   **Dữ liệu đầu vào (Input):**
    *   Mã số báo cáo năng lực hiện tại của học sinh (`is_latest = 1`).
*   **Các bước thực hiện:**
    1. Đăng nhập tài khoản Admin/Teacher.
    2. Truy cập chi tiết báo cáo năng lực của học sinh.
    3. Nhấp nút "Tạo lại báo cáo" (Regenerate).
*   **Kết quả mong đợi (Expected Results):**
    *   Hệ thống kiểm tra Rate Limit (nếu yêu cầu kế tiếp thực hiện dưới 10 phút, chặn và hiển thị thông báo cảnh báo).
    *   Báo cáo năng lực cũ được cập nhật thành `is_latest = 0`.
    *   Báo cáo năng lực mới được tạo ngầm ở trạng thái `PENDING` và cập nhật thành `COMPLETED` sau khi AI chạy xong.
    *   Bảng `audit_logs` ghi nhận 2 bản ghi hành động `regenerate` ở trạng thái: `Started` và `Success`.

### TC-F-03: Tích hợp dữ liệu liên thông sang Module M8 (Learning Path DTO)
*   **Mô tả:** Kiểm tra độ sạch của dữ liệu truyền đi, lọc chéo học liệu và xử lý dự phòng (Fallback).
*   **Dữ liệu đầu vào (Input):**
    *   Bản ghi phân tích năng lực đã hoàn thành (`status = 'COMPLETED'`).
*   **Các bước thực hiện:**
    1. Hệ thống hoặc API của Module M8 gọi hàm `GetLearningPathInputAsync` với ID báo cáo.
*   **Kết quả mong đợi (Expected Results):**
    *   Trả về đối tượng DTO chuẩn chứa: `EstimatedCefrLevel`, `TargetCefrLevel`, `PrimaryGoal` và `PriorityTopics`.
    *   Danh sách `PriorityTopics` tự động loại bỏ các chủ đề chưa được phê duyệt hoặc thiếu học liệu (Lesson/Quiz/Task), thay thế bằng các chủ đề dự phòng phù hợp cùng kỹ năng.
    *   Không lộ lọt mật khẩu hoặc API Key.
    *   Bảng `audit_logs` ghi nhận 1 bản ghi hành động `linked_to_path`.

---

## 2. KỊCH BẢN NGOẠI LUỒNG & BẢO MẬT (EDGE CASES & SECURITY TEST CASES)

### TC-SEC-01: Chống tấn công IDOR thay đổi ID trên URL
*   **Mô tả:** Đảm bảo Học sinh A không thể xem trộm báo cáo năng lực của Học sinh B bằng cách thay đổi ID trên thanh địa chỉ.
*   **Dữ liệu đầu vào (Input):**
    *   Học sinh A đăng nhập (ID: 101). Báo cáo của học sinh B có ID là 2002.
*   **Các bước thực hiện:**
    1. Đăng nhập tài khoản Học sinh A.
    2. Nhập trực tiếp trên thanh địa chỉ URL: `/CompetencyAnalysis/Details/2002`.
*   **Kết quả mong đợi (Expected Results):**
    *   Hệ thống chặn truy cập và ném ra ngoại lệ `UnauthorizedAccessException`.
    *   Chuyển hướng người dùng về trang báo lỗi bảo mật (HTTP 403 Forbidden).
    *   File log hệ thống (App Log) ghi nhận cảnh báo vi phạm bảo mật kèm địa chỉ IP của Học sinh A.

### TC-ERR-01: Lỗi kết nối dịch vụ AI (Fallback to Rule-based)
*   **Mô tả:** Kiểm tra khả năng tự phục hồi của hệ thống khi API AI bị sập hoặc quá hạn phản hồi.
*   **Dữ liệu đầu vào (Input):** Cấu hình sai API Key hoặc ngắt kết nối internet đến AI Server.
*   **Các bước thực hiện:**
    1. Học sinh nộp bài test.
    2. Tầng tích hợp AI trả về ngoại lệ sập kết nối.
*   **Kết quả mong đợi (Expected Results):**
    *   Hệ thống tự động kích hoạt bộ quy tắc rule-based để ước lượng trình độ dựa trên điểm số cứng.
    *   Học sinh nhận được kết quả bình thường trên giao diện (không bị crash trang trắng).
    *   Bảng `audit_logs` tự động ghi nhận hành động `ai_failed` kèm nội dung lỗi kỹ thuật chi tiết phục vụ truy vết.

---

## 3. KỊCH BẢN DEMO NGHIỆM THU LIÊN THÔNG (END-TO-END DEMO SCRIPT)

### Kịch bản Demo cho Hội đồng Nghiệm thu:
1.  **Actor Học sinh:**
    *   Đăng nhập tài khoản học sinh -> Làm bài thi test đầu vào (Module M6) -> Nhấn nộp bài.
    *   Trình diễn màn hình chờ phân tích sinh động với biểu tượng loading mượt mà.
    *   Trình diễn màn hình kết quả trực quan (CEFR B1, các biểu đồ thanh tiến trình kỹ năng đổi màu tương ứng độ chính xác).
    *   Nhấn nút "Liên kết học tập" -> Chuyển tiếp mượt mà sang sơ đồ lộ trình bài học của Module M8.
2.  **Actor Quản trị viên (Admin):**
    *   Admin truy cập trang quản lý `/Admin/CompetencyHistory`.
    *   Bấm chọn "Xem nhật ký kiểm toán" -> Trình diễn bảng lịch sử hoạt động viết bằng ngôn ngữ tự nhiên tiếng Việt dễ hiểu (ví dụ: *"Giáo viên Nguyễn Văn A đã xem báo cáo năng lực mã số #123 của Học sinh B"*).
    *   Trình diễn bảng giám sát tài nguyên tiêu hao AI Token của phiên demo vừa chạy để chứng minh khả năng kiểm soát tài chính.

---

## 4. CHECKLIST TIÊU CHÍ HOÀN THÀNH (DEFINITION OF DONE - DoD)

- [x] **Cơ sở dữ liệu (Database):** Các bảng và chỉ mục tối ưu, bảng audit log được khóa quyền xóa vật lý để đảm bảo tính bất biến.
- [x] **Giao diện (UI/UX):** Responsive trên thiết bị di động, xử lý giao diện trống (Empty States) khi chưa có dữ liệu báo cáo.
- [x] **Bảo mật (Security):** Ngăn chặn thành công lỗ hổng IDOR, che giấu các khóa bí mật hệ thống khỏi log.
- [x] **Cô lập lỗi (Fault Isolation):** Sự cố ghi Audit log được bao bọc bằng `try-catch` và fallback log cục bộ, tuyệt đối không làm rollback giao dịch lưu báo cáo của học sinh.
