# RULES.md — AI Study English Project

> **⚠️ MANDATORY: Mọi AI agent PHẢI đọc file này TRƯỚC KHI thực hiện bất kỳ thay đổi nào trong dự án.**
> Không được bỏ qua bất kỳ quy tắc nào. Vi phạm rules = code bị reject.

---

## 1. Tổng quan dự án

| Thuộc tính         | Giá trị                                             |
|--------------------|------------------------------------------------------|
| Tên dự án          | AI Study English (DuAnTotNghiep)                     |
| Framework          | ASP.NET Core MVC (.NET 10)                           |
| Database           | SQL Server + Entity Framework Core                   |
| ORM                | EF Core (Code-First với Fluent API)                  |
| CSS                | TailwindCSS (build qua `npx tailwindcss`) + Vanilla CSS cho module riêng |
| Authentication     | Cookie-based (`CookieAuthenticationDefaults`)        |
| Architecture       | MVC + Repository + Service Layer                     |
| Areas              | `Admin`, `Student`, `Teacher`                        |
| Font               | Roboto (Google Fonts)                                |
| Primary Color      | `#2563EB`                                            |

---

## 2. Kiến trúc & Cấu trúc thư mục

### 2.1 Quy tắc phân tầng (MANDATORY)

```
Controller → Service → Repository → DbContext
     ↓           ↓           ↓
  ViewModel     DTO        Model (Entity)
```

**Quy tắc nghiêm ngặt:**
- **Controller** KHÔNG được truy cập `DbContext` trực tiếp (trừ query đọc đơn giản không cần reuse).
- **Controller** chỉ gọi **Service**. Controller chỉ chứa logic điều hướng, không chứa business logic.
- **Service** chứa business logic. Service gọi **Repository** hoặc `DbContext` nếu cần.
- **Repository** chỉ chứa data access logic (CRUD, query).
- **KHÔNG** tạo God Service (>500 dòng) — tách thành nhiều service nhỏ theo Single Responsibility.

### 2.2 Cấu trúc thư mục

```
DuAnTotNghiep/
├── Areas/
│   ├── Admin/           # Quản trị: Controllers, Views
│   ├── Student/         # Học viên: Controllers, Views
│   └── Teacher/         # Giáo viên: Controllers, Views
├── Controllers/         # Shared controllers (Account, Home, Profile)
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── Seeders/         # Database seeders
│   └── AIStudyEnglish_Schema.sql
├── DTOs/                # Data Transfer Objects (theo feature folder)
├── Enums/               # String-const enum classes
├── Exceptions/          # Custom exceptions (BusinessException, NotFoundException)
├── Extensions/          # Extension methods
├── Filters/             # Action Filters (RequireOnboarding, RequirePlacementTest)
├── Helpers/             # Utility/helper classes
├── Migrations/          # EF Core migrations
├── Models/              # Entity models (1 file = 1 class)
├── Repositories/
│   ├── Interfaces/      # IXxxRepository
│   └── XxxRepository.cs
├── Services/
│   ├── Interfaces/      # IXxxService
│   ├── Background/      # Hosted/Background services
│   └── XxxService.cs
├── ViewModels/          # Theo feature folder (LearningPath/, Onboarding/, ...)
├── Views/
│   └── Shared/          # Layouts: _StudentLayout, _AdminLayout, _TeacherLayout, _AuthLayout
├── wwwroot/
│   └── css/             # site.css (Tailwind output) + module CSS riêng
├── DESIGN.md            # Design system reference
└── RULES.md             # File này
```

### 2.3 Quy tắc tạo file mới

