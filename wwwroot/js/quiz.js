/* Quiz Interactive and Timer Logic */
document.addEventListener("DOMContentLoaded", function () {
    const options = document.querySelectorAll(".quiz-option-item");
    const explanationPanel = document.getElementById("quizExplanationPanel");
    const explanationText = document.getElementById("quizExplanationText");
    const submitBtn = document.getElementById("submitQuizBtn");
    const nextBtn = document.getElementById("nextQuestionBtn");
    const timerText = document.getElementById("timerValue");

    let timerMinutes = 15;
    let timerSeconds = 0;
    let timerInterval = null;

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
                    alert("Hết thời gian làm bài!");
                    document.getElementById("quizForm")?.submit();
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

    options.forEach(option => {
        option.addEventListener("click", function () {
            // Only allow selection before submission
            const container = this.closest(".question-container");
            if (container.classList.contains("submitted")) return;

            const groupOptions = container.querySelectorAll(".quiz-option-item");
            groupOptions.forEach(opt => opt.classList.remove("selected"));
            this.classList.add("selected");

            const input = container.querySelector(".selected-option-input");
            if (input) {
                input.value = this.getAttribute("data-letter");
            }
        });
    });

    if (submitBtn) {
        submitBtn.addEventListener("click", function () {
            const containers = document.querySelectorAll(".question-container");
            let answered = true;

            containers.forEach(container => {
                const selected = container.querySelector(".quiz-option-item.selected");
                if (!selected) {
                    answered = false;
                }
            });

            if (!answered) {
                alert("Vui lòng trả lời đầy đủ các câu hỏi trước khi nộp bài!");
                return;
            }

            clearInterval(timerInterval);
            document.getElementById("quizForm")?.submit();
        });
    }
});
