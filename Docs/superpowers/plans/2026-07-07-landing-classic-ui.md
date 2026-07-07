# Landing Classic UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Nâng cấp landing page thành mặt tiền công khai đồng bộ với Admin, Teacher, Student và auth, tối ưu cho người thật tìm hiểu sản phẩm, đăng kí và đăng nhập.

**Architecture:** Giữ nguyên `HomeController.Index`, URL `/`, `_Navbar` nếu còn được dùng ở nơi khác, và route auth hiện có. `_Layout.cshtml` trở thành public shell top-navigation thay vì sidebar công khai, `public-shell.css` dùng token `--rs-*`, `public-shell.js` quản lý mobile nav, còn `Views/Home/Index.cshtml` chỉ chứa nội dung landing có cấu trúc rõ ràng.

**Tech Stack:** ASP.NET Core MVC/Razor, Tailwind utility CSS hiện có, CSS thuần với `role-shell.css`, Noto Sans, vanilla JavaScript, Node test runner, Playwright package hiện có cho screenshot asset nếu cần, `dotnet build`, `designmd lint`, Chrome/in-app browser cho visual QA.

---

## Design Direction

- Landing là phần giới thiệu sản phẩm, nhưng vẫn phải cùng DNA với không gian role: classic academic, composed, trustworthy.
- Color strategy: restrained public brand. Nền ivory, surface ấm, charcoal CTA, brass dùng ít cho focus và accent.
- Theme scene: người dùng mới đang cân nhắc một nền tảng học tiếng Anh trên laptop hoặc điện thoại, cần hiểu nhanh "học viên được gì, giảng viên quản được gì, admin vận hành ra sao".
- Named anchors nội bộ: Teacher sidebar/header mới, Admin/Student role shell, `DESIGN.md` "Quiet Academic Command Room".
- Không dùng AI-purple gradient, headline gradient, fake metrics, fake testimonials, grid 3 card giống nhau, link `#`, icon SVG tự vẽ rải rác, hoặc landing copy kiểu chung chung.
- Visual assets phải là sản phẩm thật: ưu tiên screenshot thật của Student/Teacher/Admin sau khi app chạy. Không dùng ảnh stock người ngồi học nếu nó không cho thấy sản phẩm.

## Current Audit Baseline

- `Views/Home/Index.cshtml` đang dùng blue/indigo gradient, `text-transparent bg-clip-text`, bokeh blur circles, three equal feature cards, fake metrics `15,000+`, `650+`, `98.5%`, `24/7`, testimonials giả và nhiều `href="#"`.
- `_Layout.cshtml` đang là guest layout kiểu app shell với sidebar công khai, màu slate/blue và script inline. Điều này không khớp Teacher/Admin/Student shell hiện tại.
- Landing hiện chưa có CSS public riêng, chưa dùng `role-shell.css` và chưa có test khóa các anti-pattern.
- Brand hiển thị `DuAnTotNghiep` thay vì `AI Study English`, trong khi role shell đã dùng `AI Study English`.

---

### Task 1: Lock the public landing contract with failing tests

**Files:**
- Create: `Tests/Ui/PublicLanding.Tests.cjs`
- Test: `Views/Shared/_Layout.cshtml`
- Test: `Views/Home/Index.cshtml`
- Test: `wwwroot/css/public-shell.css`
- Test: `wwwroot/js/public-shell.js`

- [ ] **Step 1: Add public layout and landing tests**

Create `Tests/Ui/PublicLanding.Tests.cjs`:

