**MODULE M9 - GIAO DIỆN LEARNING PATH KIỂU DUOLINGO**

**AI STUDY ENGLISH - ASP.NET MVC + RAZOR VIEW + SQL SERVER**

*Tài liệu phân tích nghiệp vụ, CSDL, Backend, Controller/View MVC và danh sách task triển khai*

|  |  |
| --- | --- |
| **Dự án** | AI Study English - hệ thống học Tiếng Anh cá nhân hóa bằng AI Tutor và Learning Path |
| **Module** | M9 - Giao diện Learning Path kiểu Duolingo |
| **Hướng triển khai** | ASP.NET MVC, Razor View, Service/Repository, SQL Server |
| **Actor chính** | Học sinh, Admin/Giáo viên xem preview, AI Engine cung cấp dữ liệu path |
| **Nguyên tắc bắt buộc** | Task 1 luôn rà soát và chỉnh CSDL trước để đúng nghiệp vụ |
| **Module liên kết** | M8 AI Learning Path, M10 Kế hoạch học tập, M11 Bài học, M12 Quiz, M13 AI Feedback, M16 Progress, M17 Replanning |

# Tóm tắt nhanh

|  |  |
| --- | --- |
| **Nội dung** | **Chốt nghiệp vụ** |
| **Mục đích** | Hiển thị learning path cá nhân hóa thành giao diện node/chặng trực quan giống Duolingo để học sinh biết bước hiện tại, bước tiếp theo và các bước cần ôn lại. |
| **Nguồn dữ liệu** | Dữ liệu path do M8 tạo ra, lưu trong student\_learning\_paths và learning\_path\_nodes. |
| **Trạng thái node** | LOCKED, AVAILABLE, IN\_PROGRESS, COMPLETED, NEED\_REVIEW, SKIPPED. *(Cập nhật: dùng NEED\_REVIEW theo codebase, không dùng CURRENT riêng — tính logic ở Service)* |
| **Hành động từ node** | Mở bài học, quiz, practice, review, test hoặc AI Tutor theo node\_type. |
| **Nguyên tắc UI** | Không hiển thị quá nhiều nhiệm vụ cùng lúc; tập trung vào node hiện tại, nhiệm vụ hôm nay và lý do AI đề xuất. |
| **MVC cần có** | LearningPathController, PathViewService, Razor View Index/Details, partial \_PathNodeCard, CSS responsive path layout. |

# I. Phạm vi module

Module M9 - Giao diện Learning Path kiểu Duolingo chịu trách nhiệm biến dữ liệu lộ trình học do M8 sinh ra thành trải nghiệm học tập trực quan cho học sinh. Module này không tự tạo path mới, không tự phân tích năng lực và không tự chấm quiz. Nó nhận dữ liệu path/node đã được backend lưu, hiển thị đúng trạng thái node, kiểm soát quyền truy cập node, điều hướng học sinh đến bài học/quiz/practice phù hợp và cập nhật giao diện sau khi học sinh hoàn thành nhiệm vụ.

* Hiển thị learning path dạng node/chặng thay vì bảng danh sách khô khan.
* Hiển thị node hiện tại, node đã hoàn thành, node bị khóa, node cần ôn lại và node bị bỏ qua.
* Cho học sinh bấm vào node đang mở để vào bài học, quiz, bài luyện tập hoặc AI Tutor.
* Hiển thị lý do AI đề xuất node, mục tiêu ngắn, thời lượng ước tính và hành động cần làm.
* ~~Tích hợp với M10 để hiển thị nhiệm vụ hôm nay/tuần này.~~ *(M10 chưa tồn tại — dùng ScheduledDate của node thay thế)*
* Tích hợp với M16 để hiển thị tiến độ tổng quan, streak và phần trăm hoàn thành.
* ~~Tích hợp với M13/M17 để hiển thị node cần ôn lại hoặc path đã được điều chỉnh.~~ *(M13 chưa tồn tại — NEED_REVIEW status đã có sẵn từ ProgressController)*

# II. CSDL cần bám theo

