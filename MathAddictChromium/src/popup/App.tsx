export default function App() {
    const launchUnity = () => {
        // launch a new tab with the target link with the extension ready to load
        chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
            const tab = tabs[0];
            console.log("Creating tab!");
            if (tab.id) {
                console.log("Calling to inject unity!");
                chrome.scripting.executeScript({
                    target: { tabId: tab.id },
                    files: ["injectUnity.js"],  // this is the built file name chrome expects rather than the actual file at src/
                });
            }
        });
    };

    return (
        <div>
            <h1>Unity Launcher</h1>
            <button onClick={launchUnity}>Launch Game</button>
        </div>
    );
}
