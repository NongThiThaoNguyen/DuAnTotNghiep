**MODULE M10 - KẾ HOẠCH HỌC TẬP THÁNG/TUẦN/NGÀY**

**AI STUDY ENGLISH - ASP.NET MVC + RAZOR VIEW + SQL SERVER**

*Tài liệu phân tích nghiệp vụ, CSDL, Backend, Controller/View MVC và
danh sách task triển khai*

| **Dự án** | AI Study English - hệ thống học Tiếng Anh cá nhân hóa bằng AI Tutor và Learning Path |
|----|----|
| **Module** | M10 - Kế hoạch học tập tháng/tuần/ngày |
| **Hướng triển khai** | ASP.NET MVC, Razor View, Service/Repository, SQL Server |
| **Actor chính** | Học sinh, AI Engine; Admin/Giáo viên xem hỗ trợ nếu có quyền |
| **Nguyên tắc bắt buộc** | Task 1 luôn rà soát và chỉnh CSDL trước để đúng nghiệp vụ |
| **Module liên kết** | M8 AI Learning Path, M9 Path UI, M11 Lesson, M12 Quiz, M16 Progress, M17 AI Replanning, M18 Notification |

# Tóm tắt nhanh

| **Nội dung** | **Chốt nghiệp vụ** |
|----|----|
| **Mục đích** | Biến Learning Path do AI tạo thành kế hoạch hành động cụ thể theo tháng, tuần, ngày để học sinh biết hôm nay học gì, học bao lâu và làm nhiệm vụ nào. |
| **Nguồn dữ liệu** | student_learning_paths và learning_path_nodes từ M8; progress từ M16; completion của lesson/quiz/practice từ M11/M12. |
| **Đối tượng hiển thị** | Daily tasks, weekly objectives, monthly phases, status, estimated_minutes, scheduled_date, overdue/rescheduled information. |
| **Trạng thái nhiệm vụ** | Dùng ProgressStatus hiện có: LOCKED, AVAILABLE, IN_PROGRESS, COMPLETED, NEED_REVIEW, SKIPPED. Trạng thái today/overdue là computed trong ViewModel (không phải status DB). |
| **Nguyên tắc UI** | Ưu tiên nhiệm vụ hôm nay, không nhồi quá nhiều việc; có nút Tiếp tục học và liên kết trực tiếp đến node/bài học/quiz. |
| **MVC cần có** | Areas/Student/StudyPlanController, IStudyPlanService (dùng chung Student + Admin), Razor Views (Areas/Student/Views/StudyPlan/), partial \_DailyTaskCard, IPathViewService cho điều hướng + completion. |

# I. Phạm vi module

Module M10 - Kế hoạch học tập tháng/tuần/ngày chịu trách nhiệm hiển thị
và quản lý kế hoạch học tập chi tiết được sinh ra từ Learning Path.
Module này không tự tạo lộ trình học mới; nó nhận path/node từ M8,
chuyển thành lịch học dễ theo dõi theo tháng, tuần, ngày và cho học sinh
đánh dấu tiến độ thực hiện. Đây là module giúp AI Study English trả lời
rõ câu hỏi thực tế của học sinh: hôm nay cần học gì, học trong bao lâu,
làm bài nào và sau khi hoàn thành thì bước tiếp theo là gì.

- Hiển thị kế hoạch học theo tháng, tuần và ngày.

- Hiển thị nhiệm vụ hôm nay, nhiệm vụ quá hạn và nhiệm vụ tiếp theo.

- Cho học sinh đánh dấu hoàn thành hoặc bỏ qua nhiệm vụ theo quy tắc.

- Điều hướng từ nhiệm vụ sang bài học, quiz, practice hoặc AI Tutor.

- Ghi nhận dữ liệu hoàn thành để M16 Progress và M17 Replanning sử dụng.

- Không cho học sinh chỉnh lịch học của người khác hoặc tự sửa trạng
  thái trái nghiệp vụ.

- Chuẩn bị nền để Phase 2 có thể tự động dời lịch, nhắc học và
  replanning.

# II. CSDL cần bám theo

- student_learning_paths: lộ trình active/archived của học sinh.

- learning_path_nodes: node học có scheduled_date, estimated_minutes,
  status, node_type, order_index.

- study_activity_logs: lịch sử hoạt động học, dùng để xác định hoàn
  thành hoặc bỏ lỡ nhiệm vụ.

- student_progress_snapshots: dữ liệu tiến độ tổng quan theo
  ngày/tuần/tháng.

- ai_replanning_events: sự kiện dời lịch, đổi kế hoạch hoặc gợi ý điều
  chỉnh path.

- notifications: dùng ở M18 để nhắc nhiệm vụ hôm nay/quá hạn.

- original_lessons, quizzes, practice_tasks, ai_tutor_conversations:
  đích điều hướng của từng task.

# III. Phân quyền trong module

| **Vai trò** | **Phạm vi** | **Quyền chính** |
|----|----|----|
| **Học sinh** | Dữ liệu cá nhân | Xem kế hoạch của mình, mở nhiệm vụ, đánh dấu hoàn thành/bỏ qua theo rule, xem lịch sử thực hiện. |
| **Admin** | Toàn hệ thống | Xem kế hoạch học tập của học sinh để hỗ trợ debug, báo cáo, không chỉnh dữ liệu học nếu không có nghiệp vụ rõ. |
| **Giáo viên/Reviewer** | Phạm vi được cấp | Xem kế hoạch học của học sinh nếu được phân quyền để tư vấn, kiểm tra chất lượng nội dung liên quan. |
| **AI Engine** | Tác nhân hệ thống | Sinh kế hoạch từ M8, đề xuất dời lịch hoặc thay đổi từ M17; không trực tiếp thao tác View. |