```javascript
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const test = require("node:test");

const root = path.resolve(__dirname, "../..");

function read(relativePath) {
  const filePath = path.join(root, relativePath);
  return fs.existsSync(filePath) ? fs.readFileSync(filePath, "utf8") : "";
}

test("Public layout uses the shared classic shell without the old guest sidebar", () => {
  const layout = read("Views/Shared/_Layout.cshtml");

  assert.match(layout, /class="[^"]*public-shell[^"]*role-shell/);
  assert.match(layout, /css\/role-shell\.css/);
  assert.match(layout, /css\/public-shell\.css/);
  assert.match(layout, /js\/public-shell\.js/);
  assert.match(layout, /AI Study English/);
  assert.match(layout, /rs-brand/);
  assert.match(layout, /rs-skip-link/);
  assert.match(layout, /id="mainContent"/);
  assert.doesNotMatch(layout, /mobileSidebar|sidebarOverlay|Danh mục chính|DuAnTotNghiep|bg-blue|text-blue|slate-50/);
  assert.doesNotMatch(layout, /<script>[\s\S]*sidebarToggle[\s\S]*<\/script>/);
});

test("Landing page has real navigation targets and no fake SaaS metrics", () => {
  const home = read("Views/Home/Index.cshtml");

  assert.match(home, /id="features"/);
  assert.match(home, /id="student"/);
  assert.match(home, /id="teacher"/);
  assert.match(home, /id="admin"/);
  assert.match(home, /asp-controller="Account"\s+asp-action="Register"|asp-action="Register"\s+asp-controller="Account"/);
  assert.match(home, /asp-controller="Account"\s+asp-action="Login"|asp-action="Login"\s+asp-controller="Account"/);
  assert.doesNotMatch(home, /href="#"/);
  assert.doesNotMatch(home, /15,000\+|650\+|98\.5%|24\/7|Nhận xét từ người dùng|Testimonials/i);
  assert.doesNotMatch(home, /text-transparent|bg-clip-text|from-blue|to-indigo|gradient-to-r|blur-3xl|rounded-3xl/);
});

test("Public shell CSS uses role tokens and responsive public navigation", () => {
  const css = read("wwwroot/css/public-shell.css");

  assert.match(css, /var\(--rs-canvas\)/);
  assert.match(css, /var\(--rs-surface\)/);
  assert.match(css, /var\(--rs-ink\)/);
  assert.match(css, /var\(--rs-brass\)/);
  assert.match(css, /\.public-header/);
  assert.match(css, /\.public-mobile-panel/);
  assert.match(css, /@media \(max-width:\s*768px\)/);
  assert.match(css, /@media \(prefers-reduced-motion:\s*reduce\)/);
  assert.doesNotMatch(css, /2563EB|4F46E5|blue|indigo|purple|glass|radial-gradient/i);
});

test("Public navigation script is hook based and keyboard friendly", () => {
  const script = read("wwwroot/js/public-shell.js");

  assert.match(script, /\[data-public-menu-toggle\]/);
  assert.match(script, /\[data-public-menu\]/);
  assert.match(script, /aria-expanded/);
  assert.match(script, /event\.key === "Escape"/);
});
```

- [ ] **Step 2: Run the new landing tests and verify they fail**

Run:

```powershell
node --test Tests\Ui\PublicLanding.Tests.cjs
```

Expected: FAIL because `_Layout.cshtml` still uses the old public sidebar, `public-shell.css` and `public-shell.js` do not exist, and the landing page still has blue/indigo gradient, fake metrics and `href="#"`.

---

### Task 2: Convert `_Layout.cshtml` into a public top-navigation shell

**Files:**
- Modify: `Views/Shared/_Layout.cshtml`
- Create: `wwwroot/js/public-shell.js`

- [ ] **Step 1: Replace the public sidebar layout with a top navigation layout**

Use this structure in `Views/Shared/_Layout.cshtml`:

