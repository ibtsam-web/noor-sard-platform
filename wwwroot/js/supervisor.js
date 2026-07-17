document.addEventListener("DOMContentLoaded", function () {
    const targetPartsInput = document.getElementById("targetParts");
    const completedPartsInput = document.getElementById("completedParts");
    const completedPartsValue = document.getElementById(
        "completedPartsValue"
    );

    const increaseButton = document.getElementById("increaseCompleted");
    const decreaseButton = document.getElementById("decreaseCompleted");

    const percentageValue = document.getElementById(
        "participantPercentageValue"
    );

    const percentageTrack = document.querySelector(
        ".participant-progress-track"
    );

    const percentageFill = document.getElementById(
        "participantProgressFill"
    );

    function getTargetParts() {
        return Math.min(
            30,
            Math.max(1, Number(targetPartsInput?.value) || 1)
        );
    }

    function getCompletedParts() {
        return Math.max(
            0,
            Number(completedPartsInput?.value) || 0
        );
    }

    function updateProgressDisplay() {
        const targetParts = getTargetParts();
        let completedParts = getCompletedParts();

        if (completedParts > targetParts) {
            completedParts = targetParts;
        }

        if (targetPartsInput) {
            targetPartsInput.value = targetParts;
        }

        if (completedPartsInput) {
            completedPartsInput.value = completedParts;
        }

        if (completedPartsValue) {
            completedPartsValue.textContent = completedParts;
        }

        const percentage = Math.round(
            (completedParts / targetParts) * 100
        );

        if (percentageValue) {
            percentageValue.textContent = `${percentage}%`;
        }

        if (percentageFill) {
            percentageFill.style.width = `${percentage}%`;
        }

        if (percentageTrack) {
            percentageTrack.setAttribute(
                "aria-valuenow",
                percentage
            );
        }
    }

    increaseButton?.addEventListener("click", function () {
        const targetParts = getTargetParts();
        const completedParts = getCompletedParts();

        if (completedParts < targetParts && completedPartsInput) {
            completedPartsInput.value = completedParts + 1;
            updateProgressDisplay();
        }
    });

    decreaseButton?.addEventListener("click", function () {
        const completedParts = getCompletedParts();

        if (completedParts > 0 && completedPartsInput) {
            completedPartsInput.value = completedParts - 1;
            updateProgressDisplay();
        }
    });

    targetPartsInput?.addEventListener("input", function () {
        updateProgressDisplay();
    });

    updateProgressDisplay();
});