| Loại file        | Đặt ở đâu                                          | Naming                         |
|------------------|------------------------------------------------------|--------------------------------|
| Entity Model     | `Models/`                                            | `PascalCase.cs` (số ít)       |
| DTO              | `DTOs/{FeatureName}/`                                | `XxxDto.cs`, `XxxRequest.cs`  |
| ViewModel        | `ViewModels/{FeatureName}/`                          | `XxxViewModel.cs`             |
| Service          | `Services/` + Interface tại `Services/Interfaces/`   | `XxxService.cs` / `IXxxService.cs` |
| Repository       | `Repositories/` + Interface tại `Repositories/Interfaces/` | `XxxRepository.cs` / `IXxxRepository.cs` |
| Controller       | `Areas/{Role}/Controllers/` hoặc `Controllers/`     | `XxxController.cs`            |
| View             | `Areas/{Role}/Views/{Controller}/`                   | `Index.cshtml`, `_PartialName.cshtml` |
| Enum             | `Enums/`                                             | `XxxEnums.cs` hoặc `XxxType.cs` |
| CSS module        | `wwwroot/css/`                                       | `feature-name.css` (kebab-case) |
| Filter           | `Filters/`                                           | `XxxFilter.cs`                |
| Exception        | `Exceptions/`                                        | `XxxException.cs`             |

---

## 3. Clean Code Standards (Chuẩn doanh nghiệp)

### 3.1 Nguyên tắc SOLID bắt buộc

1. **Single Responsibility** — Mỗi class chỉ làm MỘT việc. Mỗi method chỉ làm MỘT việc.
2. **Open/Closed** — Mở rộng qua interface/abstract, KHÔNG sửa code cũ khi thêm tính năng mới.
3. **Liskov Substitution** — Subclass phải thay thế được parent mà không làm hỏng logic.
4. **Interface Segregation** — KHÔNG tạo interface quá lớn. Tách thành interface nhỏ, rõ mục đích.
5. **Dependency Inversion** — Inject interface, KHÔNG inject concrete class. Mọi service đều phải có interface.

### 3.2 Quy tắc đặt tên (Naming Conventions)

| Thành phần          | Convention       | Ví dụ                              |
|---------------------|------------------|-------------------------------------|
| Class, Interface    | PascalCase       | `LearningTopicService`, `IAuthService` |
| Method              | PascalCase       | `GetCurrentPathPageAsync()`        |
| Async method        | Hậu tố `Async`  | `SeedAsync()`, `BuildNodeTargetUrlAsync()` |
| Private field       | `_camelCase`     | `_pathViewService`, `_context`     |
| Parameter, Variable | camelCase        | `userId`, `nodeIndex`              |
| Constant            | PascalCase       | `public const string Locked = "LOCKED"` |
| CSS class           | BEM kebab-case   | `lp-node-card__title`, `node-completed` |
| CSS file            | kebab-case       | `learning-path.css`                |
| Database column     | snake_case       | `learning_path_id`, `created_at`   |
| Database table      | snake_case (số nhiều) | `learning_path_nodes`, `study_activity_logs` |

### 3.3 Quy tắc method

- **Tối đa 30 dòng** cho mỗi method. Nếu dài hơn → tách thành private methods.
- **Tối đa 4 parameters**. Nếu nhiều hơn → dùng DTO/request object.
- **Tên method phải mô tả hành động**: `GetUserByIdAsync()` ✅, `Process()` ❌, `DoStuff()` ❌.
- Method trả về `bool` bắt đầu bằng `Is`, `Has`, `Can`: `IsValid()`, `CanOpenNodeAsync()`.
- Method async PHẢI có hậu tố `Async`.

### 3.4 Quy tắc class

- **Tối đa 300 dòng** cho mỗi class. Nếu dài hơn → tách service.
- **1 file = 1 class** (không gộp nhiều class vào 1 file).
- Mọi public class PHẢI có XML doc comment `/// <summary>`.
- Constructor inject tối đa **5 dependencies**. Nếu nhiều hơn → refactor, tách service.

### 3.5 Enum pattern (Đặc thù dự án)

Dự án dùng **string-const static class** thay vì C# enum. PHẢI tuân theo pattern này:

```csharp
// ✅ ĐÚNG — Pattern hiện tại
public static class ProgressStatus
{
    public const string Locked = "LOCKED";
    public const string Available = "AVAILABLE";
    
    public static readonly string[] All = { Locked, Available };
    
    public static bool IsValid(string status)
    {
        if (string.IsNullOrWhiteSpace(status)) return false;
        return Array.Exists(All, s => s.Equals(status, StringComparison.OrdinalIgnoreCase));
    }
}

// ❌ SAI — Không dùng C# enum
public enum ProgressStatus { Locked, Available }
```