```cshtml
@using System.Security.Claims
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - AI Study English</title>
    <meta name="description" content="@ViewData["MetaDescription"]" />
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Noto+Sans:wght@400;500;600;700;800&display=swap" rel="stylesheet">
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
    <link rel="stylesheet" href="~/css/role-shell.css" asp-append-version="true" />
    <link rel="stylesheet" href="~/css/public-shell.css" asp-append-version="true" />
    <link rel="stylesheet" href="~/DuAnTotNghiep.styles.css" asp-append-version="true" />
</head>
<body class="public-shell role-shell">
    <a class="rs-skip-link" href="#mainContent">Bỏ qua đến nội dung chính</a>

    <header class="public-header" data-public-header>
        <div class="public-header-inner">
            <a asp-area="" asp-controller="Home" asp-action="Index" class="rs-brand public-brand">
                <span class="rs-brand-mark">D</span>
                <span class="rs-brand-copy">
                    <strong>AI Study English</strong>
                    <small>Học tiếng Anh có lộ trình</small>
                </span>
            </a>

            <nav class="public-nav" aria-label="Điều hướng chính">
                <a href="#features">Tính năng</a>
                <a href="#student">Học viên</a>
                <a href="#teacher">Giảng viên</a>
                <a href="#admin">Quản trị</a>
            </nav>

            <div class="public-actions">
                <a asp-area="" asp-controller="Account" asp-action="Login" class="public-link">Đăng nhập</a>
                <a asp-area="" asp-controller="Account" asp-action="Register" class="public-button">Đăng kí</a>
                <button type="button" class="public-menu-button" data-public-menu-toggle aria-expanded="false" aria-controls="publicMobileMenu">
                    <span class="sr-only">Mở menu</span>
                    <span aria-hidden="true">☰</span>
                </button>
            </div>
        </div>

        <nav id="publicMobileMenu" class="public-mobile-panel" data-public-menu hidden aria-label="Điều hướng di động">
            <a href="#features">Tính năng</a>
            <a href="#student">Học viên</a>
            <a href="#teacher">Giảng viên</a>
            <a href="#admin">Quản trị</a>
            <a asp-area="" asp-controller="Account" asp-action="Login">Đăng nhập</a>
            <a asp-area="" asp-controller="Account" asp-action="Register">Đăng kí</a>
        </nav>
    </header>

    <main id="mainContent" class="public-main">
        @RenderBody()
    </main>

    <footer class="public-footer">
        <div class="public-footer-inner">
            <span>&copy; 2026 AI Study English</span>
            <nav aria-label="Liên kết chân trang">
                <a asp-area="" asp-controller="Home" asp-action="Privacy">Chính sách bảo mật</a>
                <a asp-area="" asp-controller="Account" asp-action="Login">Đăng nhập</a>
                <a asp-area="" asp-controller="Account" asp-action="Register">Đăng kí</a>
            </nav>
        </div>
    </footer>

    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>
    <script src="~/js/public-shell.js" asp-append-version="true"></script>
    <partial name="_ToastNotifications" />
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

- [ ] **Step 2: Add hook-based mobile navigation**

Create `wwwroot/js/public-shell.js`:

```javascript
(function () {
  const toggle = document.querySelector("[data-public-menu-toggle]");
  const menu = document.querySelector("[data-public-menu]");

  if (!toggle || !menu) return;

  function setOpen(isOpen) {
    toggle.setAttribute("aria-expanded", String(isOpen));
    menu.hidden = !isOpen;
    document.body.classList.toggle("public-menu-open", isOpen);
  }

  toggle.addEventListener("click", () => {
    const isOpen = toggle.getAttribute("aria-expanded") === "true";
    setOpen(!isOpen);
  });

  menu.querySelectorAll("a").forEach((link) => {
    link.addEventListener("click", () => setOpen(false));
  });

  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
      setOpen(false);
    }
  });

  window.addEventListener("resize", () => {
    if (window.innerWidth > 768) {
      setOpen(false);
    }
  });
})();
```

---

### Task 3: Create `public-shell.css`

**Files:**
- Create: `wwwroot/css/public-shell.css`

- [ ] **Step 1: Add public shell, CTA, section, and responsive styles**

Create `wwwroot/css/public-shell.css`:

```css
.public-shell {
  min-height: 100dvh;
  background: var(--rs-canvas);
  color: var(--rs-ink);
  font-family: var(--rs-font-sans);
  scroll-behavior: smooth;
}

.public-header {
  position: sticky;
  top: 0;
  z-index: 30;
  border-bottom: 1px solid var(--rs-line);
  background: rgb(255 254 250 / 0.96);
}

.public-header-inner,
.public-footer-inner,
.public-section {
  width: min(100%, var(--rs-content-max));
  margin: 0 auto;
  padding-inline: var(--rs-space-page-x);
}

.public-header-inner {
  display: flex;
  min-height: var(--rs-account-height);
  align-items: center;
  justify-content: space-between;
  gap: 1.25rem;
}

