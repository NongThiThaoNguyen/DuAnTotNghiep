# Auth Classic UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Nâng cấp trang đăng nhập và đăng kí thành một auth flow classic academic, đồng bộ font, màu, nhịp spacing và trạng thái tương tác với Admin, Teacher và Student.

**Architecture:** Giữ nguyên `AccountController`, `LoginViewModel`, `RegisterViewModel`, route, antiforgery, validation và luồng điều hướng theo role. Auth dùng `_AuthLayout.cshtml` làm shell chung, nạp `role-shell.css` để lấy token hệ thống, nạp `auth-shell.css` cho bố cục auth, và dùng `auth-shell.js` cho password toggle thay vì script nhúng trong từng view.

**Tech Stack:** ASP.NET Core MVC/Razor, CSS thuần dùng token `--rs-*`, Noto Sans, vanilla JavaScript, jQuery validation hiện có, Node test runner, `dotnet build`, `designmd lint`, Chrome/in-app browser cho visual QA.

---

## Design Direction

- Auth là cửa vào sản phẩm, không phải landing hero. Giao diện cần bình tĩnh, rõ ràng, đáng tin và nhanh để người thật đăng nhập.
- Color strategy: restrained. Dùng canvas ivory, surface ấm, charcoal ink, stone border, brass focus/active giống role shell.
- Theme scene: học viên hoặc giảng viên mở laptop vào buổi tối trước giờ học, cần đăng nhập nhanh, đọc tiếng Việt rõ và không bị phân tâm bởi hiệu ứng trang trí.
- Giữ Noto Sans vì đang được chọn cho tiếng Việt trong `DESIGN.md`.
- Không dùng purple/blue gradient, glassmorphism, dark mesh, icon nổi, emoji trong tiêu đề, nút pill quá mức hoặc CDN riêng cho Bootstrap/Font Awesome trên auth.
- Auth phải có đủ trạng thái: default, validation error, success message từ `TempData`, loading submit, disabled submit, password visible/hidden, mobile 390px, keyboard focus.

## Current Audit Baseline

- `Views/Account/Login.cshtml` và `Views/Account/Register.cshtml` đang `Layout = null`, tự khai báo toàn bộ HTML document.
- Hai view nạp Bootstrap CDN, Font Awesome CDN và `~/css/login.css` riêng, tách khỏi token `role-shell.css`.
- `wwwroot/css/login.css` dùng Outfit, purple `#6C63FF`, dark mesh, radial gradients, glassmorphism, blur, floating icons và border radius 28px.
- Password toggle đang là JavaScript nhúng lặp giữa Login và Register.
- `_AuthLayout.cshtml` tồn tại nhưng chưa được dùng bởi hai view auth chính, còn màu slate/blue và logo `DuAnTotNghiep`.
- Thiếu test khóa quy tắc "auth phải đồng bộ role shell".

---

### Task 1: Lock the auth shell contract with failing tests

**Files:**
- Create: `Tests/Ui/AuthShell.Tests.cjs`
- Test: `Views/Shared/_AuthLayout.cshtml`
- Test: `Views/Account/Login.cshtml`
- Test: `Views/Account/Register.cshtml`
- Test: `wwwroot/css/auth-shell.css`
- Test: `wwwroot/js/auth-shell.js`

- [ ] **Step 1: Add auth structure, palette, and maintainability tests**

Create `Tests/Ui/AuthShell.Tests.cjs`:

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