### 3.6 Error handling

- Dùng `BusinessException` cho lỗi nghiệp vụ (validation, business rule).
- Dùng `NotFoundException` cho entity không tìm thấy.
- Controller: bắt exception → set `TempData["ErrorMessage"]` → redirect.
- Service: throw exception rõ ràng, KHÔNG return null khi thất bại.
- KHÔNG dùng `try-catch` bao trùm toàn bộ method — chỉ catch những exception cụ thể.

```csharp
// ✅ ĐÚNG
if (entity == null)
    throw new NotFoundException("Không tìm thấy bài học.");

// ❌ SAI
try { ... } catch (Exception ex) { return null; }
```

---

## 4. Database & Entity Framework

### 4.1 Model conventions

- Mỗi entity PHẢI có property `Id` kiểu `int` (auto-increment).
- Sử dụng `Fluent API` trong `ApplicationDbContext.OnModelCreating()` cho cấu hình bảng — KHÔNG dùng Data Annotations trên Model.
- Navigation property dùng `virtual` keyword.
- Collection navigation khởi tạo = `new List<T>()`.
- Required string property: `= null!;` (pattern hiện tại).

### 4.2 Query conventions

- **Luôn dùng `AsNoTracking()`** cho read-only queries.
- **Dùng `Include()`** rõ ràng — KHÔNG rely on lazy loading.
- **Tránh N+1** — dùng `Include()` / `ThenInclude()` hoặc projection query.
- Phân trang bắt buộc cho danh sách > 20 items: `Skip()` + `Take()`.
- Raw SQL chỉ dùng khi EF query quá phức tạp — PHẢI có comment giải thích lý do.

### 4.3 Migration

- KHÔNG sửa migration đã push lên remote.
- Mỗi migration PHẢI có tên mô tả: `AddLearningPathNodes`, `UpdateQuizSchema`.
- Script SQL thay đổi schema lớn lưu tại root: `migration_xxx.sql`.

---

## 5. Controller Rules

### 5.1 Pattern bắt buộc

```csharp
[Area("Student")]                      // Khai báo Area
[Authorize(Roles = "STUDENT")]         // Authorization rõ ràng
public class XxxController : Controller
{
    private readonly IXxxService _xxxService;
    private readonly ILogger<XxxController> _logger;

    public XxxController(IXxxService xxxService, ILogger<XxxController> logger)
    {
        _xxxService = xxxService;
        _logger = logger;
    }

    // Lấy userId từ Claims — PHẢI dùng helper method
    private int GetUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdStr, out var userId);
        return userId;
    }
}
```

### 5.2 Quy tắc

- **KHÔNG** chứa business logic — delegate cho Service.
- Mỗi action method PHẢI có `[HttpGet]` hoặc `[HttpPost]` attribute rõ ràng.
- `POST` action PHẢI kiểm tra `ModelState.IsValid` trước khi xử lý.
- Sử dụng `TempData["SuccessMessage"]` và `TempData["ErrorMessage"]` cho flash messages.
- Roles: `"ADMIN"`, `"STUDENT"`, `"TEACHER"` — viết in hoa.

---

## 6. Service Rules

### 6.1 Pattern bắt buộc

```csharp
// Interface — tại Services/Interfaces/
public interface IXxxService
{
    Task<ResultDto> DoSomethingAsync(int id);
}

// Implementation — tại Services/
public class XxxService : IXxxService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<XxxService> _logger;

    public XxxService(ApplicationDbContext context, ILogger<XxxService> logger)
    {
        _context = context;
        _logger = logger;
    }
}
```

### 6.2 Quy tắc

- Mọi service PHẢI có **interface** tương ứng.
- Mọi service PHẢI được **đăng ký DI** trong `Program.cs` (`AddScoped`).
- Sử dụng `ILogger<T>` — KHÔNG dùng `Console.WriteLine` hay `Debug.Print`.
- Background service kế thừa `BackgroundService` và đặt tại `Services/Background/`.

---

## 7. View & Frontend Rules

### 7.1 Layout system

