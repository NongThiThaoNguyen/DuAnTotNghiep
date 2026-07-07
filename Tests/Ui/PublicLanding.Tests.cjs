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
  assert.match(home, /public-role-row--image-left/);
  assert.match(home, /public-role-row--image-right/);
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
  assert.match(css, /@keyframes public-rise/);
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

test("Landing preview assets exist", () => {
  for (const image of [
    "wwwroot/images/landing/student-dashboard-preview.png",
    "wwwroot/images/landing/student-learning-path-preview.png",
    "wwwroot/images/landing/teacher-workspace-preview.png"
  ]) {
    assert.ok(fs.existsSync(path.join(root, image)), `${image} should exist`);
  }
});
