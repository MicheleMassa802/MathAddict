const debugPrefix = "[MathAddict][Popup]";
const targetHost = "mathacademy.com";
const targetPage = "mathacademy.com/tasks/";

const appendElement = "appendDiv";
const removeElement = "removeDiv";
const connectHw = "connectHw";
const disconnectHw = "disconnectHw";
const toggleTLNSAutoSave = "toggleTLNSPref";
const statusElement = "status";
const connectHwBtn = document.getElementById("connectHw");
const status = document.getElementById("status");

let localExtensionState = {};

//////////////////////////////////////////
// Direct communication with Content.js //
//////////////////////////////////////////
function handleAppendDivClick() {

    chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
        const tabId = tabs[0]?.id;
        if (tabId) {
            chrome.tabs.sendMessage(tabId, {
                action: appendElement,
                text: "Hello from the extension!",
            }, (response) => {
                // no-op
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
                // no-op
            });
        } else {
            console.error(debugPrefix, "[HandleRemoveDivClick] Can't send 'REMOVE' message. No tab found for Content.js");
        }
    });
}

function handleConnectHwClick() {
    if (!localExtensionState.hwEnabled) {
        localExtensionState.hwEnabled = true;
        connectHwBtn.textContent = "TLNS (ON)";
        connectHwBtn.style.borderColor = "#3BDA16";
        status.textContent = "TLNS Connected!";
    } else {
        localExtensionState.hwEnabled = false;
        connectHwBtn.textContent = "TLNS (OFF)";
        connectHwBtn.style.borderColor = "#9FA0FD";
        status.textContent = "TLNS Disconnected.";
    }

    toggleHardware(localExtensionState.hwEnabled);
    // localExtensionState.hwEnabled doesn't update the global state as content.js takes
    // care of that
}

function toggleHardware(connect) {
    chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
        const tabId = tabs[0]?.id;
        if (tabId) {
            chrome.tabs.sendMessage(tabId, { action: connect ? connectHw : disconnectHw }, (response) => {
                // no-op
            });
        } else {
            console.error(debugPrefix, "[HandleConnectHwClick] Can't send '", connect ? "Connect" : "Disconnect" ,"' message. No tab found for Content.js");
        }
    });
}

function handleToggleTLNSAutoSave(newValue) {
    chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
        const tabId = tabs[0]?.id;
        if (tabId) {
            chrome.tabs.sendMessage(tabId, { action: toggleTLNSAutoSave, value: newValue }, (response) => {
                // no-op
            });
        } else {
            console.error(debugPrefix, "[HandleToggleTLNSAutoSave] Can't send 'TOGGLE' message. No tab found for Content.js");
        }
    });
}


//////////////////////////
// Control init buttons //
//////////////////////////
chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
    const currUrl = tabs[0]?.url || "";
    const inTargetSite = currUrl.includes(targetHost);
    const inTargetPage = currUrl.includes(targetPage);

    const appendBtn = document.getElementById(appendElement);
    const removeBtn = document.getElementById(removeElement);
    const status = document.getElementById(statusElement);

    if (!inTargetSite) {
        appendBtn.disabled = true;
        removeBtn.disabled = true;
        status.textContent = "This extension only works on " + targetHost + "!";
    } else if (!inTargetPage) {
        appendBtn.disabled = true;
        removeBtn.disabled = true;
        status.textContent = "This extension only works on task pages!";
    }
});

const tlnsToggle = document.getElementById('tlnsToggle');
document.getElementById(appendElement).addEventListener("click", handleAppendDivClick);
document.getElementById(removeElement).addEventListener("click", handleRemoveDivClick);
document.getElementById(connectHw).addEventListener("click", handleConnectHwClick);
tlnsToggle.addEventListener("change", () => {
    const newPrefValue = tlnsToggle.checked;
    handleToggleTLNSAutoSave(newPrefValue);
});


////////////////////////////////////////////
// STATE HANDLING (through background.js) //
////////////////////////////////////////////
function getExtensionState(callback) {
    chrome.runtime.sendMessage({ action: "getState" }, (state) => {
        callback(state);
    });
}


/////////////
// Startup //
/////////////
getExtensionState((state) => {
    // note we can only do this due to the fact background.js always runs, content.js runs before
    // popup.js (as it's injected into the page upon load / reload), and popup.js only runs whenever
    // the user opens the popup itself. So background.js has default values => content.js sets them
    // up => popup.js fetches the updated values from background.js
    localExtensionState = state;
    tlnsToggle.checked = state.tlnsPrefValue;
});


