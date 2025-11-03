const debugPrefix = "[MathAddict][Popup]";
const targetHost = "mathacademy.com";

const appendElement = "appendDiv";
const removeElement = "removeDiv";
const statusElement = "status";

function handleAppendDivClick() {
    console.log(debugPrefix, "[HandleAppendDivClick] Clicked button to append new div!");

    chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
        const tabId = tabs[0]?.id;
        if (tabId) {
            chrome.tabs.sendMessage(tabId, {
                action: appendElement,
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
            chrome.tabs.sendMessage(tabId, { action: removeElement }, (response) => {
                console.log(debugPrefix, "[HandleRemoveDivClick] Remove message sent to content script. Response:", response);
            });
        } else {
            console.error(debugPrefix, "[HandleRemoveDivClick] Can't send 'REMOVE' message. No tab found for Content.js");
        }
    });
}

//////////////////////////
// Control init buttons //
//////////////////////////
chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
    const currUrl = tabs[0]?.url || "";
    const inTargetSite = currUrl.includes(targetHost);

    const appendBtn = document.getElementById(appendElement);
    const removeBtn = document.getElementById(removeElement);
    const status = document.getElementById(statusElement);

    if (!inTargetSite) {
        appendBtn.disabled = true;
        removeBtn.disabled = true;
        status.textContent = "This extension only works on " + targetHost + "!";
    }
});

document.getElementById(appendElement).addEventListener("click", handleAppendDivClick);
document.getElementById(removeElement).addEventListener("click", handleRemoveDivClick);
// appendDiv & removeDiv are the button element id