.public-nav,
.public-actions,
.public-footer nav {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.public-nav a,
.public-link,
.public-footer a {
  color: var(--rs-ink-muted);
  font-size: 0.9rem;
  font-weight: 700;
  text-decoration: none;
}

.public-nav a:hover,
.public-link:hover,
.public-footer a:hover {
  color: var(--rs-brass-deep);
}

.public-button,
.public-button-secondary,
.public-menu-button {
  display: inline-flex;
  min-height: var(--rs-hit-area);
  align-items: center;
  justify-content: center;
  border-radius: var(--rs-radius-control);
  font-weight: 800;
  text-decoration: none;
  transition: background-color var(--rs-transition), border-color var(--rs-transition), color var(--rs-transition), transform var(--rs-transition);
}

.public-button {
  border: 1px solid var(--rs-ink);
  background: var(--rs-ink);
  color: var(--rs-surface);
  padding: 0.65rem 0.95rem;
}

.public-button:hover {
  border-color: var(--rs-brass-deep);
  background: var(--rs-brass-deep);
}

.public-button-secondary,
.public-menu-button {
  border: 1px solid var(--rs-line);
  background: var(--rs-surface);
  color: var(--rs-ink);
  padding: 0.65rem 0.95rem;
}

.public-button:active,
.public-button-secondary:active,
.public-menu-button:active {
  transform: translateY(1px);
}

.public-menu-button {
  display: none;
  width: var(--rs-hit-area);
  padding: 0;
}

.public-mobile-panel {
  display: grid;
  gap: 0.25rem;
  padding: 0.75rem var(--rs-space-page-x) 1rem;
  border-top: 1px solid var(--rs-line);
  background: var(--rs-surface);
}

.public-mobile-panel a {
  min-height: var(--rs-hit-area);
  border-radius: var(--rs-radius-control);
  color: var(--rs-ink);
  font-weight: 800;
  padding: 0.7rem 0.85rem;
  text-decoration: none;
}

.public-mobile-panel a:hover {
  background: var(--rs-surface-muted);
}

.public-main {
  min-height: 70dvh;
}

.public-section {
  padding-top: clamp(3rem, 8vw, 6rem);
  padding-bottom: clamp(3.5rem, 8vw, 6.5rem);
}

.public-hero {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(20rem, 34rem);
  gap: clamp(2rem, 5vw, 4.5rem);
  align-items: center;
}

.public-hero h1 {
  max-width: 13ch;
  margin: 0;
  color: var(--rs-ink);
  font-size: clamp(2.5rem, 5vw, 5rem);
  font-weight: 800;
  letter-spacing: -0.03em;
  line-height: 1.02;
  text-wrap: balance;
}

.public-lede {
  max-width: 62ch;
  margin: 1.25rem 0 0;
  color: var(--rs-ink-muted);
  font-size: 1.06rem;
  line-height: 1.75;
}

.public-hero-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin-top: 1.75rem;
}

.public-preview {
  overflow: hidden;
  border: 1px solid var(--rs-line);
  border-radius: var(--rs-radius-panel);
  background: var(--rs-surface);
}

.public-preview img {
  width: 100%;
  height: auto;
}

.public-section-header {
  display: grid;
  max-width: 46rem;
  gap: 0.75rem;
}

.public-section-header h2 {
  margin: 0;
  color: var(--rs-ink);
  font-size: clamp(1.75rem, 3vw, 2.75rem);
  font-weight: 800;
  letter-spacing: -0.02em;
  line-height: 1.12;
}

.public-section-header p {
  margin: 0;
  color: var(--rs-ink-muted);
  line-height: 1.7;
}

.public-feature-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(16rem, 1fr));
  gap: 1rem;
  margin-top: 1.5rem;
}

.public-feature {
  border: 1px solid var(--rs-line);
  border-radius: var(--rs-radius-panel);
  background: var(--rs-surface);
  padding: 1.2rem;
}

.public-feature h3 {
  margin: 0;
  color: var(--rs-ink);
  font-size: 1rem;
  font-weight: 800;
}

.public-feature p {
  margin: 0.55rem 0 0;
  color: var(--rs-ink-muted);
  font-size: 0.92rem;
  line-height: 1.65;
}

.public-role-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.1fr) minmax(18rem, 0.9fr);
  gap: 1.25rem;
  margin-top: 1.5rem;
}