* student\_learning\_paths: lộ trình cá nhân hóa của học sinh, tiêu đề, mô tả, trạng thái, ngày bắt đầu/kết thúc.
* learning\_path\_nodes: từng node trong path, node\_type, status, order\_index, scheduled\_date, estimated\_minutes, ai\_reason.
* learning\_topics: chủ đề/kỹ năng Tiếng Anh mà node đang nhắm đến.
* original\_lessons: bài học Tiếng Anh liên kết với node loại LEARN.
* quizzes: quiz liên kết với node loại QUIZ.
* practice\_tasks: bài luyện tập liên kết với node loại PRACTICE.
* study\_activity\_logs và student\_progress\_snapshots: dữ liệu tiến độ dùng để hiển thị phần trăm và lịch sử học.
* ai\_feedbacks và ai\_replanning\_events: dữ liệu để cảnh báo review\_needed hoặc path vừa được điều chỉnh.

# III. Phân quyền trong module

|  |  |  |
| --- | --- | --- |
| **Vai trò** | **Phạm vi** | **Quyền chính** |
| **Học sinh** | Dữ liệu cá nhân | Xem learning path của mình, mở node đang khả dụng, xem lý do AI đề xuất, vào bài học/quiz/practice từ node. |
| **Admin** | Toàn hệ thống | Xem preview path của học sinh, kiểm tra lỗi path, hỗ trợ debug trạng thái node, không sửa path tùy tiện nếu không có nghiệp vụ. |
| **Giáo viên/Reviewer** | Phạm vi được cấp | Xem path của học sinh nếu được phân quyền để tư vấn, kiểm tra nội dung node liên kết với bài học/câu hỏi. |
| **AI Engine** | Tác nhân hệ thống | Cung cấp dữ liệu path/node từ M8, cập nhật đề xuất ôn lại qua M13/M17; không truy cập trực tiếp View. |

# IV. Luồng tổng quát

## Luồng 1: Học sinh xem Learning Path

1. Học sinh đăng nhập và đã hoàn thành Onboarding, Placement Test, AI Analysis và có path do M8 tạo.

2. Học sinh mở trang Learning Path.

3. Controller kiểm tra quyền sở hữu path và lấy danh sách node.

4. View hiển thị node theo thứ tự, trạng thái, icon, thời lượng, mục tiêu và lý do AI đề xuất.

5. Học sinh chọn node đang mở để bắt đầu học.

## Luồng 2: Học sinh mở node và học/làm bài

1. Học sinh bấm node available/current.

2. Backend kiểm tra node status và điều kiện mở khóa.

3. Tùy node\_type, hệ thống điều hướng đến Lesson, Quiz, Practice, Review, Test hoặc AI Tutor.

4. Sau khi hoàn thành activity, module liên quan cập nhật progress và node status.

5. Học sinh quay lại path và thấy node tiếp theo được mở nếu đủ điều kiện.

## Luồng 3: Node cần ôn lại hoặc path được AI điều chỉnh

1. Sau quiz hoặc feedback, M13/M17 đánh dấu topic yếu hoặc gợi ý ôn lại.

2. Learning path node được cập nhật review\_needed hoặc tạo node review mới.

3. Path UI hiển thị cảnh báo ôn lại kèm lý do.

4. Học sinh vào node review để học lại bài/quiz liên quan.

# V. Danh sách task triển khai

1. Rà soát nền CSDL Learning Path UI

2. Chuẩn hóa trạng thái node và quy ước hiển thị

3. Tạo Entity/ViewModel/DTO cho Path UI

4. Xây dựng PathViewService lấy path hiện tại

5. Kiểm tra quyền sở hữu path và phân quyền truy cập

6. Xây dựng LearningPathController cho học sinh

7. Xây dựng Razor View hiển thị path dạng node

8. Tạo partial view \_PathNodeCard và \_PathStageHeader

9. Hiển thị trạng thái locked/current/in\_progress/completed/review\_needed/skipped

10. Hiển thị nhiệm vụ hôm nay và node kế tiếp

11. Điều hướng node sang Lesson/Quiz/Practice/AI Tutor

12. Kiểm tra điều kiện mở khóa node

13. Cập nhật UI sau khi hoàn thành bài học/quiz/practice

14. Hiển thị lý do AI đề xuất và cảnh báo ôn lại

15. Thiết kế giao diện responsive kiểu Duolingo

16. Hiển thị progress bar, streak và tổng quan path

17. Hiển thị lịch sử path/version hoặc path đã archived

18. Admin/Giáo viên preview path học sinh dạng read-only

19. Tối ưu hiệu năng, logging và xử lý lỗi giao diện

20. Test case, seed data demo và bàn giao module

# VI. Chi tiết từng task

## Task 1: Rà soát nền CSDL Learning Path UI