| Role     | Layout file                    | Dùng cho                  |
|----------|--------------------------------|---------------------------|
| Chung    | `_Layout.cshtml`               | Homepage, public pages    |
| Auth     | `_AuthLayout.cshtml`           | Login, Register, Reset    |
| Admin    | `_AdminLayout.cshtml`          | Admin area                |
| Student  | `_StudentLayout.cshtml`        | Student area (sidebar)    |
| Teacher  | `_TeacherLayout.cshtml`        | Teacher area              |

### 7.2 View conventions

- Partial view bắt đầu bằng `_`: `_PathNodeCard.cshtml`, `_PathProgressBar.cshtml`.
- View PHẢI khai báo `@model` ở dòng đầu tiên.
- KHÔNG viết C# logic phức tạp trong View — chuyển logic vào ViewModel hoặc Service.
- `ViewData["Title"]` PHẢI được set trong mọi View.

### 7.3 CSS conventions

- **TailwindCSS** cho layout chung (`site.css` — compile từ `Styles/site.css`).
- **Vanilla CSS riêng** cho module phức tạp (`learning-path.css`).
- Sử dụng **BEM naming**: `.block__element--modifier` hoặc `.lp-node-card__title`.
- KHÔNG dùng `!important` — fix specificity đúng cách.
- KHÔNG dùng inline style trong View (trừ dynamic value như `width: @percent%`).
- Khai báo CSS module trong View: `<link rel="stylesheet" href="~/css/module-name.css" asp-append-version="true" />`.

### 7.4 Design System (DESIGN.md)

Mọi UI mới PHẢI tuân theo [DESIGN.md](DESIGN.md):

| Token           | Giá trị      |
|-----------------|--------------|
| Primary Blue    | `#2563EB`    |
| Primary Hover   | `#1D4ED8`    |
| Background      | `#F8FAFC`    |
| Surface         | `#FFFFFF`    |
| Border          | `#E2E8F0`    |
| Text Primary    | `#0F172A`    |
| Text Secondary  | `#64748B`    |
| Success         | `#22C55E`    |
| Warning         | `#F59E0B`    |
| Danger          | `#EF4444`    |
| Border Radius   | Cards: `16px`, Buttons: `12px`, Inputs: `12px` |
| Font            | `Roboto` (300, 400, 500, 700) |

---

## 8. ViewModel & DTO Rules

### 8.1 Phân biệt

| Loại       | Mục đích                                    | Nơi đặt              |
|------------|----------------------------------------------|-----------------------|
| ViewModel  | Truyền data từ Controller → View             | `ViewModels/{Feature}/` |
| DTO        | Truyền data giữa các tầng / request-response | `DTOs/{Feature}/`     |
| Model      | Entity mapping với database                  | `Models/`             |

### 8.2 Quy tắc

- ViewModel KHÔNG chứa logic phức tạp — chỉ properties và computed properties đơn giản.
- KHÔNG trả Entity Model trực tiếp cho View — PHẢI map sang ViewModel.
- DTO cho Create/Update PHẢI có validation attributes (`[Required]`, `[StringLength]`).

---

## 9. Testing Rules

### 9.1 Quy tắc đặt tên test

```
Tests/
├── M9CoreDefinitionsTests.cs           # Module 9: unit tests
├── M9PathViewServiceTests.cs           # Module 9: service tests  
├── M9LearningPathControllerTests.cs    # Module 9: controller tests
```

- File test đặt tại `Tests/` folder.
- Naming: `{Module}{Component}Tests.cs`.
- Method test: `{MethodName}_{Scenario}_{ExpectedResult}`.

### 9.2 Pattern

```csharp
[Fact]
public void GetCssClass_WhenStatusIsCompleted_ReturnsNodeCompleted()
{
    // Arrange
    var status = ProgressStatus.Completed;
    
    // Arrange → Act → Assert
    var result = GetCssClass(status);
    
    Assert.Equal("node-completed", result);
}
```

---

## 10. Git & Collaboration Rules

### 10.1 Branch naming

```
{tên_thành_viên}-{module}
```

Ví dụ: `ngthebao-md9`, `nguyenvan-md5`.

