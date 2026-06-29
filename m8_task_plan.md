**MODULE M8 - AI LEARNING PATH ENGINE**

**Dự án AI Study English - Phân tích nghiệp vụ, CSDL, Backend, MVC và
danh sách task**

Tài liệu này chốt nội dung triển khai module M8 - AI Learning Path
Engine theo hướng ASP.NET MVC. Module M8 nhận dữ liệu từ Onboarding,
Placement Test và AI phân tích năng lực để tạo lộ trình học Tiếng Anh cá
nhân hóa theo tháng, tuần, ngày và node học. Mục tiêu là giúp học sinh
biết rõ cần học gì, học theo thứ tự nào, học bao lâu và vì sao AI đề
xuất như vậy.

Nguyên tắc triển khai bắt buộc: Task 1 luôn là rà soát/chỉnh CSDL để
đúng nghiệp vụ trước. Chỉ sau khi CSDL ổn định mới triển khai Model,
Service, Controller, Razor View, xử lý AI, phân quyền và kiểm thử.

# Tóm tắt nhanh

| **Nội dung** | Chốt nghiệp vụ |
|----|----|
| **Mục tiêu module** | Sinh learning path cá nhân hóa dựa trên profile, placement test, competency analysis, topic/lesson/quiz đã duyệt. |
| **Đầu vào chính** | Learning profile từ M3, Placement Test từ M6, Competency Report từ M7, khung kỹ năng/topic từ M4, bài học/quiz từ M11/M12. |
| **Đầu ra chính** | StudentLearningPath, LearningPathNode, lý do AI đề xuất, trạng thái node, dữ liệu cho M9 Path UI và M10 Kế hoạch học tập. |
| **Actor chính** | Học sinh, AI Engine, Admin, Giáo viên/Content Reviewer. |
| **Ràng buộc quan trọng** | AI chỉ được chọn topic, lesson, quiz đã active/published/approved; không dùng nội dung chưa duyệt hoặc nguồn không hợp pháp. |

# I. Phạm vi module

Module M8 - AI Learning Path Engine quản lý quá trình tạo và lưu lộ
trình học Tiếng Anh cá nhân hóa. Module không chỉ gọi AI để sinh văn
bản, mà phải chuyển kết quả AI thành dữ liệu có cấu trúc để hệ thống
dùng được trong Path UI, kế hoạch học tập, quiz, feedback và progress
tracking.

- Nhận dữ liệu đầu vào từ hồ sơ học tập, bài test đầu vào, báo cáo năng
  lực và khung kỹ năng Tiếng Anh.

- Tạo lộ trình macro theo giai đoạn/tháng và micro theo tuần/ngày.

- Tạo danh sách node học gồm learn, quiz, practice, review, test,
  ai_tutor.

- Lưu lý do AI đề xuất từng node để học sinh hiểu vì sao cần học nội
  dung đó.

- Kiểm soát nội dung được đưa vào path: chỉ dùng topic, lesson, quiz đã
  duyệt và còn hoạt động.

- Hỗ trợ tạo lại lộ trình khi mục tiêu thay đổi hoặc khi M17 AI
  Replanning yêu cầu.

# II. CSDL hiện tại/cần bám theo

- student_learning_profiles: hồ sơ học tập, mục tiêu, thời gian học,
  trình độ tự đánh giá.

- placement_tests, test_attempts, test_answers: dữ liệu bài test đầu
  vào.

- competency_analyses, competency_skill_scores: báo cáo năng lực từ M7.

- english_skills, english_proficiency_levels, learning_topics,
  learning_objectives: khung kỹ năng/topic dùng làm xương sống lộ trình.

- original_lessons, quizzes, practice_tasks: nội dung học/luyện tập đã
  duyệt để gắn vào node.

- learning_path_templates, learning_path_template_nodes: mẫu lộ trình và
  node mẫu nếu dùng fallback/template.

- student_learning_paths, learning_path_nodes: lộ trình cá nhân và các
  bước học cụ thể của từng học sinh.

