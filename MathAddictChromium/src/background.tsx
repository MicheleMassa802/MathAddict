chrome.runtime.onInstalled.addListener(() => {
    chrome.tabs.create({ url: "about:blank" }, (tab) => {
        if (tab.id) {
            chrome.scripting.executeScript({
                target: { tabId: tab.id },
                files: ["src/inject-unity.tsx"],
            });
        }
    });
});