# IV. Luồng tổng quát

## Luồng 1: Học sinh xem kế hoạch hôm nay

> 1\. Học sinh đăng nhập và có active learning path.
>
> 2\. Học sinh mở trang Kế hoạch học tập hoặc Dashboard.
>
> 3\. Hệ thống lấy các node/task có scheduled_date hôm nay hoặc task quá
> hạn cần ưu tiên.
>
> 4\. View hiển thị nhiệm vụ hôm nay, thời lượng, kỹ năng, hành động cần
> làm và nút bắt đầu.
>
> 5\. Học sinh bấm nhiệm vụ để chuyển sang Lesson/Quiz/Practice/AI
> Tutor.

## Luồng 2: Xem kế hoạch tuần/tháng

> 1\. Học sinh chọn tab Tuần hoặc Tháng.
>
> 2\. Hệ thống nhóm node theo ngày/tuần/tháng dựa trên scheduled_date
> hoặc path_phase.
>
> 3\. View hiển thị mục tiêu tuần, số nhiệm vụ, số phút học dự kiến và
> tỷ lệ hoàn thành.
>
> 4\. Học sinh mở từng ngày để xem nhiệm vụ chi tiết.

## Luồng 3: Hoàn thành, bỏ qua hoặc dời nhiệm vụ

> 1\. Sau khi học sinh hoàn thành bài học/quiz/practice, module liên
> quan gọi service cập nhật task/node.
>
> 2\. StudyPlanService cập nhật trạng thái completed và ghi activity
> log.
>
> 3\. Nếu nhiệm vụ bị bỏ qua hoặc quá hạn, hệ thống đánh dấu
> skipped/overdue và chuyển dữ liệu cho M17 xử lý nếu bật replanning.
>
> 4\. Dashboard và Path UI cập nhật lại tiến độ.

# V. Danh sách task triển khai

> **⚠️ QUY TẮC BẮT BUỘC TRƯỚC KHI CODE:**
> - Đọc `RULES.md` trước khi code bất kỳ task nào.
> - Đọc `DESIGN.md` trước khi code bất kỳ View (.cshtml) hoặc CSS nào.
> - Routes Student: `/Student/StudyPlan/...` (Area Student). Routes Admin: `/Admin/StudyPlans/...` (Area Admin).
> - Dùng `ProgressStatus` enum hiện tại — KHÔNG tạo enum status mới.
> - Dùng `learning_path_nodes` trực tiếp — KHÔNG tạo bảng `study_plan_tasks` riêng.
> - Dùng `IPathViewService` có sẵn cho logic điều hướng và completion — KHÔNG viết lại.

> 1\. Task 1: Rà soát nền CSDL Kế hoạch học tập
>
> 2\. Task 2: Chuẩn hóa trạng thái nhiệm vụ và quy tắc lịch học
>
> 3\. Task 3: Tạo Entity/ViewModel/DTO cho Study Plan
>
> 4\. Task 4: Xây dựng StudyPlanService đọc kế hoạch từ Learning Path
>
> 5\. Task 5: Tạo màn hình kế hoạch hôm nay
>
> 6\. Task 6: Tạo màn hình kế hoạch tuần
>
> 7\. Task 7: Tạo màn hình kế hoạch tháng
>
> 8\. Task 8: Tạo partial view \_DailyTaskCard
>
> 9\. Task 9: Điều hướng nhiệm vụ sang Lesson/Quiz/Practice/AI Tutor
>
> 10\. Task 10: Đánh dấu hoàn thành nhiệm vụ học
>
> 11\. Task 11: Xử lý nhiệm vụ bỏ qua, quá hạn và chưa hoàn thành
>
> 12\. Task 12: Tính tổng thời lượng học và kiểm soát quá tải lịch
>
> 13\. Task 13: Liên kết kế hoạch với Progress Tracking M16
>
> 14\. Task 14: Chuẩn bị dữ liệu cho AI Replanning M17
>
> 15\. Task 15: Tích hợp thông báo nhắc học M18
>
> 16\. Task 16: Admin/Giáo viên xem kế hoạch học sinh dạng read-only
>
> 17\. Task 17: Tối ưu truy vấn và cache kế hoạch
>
> 18\. Task 18: Xử lý lỗi dữ liệu path/node thiếu hoặc sai
>
> 19\. Task 19: Thiết kế UI responsive và trải nghiệm học sinh
>
> 20\. Task 20: Test case, seed data demo và bàn giao module

# VI. Chi tiết từng task

## Task 1: Rà soát nền CSDL Kế hoạch học tập

**Mục tiêu:** Chuẩn hóa nền dữ liệu để module kế hoạch học tập có thể
hiển thị đúng theo tháng/tuần/ngày và liên kết với path/node.

### 1. Nghiệp vụ cần hiểu

- Kế hoạch học tập được sinh từ Learning Path, không phải dữ liệu rời
  rạc.

- Task 1 bắt buộc rà soát CSDL trước để tránh code View trên dữ liệu
  thiếu scheduled_date/status.