test("Auth layout consumes the shared classic design system", () => {
  const layout = read("Views/Shared/_AuthLayout.cshtml");

  assert.match(layout, /class="[^"]*auth-shell[^"]*role-shell/);
  assert.match(layout, /css\/role-shell\.css/);
  assert.match(layout, /css\/auth-shell\.css/);
  assert.match(layout, /js\/auth-shell\.js/);
  assert.match(layout, /Noto\+Sans/);
  assert.match(layout, /rs-skip-link/);
  assert.match(layout, /id="mainContent"/);
  assert.match(layout, /AI Study English/);
  assert.doesNotMatch(layout, /bg-blue|text-blue|slate-50|DuAnTotNghiep/);
});

test("Login and register use the shared Auth layout", () => {
  for (const viewPath of ["Views/Account/Login.cshtml", "Views/Account/Register.cshtml"]) {
    const view = read(viewPath);

    assert.match(view, /Layout\s*=\s*"_AuthLayout"/);
    assert.doesNotMatch(view, /<!DOCTYPE html>|<html|<head>|<body/i);
    assert.doesNotMatch(view, /cdn\.jsdelivr|cdnjs\.cloudflare|font-awesome|login\.css/i);
    assert.doesNotMatch(view, /floating-elements|login-card-glass|emoji|🚀|👋/i);
    assert.match(view, /class="[^"]*auth-card/);
    assert.match(view, /class="[^"]*auth-form/);
    assert.match(view, /asp-validation-summary="All"/);
    assert.match(view, /asp-validation-for=/);
  }
});

test("Auth CSS uses role tokens and avoids the old glass UI", () => {
  const css = read("wwwroot/css/auth-shell.css");

  assert.match(css, /var\(--rs-canvas\)/);
  assert.match(css, /var\(--rs-surface\)/);
  assert.match(css, /var\(--rs-ink\)/);
  assert.match(css, /var\(--rs-brass\)/);
  assert.match(css, /var\(--rs-radius-control\)/);
  assert.match(css, /@media \(max-width:\s*640px\)/);
  assert.match(css, /@media \(prefers-reduced-motion:\s*reduce\)/);
  assert.doesNotMatch(css, /6C63FF|31108F|4c1d95|Outfit|glass|blur\(25px\)|radial-gradient/i);
});

test("Auth password toggles are unobtrusive and accessible", () => {
  const script = read("wwwroot/js/auth-shell.js");
  const login = read("Views/Account/Login.cshtml");
  const register = read("Views/Account/Register.cshtml");

  assert.match(script, /\[data-auth-password-toggle\]/);
  assert.match(script, /aria-pressed/);
  assert.match(script, /aria-label/);
  assert.match(script, /type === "password"/);
  assert.match(login, /data-auth-password-toggle/);
  assert.match(register, /data-auth-password-toggle/);
  assert.match(register, /data-auth-password-toggle="confirm"/);
});
```

- [ ] **Step 2: Run the new test and verify it fails for the current UI**

Run:

```powershell
node --test Tests\Ui\AuthShell.Tests.cjs
```

Expected: FAIL because Login/Register are standalone documents, `auth-shell.css` and `auth-shell.js` do not exist, and the old purple/glass UI is still referenced.

---

### Task 2: Rebuild `_AuthLayout.cshtml` as the shared auth shell

**Files:**
- Modify: `Views/Shared/_AuthLayout.cshtml`

- [ ] **Step 1: Replace the slate/blue auth layout with a role-shell based layout**

Use this structure in `Views/Shared/_AuthLayout.cshtml`:

```cshtml
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
    <link rel="stylesheet" href="~/css/auth-shell.css" asp-append-version="true" />
    <link rel="stylesheet" href="~/DuAnTotNghiep.styles.css" asp-append-version="true" />
