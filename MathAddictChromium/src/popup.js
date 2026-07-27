const debugPrefix = "[MathAddict][Popup]";
const targetHost = "mathacademy.com";
const targetPage = "mathacademy.com/tasks/";

let tlnsConnected = false;

const appendElement = "appendDiv";
const removeElement = "removeDiv";
const connectHw = "connectHw";
const disconnectHw = "disconnectHw";
const toggleTLNSAutoSave = "toggleTLNSPref"
const statusElement = "status";
const connectHwBtn = document.getElementById("connectHw");
const status = document.getElementById("status");

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
    if (!tlnsConnected) {
        tlnsConnected = true;
        connectHwBtn.textContent = "TLNS (ON)";
        connectHwBtn.style.borderColor = "#3BDA16";
        status.textContent = "TLNS Connected!";
    } else {
        tlnsConnected = false;
        connectHwBtn.textContent = "TLNS (OFF)";
        connectHwBtn.style.borderColor = "#9FA0FD";
        status.textContent = "TLNS Disconnected.";
    }

    toggleHardware(tlnsConnected);
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
    let tlnsValue;
    loadPlayerTLNSPref((storedPref) => {
        tlnsValue = storedPref;
        tlnsToggle.checked = storedPref;
    });
    console.log("Toggling value prev: " + tlnsValue + " with new: " + newValue);
    savePlayerTLNSPref(newValue);

    let tlnsNewValue;
    loadPlayerTLNSPref((newStoredPref) => {
        tlnsNewValue = newStoredPref;
        tlnsToggle.checked = newStoredPref;
    });
    console.log("Saved new value " + tlnsNewValue);

    // chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
    //     const tabId = tabs[0]?.id;
    //     if (tabId) {
    //         chrome.tabs.sendMessage(tabId, { action: toggleTLNSAutoSave, value: newValue }, (response) => {
    //             // no-op
    //         });
    //     } else {
    //         console.error(debugPrefix, "[HandleToggleTLNSAutoSave] Can't send 'TOGGLE' message. No tab found for Content.js");
    //     }
    // });
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

document.getElementById(appendElement).addEventListener("click", handleAppendDivClick);
document.getElementById(removeElement).addEventListener("click", handleRemoveDivClick);
document.getElementById(connectHw).addEventListener("click", handleConnectHwClick);
const tlnsToggle = document.getElementById('tlnsToggle');
tlnsToggle.addEventListener("change", () => {
    const newPrefValue = tlnsToggle.checked;
    handleToggleTLNSAutoSave(newPrefValue);
});
// appendDiv & removeDiv are the button element id


function savePlayerTLNSPref(autoStart, callback) {
    chrome.storage.sync.set({ [playerAutoStartTLNSKey]: autoStart }, () => {
        if (typeof callback === 'function') {
            callback(autoStart);
        }
    });
}

function loadPlayerTLNSPref(callback) {
    chrome.storage.sync.get(playerAutoStartTLNSKey, (res) => {
        const autoStartTLNS = res[playerAutoStartTLNSKey] ?? false;
        callback(autoStartTLNS);
    });
}

const playerAutoStartTLNSKey = "autoTLNS";
