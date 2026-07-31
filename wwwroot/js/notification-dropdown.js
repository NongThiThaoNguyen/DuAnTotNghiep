(function () {
  "use strict";

  document.addEventListener("DOMContentLoaded", () => {
    const bellBtn = document.getElementById("notificationBellBtn");
    const dropdownMenu = document.getElementById("notificationDropdownMenu");
    const dropdownBody = document.getElementById("notificationDropdownBody");
    const badge = document.getElementById("notificationBadge");
    const markAllBtn = document.getElementById("markAllNotificationsReadBtn");

    if (!bellBtn || !dropdownMenu) return;

    // Toggle dropdown
    bellBtn.addEventListener("click", (e) => {
      e.stopPropagation();
      const isHidden = dropdownMenu.classList.contains("d-none");
      if (isHidden) {
        openDropdown();
      } else {
        closeDropdown();
      }
    });

    // Close when clicking outside
    document.addEventListener("click", (e) => {
      if (!dropdownMenu.contains(e.target) && !bellBtn.contains(e.target)) {
        closeDropdown();
      }
    });

    // Close on Escape key
    document.addEventListener("keydown", (e) => {
      if (e.key === "Escape") {
        closeDropdown();
      }
    });

    function openDropdown() {
      dropdownMenu.classList.remove("d-none");
      dropdownMenu.setAttribute("aria-hidden", "false");
      bellBtn.setAttribute("aria-expanded", "true");

      fetchNotifications();
    }

    function closeDropdown() {
      dropdownMenu.classList.add("d-none");
      dropdownMenu.setAttribute("aria-hidden", "true");
      bellBtn.setAttribute("aria-expanded", "false");
    }

    async function fetchNotifications() {
      if (!dropdownBody) return;
      dropdownBody.innerHTML = '<div class="rs-notification-loading"><i class="fa-solid fa-spinner fa-spin me-1"></i> Đang tải thông báo...</div>';

      try {
        const response = await fetch("/api/notifications");
        if (!response.ok) {
          throw new Error("Không thể tải thông báo");
        }
        const data = await response.json();
        renderNotifications(data);
        updateBadge(data.filter(n => !n.isRead).length);
      } catch (error) {
        console.error("Error fetching notifications:", error);
        dropdownBody.innerHTML = '<div class="rs-notification-empty">Không thể tải danh sách thông báo.</div>';
      }
    }

    function renderNotifications(items) {
      if (!dropdownBody) return;

      if (!items || items.length === 0) {
        dropdownBody.innerHTML = '<div class="rs-notification-empty"><i class="fa-regular fa-bell-slash d-block mb-2" style="font-size:1.5rem; opacity:0.6;"></i>Không có thông báo nào.</div>';
        return;
      }

      let html = "";
      items.forEach(item => {
        const unreadClass = item.isRead ? "" : "unread";
        const iconInfo = getIconForType(item.notificationType);
        
        html += `
          <div class="rs-notification-item ${unreadClass}" data-id="${item.id}">
            <div class="rs-notification-icon ${iconInfo.bgClass}">
              <i class="${iconInfo.icon}"></i>
            </div>
            <div class="rs-notification-content-wrap">
              <div class="rs-notification-item-title">${escapeHtml(item.title)}</div>
              <div class="rs-notification-item-text">${escapeHtml(item.content)}</div>
              <div class="rs-notification-item-time">${escapeHtml(item.timeAgo)}</div>
            </div>
          </div>
        `;
      });

      dropdownBody.innerHTML = html;

      // Attach click events to mark item as read
      const itemElements = dropdownBody.querySelectorAll(".rs-notification-item");
      itemElements.forEach(el => {
        el.addEventListener("click", async () => {
          const id = el.getAttribute("data-id");
          if (el.classList.contains("unread") && id) {
            await markAsRead(id, el);
          }
        });
      });
    }

    function getIconForType(type) {
      switch ((type || "").toUpperCase()) {
        case "SUCCESS":
        case "ACHIEVEMENT":
          return { icon: "fa-solid fa-trophy", bgClass: "type-SUCCESS" };
        case "WARNING":
          return { icon: "fa-solid fa-triangle-exclamation", bgClass: "type-WARNING" };
        case "SYSTEM":
          return { icon: "fa-solid fa-gear", bgClass: "type-SYSTEM" };
        case "COURSE":
          return { icon: "fa-solid fa-book-open", bgClass: "" };
        default:
          return { icon: "fa-solid fa-bell", bgClass: "" };
      }
    }

    async function markAsRead(id, el) {
      try {
        const response = await fetch(`/api/notifications/mark-read/${id}`, {
          method: "POST",
          headers: { "Content-Type": "application/json" }
        });
        if (response.ok) {
          const resData = await response.json();
          el.classList.remove("unread");
          updateBadge(resData.unreadCount);
        }
      } catch (err) {
        console.error("Failed to mark as read", err);
      }
    }

    if (markAllBtn) {
      markAllBtn.addEventListener("click", async (e) => {
        e.stopPropagation();
        try {
          const response = await fetch("/api/notifications/mark-all-read", {
            method: "POST",
            headers: { "Content-Type": "application/json" }
          });
          if (response.ok) {
            const unreadItems = dropdownBody.querySelectorAll(".rs-notification-item.unread");
            unreadItems.forEach(el => el.classList.remove("unread"));
            updateBadge(0);
          }
        } catch (err) {
          console.error("Failed to mark all as read", err);
        }
      });
    }

    function updateBadge(unreadCount) {
      if (!badge) return;
      if (unreadCount > 0) {
        badge.textContent = unreadCount > 99 ? "99+" : unreadCount;
        badge.classList.remove("d-none");
      } else {
        badge.classList.add("d-none");
      }
    }

    function escapeHtml(text) {
      if (!text) return "";
      return text
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
    }
  });
})();