**Mục tiêu:** Chuẩn hóa dữ liệu path/node để giao diện có thể hiển thị đúng trạng thái, thứ tự và điều kiện mở khóa.

### 1. Nghiệp vụ cần hiểu

* Path UI chỉ là lớp hiển thị nhưng phụ thuộc rất mạnh vào dữ liệu M8.
* Nếu CSDL thiếu status/order/node\_type thì UI không thể đúng nghiệp vụ.
* Task 1 bắt buộc rà soát CSDL trước khi code Controller/View.

### 2. CSDL cần làm

* Kiểm tra student\_learning\_paths: student\_id, title, status, start\_date, target\_end\_date, generated\_by\_ai.
* Kiểm tra learning\_path\_nodes: learning\_path\_id, node\_type, status, order\_index, scheduled\_date, estimated\_minutes, ai\_reason.
* ~~Bổ sung ui\_icon, ui\_color, short\_label~~ → Không cần thêm vào DB, tính ở ViewModel theo NodeType/Status.
* Đảm bảo FK đến lesson/quiz/practice/topic không bị orphan.

### 3. Backend / Service cần làm

* Viết script kiểm tra dữ liệu node thiếu order/status/type.
* Tạo enum/constant cho node status và node type.
* Không để frontend tự quyết trạng thái nghiệp vụ.

### 4. Controller / View MVC đề xuất

* Chưa code View trong task này; chỉ chuẩn bị nền dữ liệu cho LearningPathController.
* Tạo tài liệu mapping field DB -> PathNodeViewModel.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Thiếu node hiện tại thì phải có rule chọn node đầu tiên available.
* Node trỏ đến lesson/quiz chưa publish phải bị ẩn hoặc cảnh báo.
* Không migration làm mất path cũ.

### 6. Tiêu chí hoàn thành

* CSDL đủ field hiển thị path.
* Có enum status/type thống nhất.
* Không còn dữ liệu node mồ côi.

## Task 2: Chuẩn hóa trạng thái node và quy ước hiển thị

**Mục tiêu:** Chốt chuẩn trạng thái node để toàn bộ UI, Service và module liên quan dùng cùng một quy ước.

### 1. Nghiệp vụ cần hiểu

* Trạng thái node là ngôn ngữ chung giữa M8, M9, M12, M13, M16, M17.
* Nếu mỗi module hiểu status khác nhau, path sẽ lỗi rất khó debug.

### 2. CSDL cần làm

* Chuẩn status: LOCKED, AVAILABLE, IN\_PROGRESS, COMPLETED, NEED\_REVIEW, SKIPPED. *(Cập nhật: bỏ CURRENT, bỏ REVIEW\_NEEDED — dùng theo enum ProgressStatus.cs đã có)*
* Codebase dùng UPPER\_CASE. Enum đã có tại `Enums/ProgressStatus.cs`.
* `completed_at` đã có sẵn trong LearningPathNode.cs. Không cần thêm field.

### 3. Backend / Service cần làm

* Tạo NodeStatusHelper hoặc enum LearningPathNodeStatus.
* Viết hàm CanOpenNode(status).
* Viết hàm GetDisplayStyle(status).

### 4. Controller / View MVC đề xuất

* Không cần view riêng; dùng helper trong Razor/TagHelper.
* Chuẩn CSS class theo status.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Không cho mở LOCKED.
* REVIEW\_NEEDED phải mở được nếu mục đích là ôn lại.
* SKIPPED phải có lý do hoặc log.

### 6. Tiêu chí hoàn thành

* Status dùng thống nhất toàn hệ thống.
* UI hiển thị đúng màu/icon theo status.
* Service không hard-code string rải rác.

## Task 3: Tạo Entity/ViewModel/DTO cho Path UI

**Mục tiêu:** Tạo lớp dữ liệu trung gian cho View để tránh Razor query trực tiếp Entity phức tạp.

### 1. Nghiệp vụ cần hiểu

* MVC nên dùng ViewModel riêng cho UI path.
* Entity DB không nên đưa thẳng ra View nếu chứa dữ liệu không cần thiết.

### 2. CSDL cần làm

* Không bắt buộc tạo bảng mới.
* Đọc từ student\_learning\_paths, learning\_path\_nodes, topics, lessons, quizzes, practice\_tasks.

### 3. Backend / Service cần làm