- ai_prompt_templates, ai_usage_logs, ai_generated_contents: prompt, log
  gọi AI, output AI và version phục vụ truy vết.

# III. Phân quyền vai trò

| **Vai trò** | **Phạm vi** | **Quyền chính trong M8** |
|----|----|----|
| Học sinh | Dữ liệu cá nhân | Xem path của mình, yêu cầu tạo lại path khi mục tiêu thay đổi nếu hệ thống cho phép, xem lý do AI đề xuất. |
| Admin | Toàn hệ thống | Cấu hình template path, prompt AI, xem log tạo path, xử lý lỗi AI, quản lý dữ liệu nền. |
| Giáo viên/Reviewer | Nội dung được phân quyền | Kiểm tra path template, xác nhận topic/lesson/quiz phù hợp, duyệt nội dung trước khi AI dùng. |
| AI Engine | Tác nhân hệ thống | Phân tích input và sinh path có cấu trúc theo schema được cấu hình. |

# IV. Luồng tổng quát

Luồng 1: Tạo lộ trình lần đầu sau placement test

1\. Học sinh hoàn thành onboarding và placement test.

2\. M7 tạo competency report gồm điểm mạnh/yếu, level hiện tại và topic
ưu tiên.

3\. M8 lấy profile, competency report, topic/lesson/quiz hợp lệ.

4\. AI sinh learning path theo tháng/tuần/ngày và danh sách node.

5\. Backend validate output AI, mapping thành student_learning_paths và
learning_path_nodes.

6\. Học sinh xem path trên M9 và kế hoạch chi tiết trên M10.

Luồng 2: Tạo lại lộ trình khi mục tiêu thay đổi

1\. Học sinh thay đổi goal, target level, target score hoặc thời gian
học.

2\. Hệ thống cảnh báo path hiện tại có thể không còn phù hợp.

3\. M8 tạo phiên bản path mới hoặc đánh dấu path cũ archived/paused.

4\. Path mới được lưu version, có lý do thay đổi và không mất lịch sử
path cũ.

# V. Danh sách task triển khai

1\. Rà soát và chuẩn hóa CSDL AI Learning Path

2\. Chuẩn hóa input từ Onboarding, Placement Test và Competency Analysis

3\. Thiết kế Entity/Model/ViewModel cho Learning Path

4\. Xây dựng LearningPathService và Repository/DAL

5\. Thiết kế prompt và output schema cho AI Learning Path

6\. Tạo lộ trình lần đầu sau khi M7 hoàn tất phân tích năng lực

7\. Mapping output AI thành StudentLearningPath và LearningPathNode

8\. Quản lý Learning Path Template và Node Template

9\. Xử lý điều kiện mở khóa node và prerequisite topic

10\. Xây dựng LearningPathController phía học sinh

11\. Xây dựng View hiển thị bản tóm tắt learning path

12\. Xây dựng Admin/Teacher View quản lý template và lịch sử tạo path

13\. Tạo lại path khi học sinh đổi mục tiêu học tập

14\. Xử lý AI lỗi, timeout và fallback bằng template

15\. Kiểm soát nội dung hợp pháp trước khi đưa vào path

16\. Lưu version, AI reasoning summary và AI usage log

17\. Phân quyền và kiểm tra ownership dữ liệu path

18\. Tích hợp M8 với M9, M10, M12, M13, M16 và M17

19\. Kiểm thử nghiệp vụ và test case cho Learning Path

20\. Checklist bàn giao module M8

# VI. Chi tiết từng task

## Task 1: Rà soát và chuẩn hóa CSDL AI Learning Path

**Mục tiêu:** Chuẩn hóa nền dữ liệu để module AI Learning Path có thể
tạo, lưu, version hóa và hiển thị lộ trình ổn định.

**1. Nghiệp vụ cần hiểu**

- Learning Path là dữ liệu lõi, không chỉ là text AI trả về.

- Path phải có bảng cha và bảng con node.

- Node cần gắn topic/lesson/quiz/practice nếu có.

- Phải lưu lý do AI đề xuất và trạng thái node.

**2. CSDL / Backend cần làm**

