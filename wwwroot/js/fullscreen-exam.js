/**
 * Fullscreen Exam Engine - Anticheat, Monitoring & Fullscreen Mode
 */
(function (window, document) {
  "use strict";

  const FullscreenExam = {
    config: {
      attemptId: 0,
      attemptType: "PLACEMENT_TEST", // 'PLACEMENT_TEST' | 'QUIZ' | 'LEVEL_UP'
      maxViolations: 3,
      reportUrl: "/Student/PlacementTest/ReportViolation",
      formId: "testForm",
      autoSubmitOnMaxViolations: false,
      terminationUrl: "/Student/PlacementTest/Intro?allowRetake=true",
    },

    state: {
      isExamStarted: false,
      isSubmitting: false,
      fullscreenExitCount: 0,
      tabSwitchCount: 0,
      lastBlurTime: 0,
      violationLogs: [],
    },

    init: function (customConfig) {
      this.config = Object.assign({}, this.config, customConfig);

      // Prevent right-click during exam
      document.addEventListener("contextmenu", function (e) {
        if (
          FullscreenExam.state.isExamStarted &&
          !FullscreenExam.state.isSubmitting
        ) {
          e.preventDefault();
          return false;
        }
      });

      // Prevent common devtools / reload hotkeys
      document.addEventListener("keydown", function (e) {
        if (
          !FullscreenExam.state.isExamStarted ||
          FullscreenExam.state.isSubmitting
        )
          return;

        // F5, Ctrl+R, Ctrl+F5 (Reload)
        if (e.key === "F5" || (e.ctrlKey && (e.key === "r" || e.key === "R"))) {
          e.preventDefault();
          FullscreenExam.showToast(
            "Vui lòng không tải lại trang trong khi làm bài!",
            "warning",
          );
          return false;
        }

        // F12, Ctrl+Shift+I, Ctrl+Shift+J, Ctrl+U (DevTools)
        if (
          e.key === "F12" ||
          (e.ctrlKey &&
            e.shiftKey &&
            (e.key === "I" || e.key === "J" || e.key === "C")) ||
          (e.ctrlKey && (e.key === "u" || e.key === "U"))
        ) {
          e.preventDefault();
          return false;
        }
      });

      // Before unload guard
      window.addEventListener("beforeunload", function (e) {
        if (
          FullscreenExam.state.isExamStarted &&
          !FullscreenExam.state.isSubmitting
        ) {
          const msg =
            "Bài kiểm tra đang diễn ra. Nếu rời khỏi trang, dữ liệu chưa nộp có thể bị mất!";
          e.preventDefault();
          e.returnValue = msg;
          return msg;
        }
      });

      // Attach listeners for Fullscreen and Visibility
      this.bindMonitoringEvents();
    },

    startExam: function () {
      this.state.isExamStarted = true;

      // Request browser fullscreen
      this.requestFullscreen();

      // Hide entrance modal
      const entranceModal = document.getElementById("examEntranceModal");
      if (entranceModal) {
        entranceModal.classList.add("hidden");
      }

      // Trigger custom event so page timers can start if needed
      window.dispatchEvent(
        new CustomEvent("examStarted", { detail: { timestamp: new Date() } }),
      );
    },

    requestFullscreen: function () {
      const elem = document.documentElement;
      if (elem.requestFullscreen) {
        elem.requestFullscreen().catch((err) => {
          console.warn("Fullscreen request failed:", err);
        });
      } else if (elem.webkitRequestFullscreen) {
        /* Safari */
        elem.webkitRequestFullscreen();
      } else if (elem.msRequestFullscreen) {
        /* IE11 */
        elem.msRequestFullscreen();
      }
    },

    bindMonitoringEvents: function () {
      const self = this;

      // 1. Fullscreen change detection
      const handleFullscreenChange = function () {
        if (!self.state.isExamStarted || self.state.isSubmitting) return;

        const isFullscreen = !!(
          document.fullscreenElement ||
          document.webkitFullscreenElement ||
          document.mozFullScreenElement ||
          document.msFullscreenElement
        );

        if (!isFullscreen) {
          self.recordViolation(
            "FULLSCREEN_EXIT",
            "Học sinh đã thoát chế độ toàn màn hình (ESC hoặc F11)",
          );
        }
      };

      document.addEventListener("fullscreenchange", handleFullscreenChange);
      document.addEventListener(
        "webkitfullscreenchange",
        handleFullscreenChange,
      );
      document.addEventListener("mozfullscreenchange", handleFullscreenChange);
      document.addEventListener("MSFullscreenChange", handleFullscreenChange);

      // 2. Tab switch / Page Visibility detection
      document.addEventListener("visibilitychange", function () {
        if (!self.state.isExamStarted || self.state.isSubmitting) return;

        if (document.hidden) {
          self.recordViolation(
            "TAB_SWITCH",
            "Học sinh chuyển sang tab khác hoặc thu nhỏ trình duyệt",
          );
        }
      });

      // 3. Window Blur detection (focus lost to another app)
      window.addEventListener("blur", function () {
        if (!self.state.isExamStarted || self.state.isSubmitting) return;

        const now = Date.now();
        // Debounce with visibilitychange (prevent double counting within 1.5s)
        if (now - self.state.lastBlurTime > 1500 && !document.hidden) {
          self.state.lastBlurTime = now;
          self.recordViolation(
            "WINDOW_BLUR",
            "Học sinh chuyển sang ứng dụng khác (mất focus trình duyệt)",
          );
        }
      });
    },

    recordViolation: function (type, description) {
      const now = new Date();
      if (type === "FULLSCREEN_EXIT") {
        this.state.fullscreenExitCount++;
      } else {
        this.state.tabSwitchCount++;
      }

      const total = this.state.fullscreenExitCount + this.state.tabSwitchCount;

      this.state.violationLogs.push({
        type: type,
        description: description,
        timestamp: now.toISOString(),
        totalViolations: total,
      });

      // Update UI Counters
      this.updateViolationOverlayUI(type, description);

      // Send Realtime AJAX Report
      this.sendViolationReport(type, description);

      // Check max violations
      if (total >= this.config.maxViolations) {
        if (this.config.autoSubmitOnMaxViolations) {
          this.state.isSubmitting = true;
          this.showToast(
            "Bạn đã vượt quá số lần vi phạm cho phép. Bài thi đã bị hủy.",
            "danger",
          );
        }
      }
    },

    updateViolationOverlayUI: function (type, description) {
      const overlay = document.getElementById("examViolationOverlay");
      if (!overlay) return;

      const fsCountElem = document.getElementById("violationFullscreenCount");
      const tabCountElem = document.getElementById("violationTabCount");
      const totalCountElem = document.getElementById("violationTotalCount");
      const messageElem = document.getElementById("violationMessage");

      const total = this.state.fullscreenExitCount + this.state.tabSwitchCount;

      if (fsCountElem) fsCountElem.innerText = this.state.fullscreenExitCount;
      if (tabCountElem) tabCountElem.innerText = this.state.tabSwitchCount;
      if (totalCountElem)
        totalCountElem.innerText = `${total} / ${this.config.maxViolations}`;

      if (messageElem) {
        if (total >= this.config.maxViolations) {
          messageElem.innerHTML = `<span class="text-red-400 font-bold">CẢNH BÁO CAO ĐỘ:</span> Bạn đã vi phạm ${total} lần quy chế thi! Mọi hành vi đã được lưu vào hồ sơ báo cáo của giáo viên.`;
        } else {
          messageElem.innerText =
            description ||
            "Vui lòng quay lại màn hình toàn màn hình để tiếp tục làm bài thi.";
        }
      }

      overlay.classList.remove("hidden");
    },

    resumeExam: function () {
      const overlay = document.getElementById("examViolationOverlay");
      if (overlay) {
        overlay.classList.add("hidden");
      }
      this.requestFullscreen();
    },

    sendViolationReport: function (type, description) {
      if (!this.config.reportUrl || this.config.attemptId <= 0) return;

      const payload = {
        attemptId: this.config.attemptId,
        attemptType: this.config.attemptType,
        violationType: type,
        fullscreenExitCount: this.state.fullscreenExitCount,
        tabSwitchCount: this.state.tabSwitchCount,
        details: description,
        timestamp: new Date().toISOString(),
      };

      return fetch(this.config.reportUrl, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(payload),
      })
        .then((response) => response.json())
        .then((result) => {
          if (result.terminated && this.config.terminationUrl) {
            window.location.assign(this.config.terminationUrl);
          }
        })
        .catch((err) => {
          console.warn("Violation report network error:", err);
        });
    },

    submitExamForm: function () {
      this.state.isSubmitting = true;

      const form = document.getElementById(this.config.formId);
      if (!form) return;

      // Append violation stats to form before submit
      this.ensureHiddenInput(
        form,
        "FullscreenExitCount",
        this.state.fullscreenExitCount,
      );
      this.ensureHiddenInput(form, "TabSwitchCount", this.state.tabSwitchCount);
      this.ensureHiddenInput(
        form,
        "ViolationLog",
        JSON.stringify(this.state.violationLogs),
      );

      form.submit();
    },

    ensureHiddenInput: function (form, name, value) {
      let input = form.querySelector(`input[name="${name}"]`);
      if (!input) {
        input = document.createElement("input");
        input.type = "hidden";
        input.name = name;
        form.appendChild(input);
      }
      input.value = value;
    },

    showToast: function (msg, variant) {
      if (window.showToast) {
        window.showToast(variant || "info", msg);
      } else {
        console.log(`[Exam Alert]: ${msg}`);
      }
    },
  };

  window.FullscreenExam = FullscreenExam;
})(window, document);
