const debugPrefix = "[MathAddict][Popup]";

function handleAppendDivClick() {
    console.log(debugPrefix, "[HandleAppendDivClick] Clicked button to append new div!");

    chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
        const tabId = tabs[0]?.id;
        if (tabId) {
            chrome.tabs.sendMessage(tabId, {
                action: "appendDiv",
                text: "Hello from the extension!",
            }, (response) => {
                console.log(debugPrefix, "[HandleAppendDivClick] Append message sent to content script. Response:", response);
            });
        } else {
            console.error(debugPrefix, "[HandleAppendDivClick] Can't send 'APPEND' message. No tab found for Content.js");
        }
    });
}

function handleRemoveDivClick() {
    chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
        const tabId = tabs[0]?.id;
        if (tabId) {
            chrome.tabs.sendMessage(tabId, { action: "removeDiv" }, (response) => {
                console.log(debugPrefix, "[HandleRemoveDivClick] Remove message sent to content script. Response:", response);
            });
        } else {
            console.error(debugPrefix, "[HandleRemoveDivClick] Can't send 'REMOVE' message. No tab found for Content.js");
        }
    });
}

document.getElementById("appendDiv").addEventListener("click", handleAppendDivClick);
document.getElementById("removeDiv").addEventListener("click", handleRemoveDivClick);
// appendDiv & removeDiv are the button element id