</head>
<body class="auth-shell role-shell">
    <a class="rs-skip-link" href="#mainContent">Bỏ qua đến nội dung chính</a>

    <div class="auth-page">
        <header class="auth-header" aria-label="Điều hướng xác thực">
            <a asp-area="" asp-controller="Home" asp-action="Index" class="rs-brand auth-brand">
                <span class="rs-brand-mark">D</span>
                <span class="rs-brand-copy">
                    <strong>AI Study English</strong>
                    <small>Không gian học tiếng Anh</small>
                </span>
            </a>
            <nav class="auth-header-actions" aria-label="Liên kết tài khoản">
                <a asp-area="" asp-controller="Home" asp-action="Index" class="auth-link">Trang chủ</a>
                <a asp-area="" asp-controller="Account" asp-action="Login" class="auth-link">Đăng nhập</a>
                <a asp-area="" asp-controller="Account" asp-action="Register" class="auth-button-secondary">Đăng kí</a>
            </nav>
        </header>

        <main id="mainContent" class="auth-main">
            <section class="auth-context" aria-labelledby="authContextTitle">
                <p class="auth-eyebrow">@ViewData["AuthEyebrow"]</p>
                <h1 id="authContextTitle">@ViewData["AuthTitle"]</h1>
                <p>@ViewData["AuthDescription"]</p>
                <dl class="auth-trust-list">
                    <div>
                        <dt>Lộ trình</dt>
                        <dd>Học theo năng lực thật</dd>
                    </div>
                    <div>
                        <dt>Giảng viên</dt>
                        <dd>Theo dõi và phản hồi rõ ràng</dd>
                    </div>
                    <div>
                        <dt>Quản trị</dt>
                        <dd>Vận hành tập trung</dd>
                    </div>
                </dl>
            </section>

            <section class="auth-panel" aria-label="@ViewData["Title"]">
                @RenderBody()
            </section>
        </main>

        <footer class="auth-footer">
            <span>&copy; 2026 AI Study English</span>
            <a asp-area="" asp-controller="Home" asp-action="Privacy">Chính sách bảo mật</a>
        </footer>
    </div>

    <partial name="_ToastNotifications" />
    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/jquery-validation/dist/jquery.validate.min.js"></script>
    <script src="~/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js"></script>
    <script src="~/js/auth-shell.js" asp-append-version="true"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

- [ ] **Step 2: Keep public navigation truthful**

Keep only real links: `Home/Index`, `Account/Login`, `Account/Register`, and `Home/Privacy`. Do not add `href="#"` actions.

---

### Task 3: Create the maintainable auth stylesheet

**Files:**
- Create: `wwwroot/css/auth-shell.css`
- Modify: `Views/Account/Login.cshtml`
- Modify: `Views/Account/Register.cshtml`
- Later cleanup: `wwwroot/css/login.css`

- [ ] **Step 1: Add token-based auth CSS**

Create `wwwroot/css/auth-shell.css` with these component contracts:

```css
.auth-shell {
  min-height: 100dvh;
  background: var(--rs-canvas);
  color: var(--rs-ink);
  font-family: var(--rs-font-sans);
}

.auth-page {
  display: grid;
  min-height: 100dvh;
  grid-template-rows: auto 1fr auto;
}

.auth-header,
.auth-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 0.9rem var(--rs-space-page-x);
  border-bottom: 1px solid var(--rs-line);
  background: rgb(255 254 250 / 0.96);
}

.auth-footer {
  border-top: 1px solid var(--rs-line);
  border-bottom: 0;
  color: var(--rs-ink-muted);
  font-size: 0.82rem;
}

.auth-main {
  display: grid;
  width: min(100%, 72rem);
  margin: 0 auto;
  grid-template-columns: minmax(0, 1fr) minmax(22rem, 28rem);
  gap: clamp(2rem, 5vw, 4.5rem);
  align-items: center;
  padding: clamp(1.25rem, 4vw, 4rem) var(--rs-space-page-x);
}

.auth-context {
  max-width: 40rem;
}

.auth-eyebrow {
  margin: 0 0 0.75rem;
  color: var(--rs-brass);
  font-size: 0.72rem;
  font-weight: 800;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.auth-context h1 {
  margin: 0;
  max-width: 12ch;
  color: var(--rs-ink);
  font-size: clamp(2.1rem, 4vw, 4rem);
  font-weight: 800;
  letter-spacing: -0.025em;
  line-height: 1.05;
  text-wrap: balance;
}

.auth-context p {
  max-width: 58ch;
  margin: 1rem 0 0;
  color: var(--rs-ink-muted);
  font-size: 1rem;
  line-height: 1.7;
}

.auth-trust-list {
  display: grid;
  margin: 2rem 0 0;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 0.75rem;
}

.auth-trust-list div,
.auth-card {
  border: 1px solid var(--rs-line);
  border-radius: var(--rs-radius-panel);
  background: var(--rs-surface);
}

.auth-trust-list div {
  padding: 0.9rem;
}

.auth-trust-list dt {
  color: var(--rs-ink);
  font-size: 0.76rem;
  font-weight: 800;
}

.auth-trust-list dd {
  margin: 0.25rem 0 0;
  color: var(--rs-ink-muted);
  font-size: 0.8rem;
  line-height: 1.45;
}

.auth-card {
  padding: clamp(1.25rem, 3vw, 2rem);
}

.auth-card-header h2 {
  margin: 0;
  color: var(--rs-ink);
  font-size: 1.35rem;
  font-weight: 800;
}

.auth-card-header p {
  margin: 0.45rem 0 0;
  color: var(--rs-ink-muted);
  font-size: 0.9rem;
  line-height: 1.6;
}

.auth-form {
  display: grid;
  gap: 1rem;
  margin-top: 1.35rem;
}

.auth-field {
  display: grid;
  gap: 0.45rem;
}

.auth-label-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}

.auth-field label {
  color: var(--rs-ink);
  font-size: 0.83rem;
  font-weight: 700;
}

.auth-input-wrap {
  position: relative;
}

.auth-field input {
  width: 100%;
  min-height: 2.75rem;
  border: 1px solid var(--rs-line);
  border-radius: var(--rs-radius-control);
  background: var(--rs-surface);
  color: var(--rs-ink);
  font: inherit;
  padding: 0.72rem 0.85rem;
}

.auth-field input:focus {
  border-color: var(--rs-brass);
  box-shadow: var(--rs-focus-ring);
  outline: 0;
}

.auth-field .field-validation-error,
.auth-validation {
  color: var(--rs-danger);
  font-size: 0.8rem;
  line-height: 1.45;
}

.auth-validation {
  border: 1px solid rgb(155 47 50 / 0.25);
  border-radius: var(--rs-radius-control);
  background: var(--rs-danger-bg);
  padding: 0.75rem 0.85rem;
}

.auth-submit,
.auth-button-secondary {
  display: inline-flex;
  min-height: var(--rs-hit-area);
  align-items: center;
  justify-content: center;
  border-radius: var(--rs-radius-control);
  font-weight: 800;
  transition: background-color var(--rs-transition), color var(--rs-transition), transform var(--rs-transition), border-color var(--rs-transition);
}

.auth-submit {
  width: 100%;
  border: 1px solid var(--rs-ink);
  background: var(--rs-ink);
  color: var(--rs-surface);
  padding: 0.72rem 1rem;
}

.auth-submit:hover {
  background: var(--rs-brass-deep);
  border-color: var(--rs-brass-deep);
}

.auth-submit:active {
  transform: translateY(1px);
}

.auth-button-secondary,
.auth-link {
  color: var(--rs-ink);
  text-decoration: none;
}

.auth-button-secondary {
  border: 1px solid var(--rs-line);
  background: var(--rs-surface);
  padding: 0.62rem 0.9rem;
}

.auth-button-secondary:hover,
.auth-link:hover {
  color: var(--rs-brass-deep);
}

.auth-password-toggle {
  position: absolute;
  top: 50%;
  right: 0.55rem;
  display: inline-grid;
  width: 2rem;
  height: 2rem;
  place-items: center;
  border: 0;
  border-radius: var(--rs-radius-control);
  background: transparent;
  color: var(--rs-ink-muted);
  transform: translateY(-50%);
}

.auth-password-toggle:hover {
  background: var(--rs-surface-muted);
  color: var(--rs-ink);
}

.auth-footer-link {
  color: var(--rs-brass-deep);
  font-weight: 800;
}

@media (max-width: 900px) {
  .auth-main {
    grid-template-columns: 1fr;
    align-items: start;
  }

  .auth-context h1 {
    max-width: 16ch;
  }
}

@media (max-width: 640px) {
  .auth-header,
  .auth-footer,
  .auth-header-actions {
    align-items: flex-start;
    flex-direction: column;
  }

  .auth-trust-list {
    grid-template-columns: 1fr;
  }

  .auth-card {
    padding: 1rem;
  }
}

@media (prefers-reduced-motion: reduce) {
  .auth-submit,
  .auth-button-secondary,
  .auth-password-toggle {
    transition: none;
  }
}
```