* Tạo LearningPathPageViewModel.
* Tạo PathNodeViewModel gồm nodeId,title,type,status,estimatedMinutes,aiReason,targetUrl,isClickable.
* Tạo TodayTaskViewModel và PathProgressSummaryViewModel.

### 4. Controller / View MVC đề xuất

* Views/LearningPath/Index.cshtml nhận LearningPathPageViewModel.
* Partial \_PathNodeCard.cshtml nhận PathNodeViewModel.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Không để ViewModel chứa đáp án quiz hoặc nội dung nhạy cảm.
* Node thiếu targetUrl phải hiển thị disabled có lý do.

### 6. Tiêu chí hoàn thành

* ViewModel đủ dữ liệu render.
* Không query DB trong Razor.
* Build pass.

## Task 4: Xây dựng PathViewService lấy path hiện tại

**Mục tiêu:** Xây service lấy path hiện tại của học sinh và mapping thành ViewModel.

### 1. Nghiệp vụ cần hiểu

* Controller không nên tự join nhiều bảng.
* Service là nơi xử lý nghiệp vụ hiển thị path.

### 2. CSDL cần làm

* Query active path của user.
* Query nodes theo order\_index.
* Join topic/lesson/quiz/practice nếu cần title/target.

### 3. Backend / Service cần làm

* Tạo IPathViewService.GetCurrentPathPageAsync(userId).
* Tính currentNode, nextNode, progressPercent.
* Mapping node target URL theo node\_type.

### 4. Controller / View MVC đề xuất

* LearningPathController gọi service và trả View.
* Không để Controller chứa logic mapping dài.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Không có active path thì redirect sang M8 tạo path hoặc thông báo chưa có lộ trình.
* Nhiều active path thì chọn path mới nhất hoặc báo lỗi dữ liệu.

### 6. Tiêu chí hoàn thành

* Service trả đúng path hiện tại.
* Progress tính đúng theo node completed.
* Không lỗi khi path rỗng.

## Task 5: Kiểm tra quyền sở hữu path và phân quyền truy cập

**Mục tiêu:** Đảm bảo học sinh chỉ xem và mở learning path của chính mình.

### 1. Nghiệp vụ cần hiểu

* Learning path là dữ liệu học tập cá nhân.
* Không được tin userId từ query string nếu không kiểm tra ownership.

### 2. CSDL cần làm

* student\_learning\_paths phải có student\_id.
* learning\_path\_nodes phải gắn path\_id rõ ràng.

### 3. Backend / Service cần làm

* Tạo EnsurePathOwnerAsync.
* Admin/Teacher dùng luồng preview riêng có kiểm tra role.
* Học sinh không truyền tùy ý studentId.

### 4. Controller / View MVC đề xuất

* [Authorize(Roles="STUDENT")] cho LearningPathController student.
* Admin preview nằm trong Area Admin.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Student A không xem path Student B.
* Node id thuộc path khác phải trả 403/404.

### 6. Tiêu chí hoàn thành

* Phân quyền ownership đúng.
* Không lộ dữ liệu học sinh khác.
* Có test truy cập sai quyền.

## Task 6: Xây dựng LearningPathController cho học sinh

**Mục tiêu:** Xây Controller MVC cho học sinh xem path, mở node và xem chi tiết node.

### 1. Nghiệp vụ cần hiểu

* Controller là entry point chính của M9.
* Phải giữ gọn, gọi service thay vì xử lý nặng.

### 2. CSDL cần làm

* Không tạo bảng mới.
* Cần đọc path/nodes/status.

### 3. Backend / Service cần làm

* Action Index: xem path hiện tại.
* Action Details(nodeId): xem chi tiết node nếu cần.
* Action Open(nodeId): kiểm tra rồi redirect đến module đúng.

### 4. Controller / View MVC đề xuất

* LearningPathController.Index.
* LearningPathController.OpenNode.
* Views/LearningPath/Index.cshtml, Details.cshtml.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Không mở node locked.
* Node không tồn tại trả 404.
* Path archived không cho học sinh học tiếp.

### 6. Tiêu chí hoàn thành

* Controller chạy được.
* Index hiển thị path.
* Open node redirect đúng.

## Task 7: Xây dựng Razor View hiển thị path dạng node

**Mục tiêu:** Thiết kế View chính hiển thị lộ trình dạng chặng/node trực quan.

### 1. Nghiệp vụ cần hiểu

* Học sinh cần biết ngay hôm nay học gì.
* UI giống Duolingo là path/node chứ không phải bảng dài.