- Frontend không được tự bịa lịch học nếu backend/path chưa có dữ liệu
  hợp lệ.

### 2. CSDL cần làm

- Kiểm tra student_learning_paths có student_id, status, start_date,
  target_end_date.

- Kiểm tra learning_path_nodes có scheduled_date, estimated_minutes,
  node_type, status, order_index.

- ⚠️ `CompletedAt` đã có sẵn trong `LearningPathNode.cs` — KHÔNG thêm lại.

- ⚠️ `ScheduledDate` đã có sẵn — KHÔNG thêm `due_date` (trùng chức năng).

- Bổ sung field nếu thiếu: `RescheduledFrom` (DateOnly?), `SkippedReason` (string?).

- Tạo index theo learning_path_id + scheduled_date + status.

- Rà soát FK sang lessons/quizzes/practice_tasks.

### 3. Backend / Service cần làm

- Viết migration an toàn, không làm mất path cũ.

- Tạo enum/constant status dùng chung cho path và study plan.

- Tạo script seed path demo có dữ liệu ngày/tuần/tháng.

### 4. Controller / View MVC đề xuất

- Chưa cần View chính ở Task 1; có thể tạo trang Admin/Diagnostics tạm
  để kiểm tra dữ liệu.

- Chuẩn bị route: /Student/StudyPlan/Today, /Student/StudyPlan/Week,
  /Student/StudyPlan/Month (Areas/Student/Controllers/StudyPlanController.cs).

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- Không đổi tên field đang được M8/M9 dùng nếu chưa update đồng bộ.

- Không xóa cứng path/node đã có activity.

- Nếu thiếu scheduled_date thì phải fallback theo order_index.

### 6. Tiêu chí hoàn thành

- CSDL đủ field cho kế hoạch ngày/tuần/tháng.

- Index và FK cơ bản đầy đủ.

- Không mất dữ liệu path/node cũ.

## Task 2: Chuẩn hóa trạng thái nhiệm vụ và quy tắc lịch học

**Mục tiêu:** Thống nhất trạng thái nhiệm vụ để View, Service và
Replanning hiểu cùng một nghiệp vụ.

### 1. Nghiệp vụ cần hiểu

- Một node có status học tập; kế hoạch học cần diễn giải status thành
  nhiệm vụ hôm nay/quá hạn/hoàn thành.

- Nếu không chuẩn hóa trạng thái, UI sẽ hiển thị sai hoặc AI Replanning
  khó xử lý.

### 2. CSDL cần làm

- Dùng `ProgressStatus` đã có sẵn (`Enums/ProgressStatus.cs`): LOCKED, AVAILABLE, IN_PROGRESS, COMPLETED, NEED_REVIEW, SKIPPED.

- KHÔNG tạo bảng `study_plan_tasks` riêng. Query trực tiếp trên `learning_path_nodes`.

- `today` và `overdue` là computed trong ViewModel/Service, tính từ `ScheduledDate` + `Status`.

- Nếu cần thêm trạng thái `RESCHEDULED`/`CANCELLED` → thêm vào `ProgressStatus.cs` duy nhất.

### 3. Backend / Service cần làm

- KHÔNG tạo StudyTaskStatus mới. Dùng `ProgressStatus` hiện có.

- Viết hàm `IsOverdue(node)`: `node.ScheduledDate < today && status != COMPLETED/SKIPPED`.

- Viết hàm `IsTodayTask(node)`: `node.ScheduledDate == today && status in [AVAILABLE, IN_PROGRESS]`.

- Quy định overdue khi scheduled_date \< today và chưa
  completed/skipped.

### 4. Controller / View MVC đề xuất

- Razor dùng label tiếng Việt: Hôm nay, Đang học, Đã xong, Quá hạn, Đã
  dời, Đã bỏ qua.

- Không hard-code status rải rác trong View.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- Không để task completed bị chuyển thành overdue.

- Không tự động skipped nếu học sinh chưa xác nhận hoặc rule chưa cho
  phép.

### 6. Tiêu chí hoàn thành

- Tất cả status hiển thị nhất quán.

- Overdue tính đúng.

- Status dùng chung giữa Service và View.

## Task 3: Tạo Entity/ViewModel/DTO cho Study Plan

**Mục tiêu:** Tạo các model trung gian để Razor View hiển thị kế hoạch
rõ ràng, không xử lý business logic trong View.

### 1. Nghiệp vụ cần hiểu

- MVC View nên nhận ViewModel đã được service chuẩn bị.

- Không để Razor query database hoặc tự tính logic phức tạp.

### 2. CSDL cần làm

- Không cần tạo bảng mới nếu dùng ViewModel.

- Nếu thiếu dữ liệu trong DB thì bổ sung ở Task 1.

### 3. Backend / Service cần làm

- Tạo StudyPlanOverviewViewModel, DailyStudyTaskViewModel,
  WeeklyStudyPlanViewModel, MonthlyStudyPlanViewModel.

- Mỗi task có title, skill, topic, node_type, estimated_minutes, status,
  target_url, ai_reason.

- Tạo mapper từ learning_path_nodes sang ViewModel.

### 4. Controller / View MVC đề xuất

- Views/StudyPlan/Today.cshtml, Week.cshtml, Month.cshtml dùng
  ViewModel.

- Không đưa Entity trực tiếp ra View nếu có dữ liệu nhạy cảm.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- ViewModel phải null-safe.

