(function () {
  "use strict";

  const desktopBreakpoint = 1024;
  const sidebar = document.getElementById("teacherSidebar");
  const toggle = document.getElementById("teacherSidebarToggle");
  const overlay = document.getElementById("teacherSidebarOverlay");

  function setDrawerOpen(open) {
    if (!sidebar || !toggle || !overlay) {
      return;
    }

    const isDesktop = window.innerWidth >= desktopBreakpoint;
    const shouldOpen = isDesktop || open;
    sidebar.classList.toggle("teacher-sidebar-closed", !shouldOpen);
    overlay.hidden = isDesktop || !open;
    toggle.setAttribute("aria-expanded", String(!isDesktop && open));
    document.body.classList.toggle("teacher-drawer-open", !isDesktop && open);
  }

  function bindDrawer() {
    if (!sidebar || !toggle || !overlay) {
      return;
    }

    toggle.addEventListener("click", function () {
      setDrawerOpen(toggle.getAttribute("aria-expanded") !== "true");
    });

    overlay.addEventListener("click", function () {
      setDrawerOpen(false);
    });

    sidebar.addEventListener("click", function (event) {
      if (window.innerWidth < desktopBreakpoint && event.target.closest("a")) {
        setDrawerOpen(false);
      }
    });

    document.addEventListener("keydown", function (event) {
      if (event.key === "Escape") {
        setDrawerOpen(false);
        toggle.focus();
      }
    });

    window.addEventListener("resize", function () {
      setDrawerOpen(false);
    });

    setDrawerOpen(false);
  }

  function normalizeToastArguments(first, second, third) {
    const knownTypes = ["success", "error", "danger", "warning", "info"];
    if (knownTypes.includes(first) && typeof second === "string") {
      return {
        message: second,
        type: first === "error" ? "danger" : first,
        title: third || ""
      };
    }

    return {
      message: first || "",
      type: second === "error" ? "danger" : second || "success",
      title: third || ""
    };
  }

  function showTeacherToast(first, second, third) {
    const options = normalizeToastArguments(first, second, third);
    const container = document.getElementById("teacherToastContainer");
    if (!container || !options.message) {
      return;
    }

    const labels = {
      success: "Thành công",
      danger: "Lỗi",
      warning: "Cảnh báo",
      info: "Thông báo"
    };
    const icons = {
      success: "fa-circle-check",
      danger: "fa-circle-exclamation",
      warning: "fa-triangle-exclamation",
      info: "fa-circle-info"
    };
    const type = labels[options.type] ? options.type : "info";
    const toast = document.createElement("section");
    const icon = document.createElement("i");
    const content = document.createElement("div");
    const title = document.createElement("strong");
    const message = document.createElement("p");
    const close = document.createElement("button");

    toast.className = "teacher-toast is-" + type;
    toast.setAttribute("role", type === "danger" || type === "warning" ? "alert" : "status");

    icon.className = "fa-solid " + icons[type];
    icon.setAttribute("aria-hidden", "true");

    title.textContent = options.title || labels[type];
    message.textContent = options.message;
    content.append(title, message);

    close.type = "button";
    close.className = "teacher-toast-close";
    close.setAttribute("aria-label", "Đóng thông báo");
    close.textContent = "×";
    close.addEventListener("click", function () {
      toast.remove();
    });

    toast.append(icon, content, close);
    container.appendChild(toast);
    window.setTimeout(function () {
      toast.remove();
    }, 4500);
  }

  function showTempDataToasts() {
    const container = document.getElementById("teacherToastContainer");
    if (!container) {
      return;
    }

    const messages = [
      [container.dataset.success, "success"],
      [container.dataset.error, "danger"],
      [container.dataset.warning, "warning"],
      [container.dataset.info, "info"]
    ];

    messages.forEach(function (entry) {
      if (entry[0]) {
        showTeacherToast(entry[0], entry[1]);
      }
    });
  }

  window.showTeacherToast = showTeacherToast;
  window.showToast = showTeacherToast;

  bindDrawer();
  showTempDataToasts();
})();