### 2. CSDL cần làm

* Dùng dữ liệu ViewModel, không query DB.
* Có thể dùng order\_index để sắp xếp node.

### 3. Backend / Service cần làm

* Service chuẩn bị grouped nodes theo stage/week/day nếu cần.
* Tính CSS class và target URL.

### 4. Controller / View MVC đề xuất

* Views/LearningPath/Index.cshtml.
* Hiển thị header path, progress, today task, danh sách node.
* Có section thông báo nếu chưa có path.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Nếu path quá dài, cần chia giai đoạn hoặc collapse.
* Không hiển thị quá nhiều text trong node.

### 6. Tiêu chí hoàn thành

* View dễ đọc.
* Node đúng thứ tự.
* Không vỡ layout trên desktop.

## Task 8: Tạo partial view \_PathNodeCard và \_PathStageHeader

**Mục tiêu:** Tách card node và header chặng thành partial để code View sạch, dễ tái sử dụng.

### 1. Nghiệp vụ cần hiểu

* Path có nhiều node, nếu viết một View lớn sẽ khó bảo trì.
* Partial giúp thống nhất UI node.

### 2. CSDL cần làm

* Không tạo bảng mới.
* ui\_icon/ui\_color có thể tính từ status/type.

### 3. Backend / Service cần làm

* Tạo helper GetNodeIcon, GetNodeBadge, GetNodeCssClass.
* Service cấp short\_label và description ngắn.

### 4. Controller / View MVC đề xuất

* Partial \_PathNodeCard.cshtml.
* Partial \_PathStageHeader.cshtml.
* Dùng render partial trong Index.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Node thiếu thông tin vẫn không làm crash View.
* Partial không tự gọi service.

### 6. Tiêu chí hoàn thành

* Partial render đúng với mọi node\_type.
* Code View gọn hơn.
* Dễ đổi giao diện sau này.

## Task 9: Hiển thị trạng thái locked/current/in\_progress/completed/review\_needed/skipped

**Mục tiêu:** Hiển thị đúng trạng thái node bằng icon, màu, badge và hành vi click.

### 1. Nghiệp vụ cần hiểu

* Status không chỉ là chữ; nó quyết định người học có được bấm hay không.
* review\_needed phải nổi bật để học sinh biết cần ôn lại.

### 2. CSDL cần làm

* Đọc learning\_path\_nodes.status.
* Nếu có completed\_at, hiển thị thời điểm hoàn thành.

### 3. Backend / Service cần làm

* Map status -> display label: Bị khóa, Đang học, Đã xong, Cần ôn lại.
* Map status -> isClickable.

### 4. Controller / View MVC đề xuất

* CSS class: node-locked, node-current, node-completed, node-review.
* Tooltip/label hiển thị điều kiện mở khóa.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Không cho click bằng cả UI và backend.
* Nếu FE cho click do lỗi, backend vẫn chặn.

### 6. Tiêu chí hoàn thành

* Mỗi status hiển thị khác nhau.
* Click node locked bị chặn.
* review\_needed có cảnh báo rõ.

## Task 10: Hiển thị nhiệm vụ hôm nay và node kế tiếp

**Mục tiêu:** Tập trung trải nghiệm vào nhiệm vụ hôm nay và bước kế tiếp để học sinh không bị quá tải.

### 1. Nghiệp vụ cần hiểu

* Learning path dài dễ làm học sinh rối.
* UI cần ưu tiên today task và next node.

### 2. CSDL cần làm

* scheduled\_date trong learning\_path\_nodes hoặc study\_plan nếu có.
* Nếu không có date, lấy current node làm hôm nay.

### 3. Backend / Service cần làm

* PathViewService.GetTodayTasksAsync.
* Tính next actionable node.
* Đánh dấu overdue nếu scheduled\_date đã qua.

### 4. Controller / View MVC đề xuất

* Widget Nhiệm vụ hôm nay ở đầu trang.
* Button Tiếp tục học.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Không có scheduled\_date thì fallback current node.
* Nhiều task cùng ngày thì giới hạn hiển thị top 3.

### 6. Tiêu chí hoàn thành

* Học sinh thấy rõ cần làm gì hôm nay.
* Button tiếp tục học mở đúng node.
* Không hiển thị dồn quá nhiều task.

## Task 11: Điều hướng node sang Lesson/Quiz/Practice/AI Tutor

