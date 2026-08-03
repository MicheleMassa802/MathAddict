////////////////////////////////////////
// Forward Content => Popup Messaging //
////////////////////////////////////////
console.log("Background service worker loaded");

let lastUrl = null;

chrome.runtime.onMessage.addListener((msg, sender, sendResponse) => {
    console.log("Passing message: ", msg);
    if (msg.action === "contentEvent") {
        chrome.runtime.sendMessage({
            action: "popupEvent",
            event: msg.event,
            data: msg.data
        });

    } else if (msg.action === "getLastUrl") {
        sendResponse({ url: lastUrl });
        return true;

    } else if (msg.action === "setLastUrl") {
        lastUrl = msg.url;
        sendResponse({ status: "ok" });
        return true;
    }
});