- Kiểm tra student_learning_paths, learning_path_nodes,
  learning_path_templates, learning_path_template_nodes.

- Bổ sung field: generated_by_ai, ai_plan_summary, ai_reason, status,
  order_index, scheduled_date, estimated_minutes nếu thiếu.

- Cân nhắc path_version, archived_at, replaced_by_path_id.

- Tạo index theo student_id, status, learning_path_id, order_index.

**3. Controller / View MVC đề xuất**

- Không cần View ở Task 1; chỉ làm migration và kiểm tra schema.

- Có thể tạo Admin/Diagnostics/LearningPathSchema nếu cần kiểm tra
  nhanh.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- Không chạy migration làm mất path cũ.

- Không xóa cứng path/node đã có progress.

- Nếu có path dạng text cũ thì cần script migrate.

**5. Tiêu chí hoàn thành**

- CSDL đủ FK/index/check constraint.

- Có rule không xóa cứng dữ liệu path.

- Migration chạy được trên database dev.

## Task 2: Chuẩn hóa input từ Onboarding, Placement Test và Competency Analysis

**Mục tiêu:** Đảm bảo dữ liệu đầu vào cho AI tạo path đầy đủ, nhất quán
và có thể serialize thành prompt an toàn.

**1. Nghiệp vụ cần hiểu**

- AI phải dựa vào dữ liệu thật: profile, test attempt, competency report
  và topic hợp lệ.

- Input thiếu phải redirect hoặc báo lỗi rõ ràng.

- Danh sách nội dung đưa vào AI chỉ gồm nội dung đã duyệt.

**2. CSDL / Backend cần làm**

- Tạo LearningPathInputDto gồm goal, target level, available time, skill
  priorities.

- Lấy score theo skill/topic từ M6.

- Lấy strengths, weaknesses, priority topics từ M7.

- Lấy topic/lesson/quiz active/published/approved.

**3. Controller / View MVC đề xuất**

- Controller gọi LearningPathService.BuildInputAsync(studentId).

- Admin có thể xem preview input tại
  Admin/LearningPaths/InputPreview/{studentId}.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- Thiếu onboarding thì redirect về M3.

- Thiếu placement test thì redirect về M6.

- Thiếu competency analysis thì yêu cầu chạy M7 trước.

**5. Tiêu chí hoàn thành**

- Input DTO đầy đủ.

- Có validate dữ liệu thiếu.

- Không đưa nội dung chưa duyệt vào prompt.

## Task 3: Thiết kế Entity/Model/ViewModel cho Learning Path

**Mục tiêu:** Tạo cấu trúc model phù hợp cho MVC, tránh để Controller xử
lý entity phức tạp.

**1. Nghiệp vụ cần hiểu**

- MVC cần tách Entity, ViewModel và DTO.

- Path có nhiều tầng: tổng quan, phase, node, task.

- ViewModel phải đủ dữ liệu cho học sinh biết hôm nay cần học gì.

**2. CSDL / Backend cần làm**

- Tạo/kiểm tra Entity: StudentLearningPath, LearningPathNode,
  LearningPathTemplate, LearningPathTemplateNode.

- Tạo ViewModel: LearningPathSummaryVm, LearningPathNodeVm,
  LearningPathPreviewVm.

- Tạo constant/enum cho node type và status.

**3. Controller / View MVC đề xuất**

- LearningPathController trả ViewModel, không trả thẳng Entity.

- Partial View: \_PathNodeCard.cshtml, \_PathProgressSummary.cshtml.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- Không bind trực tiếp toàn bộ Entity từ form.

- Node thiếu lesson/quiz vẫn hiển thị được dạng task thủ công.

- Status không hợp lệ phải bị reject.

**5. Tiêu chí hoàn thành**

- Có đầy đủ Entity/ViewModel/DTO.

- View render được path/node/status cơ bản.

## Task 4: Xây dựng LearningPathService và Repository/DAL

**Mục tiêu:** Tập trung logic tạo, lưu, đọc, cập nhật path vào Service
để Controller mỏng và dễ test.

**1. Nghiệp vụ cần hiểu**

