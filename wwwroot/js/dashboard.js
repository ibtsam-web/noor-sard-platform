let partsChart = null;
document.addEventListener("DOMContentLoaded", function () {
    const chartCanvas = document.getElementById("partsChart");

    if (!chartCanvas) {
        return;
    }

    const chartContext = chartCanvas.getContext("2d");

    partsChart = new Chart(chartContext, {
        type: "bar",

        data: {
            labels: window.noorDashboardData?.chartLabels || [],

            datasets: [
                {
                    label: "عدد الساردات",
                    data: window.noorDashboardData?.chartValues || [],
                    backgroundColor: "rgba(0, 91, 112, 0.82)",
                    borderColor: "#c49a50",
                    borderWidth: 2,
                    borderRadius: 9,
                    borderSkipped: false,
                    maxBarThickness: 65
                }
            ]
        },

        options: {
            responsive: true,
            maintainAspectRatio: false,

            animation: {
                duration: 900
            },

            plugins: {
                legend: {
                    display: false
                },

                tooltip: {
                    rtl: true,
                    titleFont: {
                        family: "Cairo",
                        size: 14
                    },
                    bodyFont: {
                        family: "Cairo",
                        size: 14
                    },
                    callbacks: {
                        label: function (context) {
                            return `عدد الساردات: ${context.raw}`;
                        }
                    }
                }
            },

            scales: {
                x: {
                    grid: {
                        display: false
                    },
                    ticks: {
                        color: "#17343c",
                        font: {
                            family: "Cairo",
                            size: 13,
                            weight: "600"
                        }
                    }
                },

                y: {
                    beginAtZero: true,

                    title: {
                        display: true,
                        text: "عدد الساردات",
                        color: "#005b70",
                        font: {
                            family: "Cairo",
                            size: 15,
                            weight: "700"
                        }
                    },

                    ticks: {
                        stepSize: 5,
                        precision: 0,
                        color: "#6d8389",
                        font: {
                            family: "Cairo",
                            size: 13
                        }
                    },

                    grid: {
                        color: "rgba(0, 91, 112, 0.08)"
                    }
                }
            }
        }
    });
    const participantSearch = document.getElementById("participantSearch");
    const clearSearchButton = document.getElementById("clearParticipantSearch");
    const participantItems = Array.from(
        document.querySelectorAll(".participant-item")
    );
    const participantsEmpty = document.getElementById("participantsEmpty");
    const visibleParticipantsCount = document.getElementById(
        "visibleParticipantsCount"
    );

    function normalizeArabicText(text) {
        return text
            .trim()
            .toLowerCase()
            .replace(/[أإآ]/g, "ا")
            .replace(/ى/g, "ي")
            .replace(/ة/g, "ه")
            .replace(/[\u064B-\u065F\u0670]/g, "");
    }

    function updateParticipantsList() {
        const searchValue = normalizeArabicText(
            participantSearch?.value || ""
        );

        let visibleCount = 0;

        participantItems.forEach(function (item) {
            const participantName = normalizeArabicText(
                item.dataset.name || ""
            );

            const isVisible = participantName.includes(searchValue);

            item.hidden = !isVisible;

            if (isVisible) {
                visibleCount++;
            }
        });

        if (participantsEmpty) {
            participantsEmpty.hidden = visibleCount !== 0;
        }

        if (visibleParticipantsCount) {
            visibleParticipantsCount.textContent =
                `${visibleCount} اسمًا`;
        }
    }

    participantSearch?.addEventListener("input", updateParticipantsList);

    clearSearchButton?.addEventListener("click", function () {
        if (!participantSearch) {
            return;
        }

        participantSearch.value = "";
        participantSearch.focus();
        updateParticipantsList();
    });

    updateParticipantsList();

        if (typeof signalR !== "undefined") {
        const dashboardConnection =
            new signalR.HubConnectionBuilder()
                .withUrl("/dashboardHub")
                .withAutomaticReconnect()
                .build();

        dashboardConnection.on(
            "DashboardUpdated",
            async function () {
                await refreshDashboard();
            }
        );

        async function startDashboardConnection() {
            try {
                await dashboardConnection.start();

                console.log(
                    "تم الاتصال بالتحديث المباشر."
                );
            } catch (error) {
                console.error(
                    "تعذر الاتصال بالتحديث المباشر:",
                    error
                );

                window.setTimeout(
                    startDashboardConnection,
                    5000
                );
            }
        }

        dashboardConnection.onreconnected(
            async function () {
                await refreshDashboard();
            }
        );

        startDashboardConnection();
    }
});
function updateOverallProgress(completedParts, targetParts) {
    const safeCompleted = Math.max(0, Number(completedParts) || 0);
    const safeTarget = Math.max(0, Number(targetParts) || 0);

    const percentage = safeTarget > 0
        ? Math.min(100, (safeCompleted / safeTarget) * 100)
        : 0;

    const roundedPercentage = Math.round(percentage);

    const progressFill = document.getElementById("overallProgressFill");
    const progressValue = document.getElementById("overallProgressValue");
    const progressTrack = document.querySelector(".noor-progress-track");
    const completedText = document.getElementById("completedPartsText");
    const targetText = document.getElementById("targetPartsText");

    if (progressFill) {
        progressFill.style.width = `${roundedPercentage}%`;
    }

    if (progressValue) {
        progressValue.textContent = `${roundedPercentage}%`;
    }

    if (progressTrack) {
        progressTrack.setAttribute("aria-valuenow", roundedPercentage);
    }

    if (completedText) {
        completedText.textContent = `${safeCompleted} جزءًا`;
    }

    if (targetText) {
        targetText.textContent = `${safeTarget} جزء`;
    }
}async function refreshDashboard() {
    try {
        const response = await fetch(
            "/Home/DashboardData",
            {
                method: "GET",
                headers: {
                    "Accept": "application/json"
                },
                cache: "no-store"
            }
        );

        if (!response.ok) {
            throw new Error("تعذر جلب الإحصائيات.");
        }

        const data = await response.json();

        setElementText(
            "participantsCountValue",
            data.participantsCount
        );

        setElementText(
            "totalCompletedPartsValue",
            data.totalCompletedParts
        );

        setElementText(
            "totalTargetPartsValue",
            data.totalTargetParts
        );

        setElementText(
            "overallPercentageStatValue",
            `${data.overallPercentage}%`
        );

        setElementText(
            "yearCompletedCountValue",
            data.yearMemorizationCompletedCount
        );

        setElementText(
            "bronzeMedalCount",
            data.bronzeMedalCount
        );

        setElementText(
            "silverMedalCount",
            data.silverMedalCount
        );

        setElementText(
            "goldMedalCount",
            data.goldMedalCount
        );

        updateOverallProgress(
            data.totalCompletedParts,
            data.totalTargetParts
        );

        if (partsChart) {
            partsChart.data.labels = data.chartLabels;
            partsChart.data.datasets[0].data =
                data.chartValues;

            partsChart.update();
        }
    } catch (error) {
        console.error(
            "خطأ أثناء تحديث شاشة العرض:",
            error
        );
    }
}

function setElementText(elementId, value) {
    const element = document.getElementById(elementId);

    if (element) {
        element.textContent = value;
    }
}