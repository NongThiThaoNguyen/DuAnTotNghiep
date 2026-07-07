const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const test = require("node:test");

const root = path.resolve(__dirname, "../..");

function read(relativePath) {
  const filePath = path.join(root, relativePath);
  return fs.existsSync(filePath) ? fs.readFileSync(filePath, "utf8") : "";
}

function listCshtmlFiles(relativeDir) {
  const absoluteDir = path.join(root, relativeDir);
  const entries = fs.readdirSync(absoluteDir, { withFileTypes: true });
  return entries.flatMap(entry => {
    const child = path.join(relativeDir, entry.name);
    if (entry.isDirectory()) {
      return listCshtmlFiles(child);
    }
    return entry.isFile() && entry.name.endsWith(".cshtml") ? [child.replaceAll("\\", "/")] : [];
  });
}

test("Teacher uses the shared classic role shell", () => {
  const layout = read("Views/Shared/_TeacherLayout.cshtml");

  assert.match(layout, /role-shell teacher-shell/);
  assert.match(layout, /css\/role-shell\.css/);
  assert.match(layout, /css\/teacher-shell\.css/);
  assert.match(layout, /_TeacherSidebar/);
  assert.match(layout, /_TeacherAccountStrip/);
  assert.match(layout, /class="rs-skip-link"/);
  assert.match(layout, /id="mainContent"/);
  assert.match(layout, /Noto\+Sans/);
  assert.doesNotMatch(layout, /layout\.css|responsive\.css|Be\+Vietnam\+Pro|Poppins/);
  assert.doesNotMatch(layout, /<style[\s>]/i);
});