.public-footer {
  border-top: 1px solid var(--rs-line);
  background: var(--rs-surface);
}

.public-footer-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding-block: 1.25rem;
  color: var(--rs-ink-muted);
  font-size: 0.84rem;
}

@media (max-width: 900px) {
  .public-hero,
  .public-role-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .public-nav,
  .public-actions .public-link,
  .public-actions .public-button {
    display: none;
  }

  .public-menu-button {
    display: inline-flex;
  }

  .public-footer-inner,
  .public-footer nav {
    align-items: flex-start;
    flex-direction: column;
  }
}

@media (prefers-reduced-motion: reduce) {
  .public-button,
  .public-button-secondary,
  .public-menu-button {
    transition: none;
  }
}
```

---

### Task 4: Rewrite `Views/Home/Index.cshtml` into a real product landing page

**Files:**
- Modify: `Views/Home/Index.cshtml`

- [ ] **Step 1: Replace generic marketing content with product-specific sections**

Use this page structure:

```cshtml
@{
    ViewData["Title"] = "AI Study English";
    ViewData["MetaDescription"] = "AI Study English giúp học viên học tiếng Anh theo lộ trình, giảng viên theo dõi lớp học và quản trị viên vận hành nội dung trong một hệ thống thống nhất.";
}

<section class="public-section public-hero" aria-labelledby="landingHeroTitle">
    <div>
        <p class="auth-eyebrow">AI Study English</p>
        <h1 id="landingHeroTitle">Học tiếng Anh theo một lộ trình rõ ràng</h1>
        <p class="public-lede">
            Nền tảng kết nối học viên, giảng viên và quản trị viên trong cùng một hệ thống: học theo năng lực, theo dõi tiến độ và vận hành nội dung mà không làm người dùng bị rối.
        </p>
        <div class="public-hero-actions">
            <a asp-area="" asp-controller="Account" asp-action="Register" class="public-button">Bắt đầu học</a>
            <a asp-area="" asp-controller="Account" asp-action="Login" class="public-button-secondary">Đăng nhập</a>
        </div>
    </div>

    <figure class="public-preview">
        <img src="~/images/landing/student-dashboard-preview.png" alt="Bảng học viên AI Study English với tiến độ học tập và lộ trình hiện tại" />
    </figure>
</section>

<section id="features" class="public-section" aria-labelledby="featuresTitle">
    <header class="public-section-header">
        <h2 id="featuresTitle">Một hệ thống, ba không gian làm việc</h2>
        <p>Giao diện được tổ chức theo vai trò để mỗi người chỉ thấy những việc cần làm tiếp theo.</p>
    </header>
    <div class="public-feature-grid">
        <article class="public-feature">
            <h3>Lộ trình cho học viên</h3>
            <p>Học viên thấy bài tiếp theo, tiến độ, kết quả kiểm tra và hoạt động gần đây trong cùng một luồng học.</p>
        </article>
        <article class="public-feature">
            <h3>Công cụ cho giảng viên</h3>
            <p>Giảng viên quản lý khóa học, học viên, tài liệu, điểm số, điểm danh và tin nhắn mà không rời workspace.</p>
        </article>
        <article class="public-feature">
            <h3>Vận hành cho quản trị</h3>
            <p>Quản trị viên theo dõi người dùng, thông báo, thành tích, nội dung và trạng thái hệ thống bằng các bảng dễ quét.</p>
        </article>
    </div>
</section>

<section id="student" class="public-section public-role-grid" aria-labelledby="studentTitle">
    <div>
        <header class="public-section-header">
            <h2 id="studentTitle">Học viên biết mình nên học gì tiếp theo</h2>
            <p>Lộ trình học, placement test, kế hoạch hôm nay và tiến độ được gom lại để học viên không phải đoán bước tiếp theo.</p>
        </header>
    </div>
    <figure class="public-preview">
        <img src="~/images/landing/student-learning-path-preview.png" alt="Lộ trình học của học viên với các bước học và tiến độ" />
    </figure>
</section>

