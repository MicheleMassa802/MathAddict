// Query for current tab to send out an alert from it!
async function sayHello() {
    let [tab] = await chrome.tabs.query({active: true, currentWindow: true});
    chrome.scripting.executeScript({
        target: {tabId: tab.id},
        func: () => {
            // inside here we have access to the DOM for the webpage itself,
            // not just for the extension popup!
            alert('Hello from MathAddict');
        }
    })
}

// Tie the button element from the main page to the corresponding start function
document.getElementById("startAppBtn").addEventListener("click", sayHello);