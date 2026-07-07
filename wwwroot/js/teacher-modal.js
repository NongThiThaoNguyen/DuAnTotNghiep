(function () {
  "use strict";

  const modalElement = document.getElementById("teacherModal");
  const modalTitle = document.getElementById("teacherModalTitle");
  const modalBody = document.getElementById("teacherModalBody");
  const dialog = modalElement ? modalElement.querySelector(".modal-dialog") : null;

  if (!modalElement || !modalTitle || !modalBody || !dialog || !window.bootstrap) {
    return;
  }

  const modal = window.bootstrap.Modal.getOrCreateInstance(modalElement);
  const dialogSizes = ["modal-sm", "modal-md", "modal-lg", "modal-xl"];

  function setDialogSize(size) {
    dialog.classList.remove(...dialogSizes);
    const normalized = String(size || "xl").toLowerCase();
    dialog.classList.add(dialogSizes.includes("modal-" + normalized) ? "modal-" + normalized : "modal-xl");
  }

  function setLoading() {
    modalBody.replaceChildren();
    const loading = document.createElement("div");
    const spinner = document.createElement("span");
    const text = document.createElement("span");

    loading.className = "teacher-modal-loading";
    loading.setAttribute("role", "status");
    spinner.className = "spinner-border spinner-border-sm";
    spinner.setAttribute("aria-hidden", "true");
    text.textContent = "Đang tải nội dung";
    loading.append(spinner, text);
    modalBody.appendChild(loading);
  }

  function showError(message) {
    modalBody.replaceChildren();
    const error = document.createElement("div");
    error.className = "teacher-modal-error";
    error.setAttribute("role", "alert");
    error.textContent = message;
    modalBody.appendChild(error);
  }

  function extractContent(html) {
    const parsed = new DOMParser().parseFromString(html, "text/html");
    const page = parsed.querySelector(".teacher-page");
    const source = page || parsed.querySelector("[data-teacher-modal-content]") || parsed.body;
    const wrapper = document.createElement("div");

    wrapper.className = "teacher-modal-content";
    wrapper.innerHTML = source.innerHTML;
    wrapper.querySelectorAll("script, .teacher-page-header, .courses-header, .dashboard-header-block").forEach(function (element) {
      element.remove();
    });
    return wrapper;
  }

  function parseValidation() {
    if (window.jQuery && window.jQuery.validator && window.jQuery.validator.unobtrusive) {
      window.jQuery.validator.unobtrusive.parse(modalBody);
    }
  }

  async function loadContent(url) {
    setLoading();
    try {
      const response = await fetch(url, {
        headers: { "X-Requested-With": "XMLHttpRequest" },
        credentials: "same-origin"
      });
      if (!response.ok) {
        throw new Error("Không thể tải nội dung.");
      }

      const html = await response.text();
      modalBody.replaceChildren(extractContent(html));
      parseValidation();
    } catch (error) {
      showError(error.message || "Không thể tải nội dung. Vui lòng thử lại.");
    }
  }

  document.addEventListener("click", function (event) {
    const closeTrigger = event.target.closest("[data-teacher-modal-close]");
    if (closeTrigger) {
      event.preventDefault();
      modal.hide();
      return;
    }

    const trigger = event.target.closest("[data-teacher-modal]");
    if (!trigger || !trigger.href) {
      return;
    }

    event.preventDefault();
    modalTitle.textContent = trigger.dataset.modalTitle || trigger.title || "Thao tác";
    setDialogSize(trigger.dataset.modalSize);
    modal.show();
    loadContent(trigger.href);
  });

  modalBody.addEventListener("submit", async function (event) {
    const form = event.target.closest("form");
    if (!form) {
      return;
    }

    if (window.jQuery && window.jQuery(form).data("validator") && !window.jQuery(form).valid()) {
      return;
    }

    event.preventDefault();
    const submitButton = form.querySelector('[type="submit"]');
    if (submitButton) {
      submitButton.disabled = true;
    }

    try {
      const response = await fetch(form.action || window.location.href, {
        method: (form.method || "post").toUpperCase(),
        body: new FormData(form),
        headers: { "X-Requested-With": "XMLHttpRequest" },
        credentials: "same-origin"
      });

      if (!response.ok) {
        throw new Error("Không thể lưu thay đổi.");
      }

      if (response.redirected) {
        window.location.assign(response.url);
        return;
      }

      const html = await response.text();
      const content = extractContent(html);
      const replacementForm = content.querySelector("form");
      if (!replacementForm) {
        modal.hide();
        window.location.reload();
        return;
      }

      modalBody.replaceChildren(content);
      parseValidation();
    } catch (error) {
      const existingError = modalBody.querySelector(".teacher-modal-error");
      if (existingError) {
        existingError.textContent = error.message;
      } else {
        const errorRegion = document.createElement("div");
        errorRegion.className = "teacher-modal-error";
        errorRegion.setAttribute("role", "alert");
        errorRegion.textContent = error.message || "Không thể lưu thay đổi. Vui lòng thử lại.";
        modalBody.prepend(errorRegion);
      }
      if (submitButton) {
        submitButton.disabled = false;
      }
    }
  });
})();
