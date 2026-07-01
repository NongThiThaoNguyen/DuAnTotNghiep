UPDATE original_lessons
SET 
    content_type = 'VIDEO_LINK',
    video_url = 'https://www.youtube.com/watch?v=84GcbEEuw5c',
    content = N'<div class="lesson-intro">
    <p>Chào mừng bạn đến với bài học <strong>Nâng cao về các Thì trong tiếng Anh (Tenses)</strong>. Trong bài học này, chúng ta sẽ xem video hướng dẫn chi tiết và học các từ vựng chỉ thời gian cốt lõi nhất!</p>
</div>
<div class="vocab-section mt-4">
    <h4 class="mb-3 text-primary"><i class="fa-solid fa-book-open me-2"></i>Từ vựng trọng tâm</h4>
    <div class="vocab-list">
        <div class="vocab-card">
            <div class="vocab-header">
                <div class="vocab-word-info">
                    <h5 class="vocab-word text-gradient">Continuous</h5>
                    <span class="vocab-type">(adj)</span>
                    <span class="vocab-ipa">/kənˈtɪn.ju.əs/</span>
                </div>
                <button type="button" class="btn btn-icon btn-pronounce" data-word="Continuous" title="Nghe phát âm">
                    <i class="fa-solid fa-volume-high"></i>
                </button>
            </div>
            <div class="vocab-meaning mt-2">
                <p><strong>Nghĩa:</strong> Liên tục, tiếp diễn</p>
            </div>
        </div>

        <div class="vocab-card">
            <div class="vocab-header">
                <div class="vocab-word-info">
                    <h5 class="vocab-word text-gradient">Perfect</h5>
                    <span class="vocab-type">(adj)</span>
                    <span class="vocab-ipa">/ˈpɜː.fekt/</span>
                </div>
                <button type="button" class="btn btn-icon btn-pronounce" data-word="Perfect" title="Nghe phát âm">
                    <i class="fa-solid fa-volume-high"></i>
                </button>
            </div>
            <div class="vocab-meaning mt-2">
                <p><strong>Nghĩa:</strong> Hoàn thành, hoàn hảo</p>
            </div>
        </div>

        <div class="vocab-card">
            <div class="vocab-header">
                <div class="vocab-word-info">
                    <h5 class="vocab-word text-gradient">Simultaneously</h5>
                    <span class="vocab-type">(adv)</span>
                    <span class="vocab-ipa">/ˌsɪm.əlˈteɪ.ni.əs.li/</span>
                </div>
                <button type="button" class="btn btn-icon btn-pronounce" data-word="Simultaneously" title="Nghe phát âm">
                    <i class="fa-solid fa-volume-high"></i>
                </button>
            </div>
            <div class="vocab-meaning mt-2">
                <p><strong>Nghĩa:</strong> Đồng thời, cùng lúc</p>
            </div>
        </div>
    </div>
</div>'
WHERE id = 125;