<section id="teacher" class="public-section public-role-grid" aria-labelledby="teacherTitle">
    <div>
        <header class="public-section-header">
            <h2 id="teacherTitle">Giảng viên có một bàn làm việc gọn để theo lớp</h2>
            <p>Tài liệu, tin nhắn, lịch dạy, điểm danh và báo cáo được đặt trong shell đồng nhất để giảng viên thao tác nhanh hơn.</p>
        </header>
    </div>
    <figure class="public-preview">
        <img src="~/images/landing/teacher-workspace-preview.png" alt="Không gian giảng viên với sidebar, header và hộp thư tin nhắn" />
    </figure>
</section>

<section id="admin" class="public-section" aria-labelledby="adminTitle">
    <header class="public-section-header">
        <h2 id="adminTitle">Quản trị viên vận hành hệ thống bằng cùng một ngôn ngữ giao diện</h2>
        <p>Admin, Teacher và Student dùng chung font, màu, trạng thái active, header và responsive shell để toàn bộ website không bị rời rạc.</p>
    </header>
    <div class="public-hero-actions">
        <a asp-area="" asp-controller="Account" asp-action="Register" class="public-button">Tạo tài khoản</a>
        <a asp-area="" asp-controller="Account" asp-action="Login" class="public-button-secondary">Vào hệ thống</a>
    </div>
</section>
```

- [ ] **Step 2: Preserve useful route IDs**

Keep `#features`, `#student`, `#teacher`, and `#admin` because `_Layout.cshtml` navigation depends on them. Do not rename these anchors without changing the nav and tests in the same task.

---

### Task 5: Capture real product preview assets

**Files:**
- Create: `Tools/CaptureLandingAssets.cjs`
- Create directory: `wwwroot/images/landing/`
- Create generated asset: `wwwroot/images/landing/student-dashboard-preview.png`
- Create generated asset: `wwwroot/images/landing/student-learning-path-preview.png`
- Create generated asset: `wwwroot/images/landing/teacher-workspace-preview.png`

- [ ] **Step 1: Add a local screenshot capture script**

Create `Tools/CaptureLandingAssets.cjs`:

```javascript
const fs = require("node:fs");
const path = require("node:path");
const { chromium } = require("playwright");

const root = path.resolve(__dirname, "..");
const outputDir = path.join(root, "wwwroot/images/landing");
const baseUrl = process.argv[2] || "http://127.0.0.1:5117";

const shots = [
  {
    email: "student1@aistudyenglish.com",
    password: "Password@123",
    url: "/Student",
    file: "student-dashboard-preview.png"
  },
  {
    email: "student1@aistudyenglish.com",
    password: "Password@123",
    url: "/Student/LearningPath",
    file: "student-learning-path-preview.png"
  },
  {
    email: "teacher@aistudyenglish.com",
    password: "Password@123",
    url: "/Teacher/Messages",
    file: "teacher-workspace-preview.png"
  }
];

async function login(page, email, password) {
  await page.goto(`${baseUrl}/Account/Login`, { waitUntil: "networkidle" });
  await page.fill('input[name="Email"]', email);
  await page.fill('input[name="Password"]', password);
  await Promise.all([
    page.waitForNavigation({ waitUntil: "networkidle" }),
    page.click('button[type="submit"]')
  ]);
}

(async () => {
  fs.mkdirSync(outputDir, { recursive: true });

  const browser = await chromium.launch();
  try {
    for (const shot of shots) {
      const context = await browser.newContext({ viewport: { width: 1440, height: 920 }, deviceScaleFactor: 1 });
      const page = await context.newPage();

      await login(page, shot.email, shot.password);
      await page.goto(`${baseUrl}${shot.url}`, { waitUntil: "networkidle" });
      await page.screenshot({
        path: path.join(outputDir, shot.file),
        fullPage: false
      });

      await context.close();
    }
  } finally {
    await browser.close();
  }
})();
```

- [ ] **Step 2: Run the app and capture the assets**

Run in one terminal:

```powershell
dotnet run --urls http://127.0.0.1:5117
```

Run in another terminal:

```powershell
node Tools\CaptureLandingAssets.cjs http://127.0.0.1:5117
```

Expected: the three `.png` files are created under `wwwroot/images/landing/`. If a route redirects because seed data differs, open the route manually, confirm the current URL, and update only the matching `url` entry in the script.

- [ ] **Step 3: Add asset existence checks to `Tests/Ui/PublicLanding.Tests.cjs`**

