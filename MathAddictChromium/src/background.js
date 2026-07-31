////////////////////////////////////////
// Forward Content => Popup Messaging //
////////////////////////////////////////
chrome.runtime.onMessage.addListener((msg, sender, sendResponse) => {
    if (msg.action === "contentEvent") {
        chrome.runtime.sendMessage({
            action: "popupEvent",
            event: msg.event,
            data: msg.data
        });
    }
});