### 10.2 Commit message format

```
{type}({scope}): {description}
```

| Type     | Khi nào                                     |
|----------|----------------------------------------------|
| `feat`   | Thêm tính năng mới                          |
| `fix`    | Sửa bug                                     |
| `refactor` | Refactor không thay đổi behavior           |
| `style`  | CSS, UI, formatting                          |
| `docs`   | Cập nhật documentation                       |
| `test`   | Thêm/sửa test                               |
| `chore`  | Config, dependency, build                    |

Ví dụ:
```
feat(learning-path): add zigzag node layout with Duolingo-style design
fix(progress): correct streak calculation for inactive days
refactor(auth): extract token validation into separate service
```

### 10.3 Quy tắc push

- KHÔNG push code không build được (`dotnet build` phải pass 0 errors).
- KHÔNG push file nhạy cảm (`appsettings.json` với connection string production, `.env`).
- KHÔNG push thư mục: `bin/`, `obj/`, `node_modules/`, `.codegraph/`.

---

## 11. Performance & Security

### 11.1 Performance

- Pagination bắt buộc cho list > 20 items.
- `AsNoTracking()` cho mọi read-only query.
- KHÔNG load toàn bộ bảng vào memory (`ToListAsync()` trước khi filter).
- Cache static data (EnglishSkills, Levels) nếu cần.

### 11.2 Security

- Mọi controller trong Area PHẢI có `[Authorize(Roles = "...")]`.
- KHÔNG trust client input — validate ở cả Controller và Service.
- Sử dụng parameterized queries (EF Core tự xử lý) — KHÔNG string concatenation cho SQL.
- Password hash: `BCrypt.Net`.
- Cookie: `HttpOnly = true`, `SecurePolicy = Always`, `SameSite = Strict`.
- KHÔNG log thông tin nhạy cảm (password, token).

---

## 12. AI Agent Checklist

Trước khi thực hiện BẤT KỲ thay đổi nào, AI agent PHẢI:

- [ ] **Đọc RULES.md** (file này).
- [ ] **Đọc DESIGN.md** nếu thay đổi liên quan đến UI.
- [ ] **Kiểm tra cấu trúc thư mục** — đặt file đúng vị trí.
- [ ] **Kiểm tra interface** — mọi service mới PHẢI có interface.
- [ ] **Đăng ký DI** — thêm `AddScoped<>()` trong `Program.cs`.
- [ ] **Build thành công** — chạy `dotnet build` trước khi báo hoàn thành.
- [ ] **Không phá code cũ** — kiểm tra regression trước khi kết thúc.
- [ ] **Code sạch** — không để lại `TODO`, `HACK`, `Console.WriteLine`, code comment-out.
- [ ] **Naming đúng** — theo naming conventions ở Section 3.2.
- [ ] **Method ngắn** — tối đa 30 dòng, tối đa 4 parameters.

---

## 13. Anti-patterns (KHÔNG ĐƯỢC LÀM)

| ❌ Anti-pattern                         | ✅ Thay thế bằng                              |
|-----------------------------------------|------------------------------------------------|
| Business logic trong Controller         | Tách vào Service                               |
| DbContext trực tiếp trong Controller    | Gọi qua Service/Repository                    |
| `Console.WriteLine` để debug            | `ILogger<T>.LogInformation/LogError`           |
| Return entity Model cho View            | Map sang ViewModel                             |
| Catch `Exception` rồi nuốt lỗi         | Catch exception cụ thể, log + rethrow nếu cần |
| Copy-paste code giữa các service       | Extract shared method hoặc base class          |
| Hard-code magic strings                 | Dùng Enum class (`ProgressStatus.Locked`)      |
| CSS inline `style="..."` trong View     | Dùng CSS class                                 |
| File > 500 dòng                         | Tách thành nhiều file/class                    |
| Interface quá lớn (>10 methods)         | Tách thành interface nhỏ hơn                   |

---

> **Cập nhật lần cuối:** 2026-06-28
> **Người tạo:** AI Agent (dựa trên phân tích toàn bộ codebase)
> **Áp dụng cho:** Tất cả thành viên và AI agents làm việc trên dự án