- Không lộ prompt AI hoặc dữ liệu nội bộ không cần thiết.

### 6. Tiêu chí hoàn thành

- ViewModel render đủ thông tin.

- Code View sạch, ít logic.

- Không lỗi khi task thiếu optional field.

## Task 4: Xây dựng StudyPlanService đọc kế hoạch từ Learning Path

**Mục tiêu:** Xây service lõi lấy kế hoạch học tập từ active learning
path của học sinh.

### 1. Nghiệp vụ cần hiểu

- M10 phụ thuộc M8: phải có active path trước.

- Nếu học sinh chưa có path, cần điều hướng về Placement Test/AI
  Analysis/Generate Path.

### 2. CSDL cần làm

- Query student_learning_paths status ACTIVE.

- Query learning_path_nodes theo path_id, scheduled_date/order_index.

- Không lấy path của user khác.

### 3. Backend / Service cần làm

- Tạo IStudyPlanService và StudyPlanService.

- Hàm GetTodayPlanAsync, GetWeeklyPlanAsync, GetMonthlyPlanAsync.

- Tính current task, next task, overdue tasks, completion percent.

- Fallback schedule theo order_index nếu scheduled_date null.

### 4. Controller / View MVC đề xuất

- Areas/Student/Controllers/StudyPlanController gọi service, không query trực tiếp.

- Có action Today/Week/Month tại route `/Student/StudyPlan/...`.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- Không có active path -\> hiển thị trang hướng dẫn tạo path.

- Path rỗng -\> thông báo lỗi cấu hình.

- Không để lỗi path làm trang trắng.

### 6. Tiêu chí hoàn thành

- Service lấy đúng kế hoạch của học sinh.

- Today/Week/Month trả dữ liệu đúng.

- Không có N+1 query nghiêm trọng.

## Task 5: Tạo màn hình kế hoạch hôm nay

**Mục tiêu:** Xây màn hình nhiệm vụ hôm nay để học sinh biết cần làm gì
ngay khi vào hệ thống.

### 1. Nghiệp vụ cần hiểu

- Mục tiêu quan trọng nhất của M10 là “hôm nay học gì”.

- Màn Today nên ưu tiên 1-3 nhiệm vụ chính, không hiển thị toàn bộ path
  dài.

### 2. CSDL cần làm

- Lấy task scheduled_date = today hoặc overdue cần ưu tiên.

- Nếu không có scheduled_date thì lấy current/available node đầu tiên.

### 3. Backend / Service cần làm

- StudyPlanService.GetTodayPlanAsync trả TodayTasks, OverdueTasks,
  NextTask.

- Tính tổng phút học hôm nay.

### 4. Controller / View MVC đề xuất

- StudyPlanController.Today().

- Views/Student/StudyPlan/Today.cshtml. **Đọc `DESIGN.md` trước khi code View.**

- Widget “Tiếp tục học” ở đầu trang.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- Không có task hôm nay -\> hiển thị gợi ý học node kế tiếp.

- Nhiệm vụ quá hạn phải hiển thị nhưng không spam giao diện.

### 6. Tiêu chí hoàn thành

- Học sinh thấy nhiệm vụ hôm nay.

- Bấm vào task mở đúng node/module.

- Task quá hạn hiển thị rõ.

## Task 6: Tạo màn hình kế hoạch tuần

**Mục tiêu:** Xây màn hình kế hoạch tuần để học sinh thấy mục tiêu tuần
và phân bổ nhiệm vụ theo từng ngày.

### 1. Nghiệp vụ cần hiểu

- Kế hoạch tuần giúp người học kiểm soát tiến độ ngắn hạn.

- Tuần cần có mục tiêu, số phút học dự kiến, số task hoàn thành.

### 2. CSDL cần làm

- Nhóm node theo tuần dựa trên scheduled_date.

- Nếu scheduled_date null, fallback theo order_index và start_date.

### 3. Backend / Service cần làm

- GetWeeklyPlanAsync(studentId, weekStart).

- Tính daily groups, total_minutes, completed_count, overdue_count.

### 4. Controller / View MVC đề xuất

- Areas/Student/Controllers/StudyPlanController.Week(DateTime? weekStart).

- Areas/Student/Views/StudyPlan/Week.cshtml có tab từng ngày hoặc list theo ngày. **Đọc `DESIGN.md` trước khi code View.**

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- Không cho chọn tuần quá xa nếu chưa có path.

- Tuần không có nhiệm vụ hiển thị empty state.

### 6. Tiêu chí hoàn thành

- Xem được tuần hiện tại và chuyển tuần.

- Mỗi ngày có task rõ ràng.

- Tỷ lệ hoàn thành tuần tính đúng.

## Task 7: Tạo màn hình kế hoạch tháng

**Mục tiêu:** Xây màn hình kế hoạch tháng để học sinh thấy lộ trình
macro và các giai đoạn học.

### 1. Nghiệp vụ cần hiểu

- Kế hoạch tháng cho người học bức tranh dài hạn.

- Không nên chi tiết quá mức; chỉ cần mục tiêu tuần/giai đoạn và tỷ lệ
  hoàn thành.

### 2. CSDL cần làm

- Nhóm node theo month hoặc path_phase.

- Có thể dùng scheduled_date để group theo tháng.

### 3. Backend / Service cần làm

