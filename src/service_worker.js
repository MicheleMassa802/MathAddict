// events that get fired when the extension is clicked, using the open tab
chrome.action.onClicked.addListener(tab => {
    chrome.scripting.executeScript({
        target: {tabId: tab.id},
        func: () => {
            alert('Hello from MathAddict');
        }
    });
});