Append:

```javascript
test("Landing preview assets exist", () => {
  for (const image of [
    "wwwroot/images/landing/student-dashboard-preview.png",
    "wwwroot/images/landing/student-learning-path-preview.png",
    "wwwroot/images/landing/teacher-workspace-preview.png"
  ]) {
    assert.ok(fs.existsSync(path.join(root, image)), `${image} should exist`);
  }
});
```

Run:

```powershell
node --test Tests\Ui\PublicLanding.Tests.cjs
```

Expected: PASS after the assets are captured and referenced.

---

### Task 6: Update the design contract for public pages

**Files:**
- Modify: `DESIGN.md`

- [ ] **Step 1: Add a Public Landing Shell section to `DESIGN.md`**

Add under Components:

```markdown
### Public Landing Shell
- **Style:** Public pages use the same Noto Sans, canvas, surface, ink, brass, border, radius, focus, and status vocabulary as Admin, Teacher, Student, and Auth.
- **Structure:** `_Layout` is a top-navigation public shell. It must not use a role sidebar, because public visitors are browsing, not managing a workspace.
- **Content:** Landing copy must describe real product flows for Student, Teacher, and Admin. It must not use fake metrics, fake testimonials, dead links, generic AI SaaS promises, or visual sections that cannot be verified in the app.
- **Assets:** Hero and role visuals should be real screenshots from the running product whenever possible. If screenshots are stale, recapture them before shipping.
- **Responsive:** Public navigation collapses below 768px into a keyboard-friendly menu with Escape support and no horizontal overflow.
```

---

### Task 7: Verify landing, auth, and role shell remain aligned

**Files:**
- Test: `Tests/Ui/PublicLanding.Tests.cjs`
- Test: `Tests/Ui/AuthShell.Tests.cjs`
- Test: `Tests/Ui/RoleShellParity.Tests.cjs`
- Test: `Tests/Ui/StaticAssets.Tests.cjs`

- [ ] **Step 1: Run the full UI static suite**

Run:

```powershell
node --test Tests\Ui\PublicLanding.Tests.cjs Tests\Ui\AuthShell.Tests.cjs Tests\Ui\RoleShellParity.Tests.cjs Tests\Ui\StudentShell.Tests.cjs Tests\Ui\TeacherShell.Tests.cjs Tests\Ui\AdminSidebarState.Tests.cjs Tests\Ui\StaticAssets.Tests.cjs
```

Expected: PASS for all tests.

- [ ] **Step 2: Build and lint**

Run:

```powershell
dotnet build DuAnTotNghiep.csproj -p:UseAppHost=false
npm run design:lint
```

Expected: `Build succeeded` with 0 errors. `design:lint` may keep existing DESIGN metadata warnings, but must report 0 errors.

- [ ] **Step 3: Run the web app for visual QA**

Run:

```powershell
dotnet run --urls http://127.0.0.1:5117
```

Open:

- `http://127.0.0.1:5117/`
- `http://127.0.0.1:5117/Account/Login`
- `http://127.0.0.1:5117/Student`
- `http://127.0.0.1:5117/Teacher`
- `http://127.0.0.1:5117/Admin`

Check:

- Landing header brand, height, font, CTA shape and focus ring feel related to Teacher/Admin/Student.
- Public landing uses top nav, not role sidebar.
- CTA links go to real Login/Register routes.
- Screenshots render and are not broken.
- Mobile 390px has no horizontal overflow and menu opens/closes with click, link click and Escape.
- No visible fake stats, fake testimonials, placeholder images or dead links.

---

## Acceptance Criteria

- `/` feels like the public face of the same product, not a separate blue SaaS template.
- `_Layout.cshtml` uses `role-shell.css`, `public-shell.css`, Noto Sans and hook-based `public-shell.js`.
- Landing page contains real navigation targets, real Login/Register CTAs and no `href="#"`.
- Landing page no longer contains blue/indigo gradient text, fake metrics, fake testimonials, bokeh blur circles or rounded-3xl card stacks.
- Public visuals are product screenshots stored under `wwwroot/images/landing/`.
- The web app runs at `http://127.0.0.1:5117` after implementation for manual checking.
