/* ================================================================
   2026 Courses Page — Filter & Search Logic
   ================================================================ */
document.addEventListener('DOMContentLoaded', function () {

    const tabs       = document.querySelectorAll('.c26-chip.category-tab-ailearn');
    const cards      = document.querySelectorAll('.c26-course-card.course-card-wrapper-item');
    const searchInput = document.getElementById('courseSearchInput');
    const resultsLabel = document.getElementById('c26-results-count');
    const emptyState  = document.getElementById('c26EmptyState');

    /* ── Filter & Search ─────────────────────────────────────── */
    function filterCourses() {
        const query = searchInput ? searchInput.value.toLowerCase().trim() : '';
        const activeTab = document.querySelector('.c26-chip.category-tab-ailearn.active');
        const activeCategory = activeTab ? activeTab.getAttribute('data-category') : 'ALL';

        let visible = 0;
        cards.forEach(card => {
            const title    = (card.getAttribute('data-title') || '').toLowerCase();
            const category = card.getAttribute('data-category');

            const matchesSearch   = !query || title.includes(query);
            const matchesCategory = activeCategory === 'ALL' || category === activeCategory;

            if (matchesSearch && matchesCategory) {
                card.style.display = '';
                visible++;
            } else {
                card.style.display = 'none';
            }
        });

        // Update results counter
        if (resultsLabel) {
            resultsLabel.textContent = `${visible} kết quả`;
        }

        // Show/hide empty state
        if (emptyState) {
            emptyState.style.display = visible === 0 ? 'flex' : 'none';
        }
    }

    /* ── Category Tab Clicks ─────────────────────────────────── */
    tabs.forEach(tab => {
        tab.addEventListener('click', function () {
            tabs.forEach(t => {
                t.classList.remove('active');
                t.setAttribute('aria-pressed', 'false');
            });
            this.classList.add('active');
            this.setAttribute('aria-pressed', 'true');
            filterCourses();
        });
    });

    /* ── Live Search ─────────────────────────────────────────── */
    if (searchInput) {
        searchInput.addEventListener('input', filterCourses);

        // Clear search on Escape
        searchInput.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                this.value = '';
                filterCourses();
                this.blur();
            }
        });
    }

    /* ── Progress Bar Entry Animation ───────────────────────── */
    function animateProgressBars() {
        const bars = document.querySelectorAll('.c26-progress-fill');
        bars.forEach(bar => {
            const target = bar.style.width;
            bar.style.width = '0';
            // Small delay for paint then animate
            requestAnimationFrame(() => {
                requestAnimationFrame(() => {
                    bar.style.width = target;
                });
            });
        });
    }

    animateProgressBars();

    /* ── Initial filter run (for server-set category) ──────── */
    filterCourses();
});
