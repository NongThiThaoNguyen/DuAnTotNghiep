(function (root, factory) {
  const api = factory();

  if (typeof module === "object" && module.exports) {
    module.exports = api;
  }

  if (root) {
    root.AdminSidebarState = api;
  }
})(typeof window !== "undefined" ? window : globalThis, function () {
  "use strict";

  const storageKey = "admin-sidebar-scroll-top";

  function read(storage, key) {
    try {
      const value = Number.parseInt(storage?.getItem(key), 10);
      return Number.isFinite(value) && value > 0 ? value : 0;
    } catch {
      return 0;
    }
  }

  function restore(sidebar, storage, key = storageKey) {
    if (!sidebar) {
      return 0;
    }

    const maxScroll = Math.max(0, sidebar.scrollHeight - sidebar.clientHeight);
    const position = Math.min(read(storage, key), maxScroll);
    sidebar.scrollTop = position;
    if (sidebar.dataset) {
      sidebar.dataset.scrollState = "restored";
    }
    return position;
  }

  function save(sidebar, storage, key = storageKey) {
    if (!sidebar) {
      return;
    }

    try {
      storage?.setItem(key, String(Math.max(0, Math.round(sidebar.scrollTop))));
    } catch {
      // Storage can be unavailable in privacy-restricted browser sessions.
    }
  }

  function revealActive(sidebar, activeItem, margin = 24) {
    if (!sidebar || !activeItem) {
      return sidebar?.scrollTop ?? 0;
    }

    const current = sidebar.scrollTop;
    const itemTop = activeItem.offsetTop;
    const itemBottom = itemTop + activeItem.offsetHeight;
    const visibleTop = current + margin;
    const visibleBottom = current + sidebar.clientHeight - margin;
    let position = current;

    if (itemTop < visibleTop) {
      position = itemTop - margin;
    } else if (itemBottom > visibleBottom) {
      position = itemBottom - sidebar.clientHeight + margin;
    }

    const maxScroll = Math.max(0, sidebar.scrollHeight - sidebar.clientHeight);
    position = Math.min(Math.max(0, position), maxScroll);
    sidebar.scrollTop = position;
    return position;
  }

  function bind(sidebar, storage, key = storageKey) {
    if (!sidebar || sidebar.dataset.scrollPersistence === "bound") {
      return;
    }

    const persist = () => save(sidebar, storage, key);
    sidebar.addEventListener("scroll", persist, { passive: true });
    sidebar.addEventListener("pointerdown", persist, true);
    sidebar.addEventListener("click", persist, true);
    window.addEventListener("pagehide", persist);
    sidebar.dataset.scrollPersistence = "bound";
  }

  return { bind, restore, revealActive, save, storageKey };
});