- Learning Path liên quan nhiều module nên không viết logic trong
  Controller.

- Service kiểm tra điều kiện tạo path, nội dung hợp lệ và transaction.

- Repository/DAL đóng gói truy vấn.

**2. CSDL / Backend cần làm**

- Tạo ILearningPathService: GenerateInitialPathAsync,
  GetActivePathAsync, GetPathDetailAsync, ArchivePathAsync,
  RegeneratePathAsync.

- Dùng transaction khi lưu path và nodes.

- Không lưu path nếu output AI validate thất bại.

**3. Controller / View MVC đề xuất**

- LearningPathController gọi service.

- Admin controller dùng chung service để tránh lệch nghiệp vụ.

- Route: LearningPath/Index, LearningPath/Generate,
  LearningPath/Detail/{id}.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- Nếu đã có active path thì không tạo trùng.

- Save nodes lỗi thì rollback path cha.

- User không phải owner thì 403/404.

**5. Tiêu chí hoàn thành**

- Service tạo được path đầy đủ.

- Controller mỏng, dễ đọc.

- Có transaction khi lưu path.

## Task 5: Thiết kế prompt và output schema cho AI Learning Path

**Mục tiêu:** Định nghĩa prompt và schema để AI trả kết quả có cấu trúc,
dễ validate, dễ lưu DB.

**1. Nghiệp vụ cần hiểu**

- AI phải trả path dạng dữ liệu, không chỉ đoạn văn.

- Output cần có month/week/day/node, action_type, estimated_minutes,
  reason.

- Prompt phải giới hạn AI dùng topic/lesson/quiz được backend cung cấp.

**2. CSDL / Backend cần làm**

- Tạo prompt template module_code = LEARNING_PATH.

- Thiết kế JSON schema gồm path_title, summary, phases, weekly_plan,
  nodes.

- Lưu prompt version.

- Tạo validator kiểm tra JSON output.

**3. Controller / View MVC đề xuất**

- Admin/AiPromptsController cho phép xem/sửa prompt theo version.

- Không cho học sinh thấy prompt kỹ thuật.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- AI trả JSON lỗi thì không lưu path.

- AI tạo topic không tồn tại thì reject.

- Thời lượng vượt thời gian học khai báo thì cảnh báo hoặc giảm tải.

**5. Tiêu chí hoàn thành**

- Có prompt template rõ ràng.

- AI output validate được.

- Có log prompt version.

## Task 6: Tạo lộ trình lần đầu sau khi M7 hoàn tất phân tích năng lực

**Mục tiêu:** Triển khai luồng tạo path lần đầu ngay sau M7.

**1. Nghiệp vụ cần hiểu**

- Path đầu tiên quyết định trải nghiệm chính của học sinh.

- Chỉ tạo path khi đã có đủ profile, placement test và competency
  report.

**2. CSDL / Backend cần làm**

- Viết GenerateInitialPathAsync(studentId, competencyAnalysisId).

- Kiểm tra active path hiện có.

- Lưu audit log CREATED_LEARNING_PATH.

**3. Controller / View MVC đề xuất**

- Button “Tạo lộ trình học” ở trang kết quả M7 gọi
  LearningPath/Generate.

- Tạo xong redirect LearningPath/Index.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- Chưa đủ dữ liệu thì redirect đúng module còn thiếu.

- AI lỗi thì fallback hoặc hiển thị thử lại.

**5. Tiêu chí hoàn thành**

- Tạo được path lần đầu.

- Không tạo trùng active path.

## Task 7: Mapping output AI thành StudentLearningPath và LearningPathNode

**Mục tiêu:** Chuyển kết quả AI JSON thành dữ liệu path/node có thể hiển
thị và tracking.

**1. Nghiệp vụ cần hiểu**

- AI output phải được backend kiểm tra và mapping lại.

- Node phải có order, type, status, estimated time.

**2. CSDL / Backend cần làm**

- Tạo LearningPathMapper.

- Node đầu tiên AVAILABLE/CURRENT, node sau LOCKED.

- Validate FK topic/lesson/quiz tồn tại và approved.

