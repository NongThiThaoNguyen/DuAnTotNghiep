/* Quiz Interactive and Timer Logic */
document.addEventListener("DOMContentLoaded", function () {
  const options = document.querySelectorAll(
    ".quiz-option-item, .exam-option-label",
  );
  const explanationPanel = document.getElementById("quizExplanationPanel");
  const explanationText = document.getElementById("quizExplanationText");
  const submitBtn = document.getElementById("submitQuizBtn");
  const nextBtn = document.getElementById("nextQuestionBtn");
  const timerText = document.getElementById("timerValue");
  const examShell = document.querySelector(".quiz-taking-shell");

  let timerMinutes = 15;
  let timerSeconds = 0;
  let timerInterval = null;
  let currentQuestionIndex = 1;

  // Start timer
  if (timerText) {
    timerMinutes = parseInt(timerText.getAttribute("data-limit") || "15", 10);
    startTimer();
  }

  function startTimer() {
    timerInterval = setInterval(() => {
      if (timerSeconds === 0) {
        if (timerMinutes === 0) {
          clearInterval(timerInterval);
          if (window.adminAlert) {
            window
              .adminAlert({
                title: "Hết giờ",
                message: "Hết thời gian làm bài!",
                variant: "warning",
                okText: "Nộp bài",
              })
              .then(() => {
                document.getElementById("quizForm")?.submit();
              });
          } else {
            document.getElementById("quizForm")?.submit();
          }
          return;
        }
        timerMinutes--;
        timerSeconds = 59;
      } else {
        timerSeconds--;
      }

      const minStr = timerMinutes < 10 ? "0" + timerMinutes : timerMinutes;
      const secStr = timerSeconds < 10 ? "0" + timerSeconds : timerSeconds;
      timerText.textContent = `${minStr}:${secStr}`;
    }, 1000);
  }

  options.forEach((option) => {
    option.addEventListener("click", function () {
      // Only allow selection before submission
      const container = this.closest("[data-question-id]");
      if (container.classList.contains("submitted")) return;

      const groupOptions = container.querySelectorAll(
        ".quiz-option-item, .exam-option-label",
      );
      groupOptions.forEach((opt) => opt.classList.remove("selected"));
      this.classList.add("selected");

      const input = container.querySelector(".selected-option-input");
      if (input) {
        input.value = this.getAttribute("data-letter");
      }

      updateProgress();
    });
  });

  function showQuestion(index) {
    if (!examShell) return;
    const questions = examShell.querySelectorAll(".quiz-current-question");
    if (!questions.length) return;

    currentQuestionIndex = Math.max(1, Math.min(index, questions.length));
    questions.forEach((question) => {
      question.classList.toggle(
        "is-current",
        Number(question.dataset.questionIndex) === currentQuestionIndex,
      );
      question.classList.toggle(
        "is-hidden",
        Number(question.dataset.questionIndex) !== currentQuestionIndex,
      );
    });

    examShell.querySelectorAll("[data-quiz-index]").forEach((button) => {
      button.classList.toggle(
        "is-current",
        Number(button.dataset.quizIndex) === currentQuestionIndex,
      );
    });
  }

  function updateProgress() {
    if (!examShell) return;
    const questions = examShell.querySelectorAll(".quiz-current-question");
    const answered = Array.from(questions).filter((question) => {
      const input = question.querySelector(".selected-option-input");
      return input && input.value;
    }).length;
    const percent = questions.length
      ? Math.round((answered / questions.length) * 100)
      : 0;
    const countText = document.getElementById("answeredCountText");
    const percentText = document.getElementById("answeredPercentText");
    const progressBar = document.getElementById("progressBar");
    if (countText) countText.textContent = answered;
    if (percentText) percentText.textContent = `${percent}%`;
    if (progressBar) progressBar.style.width = `${percent}%`;

    questions.forEach((question) => {
      const answeredQuestion = question.querySelector(
        ".selected-option-input",
      )?.value;
      const button = examShell.querySelector(
        `[data-quiz-index="${question.dataset.questionIndex}"]`,
      );
      if (button)
        button.classList.toggle("is-answered", Boolean(answeredQuestion));
    });
  }

  if (examShell) {
    examShell.querySelectorAll("[data-quiz-index]").forEach((button) => {
      button.addEventListener("click", () =>
        showQuestion(Number(button.dataset.quizIndex)),
      );
    });
    examShell.querySelectorAll("[data-quiz-next]").forEach((button) => {
      button.addEventListener("click", () =>
        showQuestion(currentQuestionIndex + 1),
      );
    });
    examShell.querySelectorAll("[data-quiz-prev]").forEach((button) => {
      button.addEventListener("click", () =>
        showQuestion(currentQuestionIndex - 1),
      );
    });
    const sideSubmit = document.getElementById("submitQuizBtnSide");
    if (sideSubmit)
      sideSubmit.addEventListener("click", () => submitBtn?.click());
    updateProgress();
    showQuestion(1);
  }

  if (submitBtn) {
    submitBtn.addEventListener("click", function () {
      const containers = document.querySelectorAll("[data-question-id]");
      let answered = true;

      containers.forEach((container) => {
        const selected = container.querySelector(
          ".quiz-option-item.selected, .exam-option-label.selected",
        );
        if (!selected) {
          answered = false;
        }
      });

      if (!answered) {
        if (window.adminAlert) {
          window.adminAlert({
            title: "Chưa hoàn thành",
            message: "Vui lòng trả lời đầy đủ các câu hỏi trước khi nộp bài!",
            variant: "warning",
          });
        } else if (window.showToast) {
          window.showToast(
            "warning",
            "Vui lòng trả lời đầy đủ các câu hỏi trước khi nộp bài!",
          );
        }
        return;
      }

      clearInterval(timerInterval);
      if (window.FullscreenExam) {
        window.FullscreenExam.submitExamForm();
      } else {
        document.getElementById("quizForm")?.submit();
      }
    });
  }
});
