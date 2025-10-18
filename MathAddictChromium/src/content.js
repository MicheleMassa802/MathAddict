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

const observer = new MutationObserver((mutations) => {
    console.log('[Observer] Mutation batch received:', mutations.length);

    for (const mutation of mutations) {
        console.log('[Observer] Mutation type:', mutation.type);

        for (const node of mutation.addedNodes) {
            console.log('[Observer] Node added:', node);

            if (!(node instanceof HTMLElement)) {
                console.log('[Observer] Skipped non-HTMLElement node');
                continue;
            }

            // Check if the added node itself is the result box
            if (node.classList.contains('questionWidget-result')) {
                console.log('[Observer] Found result box directly:', node);
                handleResultBox(node);
                continue;
            }

            // Or if it contains the result box somewhere inside
            const resultBox = node.querySelector('.questionWidget-result');
            if (resultBox) {
                console.log('[Observer] Found result box inside added node:', resultBox);
                handleResultBox(resultBox);
            } else {
                console.log('[Observer] No result box found in this node');
            }
        }
    }
});

function handleResultBox(resultBox) {
    const isCorrect = !!resultBox.querySelector('.questionWidget-correctText');
    const isIncorrect = !!resultBox.querySelector('.questionWidget-incorrectText');

    if (isCorrect) {
        console.log('[Extension] ✅ Correct answer detected');
        // call Unity to set the wager
        const iframe = document.querySelector('iframe[src*="GameBuild/index.html"]');
        iframe.contentWindow.postMessage({
            type: 'UNITY_COMMAND',
            method: 'SetWager',
            value: '100.5'
        }, '*');

    } else if (isIncorrect) {
        console.log('[Extension] ❌ Incorrect answer detected');
    } else {
        console.log('[Extension] 🤔 Answer result detected, but unclear');
    }
}

observer.observe(document.body, {
    childList: true,
    subtree: true,
});

// inject unity loader instance result into page context
const script = document.createElement('script');
script.src = chrome.runtime.getURL('src/unityRelay.js');
script.onload = () => script.remove();
(document.head || document.documentElement).appendChild(script);
