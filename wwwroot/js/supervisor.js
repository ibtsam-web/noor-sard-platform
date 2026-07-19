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

    const step = 0.5;

    function normalizeParts(value) {
        return Math.round(value * 2) / 2;
    }

    function formatParts(value) {
        return Number.isInteger(value)
            ? value.toString()
            : value.toFixed(1);
    }

    function getTargetParts() {
        const value = Number(targetPartsInput?.value);

        return normalizeParts(
            Math.min(
                30,
                Math.max(0.5, value || 0.5)
            )
        );
    }

    function getCompletedParts() {
        const value = Number(completedPartsInput?.value);

        return normalizeParts(
            Math.max(0, value || 0)
        );
    }

    function updateProgressDisplay() {
        const targetParts = getTargetParts();
        let completedParts = getCompletedParts();

        if (completedParts > targetParts) {
            completedParts = targetParts;
        }

        if (targetPartsInput) {
            targetPartsInput.value = formatParts(targetParts);
        }

        if (completedPartsInput) {
            completedPartsInput.value = formatParts(completedParts);
        }

        if (completedPartsValue) {
            completedPartsValue.textContent =
                formatParts(completedParts);
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
            completedPartsInput.value = normalizeParts(
                Math.min(
                    completedParts + step,
                    targetParts
                )
            );

            updateProgressDisplay();
        }
    });

    decreaseButton?.addEventListener("click", function () {
        const completedParts = getCompletedParts();

        if (completedParts > 0 && completedPartsInput) {
            completedPartsInput.value = normalizeParts(
                Math.max(
                    completedParts - step,
                    0
                )
            );

            updateProgressDisplay();
        }
    });

    targetPartsInput?.addEventListener("input", function () {
        updateProgressDisplay();
    });

    targetPartsInput?.addEventListener("change", function () {
        updateProgressDisplay();
    });

    updateProgressDisplay();
});