**Mục tiêu:** Điều hướng từ node sang đúng module học tập tương ứng.

### 1. Nghiệp vụ cần hiểu

* Path UI là điểm khởi động hành động học.
* Node không tự xử lý lesson/quiz mà redirect sang module chuyên trách.

### 2. CSDL cần làm

* Node có lesson\_id/quiz\_id/practice\_task\_id/topic\_id.
* node\_type quyết định target.

### 3. Backend / Service cần làm

* BuildNodeTargetUrlAsync theo node\_type.
* OpenNodeAsync kiểm tra quyền rồi redirect.

### 4. Controller / View MVC đề xuất

* LearningPath/OpenNode/{nodeId}.
* Redirect Lesson/Details, Quiz/Take, Practice/Details, AiTutor/Index.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Node thiếu target thì hiển thị lỗi cấu hình.
* Không mở content chưa publish/approved.

### 6. Tiêu chí hoàn thành

* LEARN mở lesson.
* QUIZ mở quiz.
* PRACTICE mở practice.
* AI\_TUTOR mở chat đúng context.

## Task 12: Kiểm tra điều kiện mở khóa node

**Mục tiêu:** Thực thi điều kiện khóa/mở node dựa trên node trước, điểm quiz hoặc trạng thái hoàn thành.

### 1. Nghiệp vụ cần hiểu

* Duolingo-style path cần khóa/mở từng bước.
* Điều kiện mở khóa phải nằm ở backend, không chỉ CSS.

### 2. CSDL cần làm

* unlock\_condition nếu có trong learning\_path\_nodes hoặc template\_nodes.
* Đọc node trước theo order\_index.

### 3. Backend / Service cần làm

* CanUnlockNodeAsync.
* Kiểm tra prerequisite completed, min score nếu có.
* Mở node tiếp theo khi đủ điều kiện.

### 4. Controller / View MVC đề xuất

* OpenNode chặn nếu CanUnlock=false.
* Hiển thị lý do khóa trong View.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Node đầu tiên phải available.
* Nếu node trước bị skipped theo rule, xử lý rõ.
* Dữ liệu sai không làm mở toàn path.

### 6. Tiêu chí hoàn thành

* Điều kiện mở khóa đúng.
* UI và backend nhất quán.
* Có thông báo lý do node bị khóa.

## Task 13: Cập nhật UI sau khi hoàn thành bài học/quiz/practice

**Mục tiêu:** Cập nhật lại giao diện path sau khi học sinh hoàn thành activity ở module khác.

### 1. Nghiệp vụ cần hiểu

* M9 không chấm quiz nhưng phải phản ánh kết quả mới.
* Sau khi hoàn thành lesson/quiz, node status phải thay đổi.

### 2. CSDL cần làm

* learning\_path\_nodes.status, completed\_at.
* study\_activity\_logs ghi hoạt động.
* student\_progress\_snapshots cập nhật tiến độ.

### 3. Backend / Service cần làm

* Expose MarkNodeCompletedAsync cho module M11/M12 gọi.
* RecalculatePathProgressAsync.
* Mở node tiếp theo nếu đủ điều kiện.

### 4. Controller / View MVC đề xuất

* Sau khi Lesson/Quiz complete redirect về LearningPath/Index hoặc hiển thị nút Quay về path.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Không đánh dấu complete nếu quiz chưa đạt yêu cầu.
* Không complete node thuộc path người khác.

### 6. Tiêu chí hoàn thành

* Hoàn thành bài học cập nhật node.
* Node tiếp theo mở đúng.
* Progress tăng chính xác.

## Task 14: Hiển thị lý do AI đề xuất và cảnh báo ôn lại

**Mục tiêu:** Hiển thị lý do AI đề xuất node và cảnh báo ôn lại để tăng tính minh bạch.

### 1. Nghiệp vụ cần hiểu

* Học sinh cần hiểu vì sao phải học node này.
* AI reason giúp path có cảm giác cá nhân hóa.

### 2. CSDL cần làm

* learning\_path\_nodes.ai\_reason.
* ai\_feedbacks/replanning\_events nếu có review reason.

### 3. Backend / Service cần làm

* Format AI reason ngắn gọn.
* Nếu reason quá dài thì cắt ngắn và có nút xem thêm.

### 4. Controller / View MVC đề xuất

