const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const test = require("node:test");

const root = path.resolve(__dirname, "../..");

function read(relativePath) {
  const filePath = path.join(root, relativePath);
  return fs.existsSync(filePath) ? fs.readFileSync(filePath, "utf8") : "";
}

test("role shell owns the shared Teacher geometry", () => {
  const css = read("wwwroot/css/role-shell.css");

  assert.match(css, /--rs-sidebar-width:\s*17\.25rem/);
  assert.match(css, /--rs-account-height:\s*4\.5rem/);
  assert.match(css, /--rs-mobile-bar-height:\s*3\.5rem/);
  assert.match(css, /\.rs-shell-frame\s*{[\s\S]*grid-template-columns:\s*var\(--rs-sidebar-width\)/);
  assert.match(css, /\.rs-account-strip\s*{[\s\S]*min-height:\s*var\(--rs-account-height\)/);
  assert.match(css, /@media \(max-width:\s*1023px\)[\s\S]*\.rs-mobile-topbar/);
});

test("all role layouts consume the shared shell primitives", () => {
  const layouts = [
    read("Views/Shared/_TeacherLayout.cshtml"),
    read("Views/Shared/_StudentLayout.cshtml"),
    read("Views/Shared/_AdminLayout.cshtml")
  ];

  for (const layout of layouts) {
    assert.match(layout, /rs-shell-frame/);
    assert.match(layout, /rs-main-content/);
    assert.match(layout, /rs-mobile-topbar/);
  }
});

test("Admin and Student account headers match the Teacher structure", () => {
  const sources = [
    read("Views/Shared/_AdminLayout.cshtml"),
    read("Views/Shared/_StudentAccountStrip.cshtml")
  ];

  for (const source of sources) {
    assert.match(source, /rs-account-strip/);
    assert.match(source, /rs-account-context/);
    assert.match(source, /rs-account-actions/);
    assert.match(source, /rs-account-avatar/);
  }
});

test("Admin and Student use the shared explicit drawer behavior", () => {
  const script = read("wwwroot/js/role-shell.js");
  const roles = [
    {
      layout: read("Views/Shared/_StudentLayout.cshtml"),
      sidebar: read("Views/Shared/_StudentSidebar.cshtml")
    },
    {
      layout: read("Views/Shared/_AdminLayout.cshtml"),
      sidebar: read("Views/Shared/_AdminLayout.cshtml")
    }
  ];

  assert.match(script, /\[data-rs-sidebar\]/);
  assert.match(script, /dataset\.rsClosedClass/);
  assert.match(script, /event\.key === "Escape"/);

  for (const { layout, sidebar } of roles) {
    assert.match(layout, /data-rs-shell/);
    assert.match(layout, /data-rs-closed-class/);
    assert.match(layout, /data-rs-toggle/);
    assert.match(layout, /data-rs-overlay/);
    assert.match(layout, /js\/role-shell\.js/);
    assert.match(sidebar, /data-rs-sidebar/);
  }
});