- [ ] **Step 2: Keep `login.css` out of the rendered auth pages**

After Login/Register no longer reference `~/css/login.css`, delete `wwwroot/css/login.css` or leave it only if another page references it. Verify with:

```powershell
rg "login\.css|login-card-glass|floating-elements|6C63FF" Views wwwroot -g "!wwwroot/css/login.css"
```

Expected: no output.

---

### Task 4: Rewrite the Login view to use the shared auth shell

**Files:**
- Modify: `Views/Account/Login.cshtml`

- [ ] **Step 1: Replace the standalone Login document with Razor content**

Use this structure:

```cshtml
@model DuAnTotNghiep.Models.ViewModels.LoginViewModel
@{
    Layout = "_AuthLayout";
    ViewData["Title"] = "Đăng nhập";
    ViewData["MetaDescription"] = "Đăng nhập AI Study English để tiếp tục học, giảng dạy hoặc quản trị.";
    ViewData["AuthEyebrow"] = "Đăng nhập";
    ViewData["AuthTitle"] = "Tiếp tục không gian học tiếng Anh";
    ViewData["AuthDescription"] = "Một tài khoản dùng cho học viên, giảng viên và quản trị viên. Sau khi đăng nhập, hệ thống sẽ đưa bạn đến đúng không gian làm việc.";
}

<article class="auth-card">
    <header class="auth-card-header">
        <h2>Đăng nhập</h2>
        <p>Nhập email và mật khẩu đã được cấp để tiếp tục.</p>
    </header>

    <form asp-controller="Account" asp-action="Login" method="post" class="auth-form" data-auth-form>
        @Html.AntiForgeryToken()

        @if (!ViewData.ModelState.IsValid)
        {
            <div class="auth-validation" role="alert">
                <div asp-validation-summary="All"></div>
            </div>
        }

        <div class="auth-field">
            <label asp-for="Email">Email</label>
            <input asp-for="Email" type="email" autocomplete="email" placeholder="student1@aistudyenglish.com" />
            <span asp-validation-for="Email"></span>
        </div>

        <div class="auth-field">
            <div class="auth-label-row">
                <label asp-for="Password">Mật khẩu</label>
                <a asp-action="ForgotPassword" class="auth-footer-link">Quên mật khẩu?</a>
            </div>
            <div class="auth-input-wrap">
                <input asp-for="Password" type="password" autocomplete="current-password" placeholder="Nhập mật khẩu" data-auth-password-input="password" />
                <button class="auth-password-toggle" type="button" data-auth-password-toggle="password" aria-label="Hiện mật khẩu" aria-pressed="false">
                    <span aria-hidden="true">Aa</span>
                </button>
            </div>
            <span asp-validation-for="Password"></span>
        </div>

        <label class="auth-check">
            <input asp-for="RememberMe" type="checkbox" />
            <span>Ghi nhớ đăng nhập trên thiết bị này</span>
        </label>

        <button type="submit" class="auth-submit" data-auth-submit>Đăng nhập</button>

        <p class="auth-switch">
            Chưa có tài khoản?
            <a asp-action="Register" class="auth-footer-link">Đăng kí tài khoản</a>
        </p>
    </form>
</article>
```

- [ ] **Step 2: Confirm model property names**

Before editing, confirm `LoginViewModel` contains `Email`, `Password`, and `RememberMe`. If `RememberMe` has a different name, keep the existing property and update the test only for rendered behavior, not the model contract.

---

### Task 5: Rewrite the Register view to use the shared auth shell

**Files:**
- Modify: `Views/Account/Register.cshtml`

- [ ] **Step 1: Replace the standalone Register document with Razor content**