* Trong \_PathNodeCard hiển thị AI reason hoặc badge AI đề xuất.
* Node review\_needed hiển thị cảnh báo rõ.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Không hiển thị chain-of-thought/prompt nội bộ.
* Không hiển thị reason null làm xấu UI.

### 6. Tiêu chí hoàn thành

* Node có lý do dễ hiểu.
* review\_needed có cảnh báo và hành động ôn lại.
* Không lộ dữ liệu nội bộ AI.

## Task 15: Thiết kế giao diện responsive kiểu Duolingo

**Mục tiêu:** Thiết kế UI responsive, đơn giản và có cảm giác học theo chặng như Duolingo.

### 1. Nghiệp vụ cần hiểu

* MVP dùng Razor nhưng vẫn phải dễ dùng.
* Responsive quan trọng nếu học sinh dùng màn hình nhỏ.

### 2. CSDL cần làm

* Không tạo bảng mới.
* Có thể lưu ui metadata ở ViewModel.

### 3. Backend / Service cần làm

* Tạo CSS riêng path.css.
* Không phụ thuộc thư viện nặng nếu không cần.

### 4. Controller / View MVC đề xuất

* Layout dọc cho mobile, zigzag nhẹ cho desktop nếu kịp.
* Button rõ, node đủ lớn để bấm.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Không dùng màu là tín hiệu duy nhất; cần text/icon.
* Không để node overlap trên màn hình nhỏ.

### 6. Tiêu chí hoàn thành

* Desktop/mobile không vỡ layout.
* Node dễ bấm.
* Giao diện đủ trực quan để demo.

## Task 16: Hiển thị progress bar, streak và tổng quan path

**Mục tiêu:** Hiển thị tổng quan tiến độ để học sinh biết đã đi được bao xa.

### 1. Nghiệp vụ cần hiểu

* Path UI cần phản hồi tiến bộ để giữ động lực.
* Progress dùng dữ liệu M16.

### 2. CSDL cần làm

* student\_progress\_snapshots hoặc tính từ node completed/total.
* study\_activity\_logs nếu cần total minutes.

### 3. Backend / Service cần làm

* CalculateProgressSummaryAsync.
* Tính completedNodes, totalNodes, percent, streak nếu có.

### 4. Controller / View MVC đề xuất

* Progress bar ở đầu path.
* Card tổng quan: % hoàn thành, streak, số node còn lại.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Không chia cho 0 khi path chưa có node.
* Streak thiếu dữ liệu thì ẩn.

### 6. Tiêu chí hoàn thành

* Progress hiển thị chính xác.
* Hoàn thành node làm progress thay đổi.
* Không lỗi path rỗng.

## Task 17: Hiển thị lịch sử path/version hoặc path đã archived

**Mục tiêu:** Cho học sinh/Admin xem path cũ hoặc phiên bản path nếu M17 điều chỉnh.

### 1. Nghiệp vụ cần hiểu

* AI Replanning có thể tạo path version mới.
* Cần tránh mất lịch sử path.

### 2. CSDL cần làm

* student\_learning\_paths.status: ACTIVE, COMPLETED, PAUSED, ARCHIVED.
* ai\_replanning\_events lưu lý do đổi path.

### 3. Backend / Service cần làm

* GetPathHistoryAsync.
* Chỉ path active là path học chính.
* Archived path xem read-only.

### 4. Controller / View MVC đề xuất

* LearningPath/History.cshtml.
* Admin preview path history nếu cần.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Không cho học sinh chỉnh path cũ.
* Không nhầm active path với archived path.

### 6. Tiêu chí hoàn thành

* Xem được lịch sử path.
* Path cũ không còn dùng để học.
* Replanning reason hiển thị nếu có.

## Task 18: Admin/Giáo viên preview path học sinh dạng read-only

**Mục tiêu:** Cho Admin/Giáo viên xem preview path học sinh để hỗ trợ tư vấn và debug.

### 1. Nghiệp vụ cần hiểu

* Admin cần kiểm tra path khi học sinh báo lỗi.
* Teacher xem path để tư vấn nhưng không nên sửa trực tiếp nếu không có quyền.

### 2. CSDL cần làm

* Query path theo student\_id có kiểm tra role.
* Không tạo bảng mới.

### 3. Backend / Service cần làm

* AdminPathPreviewService.GetStudentPathAsync.
* Log khi Admin xem dữ liệu học sinh nếu cần.

### 4. Controller / View MVC đề xuất

