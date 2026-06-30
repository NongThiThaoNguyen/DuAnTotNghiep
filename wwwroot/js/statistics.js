/* Statistics chart logic with Chart.js */
document.addEventListener("DOMContentLoaded", function () {
    const weeklyCtx = document.getElementById("weeklyStudyChart");
    const skillsCtx = document.getElementById("skillsChart");

    if (weeklyCtx && typeof Chart !== "undefined") {
        const labels = JSON.parse(weeklyCtx.getAttribute("data-labels") || "[]");
        const minutes = JSON.parse(weeklyCtx.getAttribute("data-minutes") || "[]");

        new Chart(weeklyCtx, {
            type: "bar",
            data: {
                labels: labels,
                datasets: [{
                    label: "Thời gian học (phút)",
                    data: minutes,
                    backgroundColor: "#6C63FF",
                    borderRadius: 6,
                    borderWidth: 0
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: "#E9ECF5"
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        }
                    }
                }
            }
        });
    }

    if (skillsCtx && typeof Chart !== "undefined") {
        const labels = JSON.parse(skillsCtx.getAttribute("data-labels") || "[]");
        const scores = JSON.parse(skillsCtx.getAttribute("data-scores") || "[]");

        new Chart(skillsCtx, {
            type: "doughnut",
            data: {
                labels: labels,
                datasets: [{
                    data: scores,
                    backgroundColor: [
                        "#6C63FF",
                        "#4ADE80",
                        "#60A5FA",
                        "#FB923C",
                        "#F472B6"
                    ],
                    borderWidth: 2,
                    borderColor: "#FFFFFF"
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: "bottom",
                        labels: {
                            boxWidth: 12,
                            padding: 15,
                            font: {
                                family: "Poppins",
                                size: 11
                            }
                        }
                    }
                },
                cutout: "70%"
            }
        });
    }
});
