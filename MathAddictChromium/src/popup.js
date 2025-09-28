function handleAppendDivClick() {
    console.log("Clicked button to append new div!");

    chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
        const tabId = tabs[0]?.id;
        if (tabId) {
            chrome.tabs.sendMessage(tabId, {
                action: "appendDiv",
                text: "Hello from the extension!",
            }, (response) => {
                console.log("Message sent to content script. Response:", response);
            });
        } else {
            console.error("No active tab found");
        }
    });
}

function handleRemoveDivClick() {
    chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
        const tabId = tabs[0]?.id;
        if (tabId) {
            chrome.tabs.sendMessage(tabId, { action: "removeDiv" }, (response) => {
                console.log("Remove message sent. Response:", response);
            });
        } else {
            console.error("No active tab found");
        }
    });
}

document.getElementById("appendDiv").addEventListener("click", handleAppendDivClick);
document.getElementById("removeDiv").addEventListener("click", handleRemoveDivClick);
// appendDiv & removeDiv are the button element id