Use this structure:

```cshtml
@model DuAnTotNghiep.Models.ViewModels.RegisterViewModel
@{
    Layout = "_AuthLayout";
    ViewData["Title"] = "Đăng kí";
    ViewData["MetaDescription"] = "Tạo tài khoản AI Study English để bắt đầu học tiếng Anh theo lộ trình phù hợp.";
    ViewData["AuthEyebrow"] = "Tạo tài khoản";
    ViewData["AuthTitle"] = "Bắt đầu với một lộ trình rõ ràng";
    ViewData["AuthDescription"] = "Tạo tài khoản học viên mới. Sau khi đăng kí, bạn có thể đăng nhập và hoàn thành các bước thiết lập hồ sơ học tập.";
}

<article class="auth-card">
    <header class="auth-card-header">
        <h2>Đăng kí tài khoản</h2>
        <p>Thông tin này giúp hệ thống nhận diện bạn trong quá trình học.</p>
    </header>

    <form asp-controller="Account" asp-action="Register" method="post" class="auth-form" data-auth-form>
        @Html.AntiForgeryToken()

        @if (!ViewData.ModelState.IsValid)
        {
            <div class="auth-validation" role="alert">
                <div asp-validation-summary="All"></div>
            </div>
        }

        <div class="auth-field">
            <label asp-for="FullName">Họ và tên</label>
            <input asp-for="FullName" autocomplete="name" placeholder="Nguyễn Minh Anh" />
            <span asp-validation-for="FullName"></span>
        </div>

        <div class="auth-field">
            <label asp-for="Email">Email</label>
            <input asp-for="Email" type="email" autocomplete="email" placeholder="student1@aistudyenglish.com" />
            <span asp-validation-for="Email"></span>
        </div>

        <div class="auth-field">
            <label asp-for="Password">Mật khẩu</label>
            <div class="auth-input-wrap">
                <input asp-for="Password" type="password" autocomplete="new-password" placeholder="Tạo mật khẩu" data-auth-password-input="password" />
                <button class="auth-password-toggle" type="button" data-auth-password-toggle="password" aria-label="Hiện mật khẩu" aria-pressed="false">
                    <span aria-hidden="true">Aa</span>
                </button>
            </div>
            <span asp-validation-for="Password"></span>
        </div>

        <div class="auth-field">
            <label asp-for="ConfirmPassword">Xác nhận mật khẩu</label>
            <div class="auth-input-wrap">
                <input asp-for="ConfirmPassword" type="password" autocomplete="new-password" placeholder="Nhập lại mật khẩu" data-auth-password-input="confirm" />
                <button class="auth-password-toggle" type="button" data-auth-password-toggle="confirm" aria-label="Hiện mật khẩu xác nhận" aria-pressed="false">
                    <span aria-hidden="true">Aa</span>
                </button>
            </div>
            <span asp-validation-for="ConfirmPassword"></span>
        </div>

        <button type="submit" class="auth-submit" data-auth-submit>Đăng kí tài khoản</button>

        <p class="auth-switch">
            Đã có tài khoản?
            <a asp-action="Login" class="auth-footer-link">Đăng nhập</a>
        </p>
    </form>
</article>
```

- [ ] **Step 2: Keep registration honest**

Do not render Google/Facebook buttons unless the backend has real OAuth actions. A disabled social button still looks like a broken promise.

---

### Task 6: Move auth behavior into `auth-shell.js`

**Files:**
- Create: `wwwroot/js/auth-shell.js`

- [ ] **Step 1: Add password visibility and submit feedback**

Create `wwwroot/js/auth-shell.js`:

