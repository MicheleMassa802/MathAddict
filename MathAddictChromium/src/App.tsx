import './App.css'
import {useState} from "react";

// this code executes inside the context of the popup
function App() {

    const [color, setColor] = useState('white');  // stored in popup code

    const onclick = async () => {
        let [tab] = await chrome.tabs.query({active: true, currentWindow: true});
        chrome.scripting.executeScript<string[], void>({
            target: {tabId: tab.id!},
            args: [color],
            // this function executes inside the context of the active tab
            // the color from the popup gets passed through the executeScript parameters
            func: (color) => {
                document.body.style.backgroundColor = color;
            }
        })
    }
    return (
        <>
            <h1>Math Addict 4 Chromium</h1>
            <div>
                <h3>Select a color!</h3>
                <input type="color" onChange={(e) => setColor(e.currentTarget.value)}/>
            </div>

            <div className="card">
                <button onClick={onclick}>
                    Set Color
                </button>
            </div>
            <p className="read-the-docs">
                Welcome!
            </p>
        </>
    )
}

export default App