- GetMonthlyPlanAsync(studentId, year, month).

- Tính số tuần, tổng task, completed, review_needed.

### 4. Controller / View MVC đề xuất

- Areas/Student/Controllers/StudyPlanController.Month(int? year, int? month).

- Areas/Student/Views/StudyPlan/Month.cshtml dạng calendar/list tùy thời gian. **Đọc `DESIGN.md` trước khi code View.**

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- Tháng chưa có dữ liệu -\> thông báo chưa sinh kế hoạch.

- Không render quá nhiều node nếu path dài.

### 6. Tiêu chí hoàn thành

- Xem được kế hoạch tháng.

- Hiển thị số nhiệm vụ và % hoàn thành.

- Không vỡ UI khi nhiều task.

## Task 8: Tạo partial view \_DailyTaskCard

**Mục tiêu:** Tạo partial view \_DailyTaskCard để tái sử dụng hiển thị
nhiệm vụ trong Today/Week/Month.

### 1. Nghiệp vụ cần hiểu

- Task card là đơn vị hiển thị lặp lại nhiều nơi.

- Tách partial giúp View gọn và thống nhất UI.

### 2. CSDL cần làm

- Không tạo bảng mới.

- Task card nhận ViewModel đã chuẩn bị.

### 3. Backend / Service cần làm

- Chuẩn bị properties: title, skill, action_type, status_label, minutes,
  target_url, ai_reason.

- Không gọi DB trong partial.

### 4. Controller / View MVC đề xuất

- Areas/Student/Views/StudyPlan/\_DailyTaskCard.cshtml hoặc
  Areas/Student/Views/Shared/\_DailyTaskCard.cshtml. **Đọc `DESIGN.md` trước khi code View.**

- CSS class theo status (dùng ProgressStatus constants).

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- Task thiếu target_url thì disable nút bắt đầu.

- AI reason dài thì truncate.

### 6. Tiêu chí hoàn thành

- Partial dùng lại ở Today/Week/Month.

- Task card hiển thị đúng mọi status.

- Không có logic DB trong View.

## Task 9: Điều hướng nhiệm vụ sang Lesson/Quiz/Practice/AI Tutor

**Mục tiêu:** Điều hướng nhiệm vụ đến đúng chức năng học tập: Lesson,
Quiz, Practice hoặc AI Tutor.

### 1. Nghiệp vụ cần hiểu

- Study plan chỉ là nơi bắt đầu; việc học thực tế nằm ở các module khác.

- Điều hướng phải đi qua backend để kiểm tra quyền và trạng thái.

### 2. CSDL cần làm

- Node có lesson_id, quiz_id, practice_task_id hoặc topic_id.

- node_type quyết định target.

### 3. Backend / Service cần làm

- ⚠️ Dùng `IPathViewService.BuildNodeTargetUrlAsync(node)` có sẵn từ M9 — KHÔNG viết lại.

- ⚠️ Dùng `IPathViewService.CanOpenNodeAsync(nodeId, userId)` có sẵn — KHÔNG viết lại.

- Ghi activity log khi học sinh bắt đầu task nếu cần.

### 4. Controller / View MVC đề xuất

- Trong View, link trực tiếp đến `/Student/LearningPath/OpenNode/{nodeId}` (action có sẵn từ M9).

- Hoặc StudyPlanController.OpenTask(int nodeId) gọi `IPathViewService.BuildNodeTargetUrlAsync`.

- Redirect đến Lesson/Details, Quiz/Take, Practice/Details,
  AiTutor/Index.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- Task locked/cancelled không được mở.

- Content chưa published/approved thì chặn và báo lỗi cấu hình.

- Node thuộc user khác trả 403/404.

### 6. Tiêu chí hoàn thành

- LEARN mở lesson.

- QUIZ mở quiz.

- PRACTICE mở task.

- AI_TUTOR mở đúng context.

## Task 10: Đánh dấu hoàn thành nhiệm vụ học

**Mục tiêu:** Cho phép cập nhật trạng thái hoàn thành nhiệm vụ khi học
sinh hoàn tất hoạt động.

### 1. Nghiệp vụ cần hiểu

- Hoàn thành task là dữ liệu đầu vào cho progress và replanning.

- Không nên cho học sinh tự “tick done” với quiz bắt buộc nếu chưa thật
  sự làm quiz.

### 2. CSDL cần làm

- learning_path_nodes.completed_at/status.

- study_activity_logs ghi hoạt động LEARN/QUIZ/PRACTICE.

- Nếu có bảng task riêng, cập nhật completed_at.

### 3. Backend / Service cần làm

- ⚠️ Dùng `IPathViewService.MarkNodeCompletedAsync(nodeId, userId, ...)` có sẵn từ M9 — KHÔNG viết lại.

- ⚠️ Dùng `IPathViewService.TryUnlockNextNodesAsync(completedNodeId, userId)` cho unlock — KHÔNG viết lại.

- Chỉ module Lesson/Quiz/Practice gọi sau khi đủ điều kiện.

### 4. Controller / View MVC đề xuất

- Có thể có endpoint POST StudyPlan/CompleteTask cho task thủ công nhẹ.

- Với quiz/lesson nên redirect callback từ module liên quan.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- Không complete node locked.

- Quiz không đạt passing_score thì có thể review_needed thay vì
  completed.

- Không complete task người khác.

### 6. Tiêu chí hoàn thành