test("Teacher shell has a real mobile drawer and bounded main scroll", () => {
  const css = read("wwwroot/css/teacher-shell.css");

  assert.match(css, /\.teacher-shell\s*{[\s\S]*height:\s*100dvh/);
  assert.match(css, /\.teacher-shell-frame\s*{[\s\S]*overflow:\s*hidden/);
  assert.match(css, /\.teacher-content\s*{[\s\S]*overflow-y:\s*auto/);
  assert.match(css, /#teacherSidebar\.teacher-sidebar-closed/);
  assert.match(css, /\.teacher-sidebar-overlay/);
  assert.match(css, /@media \(min-width:\s*1024px\)/);
  assert.match(css, /var\(--rs-z-sidebar\)/);
  assert.match(css, /var\(--rs-z-overlay\)/);
});

test("Teacher styling removes the legacy purple SaaS identity", () => {
  const sources = [
    read("Views/Shared/_TeacherLayout.cshtml"),
    read("wwwroot/css/teacher-shell.css")
  ];

  for (const source of sources) {
    assert.doesNotMatch(source, /#6C63FF|#5A52E6|linear-gradient|backdrop-filter/i);
  }
});

test("Teacher behavior uses explicit hooks instead of global DOM rewriting", () => {
  const layout = read("Views/Shared/_TeacherLayout.cshtml");
  const shell = read("wwwroot/js/teacher-shell.js");
  const modal = read("wwwroot/js/teacher-modal.js");

  assert.doesNotMatch(layout, /polishButtons|href\*="\/Edit\/"|href\*="\/Delete\/"/);
  assert.match(shell, /teacherSidebarToggle/);
  assert.match(shell, /event\.key === "Escape"/);
  assert.match(shell, /textContent/);
  assert.match(modal, /data-teacher-modal/);
  assert.match(modal, /data-teacher-modal-close/);
  assert.match(modal, /FormData/);
  assert.match(modal, /teacher-modal-error/);
  assert.doesNotMatch(modal, /window\.alert|\balert\s*\(/);
});

test("Teacher dashboard uses calm shared components", () => {
  const view = read("Areas/Teacher/Views/Home/Index.cshtml");

  assert.match(view, /teacher-page-header/);
  assert.match(view, /teacher-stat-grid/);
  assert.match(view, /teacher-dashboard-grid/);
  assert.doesNotMatch(view, /<style[\s>]|👋|#6C63FF|gradient/i);
});

test("Teacher messages uses a direct responsive two-pane workspace", () => {
  const view = read("Areas/Teacher/Views/Messages/Index.cshtml");
  const css = read("wwwroot/css/teacher-shell.css");

  assert.match(view, /<aside class="chat-sidebar-list">/);
  assert.match(view, /<section class="chat-main-pane">/);
  assert.doesNotMatch(view, /<div class="row g-0 h-100">/);
  assert.match(css, /\.teacher-chat-container\s*{[\s\S]*grid-template-columns:\s*minmax\(19rem,\s*22rem\)\s+minmax\(0,\s*1fr\)/);
  assert.match(css, /@media \(max-width:\s*767\.98px\)[\s\S]*\.teacher-chat-container\s*{[\s\S]*grid-template-columns:\s*1fr/);
});

test("Teacher content workspace pages use shared classic components", () => {
  const files = [
    "Areas/Teacher/Views/Courses/Index.cshtml",
    "Areas/Teacher/Views/Courses/Create.cshtml",
    "Areas/Teacher/Views/Courses/Edit.cshtml",
    "Areas/Teacher/Views/Courses/Details.cshtml",
    "Areas/Teacher/Views/Courses/Delete.cshtml",
    "Areas/Teacher/Views/Lessons/Index.cshtml",
    "Areas/Teacher/Views/Lessons/Create.cshtml",
    "Areas/Teacher/Views/Lessons/Edit.cshtml",
    "Areas/Teacher/Views/Lessons/Detail.cshtml",
    "Areas/Teacher/Views/Lessons/Details.cshtml",
    "Areas/Teacher/Views/Resources/Index.cshtml",
    "Areas/Teacher/Views/Resources/Create.cshtml",
    "Areas/Teacher/Views/Resources/Edit.cshtml",
    "Areas/Teacher/Views/Resources/Details.cshtml",
    "Areas/Teacher/Views/Resources/Delete.cshtml"
  ];

  for (const file of files) {
    const source = read(file);

    assert.doesNotMatch(source, /<style[\s>]|courses\.css|purple-badge|bg-purple|text-purple|border-purple|shadow-purple|#6C63FF|#5A52E6|linear-gradient|bg-gradient|style="font-family|👋/i, file);
  }

  const courses = read("Areas/Teacher/Views/Courses/Index.cshtml");
  const lessons = read("Areas/Teacher/Views/Lessons/Index.cshtml");
  const resources = read("Areas/Teacher/Views/Resources/Index.cshtml");

  assert.match(courses, /teacher-page-header/);
  assert.match(courses, /teacher-toolbar/);
  assert.match(courses, /teacher-course-grid/);
  assert.match(lessons, /teacher-table-wrap/);
  assert.match(resources, /teacher-table-wrap/);
});

test("Teacher assessment pages use shared classic components", () => {
  const files = [
    "Areas/Teacher/Views/Assignments/Index.cshtml",
    "Areas/Teacher/Views/Assignments/Create.cshtml",
    "Areas/Teacher/Views/Assignments/Edit.cshtml",
    "Areas/Teacher/Views/Assignments/Delete.cshtml",
    "Areas/Teacher/Views/Assignments/Submissions.cshtml",
    "Areas/Teacher/Views/Assignments/Grade.cshtml",
    "Areas/Teacher/Views/Quizzes/Index.cshtml",
    "Areas/Teacher/Views/Quizzes/Create.cshtml",
    "Areas/Teacher/Views/Quizzes/Edit.cshtml",
    "Areas/Teacher/Views/Quizzes/ManageQuestions.cshtml",
    "Areas/Teacher/Views/Quizzes/QuestionBank.cshtml",
    "Areas/Teacher/Views/Quizzes/Results.cshtml",
    "Areas/Teacher/Views/Grades/Index.cshtml",
    "Areas/Teacher/Views/Grades/Pending.cshtml",
    "Areas/Teacher/Views/Grades/Grade.cshtml"
  ];

  for (const file of files) {
    const source = read(file);

    assert.doesNotMatch(source, /<style[\s>]|courses\.css|purple-badge|#6C63FF|#5A52E6|linear-gradient|bg-gradient|style="font-family|👋/i, file);
  }

  assert.match(read("Areas/Teacher/Views/Assignments/Index.cshtml"), /teacher-table-wrap/);
  assert.match(read("Areas/Teacher/Views/Quizzes/Index.cshtml"), /teacher-table-wrap/);
  assert.match(read("Areas/Teacher/Views/Grades/Index.cshtml"), /teacher-table-wrap/);
  assert.match(read("Areas/Teacher/Views/Grades/Grade.cshtml"), /teacher-grading-layout/);
});

test("Teacher views are free of legacy purple and page-level styling", () => {
  const files = listCshtmlFiles("Areas/Teacher/Views");

  for (const file of files) {
    const source = read(file);

    assert.doesNotMatch(source, /<style[\s>]|courses\.css|purple-|#6C63FF|#5A52E6|linear-gradient|bg-gradient|backdrop-filter|Be Vietnam Pro|Poppins/i, file);
  }
});
