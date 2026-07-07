const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const test = require("node:test");

const root = path.resolve(__dirname, "../..");

function read(relativePath) {
  return fs.readFileSync(path.join(root, relativePath), "utf8");
}

test("both Student layouts use the shared classic role shell", () => {
  const layouts = [
    read("Views/Shared/_StudentLayout.cshtml"),
    read("Views/Shared/_AILearnLayout.cshtml")
  ];

  for (const layout of layouts) {
    assert.match(layout, /class="[^"]*role-shell[^"]*student-shell/);
    assert.match(layout, /_StudentSidebar/);
    assert.match(layout, /css\/role-shell\.css/);
    assert.match(layout, /css\/student-shell\.css/);
    assert.match(layout, /Noto\+Sans/);
    assert.doesNotMatch(layout, /partial name="_Navbar"/);
  }
});

test("Admin and Student shells share the Vietnamese-friendly Noto Sans font", () => {
  const layouts = [
    read("Views/Shared/_AdminLayout.cshtml"),
    read("Views/Shared/_StudentLayout.cshtml"),
    read("Views/Shared/_AILearnLayout.cshtml"),
    read("Views/Shared/_OnboardingLayout.cshtml")
  ];
  const roleCss = read("wwwroot/css/role-shell.css");
  const studentCss = read("wwwroot/css/student-shell.css");
  const learningPathCss = read("wwwroot/css/learning-path.css");

  for (const layout of layouts) {
    assert.match(layout, /Noto\+Sans/);
    assert.doesNotMatch(layout, /Be\+Vietnam\+Pro/);
  }

  assert.match(roleCss, /--rs-font-sans:\s*"Noto Sans", Arial, sans-serif/);
  assert.match(roleCss, /font-family:\s*var\(--rs-font-sans\)/);
  assert.match(studentCss, /font-family:\s*var\(--rs-font-sans\)/);
  assert.match(learningPathCss, /font-family:\s*var\(--rs-font-sans\)/);
  assert.doesNotMatch(studentCss, /Be Vietnam Pro|Roboto/i);
  assert.doesNotMatch(learningPathCss, /Be Vietnam Pro|Roboto/i);
});

test("shared landing and auth layouts use the same readable Vietnamese font", () => {
  const layouts = [
    read("Views/Shared/_Layout.cshtml"),
    read("Views/Shared/_AuthLayout.cshtml")
  ];
  const siteCss = read("wwwroot/css/site.css");

  for (const layout of layouts) {
    assert.match(layout, /Noto\+Sans/);
    assert.doesNotMatch(layout, /Roboto/);
  }

  assert.match(siteCss, /--font-sans:\s*"Noto Sans", Arial, sans-serif/);
});

test("role shell exposes maintainable layout primitives for Admin and Student", () => {
  const roleCss = read("wwwroot/css/role-shell.css");
  const studentCss = read("wwwroot/css/student-shell.css");
  const layouts = [
    read("Views/Shared/_AdminLayout.cshtml"),
    read("Views/Shared/_StudentLayout.cshtml"),
    read("Views/Shared/_AILearnLayout.cshtml"),
    read("Views/Shared/_OnboardingLayout.cshtml")
  ];

  for (const token of [
    "--rs-content-max",
    "--rs-space-page-x",
    "--rs-space-page-y",
    "--rs-hit-area",
    "--rs-focus-ring",
    "--rs-z-overlay",
    "--rs-z-sidebar",
    "--rs-z-modal"
  ]) {
    assert.match(roleCss, new RegExp(token));
  }

  assert.match(roleCss, /\.rs-skip-link/);
  assert.match(studentCss, /width:\s*min\(100%, var\(--rs-content-max\)\)/);
  assert.match(studentCss, /padding:\s*var\(--rs-space-page-y\) var\(--rs-space-page-x\)/);
  assert.match(studentCss, /z-index:\s*var\(--rs-z-sidebar\)/);
  assert.match(studentCss, /z-index:\s*var\(--rs-z-overlay\)/);

  for (const layout of layouts) {
    assert.match(layout, /class="rs-skip-link"/);
    assert.match(layout, /id="mainContent"/);
  }
});

test("Student onboarding uses the shared shell instead of per-view card styles", () => {
  const layout = read("Views/Shared/_OnboardingLayout.cshtml");
  const roleCss = read("wwwroot/css/role-shell.css");
  const onboardingViews = [
    read("Areas/Student/Views/Onboarding/Goal.cshtml"),
    read("Areas/Student/Views/Onboarding/Level.cshtml"),
    read("Areas/Student/Views/Onboarding/Skills.cshtml"),
    read("Areas/Student/Views/Onboarding/StudyTime.cshtml"),
    read("Areas/Student/Views/Onboarding/Confirm.cshtml")
  ];

  assert.match(layout, /role-shell onboarding-shell/);
  assert.match(layout, /css\/role-shell\.css/);
  assert.match(layout, /Noto\+Sans/);
  assert.doesNotMatch(layout, /Roboto|Plus\+Jakarta|style="font-family/);
  assert.match(roleCss, /\.onboarding-shell/);
  assert.match(roleCss, /\.goal-radio:checked \+ \.goal-card-body/);
  assert.match(roleCss, /\.level-radio:checked \+ \.level-card-body/);
  assert.match(roleCss, /\.skill-checkbox:checked \+ \.skill-card-body/);
  assert.match(roleCss, /\.time-radio:checked \+ \.time-card-body/);

  for (const view of onboardingViews) {
    assert.doesNotMatch(view, /<style>/);
  }
});

test("the shared Student sidebar exposes only real learning destinations", () => {
  const sidebarPath = path.join(root, "Views/Shared/_StudentSidebar.cshtml");
  assert.ok(fs.existsSync(sidebarPath), "_StudentSidebar.cshtml must exist");

  const sidebar = fs.readFileSync(sidebarPath, "utf8");
  const destinations = [
    "/Dashboard",
    "/Student/LearningPath",
    "/Student/StudyPlan/Today",
    "/Student/Progress",
    "/Student/PlacementTest/Intro",
    "/Courses",
    "/Quiz",
    "/AITutor",
    "/Notes",
    "/Achievements",
    "/Profile"
  ];

  for (const destination of destinations) {
    assert.match(sidebar, new RegExp(destination.replaceAll("/", "\\/")));
  }

  assert.doesNotMatch(sidebar, /Sắp ra mắt/);
});

test("Student sidebar does not rely on Tailwind transform utilities for visibility", () => {
  const sidebar = read("Views/Shared/_StudentSidebar.cshtml");
  const layouts = [
    read("Views/Shared/_StudentLayout.cshtml"),
    read("Views/Shared/_AILearnLayout.cshtml")
  ];
  const css = read("wwwroot/css/student-shell.css");

  assert.doesNotMatch(sidebar, /-translate-x-full/);
  for (const layout of layouts) {
    assert.doesNotMatch(layout, /-translate-x-full/);
    assert.match(layout, /student-sidebar-closed/);
  }
  assert.match(css, /#studentSidebar\.student-sidebar-closed/);
  assert.match(css, /@media \(min-width: 1024px\)[\s\S]*#studentSidebar/);
});

test("Student styling removes the legacy purple AI-SaaS identity", () => {
  const cssPath = path.join(root, "wwwroot/css/student-shell.css");
  assert.ok(fs.existsSync(cssPath), "student-shell.css must exist");

  const css = fs.readFileSync(cssPath, "utf8");
  assert.match(css, /--rs-brass/);
  assert.match(css, /student-mobile-topbar/);
  assert.match(css, /prefers-reduced-motion/);
  assert.match(css, /\.student-shell \.font-sans[\s\S]*var\(--rs-font-sans\)/);
  assert.match(css, /\.student-shell \.rounded-3xl/);
  assert.match(css, /\.student-shell \.bg-gradient-to-r/);
  assert.match(css, /Font Awesome 6 Free/);
  assert.doesNotMatch(css, /#6C63FF/i);
  assert.doesNotMatch(css, /linear-gradient/i);
});

test("Student shell constrains the frame so long learning pages scroll inside main content", () => {
  const css = read("wwwroot/css/student-shell.css");

  assert.match(css, /\.student-shell\s*{[\s\S]*height:\s*100dvh/);
  assert.match(css, /\.student-shell-frame\s*{[\s\S]*height:\s*100%/);
  assert.match(css, /\.student-shell-frame\s*{[\s\S]*overflow:\s*hidden/);
  assert.match(css, /\.student-content\s*{[\s\S]*height:\s*100%/);
  assert.match(css, /\.student-content\s*{[\s\S]*overflow-y:\s*auto/);
  assert.match(css, /\.student-content\s*{[\s\S]*overscroll-behavior:\s*contain/);
  assert.match(css, /\.student-page\s*{[\s\S]*flex:\s*0 0 auto/);
  assert.doesNotMatch(css, /\.student-page\s*{[\s\S]*flex:\s*1/);
});

test("Student overview and profile avoid broken avatar fallbacks", () => {
  const views = [
    read("Areas/Student/Views/Home/Index.cshtml"),
    read("Areas/Student/Views/Profile/EditLearningProfile.cshtml")
  ];

  for (const view of views) {
    assert.doesNotMatch(view, /default-images\/avatar\.png/);
    assert.match(view, /student-overview-avatar-fallback/);
  }
});

test("Student pages use calm Admin-aligned copy without decorative emoji", () => {
  const views = [
    read("Areas/Student/Views/Home/Index.cshtml"),
    read("Areas/Student/Views/Profile/EditLearningProfile.cshtml"),
    read("Areas/Student/Views/LearningPath/Index.cshtml"),
    read("Areas/Student/Views/LearningPath/_PathProgressBar.cshtml"),
    read("Areas/Student/Views/Progress/Index.cshtml"),
    read("Areas/Student/Views/StudyPlan/Today.cshtml")
  ];

  for (const view of views) {
    assert.doesNotMatch(view, /👋|🔥|🏆|🎉|👇|✨/u);
  }
});

test("Learning path stylesheet uses the shared classic Student palette", () => {
  const css = read("wwwroot/css/learning-path.css");

  assert.match(css, /var\(--rs-ink\)/);
  assert.match(css, /var\(--rs-brass\)/);
  assert.match(css, /var\(--rs-surface\)/);
  assert.match(css, /var\(--rs-font-sans\)/);
  assert.doesNotMatch(css, /#2563EB|#1D4ED8|#60A5FA|#DBEAFE|#BFDBFE/i);
  assert.doesNotMatch(css, /Roboto/i);
  assert.doesNotMatch(css, /linear-gradient/i);
});

test("completed placement-test navigation opens the result instead of Student overview", () => {
  const controller = read("Areas/Student/Controllers/PlacementTestController.cs");
  const requirementService = read("Services/PlacementRequirementService.cs");

  assert.doesNotMatch(controller, /RedirectToAction\("Index",\s*"Home"\)/);
  assert.match(controller, /RedirectToAction\("Result"/);
  assert.match(requirementService, /completedAttempt/);
  assert.match(requirementService, /RedirectUrl\s*=\s*\$"\/Student\/PlacementTest\/Result\?attemptId=/);
});

test("Student placement-test pages stay inside the shared Student shell", () => {
  const viewStart = read("Areas/Student/Views/PlacementTest/_ViewStart.cshtml");
  const explicitViews = [
    read("Areas/Student/Views/PlacementTest/Intro.cshtml"),
    read("Areas/Student/Views/PlacementTest/Result.cshtml"),
    read("Areas/Student/Views/PlacementTest/Take.cshtml")
  ];

  assert.match(viewStart, /_StudentLayout\.cshtml/);
  for (const view of explicitViews) {
    assert.match(view, /_StudentLayout\.cshtml/);
    assert.doesNotMatch(view, /Layout\s*=\s*"_Layout"/);
  }
});

test("the Student dashboard uses calm copy without decorative emoji", () => {
  const dashboard = read("Views/Dashboard/Index.cshtml");
  assert.match(dashboard, /dashboard-next-task/);
  assert.doesNotMatch(dashboard, /👋|🔥/u);
  assert.doesNotMatch(dashboard, /purple/);
});

test("dashboard stylesheet uses the shared classic token vocabulary", () => {
  const css = read("wwwroot/css/dashboard.css");

  assert.match(css, /var\(--rs-ink\)/);
  assert.match(css, /var\(--rs-surface\)/);
  assert.match(css, /var\(--rs-brass-deep\)/);
  assert.doesNotMatch(css, /#6C63FF/i);
  assert.doesNotMatch(css, /linear-gradient/i);
});