**3. Controller / View MVC đề xuất**

- View detail dùng dữ liệu đã mapping.

- Admin có thể xem raw AI output nếu có quyền debug.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- Node thiếu action_type thì reject.

- Order trùng thì backend sắp xếp lại hoặc reject.

- Path rỗng thì không lưu.

**5. Tiêu chí hoàn thành**

- Mapping đúng entity.

- Không có node tham chiếu dữ liệu không tồn tại.

## Task 8: Quản lý Learning Path Template và Node Template

**Mục tiêu:** Tạo cơ chế template để fallback và cấu hình khung lộ trình
mẫu.

**1. Nghiệp vụ cần hiểu**

- Template giúp hệ thống không phụ thuộc hoàn toàn vào AI.

- Template dùng cho goal phổ biến như giao tiếp cơ bản, school English,
  IELTS orientation.

**2. CSDL / Backend cần làm**

- CRUD learning_path_templates và learning_path_template_nodes.

- Template có goal, start_level, target_level, duration_weeks.

- Chỉ template PUBLISHED mới dùng fallback.

**3. Controller / View MVC đề xuất**

- Admin/PathTemplatesController: Index, Create, Edit, Details, Publish,
  Archive.

- Razor View thêm/sắp xếp node template.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- Không publish template không có node.

- Không xóa template đã dùng; chỉ archive.

**5. Tiêu chí hoàn thành**

- Admin tạo/publish template.

- Template dùng được làm fallback.

## Task 9: Xử lý điều kiện mở khóa node và prerequisite topic

**Mục tiêu:** Đảm bảo học sinh học theo thứ tự hợp lý và không truy cập
node chưa đủ điều kiện.

**1. Nghiệp vụ cần hiểu**

- Path kiểu Duolingo cần locked/available/completed.

- Một số topic cần prerequisite trước.

**2. CSDL / Backend cần làm**

- Thêm unlock_condition hoặc required_node_id nếu cần.

- Viết CanAccessNodeAsync và UnlockNextNodeAfterCompletionAsync.

**3. Controller / View MVC đề xuất**

- LearningPath/StartNode/{id} kiểm tra quyền mở node.

- View hiển thị lý do node bị khóa.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- Không cho vào node locked bằng URL.

- Tránh deadlock khiến học sinh không học được gì.

**5. Tiêu chí hoàn thành**

- Khóa/mở node đúng.

- Hoàn thành node mở node tiếp theo.

## Task 10: Xây dựng LearningPathController phía học sinh

**Mục tiêu:** Tạo Controller MVC cho học sinh xem, tạo, mở node và theo
dõi path cá nhân.

**1. Nghiệp vụ cần hiểu**

- Học sinh chỉ xem path của mình.

- Controller điều hướng theo trạng thái thiếu
  onboarding/test/analysis/path.

**2. CSDL / Backend cần làm**

- Action: Index, Generate, Detail, StartNode, CompleteNode,
  RegenerateRequest.

- Dùng Authorize role STUDENT và kiểm tra ownership ở Service.

**3. Controller / View MVC đề xuất**

- Views/LearningPath/Index.cshtml, Detail.cshtml, Generate.cshtml.

- Partial \_NodeCard.cshtml.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- Chưa onboarding/test/analysis thì redirect đúng nơi.

- Không có path thì hiển thị nút tạo path.

**5. Tiêu chí hoàn thành**

- Controller đủ action chính.

- Không có logic AI nặng trong Controller.

## Task 11: Xây dựng View hiển thị bản tóm tắt learning path

**Mục tiêu:** Hiển thị path ở mức dễ hiểu trước khi làm giao diện node
nâng cao ở M9.

**1. Nghiệp vụ cần hiểu**

- Học sinh cần nhìn thấy mục tiêu, node hiện tại, số ngày/tuần và lý do
  AI.

- View cơ bản giúp test dữ liệu M8 trước.

**2. CSDL / Backend cần làm**

- Tạo ViewModel có summary, current node, next node, total nodes,
  completed nodes.

- Trả ai_plan_summary và top priority topics.

