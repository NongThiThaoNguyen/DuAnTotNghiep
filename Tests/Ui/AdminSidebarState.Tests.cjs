const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const test = require("node:test");

const modulePath = path.resolve(
  __dirname,
  "../../wwwroot/js/admin-sidebar-state.js"
);

test("restores the saved sidebar position synchronously", () => {
  assert.ok(
    fs.existsSync(modulePath),
    "admin-sidebar-state.js must provide the scroll restoration behavior"
  );

  delete require.cache[modulePath];
  const { restore } = require(modulePath);
  const sidebar = { scrollTop: 0, scrollHeight: 1600, clientHeight: 700 };
  const storage = {
    getItem(key) {
      return key === "admin-sidebar-scroll-top" ? "865" : null;
    }
  };

  const restored = restore(sidebar, storage);

  assert.equal(restored, 865);
  assert.equal(sidebar.scrollTop, 865);
});

test("clamps stale positions to the current sidebar height", () => {
  assert.ok(fs.existsSync(modulePath));

  delete require.cache[modulePath];
  const { restore } = require(modulePath);
  const sidebar = { scrollTop: 0, scrollHeight: 1200, clientHeight: 700 };
  const storage = { getItem: () => "9999" };

  const restored = restore(sidebar, storage);

  assert.equal(restored, 500);
  assert.equal(sidebar.scrollTop, 500);
});

test("reveals an active item that starts below the visible sidebar", () => {
  delete require.cache[modulePath];
  const sidebarState = require(modulePath);
  assert.equal(typeof sidebarState.revealActive, "function");

  const sidebar = {
    scrollTop: 0,
    scrollHeight: 1600,
    clientHeight: 700
  };
  const activeItem = { offsetTop: 1000, offsetHeight: 40 };

  const position = sidebarState.revealActive(sidebar, activeItem);

  assert.equal(position, 364);
  assert.equal(sidebar.scrollTop, 364);
});

test("keeps the sidebar steady when the active item is already visible", () => {
  delete require.cache[modulePath];
  const sidebarState = require(modulePath);
  assert.equal(typeof sidebarState.revealActive, "function");

  const sidebar = {
    scrollTop: 865,
    scrollHeight: 1600,
    clientHeight: 700
  };
  const activeItem = { offsetTop: 950, offsetHeight: 40 };

  const position = sidebarState.revealActive(sidebar, activeItem);

  assert.equal(position, 865);
  assert.equal(sidebar.scrollTop, 865);
});
