(function () {
    try {
        const revokeObjectURL = URL.revokeObjectURL;

        URL.revokeObjectURL = function (dataUrl) {
            const currentLogs = Array.from(document.querySelectorAll("div.log"));

            currentLogs.forEach(currentLog => {
                currentLog.parentNode.removeChild(currentLog);
            });

            const log = document.createElement("div");
            log.className = "log"
            log.innerHTML =
                "<div>File exported on " + new Date() + "</div>" +
                '<textarea style="width: 100% !important; height: 150px !important;">' + dataUrl + '</textarea>';
            document.documentElement.appendChild(log);

            return revokeObjectURL(dataUrl);
        };

    } catch (e) {

    }
})();