* Area Admin: LearningPathsController/Preview(studentId).
* View read-only, không có nút học.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Admin không có quyền không được xem.
* Không lộ prompt/internal logs không cần thiết.

### 6. Tiêu chí hoàn thành

* Admin xem được path student.
* View read-only.
* Có phân quyền rõ.

## Task 19: Tối ưu hiệu năng, logging và xử lý lỗi giao diện

**Mục tiêu:** Tối ưu hiệu năng, logging và xử lý lỗi để path page ổn định khi demo.

### 1. Nghiệp vụ cần hiểu

* Path page là màn hình học sinh mở thường xuyên.
* Nhiều join có thể chậm nếu không tối ưu.

### 2. CSDL cần làm

* Index learning\_path\_nodes(learning\_path\_id,status,order\_index).
* Index student\_learning\_paths(student\_id,status).
* audit\_logs nếu cần log open node lỗi.

### 3. Backend / Service cần làm

* Cache ngắn dữ liệu path nếu phù hợp.
* Không N+1 query khi load node.
* Try/catch lỗi mapping target.

### 4. Controller / View MVC đề xuất

* Error partial nếu path lỗi cấu hình.
* Loading state nếu Ajax.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Node trỏ content bị xóa/archived phải hiển thị lỗi cấu hình, không crash.
* Không để trang trắng khi AI/path lỗi.

### 6. Tiêu chí hoàn thành

* Trang path phản hồi nhanh.
* Lỗi dữ liệu được thông báo rõ.
* Có log đủ để debug.

## Task 20: Test case, seed data demo và bàn giao module

**Mục tiêu:** Hoàn thiện test case, seed data và bàn giao module để tích hợp các module học tập.

### 1. Nghiệp vụ cần hiểu

* Module hoàn thành khi có thể demo từ path đến node bài học/quiz.
* Seed data phải đủ trạng thái node.

### 2. CSDL cần làm

* Seed 1 student có active path.
* Seed path có ít nhất 8 node đủ trạng thái: locked/current/completed/review\_needed.
* Seed node liên kết lesson/quiz/practice.

### 3. Backend / Service cần làm

* Viết checklist test: xem path, mở node, locked node, complete node, review node.
* Test ownership và Admin preview.

### 4. Controller / View MVC đề xuất

* Kiểm tra Views trên desktop và mobile.
* Chụp màn demo nếu cần.

### 5. Xử lý ngoài luồng / lỗi nghiệp vụ

* Không bàn giao nếu node locked vẫn mở được.
* Không bàn giao nếu path không render được khi thiếu dữ liệu optional.

### 6. Tiêu chí hoàn thành

* Demo path chạy được.
* Test case luồng chính pass.
* Có seed data và tài liệu bàn giao.

# VII. Thứ tự triển khai khuyến nghị

1. Task 1 - Rà soát/chỉnh CSDL path và node.

2. Task 2 - Chuẩn hóa status/type và quy ước hiển thị.

3. Task 3 - Tạo ViewModel/DTO.

4. Task 4 đến Task 6 - Xây Service và Controller MVC.

5. Task 7 đến Task 10 - Xây giao diện path, node card, trạng thái và nhiệm vụ hôm nay.

6. Task 11 đến Task 13 - Điều hướng node, unlock condition và cập nhật trạng thái sau học.

7. Task 14 đến Task 16 - AI reason, responsive UI, progress/streak.

8. Task 17 đến Task 18 - Path history và Admin preview.

9. Task 19 - Tối ưu hiệu năng/logging/lỗi.

10. Task 20 - Test case, seed data demo và bàn giao module.

# VIII. Chốt nghiệp vụ cuối cùng

Module M9 - Giao diện Learning Path kiểu Duolingo là module trải nghiệm học tập trung tâm của AI Study English. Module này nhận path do M8 tạo ra, hiển thị thành các node học trực quan, khóa/mở node theo điều kiện, điều hướng sang bài học/quiz/practice/AI Tutor và phản ánh tiến độ học tập sau khi học sinh hoàn thành nhiệm vụ. Trong ASP.NET MVC, module nên triển khai bằng LearningPathController, PathViewService, Razor View Index/Details, partial \_PathNodeCard và CSS responsive path layout. Backend vẫn là nguồn kiểm tra quyền và điều kiện mở node; frontend chỉ hiển thị và gửi hành động. Mọi dữ liệu cá nhân của path phải kiểm tra ownership, không để học sinh xem path của người khác.