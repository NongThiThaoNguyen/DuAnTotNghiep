UPDATE original_lessons
SET 
    content_type = 'ARTICLE',
    video_url = NULL,
    content = N'<div class="lesson-intro mb-4">
    <h3 class="text-primary mb-3">Tổng quan về Các Thì trong Tiếng Anh (Tenses)</h3>
    <p>Các "Thì" (Tenses) là một trong những phần ngữ pháp quan trọng nhất trong tiếng Anh. Chúng giúp chúng ta diễn tả <strong>thời điểm</strong> xảy ra của một hành động hay sự việc (Quá khứ, Hiện tại, hoặc Tương lai).</p>
</div>

<div class="alert alert-info shadow-sm mb-4">
    <h5><i class="fa-solid fa-lightbulb text-warning me-2"></i>3 Mốc Thời Gian Chính</h5>
    <ul class="mb-0 mt-2">
        <li><strong>Past (Quá khứ):</strong> Những việc đã xảy ra và kết thúc.</li>
        <li><strong>Present (Hiện tại):</strong> Những việc đang xảy ra, thói quen, chân lý.</li>
        <li><strong>Future (Tương lai):</strong> Những việc chưa xảy ra, dự định, tiên đoán.</li>
    </ul>
</div>

<div class="vocab-section mt-4">
    <h4 class="mb-3 text-primary"><i class="fa-solid fa-layer-group me-2"></i>4 Thể của mỗi Thì</h4>
    <p>Mỗi mốc thời gian lại được chia làm 4 thể (Aspects), tạo thành 12 thì cơ bản:</p>
    <div class="row g-3">
        <div class="col-md-6">
            <div class="card h-100 border-0 shadow-sm">
                <div class="card-body">
                    <h5 class="card-title text-primary">1. Simple (Đơn)</h5>
                    <p class="card-text text-muted">Diễn tả hành động chung chung, thói quen hoặc sự thật hiển nhiên.</p>
                </div>
            </div>
        </div>
        <div class="col-md-6">
            <div class="card h-100 border-0 shadow-sm">
                <div class="card-body">
                    <h5 class="card-title text-success">2. Continuous (Tiếp diễn)</h5>
                    <p class="card-text text-muted">Diễn tả hành động đang xảy ra tại một thời điểm xác định.</p>
                </div>
            </div>
        </div>
        <div class="col-md-6">
            <div class="card h-100 border-0 shadow-sm">
                <div class="card-body">
                    <h5 class="card-title text-warning">3. Perfect (Hoàn thành)</h5>
                    <p class="card-text text-muted">Diễn tả hành động đã hoàn thành trước một thời điểm hoặc một hành động khác.</p>
                </div>
            </div>
        </div>
        <div class="col-md-6">
            <div class="card h-100 border-0 shadow-sm">
                <div class="card-body">
                    <h5 class="card-title text-danger">4. Perfect Continuous</h5>
                    <p class="card-text text-muted">Nhấn mạnh quá trình kéo dài của một hành động cho đến một thời điểm khác.</p>
                </div>
            </div>
        </div>
    </div>
</div>'
WHERE id = 119;


UPDATE original_lessons
SET 
    content_type = 'ARTICLE',
    video_url = NULL,
    content = N'<div class="lesson-intro mb-4">
    <h3 class="text-primary mb-3">Bài Tập Thực Hành Tenses</h3>
    <p>Hãy áp dụng những kiến thức đã học ở 2 bài trước để hoàn thành bài trắc nghiệm nhanh dưới đây nhé!</p>
</div>

<div class="quiz-container bg-white p-4 rounded shadow-sm border">
    <form id="practiceQuizForm">
        <!-- Question 1 -->
        <div class="mb-4">
            <h5 class="mb-3">1. She _____ to school every day.</h5>
            <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="q1" id="q1a" value="wrong">
                <label class="form-check-label" for="q1a">is going</label>
            </div>
            <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="q1" id="q1b" value="correct">
                <label class="form-check-label" for="q1b">goes</label>
            </div>
            <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="q1" id="q1c" value="wrong">
                <label class="form-check-label" for="q1c">went</label>
            </div>
        </div>

        <!-- Question 2 -->
        <div class="mb-4">
            <h5 class="mb-3">2. They _____ football at 5 PM yesterday.</h5>
            <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="q2" id="q2a" value="correct">
                <label class="form-check-label" for="q2a">were playing</label>
            </div>
            <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="q2" id="q2b" value="wrong">
                <label class="form-check-label" for="q2b">played</label>
            </div>
            <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="q2" id="q2c" value="wrong">
                <label class="form-check-label" for="q2c">are playing</label>
            </div>
        </div>

        <!-- Question 3 -->
        <div class="mb-4">
            <h5 class="mb-3">3. By the time I arrived, he _____ the work.</h5>
            <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="q3" id="q3a" value="wrong">
                <label class="form-check-label" for="q3a">finishes</label>
            </div>
            <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="q3" id="q3b" value="wrong">
                <label class="form-check-label" for="q3b">has finished</label>
            </div>
            <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="q3" id="q3c" value="correct">
                <label class="form-check-label" for="q3c">had finished</label>
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
        const total = 3;
        const form = document.getElementById("practiceQuizForm");
        
        if (form.q1.value === "correct") score++;
        if (form.q2.value === "correct") score++;
        if (form.q3.value === "correct") score++;

        const resultDiv = document.getElementById("quizResult");
        resultDiv.style.display = "block";
        
        if (score === total) {
            resultDiv.className = "mt-3 fw-bold fs-5 text-success";
            resultDiv.innerHTML = "🎉 Tuyệt vời! Bạn đã đúng " + score + "/" + total + " câu.";
        } else {
            resultDiv.className = "mt-3 fw-bold fs-5 text-warning";
            resultDiv.innerHTML = "Bạn trả lời đúng " + score + "/" + total + " câu. Hãy xem lại bài trước nhé!";
        }
    }
</script>'
WHERE id = 131;
