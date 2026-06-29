# PROMPT CHO CODEX — TRIỂN KHAI MODULE M8 (AI Learning Path Engine)

## BỐI CẢNH
Bạn đang làm việc trên dự án **AI Study English** — một ứng dụng ASP.NET Core MVC (.NET 10) tại `e:\DuAnTotNghiep`.

File kế hoạch triển khai nằm tại: `e:\DuAnTotNghiep\m8_plan.md`

## FILE BẮT BUỘC ĐỌC TRƯỚC KHI LÀM BẤT CỨ ĐIỀU GÌ

1. **`RULES.md`** (e:\DuAnTotNghiep\RULES.md) — Quy tắc code, naming, kiến trúc, anti-patterns. **PHẢI tuân thủ 100%.**
2. **`DESIGN.md`** (e:\DuAnTotNghiep\DESIGN.md) — Design system: color palette, typography, card design, button design. **BẮT BUỘC đọc khi làm bất kỳ task nào liên quan đến View (.cshtml) hoặc CSS.**
3. **`m8_plan.md`** (e:\DuAnTotNghiep\m8_plan.md) — Kế hoạch chi tiết với checkbox từng task nhỏ.

## NHIỆM VỤ
Triển khai Module M8 (AI Learning Path Engine) theo đúng file `m8_plan.md`. Thực hiện **từng task nhỏ một**, theo thứ tự từ Phase 1 đến Phase 8.

## QUY TRÌNH BẮT BUỘC (WORKFLOW)

Lặp lại chu trình sau cho **mỗi task nhỏ** (ví dụ: Task 1.1, Task 1.2, ...):

### Bước 1: ĐỌC PLAN
- Mở file `m8_plan.md`.
- Tìm task tiếp theo có checkbox `- [ ]` (chưa hoàn thành).
- Đọc kỹ yêu cầu của task đó.

### Bước 2: ĐỌC RULES (nếu chưa đọc trong phiên này)
- Mở file `RULES.md` và đọc kỹ.
- Nếu task liên quan đến UI (View/CSS) → mở thêm `DESIGN.md` và đọc kỹ.

### Bước 3: THỰC HIỆN
- Code đúng theo yêu cầu trong plan.
- Đặt file đúng đường dẫn được chỉ định.
- Tuân thủ pattern có sẵn trong codebase (xem phần REFERENCES bên dưới).
- Tuân thủ RULES.md nghiêm ngặt: naming conventions, SOLID, method ≤30 dòng, class ≤300 dòng.

### Bước 4: KIỂM TRA
- Chạy `dotnet build` tại `e:\DuAnTotNghiep`.
- Nếu build FAIL → sửa lỗi ngay, chạy lại `dotnet build` cho đến khi PASS.
- Nếu build PASS → chuyển sang bước 5.

### Bước 5: ĐÁNH DẤU HOÀN THÀNH
- Mở file `m8_plan.md`.
- Tìm đúng task vừa hoàn thành.
- Đổi `- [ ]` thành `- [x]`.
- Lưu file.

### Bước 6: LẶP LẠI
- Quay lại Bước 1 để làm task tiếp theo.
- Tiếp tục cho đến khi hết quota hoặc hoàn thành tất cả.

## REFERENCES — PATTERN CÓ SẴN TRONG CODEBASE

Khi code, hãy tham khảo các file sau để giữ nhất quán:

| Cần tạo | Tham khảo file có sẵn |
|---|---|
| Enum (string-const) | `e:\DuAnTotNghiep\Enums\ProgressStatus.cs` — copy cấu trúc static class + const string + IsValid |
| Enum NodeType (đã có) | `e:\DuAnTotNghiep\Enums\NodeType.cs` — KHÔNG tạo lại, sử dụng trực tiếp |
| Entity Model | `e:\DuAnTotNghiep\Models\StudentLearningPath.cs` — xem cấu trúc entity hiện tại |
| Entity LearningPathNode | `e:\DuAnTotNghiep\Models\LearningPathNode.cs` — xem cấu trúc node hiện tại |
| DTO | `e:\DuAnTotNghiep\DTOs\Topic\TopicDetailDto.cs` — xem pattern DTO |
| ViewModels (LearningPath) | `e:\DuAnTotNghiep\ViewModels\LearningPath\` — xem ViewModels M9 đã có |
| ViewModels (Admin) | `e:\DuAnTotNghiep\ViewModels\Admin\ReferenceSources\` — xem pattern Admin VM |
| Repository Interface | `e:\DuAnTotNghiep\Repositories\Interfaces\IReferenceSourceRepository.cs` — xem pattern |
| Repository Implementation | `e:\DuAnTotNghiep\Repositories\ReferenceSourceRepository.cs` — xem cách dùng AsNoTracking, Include |
| Service Interface | `e:\DuAnTotNghiep\Services\Interfaces\IPathViewService.cs` — xem pattern interface (M9) |
| Service Implementation | `e:\DuAnTotNghiep\Services\PathViewService.cs` — xem cách inject DbContext, query pattern |
| Controller (Student) | `e:\DuAnTotNghiep\Areas\Student\Controllers\LearningPathController.cs` — xem GetUserId(), Authorize, Area |
| Controller (Admin) | `e:\DuAnTotNghiep\Areas\Admin\Controllers\ReferenceSourcesController.cs` — xem pattern Admin CRUD |
| DI Registration | `e:\DuAnTotNghiep\Program.cs` — xem cách AddScoped |
| Razor View (Student) | `e:\DuAnTotNghiep\Areas\Student\Views\LearningPath\Index.cshtml` — xem layout, ViewBag, partial |
| Razor View (Admin) | `e:\DuAnTotNghiep\Areas\Admin\Views\ReferenceSources\Index.cshtml` — xem pattern Admin table |
| CSS | `e:\DuAnTotNghiep\wwwroot\css\learning-path.css` — xem BEM naming, prefix `lp-` |
| AI Models | `e:\DuAnTotNghiep\Models\AiPromptTemplate.cs` — xem cấu trúc prompt template |
| AI Usage Log | `e:\DuAnTotNghiep\Models\AiUsageLog.cs` — xem cấu trúc AI log |
| DbContext | `e:\DuAnTotNghiep\Data\ApplicationDbContext.cs` — xem cấu hình Fluent API |
| Competency | `e:\DuAnTotNghiep\Models\CompetencyAnalysis.cs` — xem input từ M7 |
| Test file | `e:\DuAnTotNghiep\Tests\M9PathViewServiceTests.cs` — xem pattern test |

## LƯU Ý QUAN TRỌNG

1. **Enum ProgressStatus đã có sẵn** tại `Enums/ProgressStatus.cs` với các giá trị: `LOCKED`, `AVAILABLE`, `IN_PROGRESS`, `COMPLETED`, `NEED_REVIEW`, `SKIPPED`. Sử dụng trực tiếp, KHÔNG tạo lại.
2. **Enum NodeType đã có sẵn** tại `Enums/NodeType.cs` với: `TOPIC`, `LESSON`, `QUIZ`, `PRACTICE`, `REVIEW`, `AI_TUTOR`. Sử dụng trực tiếp, KHÔNG tạo lại.
3. **Entity `LearningPathNode` và `StudentLearningPath` đã có sẵn** trong `Models/`. KHÔNG tạo lại, chỉ thêm property nếu plan yêu cầu.
4. **Entity `LearningPathTemplate` và `LearningPathTemplateNode` đã có sẵn** trong `Models/`.
5. **`IPathViewService` và `PathViewService` thuộc M9** — KHÔNG sửa, chỉ tham khảo pattern.
6. **Namespace của project là `DuAnTotNghiep`.**
7. **DbContext là `ApplicationDbContext`** tại `Data/ApplicationDbContext.cs`.
8. Khi tạo View, nhớ dùng `@model` directive với đầy đủ namespace.
9. Khi tạo CSS, dùng prefix `m8-` (ví dụ: `.m8-path-summary`) để tránh xung đột với CSS M9 (prefix `lp-`).
10. Status `NEED_REVIEW` (KHÔNG phải `REVIEW_NEEDED`).
11. KHÔNG có status `CURRENT` trong DB — tính node hiện tại bằng logic trong Service.
12. **KHÔNG tạo EF Core migration (.cs)**. Nếu cần thay đổi schema, tạo file `.sql` tại root.
13. Service PHẢI có interface tương ứng và đăng ký DI trong `Program.cs`.
14. Method async PHẢI có hậu tố `Async`.
15. Controller KHÔNG chứa business logic — delegate cho Service.
16. Mỗi task PHẢI `dotnet build` pass trước khi đánh dấu hoàn thành.

## QUY TẮC TỪ RULES.MD (TÓM TẮT)

- **Controller** → Service → Repository → DbContext (KHÔNG truy cập DbContext trực tiếp từ Controller trừ query đọc đơn giản).
- **Naming**: PascalCase cho class/method, `_camelCase` cho private field, BEM kebab-case cho CSS.
- **Method tối đa 30 dòng**, tối đa 4 parameters. Nếu dài hơn → tách private methods.
- **Class tối đa 300 dòng**. Nếu dài hơn → tách service.
- **Mọi public class PHẢI có XML doc comment** `/// <summary>`.
- **Error handling**: dùng `BusinessException` cho lỗi nghiệp vụ, `NotFoundException` cho entity không tìm thấy.
- **Enum pattern**: string-const static class (KHÔNG dùng C# enum).
- **Query**: `AsNoTracking()` cho read-only, `Include()` rõ ràng, phân trang > 20 items.
- **View**: partial view bắt đầu bằng `_`, `@model` ở dòng đầu, `ViewData["Title"]` bắt buộc.
- **CSS**: BEM naming, KHÔNG `!important`, KHÔNG inline style.

## KHI HẾT QUOTA
Nếu sắp hết quota, hãy:
1. Đảm bảo `dotnet build` pass với trạng thái hiện tại.
2. Cập nhật tất cả checkbox đã hoàn thành trong `m8_plan.md`.
3. Ghi lại task cuối cùng đã hoàn thành để phiên sau tiếp tục.
