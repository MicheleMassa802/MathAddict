////////////////////////////////////////
// Forward Content => Popup Messaging //
////////////////////////////////////////
console.log("Background service worker loaded");

let lastUrl = null;
let extensionState = {
    tlnsPrefValue: null,
    unityAppended: false,
    hwEnabled: false,
    hwConnectCount: 0,
    div1Active: false,
    div2Active: false,
    sessionBalance: 0
};

chrome.runtime.onMessage.addListener((msg, sender, sendResponse) => {
    console.log("BG.js Passing message: ", msg);

    // URL tracking
    if (msg.action === "getLastUrl") {
        sendResponse({ url: lastUrl });
        return true;
    }
    if (msg.action === "setLastUrl") {
        lastUrl = msg.url;
        sendResponse({ status: "ok" });
        return true;
    }

    // State management
    if (msg.action === "getState") {
        sendResponse(extensionState);
        return true;
    }
    if (msg.action === "setState") {
        Object.assign(extensionState, msg.data);
        sendResponse({ status: "ok" });
        return true;
    }
});