**3. Controller / View MVC đề xuất**

- Index.cshtml hiển thị tổng quan.

- Detail.cshtml hiển thị danh sách node dạng bảng/card đơn giản.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- Path chưa có node thì hiện trạng thái lỗi dữ liệu.

- Không hiển thị raw prompt/output nhạy cảm.

**5. Tiêu chí hoàn thành**

- Học sinh xem được tóm tắt path.

- Node hiện tại/tiếp theo hiển thị đúng.

## Task 12: Xây dựng Admin/Teacher View quản lý template và lịch sử tạo path

**Mục tiêu:** Cho Admin/Giáo viên kiểm tra template, lịch sử tạo path và
lỗi AI.

**1. Nghiệp vụ cần hiểu**

- Admin cần biết path nào tạo lỗi, prompt version nào được dùng.

- Reviewer cần biết AI có dùng topic/bài học hợp lệ không.

**2. CSDL / Backend cần làm**

- Tạo query lịch sử path theo student, date, status, prompt version.

- Lưu created_by/system, ai_model, error_message nếu có.

**3. Controller / View MVC đề xuất**

- Admin/LearningPaths/Index, Details, GenerationLogs.

- Teacher chỉ xem phạm vi được cấp quyền.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- Không lộ dữ liệu cá nhân quá mức.

- Không cho teacher sửa path cá nhân nếu không có quyền.

**5. Tiêu chí hoàn thành**

- Admin xem được lịch sử tạo path.

- Có màn hình log lỗi AI/path.

## Task 13: Tạo lại path khi học sinh đổi mục tiêu học tập

**Mục tiêu:** Cho phép tạo lại path khi mục tiêu hoặc thời gian học của
học sinh thay đổi.

**1. Nghiệp vụ cần hiểu**

- Mục tiêu thay đổi làm path cũ có thể không còn phù hợp.

- Không được mất lịch sử path cũ.

**2. CSDL / Backend cần làm**

- RegeneratePathAsync: archive/paused path cũ, tạo path mới version mới.

- Lưu reason và replaced_by_path_id nếu có.

**3. Controller / View MVC đề xuất**

- LearningPath/RegenerateRequest hiển thị xác nhận.

- Profile thay đổi lớn thì gợi ý tạo lại path.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- Không tạo lại quá nhiều lần trong thời gian ngắn nếu cấu hình giới
  hạn.

- Không xóa path cũ đã có progress.

**5. Tiêu chí hoàn thành**

- Tạo lại path được.

- Path cũ vẫn xem lịch sử được.

## Task 14: Xử lý AI lỗi, timeout và fallback bằng template

**Mục tiêu:** Xử lý lỗi AI để hệ thống không bị đứng khi AI timeout hoặc
output sai schema.

**1. Nghiệp vụ cần hiểu**

- AI là dịch vụ ngoài/khó đoán, phải có fallback.

- Không được lưu dữ liệu rác khi AI trả sai format.

**2. CSDL / Backend cần làm**

- Bọc call AI bằng try/catch, timeout, retry có giới hạn.

- Validate output schema.

- Fallback bằng learning_path_template nếu có.

- Ghi ai_usage_logs.

**3. Controller / View MVC đề xuất**

- View hiển thị thông báo thân thiện: đang tạo, lỗi, thử lại.

- Admin xem error detail trong khu quản trị.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- Timeout thì không tạo path lỗi.

- Output sai schema thì reject và log.

- Không gọi AI lặp vô hạn.

**5. Tiêu chí hoàn thành**

- AI lỗi không làm sập web.

- Có fallback hoặc thông báo rõ.

## Task 15: Kiểm soát nội dung hợp pháp trước khi đưa vào path

**Mục tiêu:** Đảm bảo path không dùng nội dung chưa duyệt hoặc có rủi ro
bản quyền.

**1. Nghiệp vụ cần hiểu**

- M8 phải tuân thủ M5/M20 về nguồn học liệu.

- AI chỉ được chọn nội dung hợp pháp đã duyệt.

**2. CSDL / Backend cần làm**