- Task hoàn thành cập nhật đúng.

- Progress thay đổi.

- Node tiếp theo mở nếu đủ điều kiện.

## Task 11: Xử lý nhiệm vụ bỏ qua, quá hạn và chưa hoàn thành

**Mục tiêu:** Xử lý nhiệm vụ bị bỏ qua, quá hạn hoặc chưa hoàn thành để
lịch học phản ánh thực tế.

### 1. Nghiệp vụ cần hiểu

- Học sinh có thể bỏ lỡ nhiệm vụ; hệ thống cần biết để nhắc học hoặc
  replanning.

- Overdue không đồng nghĩa thất bại, nhưng là tín hiệu cần xử lý.

### 2. CSDL cần làm

- scheduled_date, due_date, skipped_reason, rescheduled_from.

- study_activity_logs lưu SKIPPED/OVERDUE nếu cần.

### 3. Backend / Service cần làm

- MarkTaskSkippedAsync với lý do.

- Job/Service tính overdue theo ngày.

- GetOverdueTasksAsync cho Dashboard/Notification.

### 4. Controller / View MVC đề xuất

- Nút “Tạm bỏ qua” nếu nghiệp vụ cho phép.

- Màn hình hiển thị task quá hạn và action tiếp tục/dời lịch.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- Không cho skip task bắt buộc nếu rule không cho phép.

- Không tạo quá nhiều overdue duplicate.

- Bỏ qua phải ghi lý do nếu là task quan trọng.

### 6. Tiêu chí hoàn thành

- Task quá hạn hiển thị đúng.

- Skip task có log.

- Dữ liệu sẵn cho Replanning.

## Task 12: Tính tổng thời lượng học và kiểm soát quá tải lịch

**Mục tiêu:** Tính tổng thời lượng học mỗi ngày/tuần để tránh lịch học
quá tải so với onboarding.

### 1. Nghiệp vụ cần hiểu

- Onboarding có daily_study_minutes; kế hoạch phải tôn trọng thông tin
  này.

- Nếu AI tạo quá nhiều task, M10 phải cảnh báo.

### 2. CSDL cần làm

- student_learning_profiles.daily_study_minutes/weekly_study_days.

- learning_path_nodes.estimated_minutes.

### 3. Backend / Service cần làm

- CalculateDailyLoadAsync.

- Cảnh báo nếu tổng phút hôm nay vượt quá ngưỡng.

- Gợi ý dời task phụ sang ngày khác nếu bật replanning.

### 4. Controller / View MVC đề xuất

- View hiển thị tổng phút học hôm nay/tuần.

- Badge “quá tải” nếu vượt ngưỡng.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- Không tự dời lịch khi chưa bật M17 hoặc chưa có rule.

- estimated_minutes null thì dùng default theo node_type.

### 6. Tiêu chí hoàn thành

- Tổng phút học tính đúng.

- Có cảnh báo quá tải.

- Không làm hỏng path gốc.

## Task 13: Liên kết kế hoạch với Progress Tracking M16

**Mục tiêu:** Đồng bộ dữ liệu hoàn thành nhiệm vụ với module M16
Progress Tracking.

### 1. Nghiệp vụ cần hiểu

- M10 là nơi học sinh thao tác lịch, nhưng M16 là nơi tổng hợp tiến độ.

- Mọi hoàn thành/bỏ lỡ task cần có log để dashboard và báo cáo dùng
  được.

### 2. CSDL cần làm

- study_activity_logs bắt buộc ghi activity_type, student_id, node_id,
  duration, score nếu có.

- student_progress_snapshots cập nhật theo ngày nếu thiết kế realtime.

### 3. Backend / Service cần làm

- Gọi ProgressService sau MarkTaskCompleted/Skipped.

- Cập nhật completed_nodes, total_minutes, weak_topics nếu có dữ liệu.

### 4. Controller / View MVC đề xuất

- Không cần View riêng; progress hiển thị ở widget nhỏ trong Study Plan.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- Nếu ProgressService lỗi thì không làm mất completion chính; cần log
  retry.

- Không ghi trùng activity khi refresh trang.

### 6. Tiêu chí hoàn thành

- Hoàn thành task tạo activity log.

- Dashboard M16 nhận dữ liệu mới.

- Không double-count duration.

## Task 14: Chuẩn bị dữ liệu cho AI Replanning M17 ⚠️ PHASE 2

**Mục tiêu:** Chuẩn bị dữ liệu để M17 AI Replanning có thể dời lịch hoặc
điều chỉnh path.

> **⚠️ PHASE 2:** Module M17 chưa phát triển. Phase 1 chỉ tạo interface stub, KHÔNG implement thật.

### 1. Nghiệp vụ cần hiểu

- M17 cần biết task nào quá hạn, task nào skipped, điểm quiz thấp và
  lịch học đang quá tải.

- M10 không tự làm AI replanning nhưng phải cung cấp dữ liệu sạch.

### 2. CSDL cần làm

- ai_replanning_events liên kết path khi đã có replanning.

- study_activity_logs chứa skipped/overdue/missed events.

- learning_path_nodes.rescheduled_from nếu task bị dời.

### 3. Backend / Service cần làm

- Tạo ReplanningInputBuilder hoặc method BuildReplanningContextAsync.

- Gửi missed_tasks, overloaded_days, low_score_nodes cho M17.

- Lưu reason khi task bị rescheduled.

