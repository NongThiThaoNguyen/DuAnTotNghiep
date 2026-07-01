-- Update all theoretical lessons
UPDATE original_lessons
SET 
    content_type = 'ARTICLE',
    video_url = NULL,
    content = N'<div class="lesson-intro mb-4">
    <h3 class="text-primary mb-3">Tổng quan & Lý thuyết</h3>
    <p>Chào mừng bạn đến với bài học này. Dưới đây là những nội dung trọng tâm bạn cần nắm vững.</p>
</div>

<div class="alert alert-info shadow-sm mb-4">
    <h5><i class="fa-solid fa-lightbulb text-warning me-2"></i>Điểm cần lưu ý</h5>
    <ul class="mb-0 mt-2">
        <li>Hiểu rõ định nghĩa và cách sử dụng cơ bản.</li>
        <li>Ghi nhớ các từ vựng cốt lõi thường xuất hiện.</li>
        <li>Áp dụng vào giao tiếp thực tế.</li>
    </ul>
</div>

<div class="vocab-section mt-4">
    <h4 class="mb-3 text-primary"><i class="fa-solid fa-layer-group me-2"></i>Từ vựng tham khảo</h4>
    <div class="vocab-list">
        <div class="vocab-card">
            <div class="vocab-header">
                <div class="vocab-word-info">
                    <h5 class="vocab-word text-gradient">Example</h5>
                    <span class="vocab-type">(noun)</span>
                    <span class="vocab-ipa">/ɪɡˈzæm.pəl/</span>
                </div>
                <button type="button" class="btn btn-icon btn-pronounce" data-word="Example" title="Nghe phát âm">
                    <i class="fa-solid fa-volume-high"></i>
                </button>
            </div>
            <div class="vocab-meaning mt-2">
                <p><strong>Nghĩa:</strong> Ví dụ, hình mẫu</p>
            </div>
        </div>
        <div class="vocab-card">
            <div class="vocab-header">
                <div class="vocab-word-info">
                    <h5 class="vocab-word text-gradient">Knowledge</h5>
                    <span class="vocab-type">(noun)</span>
                    <span class="vocab-ipa">/ˈnɒl.ɪdʒ/</span>
                </div>
                <button type="button" class="btn btn-icon btn-pronounce" data-word="Knowledge" title="Nghe phát âm">
                    <i class="fa-solid fa-volume-high"></i>
                </button>
            </div>
            <div class="vocab-meaning mt-2">
                <p><strong>Nghĩa:</strong> Kiến thức, sự hiểu biết</p>
            </div>
        </div>
    </div>
</div>'
WHERE id NOT IN (119, 125, 131) AND title NOT LIKE N'%Thực hành%' AND title NOT LIKE N'%Practice%';

-- Update all practice lessons
UPDATE original_lessons
SET 
    content_type = 'ARTICLE',
    video_url = NULL,
    content = N'<div class="lesson-intro mb-4">
    <h3 class="text-primary mb-3">Bài Tập Thực Hành</h3>
    <p>Hãy áp dụng những kiến thức đã học để hoàn thành bài trắc nghiệm nhanh dưới đây nhé!</p>
</div>

<div class="quiz-container bg-white p-4 rounded shadow-sm border">
    <form id="practiceQuizForm">
        <!-- Question 1 -->
        <div class="mb-4">
            <h5 class="mb-3">1. Đây là câu hỏi ví dụ số 1?</h5>
            <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="q1" id="q1a" value="wrong">
                <label class="form-check-label" for="q1a">Đáp án sai A</label>
            </div>
            <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="q1" id="q1b" value="correct">
                <label class="form-check-label" for="q1b">Đáp án đúng B</label>
            </div>
            <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="q1" id="q1c" value="wrong">
                <label class="form-check-label" for="q1c">Đáp án sai C</label>
            </div>
        </div>

        <!-- Question 2 -->
        <div class="mb-4">
            <h5 class="mb-3">2. Đây là câu hỏi ví dụ số 2?</h5>
            <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="q2" id="q2a" value="correct">
                <label class="form-check-label" for="q2a">Đáp án đúng A</label>
            </div>
            <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="q2" id="q2b" value="wrong">
                <label class="form-check-label" for="q2b">Đáp án sai B</label>
            </div>
            <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="q2" id="q2c" value="wrong">
                <label class="form-check-label" for="q2c">Đáp án sai C</label>
            </div>
        </div>

        <button type="button" class="btn btn-primary px-4 py-2" onclick="checkQuiz()">
            <i class="fa-solid fa-paper-plane me-2"></i>Nộp bài
        </button>
        <div id="quizResult" class="mt-3 fw-bold fs-5" style="display: none;"></div>
    </form>
</div>

<script>
    function checkQuiz() {
        let score = 0;
        const total = 2;
        const form = document.getElementById("practiceQuizForm");
        
        if (form.q1 && form.q1.value === "correct") score++;
        if (form.q2 && form.q2.value === "correct") score++;

        const resultDiv = document.getElementById("quizResult");
        resultDiv.style.display = "block";
        
        if (score === total) {
            resultDiv.className = "mt-3 fw-bold fs-5 text-success";
            resultDiv.innerHTML = "🎉 Tuyệt vời! Bạn đã đúng " + score + "/" + total + " câu.";
        } else {
            resultDiv.className = "mt-3 fw-bold fs-5 text-warning";
            resultDiv.innerHTML = "Bạn trả lời đúng " + score + "/" + total + " câu. Cố gắng lên nhé!";
        }
    }
</script>'
WHERE id NOT IN (119, 125, 131) AND (title LIKE N'%Thực hành%' OR title LIKE N'%Practice%');
