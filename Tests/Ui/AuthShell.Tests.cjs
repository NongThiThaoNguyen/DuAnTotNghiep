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
  assert.match(layout, /auth-compact-note/);
  assert.match(layout, /auth-side-note-left/);
  assert.match(layout, /auth-side-note-right/);
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
  assert.match(css, /justify-items:\s*center/);
  assert.match(css, /\.auth-panel\s*{/);
  assert.match(css, /\.auth-side-note\s*{/);
  assert.match(css, /@media \(max-width:\s*1120px\)/);
  assert.match(css, /white-space:\s*nowrap/);
  assert.match(css, /@media \(max-width:\s*640px\)/);
  assert.match(css, /@media \(prefers-reduced-motion:\s*reduce\)/);
  assert.match(css, /@keyframes auth-rise/);
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
