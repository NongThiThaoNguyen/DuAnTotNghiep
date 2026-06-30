/* Lesson Interactive Navigation Logic */
document.addEventListener("DOMContentLoaded", function () {
    const tabs = document.querySelectorAll(".lesson-details-tab");
    const contents = document.querySelectorAll(".tab-content-item");

    tabs.forEach(tab => {
        tab.addEventListener("click", function () {
            const target = this.getAttribute("data-tab");
            
            tabs.forEach(t => t.classList.remove("active"));
            this.classList.add("active");

            contents.forEach(content => {
                if (content.id === target + "Content") {
                    content.classList.add("active");
                } else {
                    content.classList.remove("active");
                }
            });
        });
    });
});