- Filter topic/lesson/quiz/practice theo status
  active/published/approved.

- Không đưa reference_only content làm nội dung chính.

- Ghi log nếu AI đề xuất nội dung bị chặn.

**3. Controller / View MVC đề xuất**

- Admin thấy cảnh báo nếu nội dung trong path bị archived sau này.

- Teacher có màn kiểm tra node dùng nội dung hợp lệ.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- Lesson bị archived thì node cần review/update.

- Nguồn chưa duyệt không xuất hiện trong path.

**5. Tiêu chí hoàn thành**

- Không có node dùng nội dung chưa duyệt.

- Có compliance check trước khi lưu path.

## Task 16: Lưu version, AI reasoning summary và AI usage log

**Mục tiêu:** Lưu đầy đủ version, lý do AI và log usage để truy vết.

**1. Nghiệp vụ cần hiểu**

- Path AI cần giải thích được vì sao sinh ra.

- Prompt/model có thể thay đổi theo thời gian nên phải lưu version.

**2. CSDL / Backend cần làm**

- Lưu ai_plan_summary, ai_reason từng node, prompt_template_id,
  ai_model.

- Ghi ai_usage_logs token/cost/status nếu có.

- Lưu raw output có kiểm soát cho Admin debug nếu cần.

**3. Controller / View MVC đề xuất**

- Admin xem version/log trong Details.

- Student chỉ xem lý do học tập, không xem log kỹ thuật.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- Không lưu secret/API key trong log.

- Không hiển thị raw prompt cho học sinh.

**5. Tiêu chí hoàn thành**

- Có log AI usage.

- Có lý do AI đề xuất node.

## Task 17: Phân quyền và kiểm tra ownership dữ liệu path

**Mục tiêu:** Bảo vệ dữ liệu path cá nhân theo đúng role và ownership.

**1. Nghiệp vụ cần hiểu**

- Path chứa dữ liệu học tập cá nhân, không được lộ cho học sinh khác.

- Admin/Teacher chỉ xem trong quyền được cấp.

**2. CSDL / Backend cần làm**

- Service kiểm tra studentId = currentUser hoặc role hợp lệ.

- Controller dùng Authorize và kiểm tra ownership.

- Ẩn dữ liệu kỹ thuật trong ViewModel học sinh.

**3. Controller / View MVC đề xuất**

- LearningPathController chỉ cho Student xem path của mình.

- Admin/Teacher controller tách area riêng.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- Student A nhập URL path của Student B thì 403/404.

- Teacher không có quyền không xem được.

**5. Tiêu chí hoàn thành**

- Ownership đúng.

- Không lộ dữ liệu path cá nhân.

## Task 18: Tích hợp M8 với M9, M10, M12, M13, M16 và M17

**Mục tiêu:** Kết nối M8 với các module còn lại để path vận hành thật.

**1. Nghiệp vụ cần hiểu**

- M8 là trung tâm giữa AI analysis, path UI, study plan, quiz, feedback
  và progress.

- Nếu tích hợp thiếu, path chỉ là dữ liệu chết.

**2. CSDL / Backend cần làm**

- M9 đọc learning_path_nodes để hiển thị path.

- M10 chuyển nodes thành daily/weekly plan.

- M12 gắn quiz/practice vào node.

- M13 dùng node/topic để feedback.

- M16 cập nhật progress khi node hoàn thành.

- M17 dùng progress để replanning.

**3. Controller / View MVC đề xuất**

- Tạo link từ node đến Lesson/Quiz/Practice tương ứng.

- Dashboard hiển thị current path summary.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- Node tham chiếu module chưa triển khai thì hiển thị trạng thái coming
  soon hoặc task thủ công.

- Không để link chết gây lỗi 500.

**5. Tiêu chí hoàn thành**

- Path liên kết được lesson/quiz/progress.

- Hoàn thành quiz ảnh hưởng trạng thái node.

## Task 19: Kiểm thử nghiệp vụ và test case cho Learning Path

**Mục tiêu:** Kiểm thử các luồng chính, luồng thiếu dữ liệu và lỗi AI
của module M8.