### 4. Controller / View MVC đề xuất

- Nếu có nút “Đề xuất sắp xếp lại lịch”, gọi action RequestReplanning.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- Không gọi AI liên tục mỗi lần refresh.

- Không tự động rewrite toàn bộ lịch nếu học sinh chưa xác nhận theo cấu
  hình.

### 6. Tiêu chí hoàn thành

- M17 lấy được dữ liệu missed/overdue.

- Có log khi request replanning.

- Không mất path version cũ.

## Task 15: Tích hợp thông báo nhắc học M18 ⚠️ PHASE 2

**Mục tiêu:** Tích hợp thông báo nhắc học M18 để học sinh không bỏ lỡ
nhiệm vụ.

> **⚠️ PHASE 2:** Module M18 chưa phát triển. Phase 1 chỉ tạo method `BuildStudyReminder()` trả DTO, KHÔNG ghi vào DB.

### 1. Nghiệp vụ cần hiểu

- Thông báo cần dựa trên nhiệm vụ hôm nay/quá hạn.

- M10 là nguồn tốt để biết cần nhắc gì.

### 2. CSDL cần làm

- notifications có notification_type
  STUDY_REMINDER/QUIZ_PENDING/PATH_UPDATED.

- Task target URL hoặc related_entity_id.

### 3. Backend / Service cần làm

- ReminderPreviewAsync và GenerateStudyReminderAsync.

- Tạo notification cho task hôm nay, quiz chưa làm, task quá hạn.

- Chống spam bằng rule thời gian.

### 4. Controller / View MVC đề xuất

- View có thể hiển thị nút bật/tắt nhắc học hoặc link sang Notification
  settings.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- Không gửi nhắc cho task completed/cancelled.

- Không gửi quá nhiều lần trong ngày.

- Không lộ dữ liệu học sinh khác.

### 6. Tiêu chí hoàn thành

- Có thể tạo notification từ task hôm nay.

- Thông báo có link mở đúng task.

- Có chống spam cơ bản.

## Task 16: Admin/Giáo viên xem kế hoạch học sinh dạng read-only

**Mục tiêu:** Cho Admin/Giáo viên xem kế hoạch học tập của học sinh dạng
read-only để hỗ trợ tư vấn và debug.

### 1. Nghiệp vụ cần hiểu

- Admin cần xem kế hoạch khi học sinh báo lỗi.

- Giáo viên/Reviewer có thể cần xem để tư vấn học tập.

### 2. CSDL cần làm

- Query theo student_id có kiểm tra role/scope.

- Không cần bảng mới.

### 3. Backend / Service cần làm

- Dùng chung `IStudyPlanService` — KHÔNG tạo AdminStudyPlanService riêng.

- Admin controller gọi `IStudyPlanService.GetTodayPlanAsync(studentId)` với quyền admin (bỏ ownership check).

- Không cho chỉnh lịch trực tiếp nếu không có nghiệp vụ riêng.

### 4. Controller / View MVC đề xuất

- Area Admin: Areas/Admin/Controllers/StudyPlansController/Preview(studentId).

- View read-only, có filter Today/Week/Month.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- Admin không đủ quyền không được xem.

- Không hiển thị prompt AI nội bộ nếu không cần.

### 6. Tiêu chí hoàn thành

- Admin xem được kế hoạch học sinh.

- View read-only.

- Phân quyền đúng.

## Task 17: Tối ưu truy vấn và cache kế hoạch

**Mục tiêu:** Tối ưu truy vấn kế hoạch để trang Today/Week/Month phản
hồi nhanh.

### 1. Nghiệp vụ cần hiểu

- Study Plan là màn hình học sinh mở thường xuyên.

- Nếu path nhiều node, query chậm sẽ ảnh hưởng trải nghiệm.

### 2. CSDL cần làm

- Index learning_path_nodes(learning_path_id, scheduled_date, status).

- Index student_learning_paths(student_id, status).

- Có thể thêm computed/snapshot nếu dữ liệu lớn.

### 3. Backend / Service cần làm

- Không N+1 query khi load task.

- Include/Join có chọn lọc, chỉ lấy field cần thiết.

- Cache ngắn hạn TodayPlan là Phase 2. Phase 1 ưu tiên index DB đúng + `AsNoTracking()` + `Select()` projection.

### 4. Controller / View MVC đề xuất

- View không gọi API lặp nhiều lần.

- Nếu Ajax thì có loading state.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- Cache phải invalid khi task completed/replanned.

- Không cache dữ liệu học sinh cho người khác.

### 6. Tiêu chí hoàn thành

- Today page phản hồi nhanh.

- Không có N+1 rõ rệt.

- Cache không gây sai dữ liệu.

## Task 18: Xử lý lỗi dữ liệu path/node thiếu hoặc sai

**Mục tiêu:** Xử lý lỗi dữ liệu path/node thiếu hoặc sai để không làm
hỏng trải nghiệm học sinh.

### 1. Nghiệp vụ cần hiểu

- AI/path generation có thể tạo node thiếu target hoặc target bị
  archived.

- Module phải có thông báo lỗi nghiệp vụ thay vì crash.

### 2. CSDL cần làm

- Kiểm tra node thiếu lesson/quiz/practice target.

- Kiểm tra target status published/approved nếu cần.

### 3. Backend / Service cần làm

- ValidateStudyPlanIntegrityAsync.