```javascript
(function () {
  function setSubmitBusy(form) {
    const submit = form.querySelector("[data-auth-submit]");
    if (!submit) return;

    submit.disabled = true;
    submit.setAttribute("aria-busy", "true");
    submit.dataset.originalText = submit.textContent.trim();
    submit.textContent = "Đang xử lý";
  }

  document.querySelectorAll("[data-auth-password-toggle]").forEach((button) => {
    const key = button.getAttribute("data-auth-password-toggle");
    const input = document.querySelector(`[data-auth-password-input="${key}"]`);

    if (!input) return;

    button.addEventListener("click", () => {
      const type = input.getAttribute("type");
      const nextType = type === "password" ? "text" : "password";
      const isVisible = nextType === "text";

      input.setAttribute("type", nextType);
      button.setAttribute("aria-pressed", String(isVisible));
      button.setAttribute("aria-label", isVisible ? "Ẩn mật khẩu" : "Hiện mật khẩu");
    });
  });

  document.querySelectorAll("[data-auth-form]").forEach((form) => {
    form.addEventListener("submit", () => {
      if (form.checkValidity()) {
        setSubmitBusy(form);
      }
    });
  });
})();
```

- [ ] **Step 2: Run the auth tests**

Run:

```powershell
node --test Tests\Ui\AuthShell.Tests.cjs
```

Expected: PASS after Tasks 2 to 6 are complete.

---

### Task 7: Update the design contract and verify the flow

**Files:**
- Modify: `DESIGN.md`
- Test: `Tests/Ui/AuthShell.Tests.cjs`
- Test: `Tests/Ui/RoleShellParity.Tests.cjs`
- Test: `Tests/Ui/StaticAssets.Tests.cjs`

- [ ] **Step 1: Add an Auth Shell section to `DESIGN.md`**

Add under Components:

```markdown
### Public Auth Shell
- **Style:** Auth uses the same canvas, surface, ink, brass, border, radius, focus, and Noto Sans tokens as Admin, Teacher, and Student.
- **Structure:** `_AuthLayout` owns the public auth frame. Login and Register render only their card content and never declare a standalone HTML document.
- **Forms:** Inputs use visible labels, inline validation, browser autocomplete, 44px touch targets, password visibility controls, and submit loading state.
- **Visual restraint:** Auth must not use purple/blue gradients, dark mesh backgrounds, glass cards, floating decorative icons, social login buttons without backend support, emoji-led headings, or page-level scripts.
```

- [ ] **Step 2: Run the complete static UI suite**

Run:

```powershell
node --test Tests\Ui\AuthShell.Tests.cjs Tests\Ui\RoleShellParity.Tests.cjs Tests\Ui\StudentShell.Tests.cjs Tests\Ui\TeacherShell.Tests.cjs Tests\Ui\AdminSidebarState.Tests.cjs Tests\Ui\StaticAssets.Tests.cjs
```

Expected: PASS for all tests.

- [ ] **Step 3: Build and lint**

Run:

```powershell
dotnet build DuAnTotNghiep.csproj -p:UseAppHost=false
npm run design:lint
```

Expected: `Build succeeded` with 0 errors. `design:lint` may keep existing metadata warnings, but must report 0 errors.

- [ ] **Step 4: Run the web app for visual QA**

Run:

```powershell
dotnet run --urls http://127.0.0.1:5117
```

Open:

- `http://127.0.0.1:5117/Account/Login`
- `http://127.0.0.1:5117/Account/Register`

Check:

- Desktop: auth header height, brand mark, card width, context column, footer and validation all match the classic role shell.
- Mobile 390px: no horizontal overflow, no text clipped, password toggle remains inside the input, submit remains at least 44px high.
- Keyboard: Tab focus is visible on logo, nav, fields, password toggle, submit, footer links.
- Vietnamese: dấu tiếng Việt in Noto Sans is clean and line height does not clip.

---

## Acceptance Criteria

- Login/Register no longer use `Layout = null`.
- No auth page loads Bootstrap CDN, Font Awesome CDN, `login.css`, purple gradients, dark mesh, glass card or floating icons.
- Auth uses `role-shell.css`, `auth-shell.css`, Noto Sans and the `--rs-*` token family.
- Password toggle is accessible with `aria-label` and `aria-pressed`.
- Validation is inline and visible, not only color-based.
- The web app runs at `http://127.0.0.1:5117` after implementation for manual checking.
