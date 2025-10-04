let divActive = false;
const extensionDivId = "MADiv";

chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
    // create div when receiving the signal from popup.js
    console.log("Content script received message:", request);

    if (request.action === "appendDiv" && !divActive) {
        const div = document.createElement("div");
        div.id = extensionDivId;
        div.style.position = "fixed";
        div.style.width = "400px";
        div.style.height = "711px";
        div.style.bottom = "10px";
        div.style.right = "10px";
        div.style.backgroundColor = "#A46928";
        div.style.padding = "10px";
        div.style.zIndex = "9999";

        // bring in unity through an iframe to avoid CSP stuff
        const iframe = document.createElement("iframe");
        iframe.src = chrome.runtime.getURL("GameBuild/index.html");
        iframe.style.width = "100%";
        iframe.style.height = "100%";
        iframe.style.border = "none";
        div.appendChild(iframe);
        document.body.appendChild(div);

        divActive = true;
        console.log("Div appended to page");
        sendResponse({ status: "success" });

    } else if (request.action === "removeDiv") {
        const div = document.getElementById(extensionDivId);
        if (div) {
            div.remove();
            divActive = false;
            console.log("Div removed from page");
            sendResponse({ status: "removed" });
        } else {
            console.log("No div found to remove");
            sendResponse({ status: "Div to remove not found" });
        }

    } else {
        console.warn("Invalid/Unknown action received:", request.action);
        sendResponse({ status: "invalid/unknown action" });
    }
});

window.addEventListener("message", (event) => {
    if (event.data?.type === "unityResult") {
        console.log("Received from Unity:", event.data.payload);
    }
});