**1. Nghiệp vụ cần hiểu**

- Module AI path dễ lỗi ở dữ liệu thiếu, JSON sai, quyền truy cập sai.

- Cần test đủ trước khi bàn giao cho M9/M10.

**2. CSDL / Backend cần làm**

- Viết test case: đủ dữ liệu tạo path, thiếu onboarding, thiếu test,
  thiếu M7 report, AI timeout, output sai schema, nội dung chưa duyệt.

- Test service mapping và unlock node.

- Kiểm tra transaction rollback.

**3. Controller / View MVC đề xuất**

- Test MVC bằng thao tác trên View: Generate, Detail, StartNode,
  CompleteNode.

- Kiểm tra phân quyền Student/Admin/Teacher.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- Không bỏ qua test lỗi AI.

- Không dùng dữ liệu seed quá hoàn hảo khiến bỏ sót case thiếu dữ liệu.

**5. Tiêu chí hoàn thành**

- Có checklist test pass.

- Không có lỗi 500 ở luồng phổ biến.

- Có bằng chứng test bằng screenshot/log nếu nhóm cần.

## Task 20: Checklist bàn giao module M8

**Mục tiêu:** Tổng hợp checklist bàn giao module M8 cho nhóm frontend/UI
và các module liên quan.

**1. Nghiệp vụ cần hiểu**

- M8 bàn giao dữ liệu cho M9/M10/M12/M16 nên cần tài liệu rõ.

- Nếu bàn giao mập mờ, các module sau khó dùng path.

**2. CSDL / Backend cần làm**

- Tổng hợp bảng route/controller/service/model/viewmodel.

- Ghi rõ trạng thái node và ý nghĩa.

- Ghi rõ schema AI output và mapping DB.

- Ghi danh sách seed data cần có.

**3. Controller / View MVC đề xuất**

- Bàn giao ViewModel mẫu cho M9/M10.

- Bàn giao route và cách lấy current path/current node.

**4. Xử lý ngoài luồng / lỗi nghiệp vụ**

- Không bàn giao khi chưa có seed topic/lesson/quiz test.

- Không bàn giao nếu path không tạo được từ dữ liệu thật.

**5. Tiêu chí hoàn thành**

- Có tài liệu bàn giao.

- Có dữ liệu mẫu tạo được path.

- Các module M9/M10 dùng được output M8.

# VII. Thứ tự triển khai khuyến nghị

1\. Task 1 - Rà soát CSDL.

2\. Task 2 - Chuẩn hóa input từ M3/M6/M7/M4.

3\. Task 3 + 4 - Model/ViewModel và Service/Repository.

4\. Task 5 - Prompt/schema AI.

5\. Task 6 + 7 - Tạo path lần đầu và mapping output AI.

6\. Task 8 + 9 - Template fallback và unlock condition.

7\. Task 10 + 11 - Controller/View học sinh.

8\. Task 12 - Admin/Teacher quản lý template/log.

9\. Task 13 + 14 - Regenerate và fallback khi AI lỗi.

10\. Task 15 + 16 - Compliance, version và AI usage log.

11\. Task 17 + 18 - Phân quyền và tích hợp liên module.

12\. Task 19 + 20 - Test case và bàn giao.

# VIII. Chốt nghiệp vụ cuối cùng

Module M8 - AI Learning Path Engine là lõi cá nhân hóa của AI Study
English. Module nhận learning profile, placement test, competency
analysis và khung kỹ năng Tiếng Anh đã được quản trị để tạo lộ trình học
theo tháng, tuần, ngày. Backend không lưu kết quả AI dưới dạng văn bản
tự do đơn thuần mà phải mapping thành path và node có cấu trúc, có trạng
thái, thứ tự, thời lượng, hành động cụ thể và lý do đề xuất. AI chỉ được
sử dụng topic, bài học, quiz và tài nguyên đã được duyệt; mọi output
quan trọng phải lưu version, prompt/model và log để truy vết. Học sinh
chỉ xem path của mình; Admin/Giáo viên xem và quản trị theo đúng quyền.
