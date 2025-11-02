let divActive = false;
const extensionDivId = "MADiv";
const debugPrefix = "[MathAddict][Content]";

///////////////////////////////////////////
// Listening for start signal from popup //
///////////////////////////////////////////
chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
    if (request.action === "appendDiv" && !divActive) {
        console.log(debugPrefix, "[HandlePopupResponse] Starting Unity Slots Div");

        const div = document.createElement("div");
        div.id = extensionDivId;
        div.style.position = "fixed";
        div.style.width = "400px";
        div.style.height = "711px";
        div.style.bottom = "10px";
        div.style.right = "10px";
        div.style.backgroundColor = "#A46928";
        div.style.padding = "10px";
        div.style.zIndex = "9999";

        // bring in unity through an iframe to avoid CSP stuff
        const iframe = document.createElement("iframe");
        iframe.src = chrome.runtime.getURL("GameBuild/index.html");
        iframe.style.width = "100%";
        iframe.style.height = "100%";
        iframe.style.border = "none";
        div.appendChild(iframe);
        document.body.appendChild(div);

        divActive = true;
        sendResponse({ status: "success" });

    } else if (request.action === "removeDiv") {
        console.log(debugPrefix, "[HandlePopupResponse] Exiting Unity Slots Div");
        const div = document.getElementById(extensionDivId);
        if (div) {
            div.remove();
            divActive = false;
            sendResponse({ status: "removed" });
        } else {
            sendResponse({ status: "Div to remove not found" });
        }

    } else {
        console.warn("Invalid/Unknown action received:", request.action);
        sendResponse({ status: "invalid/unknown action" });
    }
});


////////////////////////////////////////
// Detecting Question Response Events //
////////////////////////////////////////
const observer = new MutationObserver((mutations) => {

    for (const mutation of mutations) {
        for (const node of mutation.addedNodes) {

            if (!(node instanceof HTMLElement)) {
                continue;
            }

            // Check if the added node itself is the result box
            if (node.classList.contains('questionWidget-result')) {
                handleResultBox(node);
                continue;
            }

            // Or if it contains the result box somewhere inside
            const resultBox = node.querySelector('.questionWidget-result');
            if (resultBox) {
                handleResultBox(resultBox);
                return;  // early return
            }
        }
    }

    console.log(debugPrefix, "[AnalyzeMutations] No question result box found in Mutated Nodes");
});

function handleResultBox(resultBox) {
    // call time
    endQuestionTimer();

    const isCorrect = !!resultBox.querySelector('.questionWidget-correctText');
    const isIncorrect = !!resultBox.querySelector('.questionWidget-incorrectText');

    if (isCorrect) {
        console.log(debugPrefix, '[HandleResultBox] CORRECT answer detected');
        // call Unity to set the wager
        const iframe = document.querySelector('iframe[src*="GameBuild/index.html"]');
        iframe.contentWindow.postMessage({
            type: 'UNITY_COMMAND',
            method: 'SetWager',
            value: currentWager.toString(),
        }, '*');

    } else if (isIncorrect) {
        console.log(debugPrefix, '[HandleResultBox] INCORRECT answer detected');

    } else {
        console.log(debugPrefix, '[HandleResultBox] WTF is that answer being detected bro, be fr...');
    }
}

observer.observe(document.body, {
    childList: true,
    subtree: true,
});

//////////////////////////////
// Debugging Unity Messages //
//////////////////////////////
window.addEventListener("message", (event) => {
    if (event.data?.type === "unityResult") {
        console.log(debugPrefix, "[HandleUnityMessage] Received message from Unity:", event.data.payload);
    }
});


/////////////////////////////
// Timing Logic for Wagers //
/////////////////////////////
const minWager = 1;
const lowWager = 2;
const midWager = 5;
const highWager = 10;
const maxWager = 25;
const allWagers = [maxWager, highWager, midWager, lowWager, minWager];
const wagerTimeSteps = 25;  // every 25 seconds, the wager becomes lower

let startTime;
let currentWager = 0.0;

function startQuestionTimer() {
    startTime = Date.now();
}

function endQuestionTimer() {
    const endTime = Date.now();
    const questionTimeSeconds = (endTime - startTime) / 1000;
    let index = Math.floor(questionTimeSeconds / wagerTimeSteps);
    index = Math.min(index, allWagers.length - 1);
    currentWager = allWagers[index];
}

/////////////////////////
// Player Data Storage //
/////////////////////////
const playerBalanceKey = "playerBalance"
let playerBalance;

function savePlayerData(newBalance) {
    chrome.storage.sync.set({ [playerBalanceKey]: newBalance }, () => {
        console.log(debugPrefix, '[SavePlayerData] Balance Saved: ', newBalance);
    });
}

function loadPlayerData() {
    chrome.storage.sync.get(playerBalanceKey, (res) => {
        if (res.playerBalance !== undefined) {
            playerBalance = res.playerBalance;
            console.log(debugPrefix, '[LoadPlayerData] Balance Loaded: ', playerBalance);
        } else {
            playerBalance = 0.0;
            console.log(debugPrefix, '[LoadPlayerData] Balance Failed to Load!\nDefaulting to $0.0');
        }
    });
}


// inject unity loader instance result into page context
const script = document.createElement('script');
script.src = chrome.runtime.getURL('src/unityRelay.js');
script.onload = () => script.remove();
(document.head || document.documentElement).appendChild(script);
