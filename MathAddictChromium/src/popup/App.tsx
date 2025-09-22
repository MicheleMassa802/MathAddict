export default function App() {
    const launchUnity = () => {
        chrome.tabs.create({ url: "about:blank" }, (tab) => {
            if (tab.id) {
                chrome.scripting.executeScript({
                    target: { tabId: tab.id },
                    files: ["src/inject-unity.js"],
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