- Gắn warning cho task lỗi cấu hình.

- Log lỗi để Admin sửa.

### 4. Controller / View MVC đề xuất

- Task lỗi cấu hình hiển thị disabled và thông báo “Nhiệm vụ đang được
  cập nhật”.

- Admin preview thấy chi tiết lỗi.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- Không để học sinh mở target đã archived.

- Không để trang trắng khi một node lỗi.

- Không tự xóa node lỗi.

### 6. Tiêu chí hoàn thành

- Path vẫn render dù có node lỗi.

- Admin nhìn thấy lỗi cấu hình.

- Có log đủ để debug.

## Task 19: QA pass UI responsive và trải nghiệm học sinh (review sau Task 5/6/7/8)

**Mục tiêu:** Review tổng thể UI responsive sau khi hoàn thành Task 5/6/7/8,
đảm bảo nhất quán và phù hợp demo ASP.NET MVC.

### 1. Nghiệp vụ cần hiểu

- ⚠️ Task này là **QA pass cuối cùng** — UI responsive đã được code trong Task 5/6/7/8 theo `DESIGN.md`.

- Học sinh cần giao diện đơn giản hơn bảng dữ liệu.

- MVP không cần calendar phức tạp nhưng phải dễ hiểu.

### 2. CSDL cần làm

- Không cần bảng mới.

- CSS/JS tĩnh đặt trong wwwroot.

### 3. Backend / Service cần làm

- Không xử lý business logic bằng JavaScript.

- Service chuẩn bị dữ liệu, View chỉ render.

### 4. Controller / View MVC đề xuất

- Today: card nhiệm vụ chính.

- Week: nhóm theo ngày.

- Month: danh sách tuần/giai đoạn.

- Partial \_DailyTaskCard dùng chung.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- Không nhồi quá nhiều màu/animation làm khó demo.

- Không dùng màu là tín hiệu duy nhất; cần text/badge.

### 6. Tiêu chí hoàn thành

- Desktop/mobile không vỡ layout.

- Học sinh dễ thấy nút bắt đầu học.

- UI nhất quán với M9 Path UI.

## Task 20: Test case, seed data demo và bàn giao module

**Mục tiêu:** Hoàn thiện test case, seed data và bàn giao module để tích
hợp vào luồng học tập đầy đủ.

### 1. Nghiệp vụ cần hiểu

- Module hoàn thành khi học sinh có thể xem kế hoạch và mở task đúng
  module.

- Seed data phải đủ hôm nay/tuần/tháng/quá hạn/hoàn thành.

### 2. CSDL cần làm

- Seed 1 student có active path và node scheduled_date trong tuần.

- Seed task today, overdue, completed, review_needed.

- Seed link sang lesson/quiz/practice.

### 3. Backend / Service cần làm

- Viết test: ownership, today plan, week plan, month plan, open task,
  complete task, overdue, skip.

- Checklist tích hợp M8/M9/M11/M12/M16.

### 4. Controller / View MVC đề xuất

- Kiểm tra Views Today/Week/Month, Admin Preview, partial card.

- Chuẩn bị dữ liệu demo trình bày.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

- Không bàn giao nếu học sinh xem được kế hoạch người khác.

- Không bàn giao nếu task không mở được đúng target.

- Không bàn giao nếu completed không cập nhật progress.

### 6. Tiêu chí hoàn thành

- Demo Today/Week/Month chạy được.

- Test case luồng chính pass.

- Có seed data và tài liệu bàn giao.

# VII. Thứ tự triển khai khuyến nghị

> 1\. Task 1 - Rà soát/chỉnh CSDL kế hoạch học tập.
>
> 2\. Task 2 - Chuẩn hóa trạng thái task và quy tắc lịch học.
>
> 3\. Task 3 - Tạo ViewModel/DTO.
>
> 4\. Task 4 - Xây StudyPlanService.
>
> 5\. Task 5 đến Task 8 - Xây màn Today/Week/Month và partial
> \_DailyTaskCard.
>
> 6\. Task 9 đến Task 11 - Điều hướng, hoàn thành, bỏ qua/quá hạn.
>
> 7\. Task 12 đến Task 15 - Tính tải học, liên kết Progress, Replanning
> và Notification.
>
> 8\. Task 16 - Admin/Giáo viên preview read-only.
>
> 9\. Task 17 đến Task 19 - Tối ưu, xử lý lỗi, UI responsive.
>
> 10\. Task 20 - Test case, seed data demo và bàn giao module.

# VIII. Chốt nghiệp vụ cuối cùng

Module M10 - Kế hoạch học tập tháng/tuần/ngày là module chuyển Learning
Path thành lịch hành động cụ thể cho học sinh. Trong ASP.NET MVC, module
nên triển khai bằng StudyPlanController, StudyPlanService, các View
Today/Week/Month và partial \_DailyTaskCard. Backend là nguồn kiểm tra
quyền, trạng thái task, điều kiện mở và cập nhật hoàn thành; frontend
chỉ hiển thị và gửi hành động hợp lệ. Module này phải liên kết chặt với
M8 AI Learning Path, M9 Path UI, M11 Lesson, M12 Quiz, M16 Progress, M17
Replanning và M18 Notification. Dữ liệu kế hoạch là dữ liệu cá nhân, bắt
buộc kiểm tra ownership để học sinh chỉ xem và thao tác kế hoạch của
chính mình.
