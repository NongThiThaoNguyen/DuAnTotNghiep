(function () {
  function setSubmitBusy(form) {
    const submit = form.querySelector("[data-auth-submit]");
    if (!submit) return;

    submit.disabled = true;
    submit.setAttribute("aria-busy", "true");
    submit.dataset.originalText = submit.textContent.trim();
    submit.textContent = "Đang xử lý";
  }

  document.querySelectorAll("[data-auth-password-toggle]").forEach((button) => {
    const key = button.getAttribute("data-auth-password-toggle");
    const input = document.querySelector(`[data-auth-password-input="${key}"]`);

    if (!input) return;

    button.addEventListener("click", () => {
      const type = input.getAttribute("type");
      const nextType = type === "password" ? "text" : "password";
      const isVisible = nextType === "text";

      input.setAttribute("type", nextType);
      button.setAttribute("aria-pressed", String(isVisible));
      button.setAttribute("aria-label", isVisible ? "Ẩn mật khẩu" : "Hiện mật khẩu");
    });
  });

  document.querySelectorAll("[data-auth-form]").forEach((form) => {
    form.addEventListener("submit", () => {
      if (form.checkValidity()) {
        setSubmitBusy(form);
      }
    });
  });
})();
