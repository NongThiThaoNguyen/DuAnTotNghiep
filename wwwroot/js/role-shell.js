(function () {
  "use strict";

  const root = document.querySelector("[data-rs-shell]");
  const sidebar = root?.querySelector("[data-rs-sidebar]");
  const toggle = root?.querySelector("[data-rs-toggle]");
  const overlay = root?.querySelector("[data-rs-overlay]");

  if (!root || !sidebar || !toggle || !overlay) return;

  const legacyClosedClass = sidebar.dataset.rsClosedClass || root.dataset.rsClosedClass;

  function setOpen(open) {
    sidebar.classList.toggle("is-closed", !open);
    if (legacyClosedClass) {
      sidebar.classList.toggle(legacyClosedClass, !open);
    }
    overlay.hidden = !open;
    toggle.setAttribute("aria-expanded", String(open));
    document.body.classList.toggle("rs-drawer-open", open);
  }

  toggle.addEventListener("click", () => {
    setOpen(toggle.getAttribute("aria-expanded") !== "true");
  });

  overlay.addEventListener("click", () => setOpen(false));

  sidebar.addEventListener("click", event => {
    const target = event.target;
    if (target instanceof Element && target.closest("a") && window.innerWidth < 1024) {
      setOpen(false);
    }
  });

  document.addEventListener("keydown", event => {
    if (event.key === "Escape") setOpen(false);
  });

  window.addEventListener("resize", () => {
    if (window.innerWidth >= 1024) setOpen(false);
  });
})();
