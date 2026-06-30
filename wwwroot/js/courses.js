/* Courses Category Tabs & Filter Logic */
document.addEventListener("DOMContentLoaded", function () {
    const tabs = document.querySelectorAll(".category-tab-ailearn");
    const cards = document.querySelectorAll(".course-card-wrapper-item");
    const searchInput = document.getElementById("courseSearchInput");

    function filterCourses() {
        const query = searchInput ? searchInput.value.toLowerCase().trim() : "";
        const activeTab = document.querySelector(".category-tab-ailearn.active");
        const activeCategory = activeTab ? activeTab.getAttribute("data-category") : "ALL";

        cards.forEach(card => {
            const title = card.getAttribute("data-title").toLowerCase();
            const category = card.getAttribute("data-category");
            
            const matchesSearch = title.includes(query);
            const matchesCategory = (activeCategory === "ALL" || category === activeCategory);

            if (matchesSearch && matchesCategory) {
                card.style.display = "block";
            } else {
                card.style.display = "none";
            }
        });
    }

    tabs.forEach(tab => {
        tab.addEventListener("click", function () {
            tabs.forEach(t => t.classList.remove("active"));
            this.classList.add("active");
            filterCourses();
        });
    });

    if (searchInput) {
        searchInput.addEventListener("input", filterCourses);
    }
});
