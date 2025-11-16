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
            sendResponse({status: "removed"});
        } else {
            sendResponse({status: "Div to remove not found"});
        }

    } else {
        console.warn("Invalid/Unknown action received:", request.action);
        sendResponse({ status: "invalid/unknown action" });
    }
});


////////////////////////////////////////
// Detecting Question Response Events //
////////////////////////////////////////
const possibleWagers = [0, 0, 0, 0, 0, 0, 1, 2, 2, 5, 5, 10];

const observer = new MutationObserver((mutations) => {

    for (const mutation of mutations) {
        // handle node additions -- answering questions and getting a correct/incorrect
        for (const node of mutation.addedNodes) {

            if (!(node instanceof HTMLElement)) {
                continue;
            }

            // Check if the added node itself is the result box
            if (node.classList.contains('questionWidget-result')) {
                handleResultBox(node);
                return;
            }

            // OR if it contains the result box somewhere inside
            const resultBox = node.querySelector('.questionWidget-result');
            if (resultBox) {
                handleResultBox(resultBox);
                return;  // early return
            }
        }

        // OR handle "disappearing" 'continue' buttons => reset start timer
        if (mutation.type === 'attributes' && (mutation.attributeName === 'style' || mutation.attributeName === 'class')) {
            const target = mutation.target;
            if (target instanceof HTMLElement && target.classList.contains('continueButton')) {
                const displayStyle = window.getComputedStyle(target).display;
                if (displayStyle === 'none') {
                    // just switched from 'block' -> 'none' implies 'continue' btn dismissed
                    handleContinueButtonDismissed();
                }
            }
        }
    }

    console.log(debugPrefix, "[AnalyzeMutations] No question result box or continue style change found in Mutated Nodes");
});

function handleResultBox(resultBox) {
    // call time to compute wager to send
    // const currentWager = endQuestionTimerAndFetchWager();
    const timeDelta = endQuestionTimerAndFetchTimeDelta();
    const currentWager = possibleWagers[Math.floor(Math.random() * possibleWagers.length)];
    const isCorrect = !!resultBox.querySelector('.questionWidget-correctText');
    const isIncorrect = !!resultBox.querySelector('.questionWidget-incorrectText');

    if (isCorrect) {
        console.log(debugPrefix, '[HandleResultBox] CORRECT answer detected');
        // send "<wager>:<timeDelta>" string!
        sendMessageToUnity("SetWager", `${currentWager.toString()}:${timeDelta}`);

    } else if (isIncorrect) {
        console.log(debugPrefix, '[HandleResultBox] INCORRECT answer detected');

    } else {
        console.log(debugPrefix, '[HandleResultBox] WTF is that answer being detected bro, be fr...');
    }
}

function handleContinueButtonDismissed() {
    // continue clicked and dismissed => start of lesson / start of new question
    console.log(debugPrefix, '[HandleContinueButtonDismissed] CONTINUE button clicked!');
    startQuestionTimer();
}

observer.observe(document.body, {
    childList: true,
    subtree: true,
    attributes: true,
    attributeFilter: ['style', 'class'],
});


//////////////////////////////
// Debugging Unity Messages //
//////////////////////////////
window.addEventListener("message", (event) => {
    if (event.data?.type === "unityResult") {
        console.log(debugPrefix, "[HandleUnityMessage] Received message from Unity:", event.data.payload);
        // keep balance up to date!
        const parsedJson = JSON.parse(event.data.payload);
        const newBalance = parsedJson?.newBalance;
        if (newBalance > 0) {
            savePlayerData(newBalance, (newBalance) => {
                // callback when save is finished
                console.log(debugPrefix, '[SavePlayerData] Balance Saved: ', newBalance);
            });
        } else {
            console.log(debugPrefix, "[HandleUnityMessage] Balance returned not valid, won't update!");
        }

    } else if (event.data?.type === "unityReady") {
        console.log(debugPrefix, "[HandleUnityLoadResponse] Unity Game Loaded Successfully");

        // go through startup sequence
        loadPlayerData((loadedBalance) => {
            // callback when load is finished
            if (loadedBalance > 0) {
                sendMessageToUnity("SetBalance", loadedBalance.toString());
                console.log(debugPrefix, '[LoadPlayerData] Balance Loaded $', loadedBalance);
            } else {
                console.log(debugPrefix, '[LoadPlayerData] Balance Failed to Load!\nDefaulting to $', loadedBalance);
            }
        });

        startQuestionTimer();
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

let startTime = Date.now();

function startQuestionTimer() {
    startTime = Date.now();
}

function endQuestionTimerAndFetchWager() {
    const endTime = Date.now();
    const questionTimeSeconds = (endTime - startTime) / 1000;
    let index = Math.floor(questionTimeSeconds / wagerTimeSteps);
    index = Math.min(index, allWagers.length - 1);
    return allWagers[index];
}

function endQuestionTimerAndFetchTimeDelta() {
    const endTime = Date.now();
    return (endTime - startTime) / 1000;  // delta in seconds
}

/////////////////////////
// Player Data Storage //
/////////////////////////
const playerBalanceKey = "playerBalance"

function savePlayerData(newBalance, callback) {
    chrome.storage.sync.set({ [playerBalanceKey]: newBalance }, () => {
        if (typeof callback === 'function') {
            callback(newBalance);
        }
    });
}

function loadPlayerData(callback) {
    chrome.storage.sync.get(playerBalanceKey, (res) => {
        const storedBalance = res[playerBalanceKey] ?? 0.0;
        callback(storedBalance);
    });
}


//////////
// Misc //
//////////

// inject unity loader instance result into page context
const script = document.createElement('script');
script.src = chrome.runtime.getURL('src/unityRelay.js');
script.onload = () => script.remove();
(document.head || document.documentElement).appendChild(script);


function sendMessageToUnity(method, arg) {
    // calls the given method while passing through the string arg
    const iframe = document.querySelector('iframe[src*="GameBuild/index.html"]');
    iframe.contentWindow.postMessage({
        type: 'UNITY_COMMAND',
        method: method,
        value: arg,
    }, '*');
}