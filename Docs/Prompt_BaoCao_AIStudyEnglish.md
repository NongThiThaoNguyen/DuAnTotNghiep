# HƯỚNG DẪN CHI TIẾT & BỘ PROMPT SIÊU ĐẦY ĐỦ CHO CODEX TẠO TEMPLATE BÁO CÁO

Tài liệu này chứa bộ prompt chi tiết nhất để bạn đưa cho Codex (Claude Code). Nó sẽ hướng dẫn Codex sử dụng toàn bộ công cụ của mình để phân tích mã nguồn thực tế của dự án **AI Study English** và tạo ra file template báo cáo chi tiết nhất.

---

### BỘ PROMPT SIÊU CHI TIẾT (COPY DÁN VÀO CODEX / CLAUDE CODE)

```text
Bạn là Codex, một kỹ sư phần mềm thực thụ và chuyên gia phân tích hệ thống. Bạn có quyền truy cập đầy đủ vào các tệp tin trong thư mục dự án `e:\DuAnTotNghiep`.
Nhiệm vụ của bạn là: Đọc hiểu cấu trúc báo cáo mẫu từ file `Docs/temple_mau.md`, kết hợp với mã nguồn thực tế của dự án "AI Study English" để tạo ra tài liệu template báo cáo chi tiết nhất tại `Docs/BaoCao_AIStudyEnglish_Template.md`.

Hãy áp dụng các công cụ của bạn (đọc tệp, tìm kiếm văn bản, liệt kê thư mục) để thực hiện chính xác các yêu cầu chi tiết sau:

### BƯỚC 1: QUÉT VÀ THU THẬP THÔNG TIN DỰ ÁN
1. Đọc tệp `Docs/temple_mau.md` và chú ý kỹ:
   - Các chương mục (Chương 1 đến Chương 6).
   - Định dạng bảng Đặc tả yêu cầu (SRS) gồm Tên, Mô tả, Tác nhân, Tiền điều kiện, Luồng chính, Luồng thay thế.
   - Định dạng bảng Từ điển dữ liệu (Data Dictionary): Tên trường, Kiểu dữ liệu, Khóa, Nullable, Mô tả.
   - Định dạng bảng Test Case: ID, Tên ca kiểm thử, Các bước thực hiện, Dữ liệu đầu vào, Kết quả mong đợi.
   - Cấu trúc trình bày code minh họa và cấu trúc thư mục.
2. Tìm kiếm và đọc tệp `schema.sql` (hoặc các biến thể schema khác trong thư mục gốc) để tìm và phân tích định nghĩa của các bảng dữ liệu sau:
   - `users` (Thông tin tài khoản)
   - `roles` (Vai trò)
   - `student_learning_profiles` (Khảo sát onboarding)
   - `placement_tests` (Bài kiểm tra đầu vào)
   - `placement_test_questions` (Câu hỏi kiểm tra)
   - `test_attempts` (Lượt làm bài kiểm tra)
   - `student_learning_paths` (Lộ trình học AI)
   - `learning_path_nodes` (Node bài học trong lộ trình)
   - `ai_generated_contents` (Nội dung câu hỏi do AI sinh)
   - `ai_prompt_templates` (Mẫu prompt gửi OpenAI)
3. Quét thư mục dự án và tệp `Program.cs` để xác định cấu trúc thư mục thực tế (.csproj, Areas, Controllers, Services, Data, Models) và cách cấu hình DI, background services.
4. Tìm và đọc các file code C# đại diện của các tầng:
   - Tầng Data/Repository: Tìm tệp chứa định nghĩa Repository (ví dụ `GenericRepository.cs` hoặc `IGenericRepository.cs`).
   - Tầng Service: Tìm các file service chính như `PlacementTestService.cs`, `LearningPathEngineService.cs` hoặc `CompetencyAnalysisOrchestrator.cs`.
   - Tầng Controller: Tìm các controller như `AccountController.cs`, `ProfileController.cs` hoặc các Controller trong `Areas/Admin/Controllers/`.

---

### BƯỚC 2: TẠO FILE `Docs/BaoCao_AIStudyEnglish_Template.md`
Hãy viết trực tiếp file template này bằng tiếng Việt, văn phong khoa học, cấu trúc chuẩn xác và chi tiết theo từng chương:

#### 1. TIÊU ĐỀ & TRANG BÌA
- Đổi tên đề tài thành: **HỆ THỐNG HỌC TIẾNG ANH THÔNG MINH - AI STUDY ENGLISH**
- Thành viên thực hiện: Nông Thị Thảo Nguyên.
- Công nghệ: ASP.NET Core MVC (.NET 10), SQL Server, OpenAI API.

#### 2. CHƯƠNG 1: GIỚI THIỆU DỰ ÁN
- **1.1 Giới thiệu dự án**: Viết lý do chọn đề tài (Xu hướng ứng dụng Generative AI vào giáo dục, nhu cầu cá nhân hóa lộ trình học tiếng Anh).
- **1.2 Yêu cầu hệ thống**: Phân tích yêu cầu chức năng cho 3 Actor:
  - *Student (Học viên)*: Làm onboarding, làm test xếp lớp, xem phân tích năng lực AI, học theo lộ trình cây bài học, xem dashboard tiến trình.
  - *Admin (Quản trị viên)*: CRUD danh mục, duyệt và xuất bản câu hỏi AI sinh, import Excel, cấu hình prompt template.
  - *Teacher (Giáo viên)*: Quản lý học viên, xem báo cáo học tập.
- **1.3 Yêu cầu phi chức năng**: Bảo mật (BCrypt, Cookie Auth, Secure Cookie), Hiệu năng (AsNoTracking, Index), Khả năng mở rộng (Layered, Repository Pattern).
- **1.4 Kế hoạch dự án**: Tạo bảng kế hoạch dự án gồm các module từ M1 đến M17 dựa trên file `BaoCao_DuAn_AIStudyEnglish.md`.

#### 3. CHƯƠNG 2: PHÂN TÍCH YÊU CẦU HỆ THỐNG
- **2.1 Sơ đồ Use Case**: 
  - Mô tả Use Case tổng quát và 3 phân hệ chính (Phân hệ Student, Phân hệ Admin, Phân hệ AI Integration).
  - Trỏ đường dẫn đến file hình ảnh `usecase_diagram.png` có sẵn trong thư mục `Docs/`.
- **2.2 Đặc tả yêu cầu hệ thống (SRS)**: Tạo các bảng đặc tả chi tiết cho các Use Case cốt lõi:
  - *UC-ONB*: Khảo sát ban đầu (Onboarding).
  - *UC-TST*: Làm bài kiểm tra xếp lớp (Placement Test).
  - *UC-ANL*: Xem phân tích năng lực AI (Competency Analysis).
  - *UC-PTH*: Xem và học theo lộ trình học tập cá nhân hóa (Learning Path).
  - *UC-GEN*: Sinh câu hỏi tự động bằng AI (Quiz Generation).
- **2.3 Sơ đồ triển khai (Deployment Diagram)**: Mô tả kiến trúc Client-Server, Web Server (ASP.NET Core), Database Server (SQL Server) và AI Provider (OpenAI). Trỏ đến file ảnh `architecture_diagram.png` hoặc mô tả bằng văn bản/Mermaid Diagram.

#### 4. CHƯƠNG 3: THIẾT KẾ ỨNG DỤNG
- **3.1 Mô hình công nghệ**: Luận giải chi tiết kiến trúc phân tầng (Presentation, Controller, Service, Repository, Data Layer). Giải thích cơ chế hoạt động của background services (`AiAnalysisBackgroundService`, `ProgressSnapshotBackgroundService`) trong việc xử lý bất đồng bộ các tác vụ AI nặng.
- **3.2 Thực thể & Cơ sở dữ liệu**:
  - Vẽ sơ đồ quan hệ thực thể (ERD) hoặc mô tả liên kết chính (trỏ đến `Docs/erd_overview.png`).
  - **Tạo bảng Từ điển dữ liệu (Data Dictionary)** chi tiết cho các bảng chính đã quét được ở Bước 1. Mỗi bảng phải có tối thiểu các cột: Tên cột, Kiểu dữ liệu, Khóa (PK/FK), Nullable (Yes/No), Mô tả ý nghĩa thực tế.
- **3.3 Thiết kế giao diện**: Vẽ sơ đồ tổ chức giao diện (Sitemap) cho Student và Admin. Mô tả chi tiết giao diện màn hình chính của học viên (Màn hình lộ trình cây Duolingo, Màn hình Dashboard tiến trình).

#### 5. CHƯƠNG 4: THỰC HIỆN DỰ ÁN (IMPLEMENTATION)
- **4.1 Cấu trúc thư mục**: Liệt kê cấu trúc thư mục thực tế của dự án ASP.NET Core MVC (bao gồm các thư mục Areas, Controllers, Services, Data, Models, wwwroot).
- **4.2 Code SQL tạo bảng**: Trích xuất code SQL thực tế từ `schema.sql` dùng để tạo các bảng: `users`, `student_learning_profiles`, `student_learning_paths`, `learning_path_nodes`, `ai_generated_contents`.
- **4.3 Code C# minh họa**: 
  - Trích xuất code thực tế của giao diện `IGenericRepository<T>` và lớp triển khai `GenericRepository<T>`.
  - Trích xuất một đoạn code Service thực tế xử lý nghiệp vụ AI hoặc chấm điểm (ví dụ: `PlacementTestService.cs` hoặc `CompetencyAnalysisOrchestrator.cs`).
  - Trích xuất code của Controller xử lý gọi service và trả về view.
- **4.4 Tích hợp AI**: 
  - Mô tả cách cấu hình OpenAI SDK trong file `Program.cs` và đọc cấu hình từ `.env`.
  - Trích xuất cấu trúc và mã nguồn của Hosted Service chạy nền xử lý hàng đợi AI (`AiAnalysisBackgroundService`).
  - Trích xuất nội dung các Prompt Template thực tế dùng để gửi tới OpenAI API để phân tích kết quả bài test xếp lớp và sinh các node lộ trình học tập.

#### 6. CHƯƠNG 5: KIỂM THỬ PHẦN MỀM
- Thiết lập bảng Test Cases chi tiết cho 3 phân hệ chính:
  1. *Phân hệ Tài khoản (Account)*: Đăng nhập thành công, Đăng nhập thất bại quá 5 lần (khóa tài khoản), Đăng ký tài khoản mới với email trùng lặp.
  2. *Phân hệ Placement Test*: Làm bài kiểm tra và nộp bài đúng hạn, Nộp bài khi hết giờ (tự động nộp), Trả lời thiếu câu hỏi tự luận ngắn.
  3. *Phân hệ Sinh Lộ trình bằng AI*: Sinh lộ trình thành công sau khi hoàn thành bài test xếp lớp, Tái cấu trúc lộ trình (Replanning) khi học viên thay đổi mục tiêu.
- Mỗi ca kiểm thử phải bao gồm: ID, Tên ca kiểm thử, Các bước thực hiện, Dữ liệu kiểm thử đầu vào, Kết quả mong đợi, Kết quả thực tế (Đạt/Không đạt).

#### 7. CHƯƠNG 6: KẾT LUẬN
- **Kết quả đạt được**: Tổng kết hệ thống đã hoàn thành những gì (M1-M9, M14, M16-M17).
- **Hạn chế**: Thời gian phản hồi của OpenAI API phụ thuộc vào đường truyền mạng và tải của máy chủ OpenAI, chi phí API token khi mở rộng quy mô.
- **Hướng phát triển**: Nghiên cứu tích hợp các mô hình AI mã nguồn mở (như Llama, Qwen) tự lưu trữ (self-hosted) để giảm chi phí API, bổ sung tính năng luyện nói nói trực tiếp với AI (AI Tutor Voice Chat).

---

### BƯỚC 3: ĐẶT CHỈ DẪN CHO CLAUDE OPUS
Ở cuối mỗi mục cần phân tích dài hoặc chèn hình ảnh giao diện, hãy thêm các khối chỉ dẫn rõ ràng để người dùng có thể gửi file template này cho Claude Opus ở bước sau:
`> [!IMPORTANT] (YÊU CẦU OPUS 4.6): Hãy viết thêm 2-3 đoạn văn phân tích sâu sắc về phần này theo văn phong học thuật cao cấp...`

Hãy thực hiện nhiệm vụ này một cách nghiêm túc, đọc kỹ mã nguồn của tôi để tạo ra file template chân thực nhất chứ không được bịa ra code hoặc bảng dữ liệu giả. Bắt đầu ngay!
```
