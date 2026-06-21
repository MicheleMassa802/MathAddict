// imports
import { connect, disconnect, send, setLogger } from "../../TLNSSubmodule/gedge_serial.js"

// state
let div1Active = false;
let div2Active = false;
let timeDelta = 100;
let sessionBalance = 0;
let hwEnabled = false;
const div1Id = 0;
const div2Id = 1;
const unityInstances = [];
const extensionDivIdPrefix = "MADiv";
const debugPrefix = "[MathAddict][Content]";

// constants
const DEFAULT_SIGNAL = "F00100I15D000500";


///////////////////////////////////////////
// Listening for start signal from popup //
///////////////////////////////////////////
chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
    if (request.action === "appendDiv" && !div1Active && !div2Active) {

        const left = AppendMADiv(div1Id);
        document.body.appendChild(left.div);
        unityInstances[div1Id] = left.iframe;
        div1Active = true;
        const right = AppendMADiv(div2Id);
        document.body.appendChild(right.div);
        unityInstances[div2Id] = right.iframe;
        div2Active = true;
        sendResponse({ status: "success" });

    } else if (request.action === "removeDiv") {
        div1Active = !RemoveMADiv(div1Id);
        div2Active = !RemoveMADiv(div2Id);

        if (div1Active || div2Active) {
            sendResponse({status: "Divs to remove not found"});
        } else {
            sendResponse({status: "removed"});
        }

    } else if (request.action === "connectHw") {
        setLogger((msg, cls) => {
            console.log(`[hardware] ${msg}`);
        });
        hwEnabled = connect();
        sendResponse({status: "connectHw handler executed connect() with result enabled = ", hwEnabled});

    } else if (request.action === "disconnectHw") {
        hwEnabled = !disconnect();
        sendResponse({status: "disconnectHw handler executed disconnect() with result enabled = ", hwEnabled});

    } else {
        console.warn("Invalid/Unknown action received:", request.action);
        sendResponse({ status: "invalid/unknown action" });
    }
});

// take care of disconnect
window.addEventListener("beforeunload", (event) => disconnect());


// Helpers
/**
 * Creates the div with the unity game loaded, at most 2 of these can be present at once!
 * @param index number between 0 (left) and 1 (right) to point to the active games
 * @returns the div component + iframe reference created
 */
function AppendMADiv(index) {
    console.log(debugPrefix, "[HandlePopupResponse] Starting Unity Slots Div");

    const div = document.createElement("div");
    div.id = extensionDivIdPrefix + index;
    div.style.position = "fixed";
    div.style.width = "405px";
    div.style.height = "716px";
    div.style.bottom = "10px";
    div.style.zIndex = "9999";
    div.style.display = "flex";
    div.style.flexDirection = "column";
    if (index === 0) {
        div.style.left = "10px";

    } else {
        div.style.right = "10px";
    }

    // bring in unity through an iframe to avoid CSP stuff (+ a frame)
    const border = document.createElement("div");
    border.style.width = "405px";
    border.style.height = "716px";
    border.style.padding = "10px";
    border.style.border = "none";
    border.style.boxSizing = "border-box";
    div.appendChild(border);

    const iframe = document.createElement("iframe");
    iframe.src = chrome.runtime.getURL("GameBuild/index.html");
    iframe.style.width = "400px";
    iframe.style.height = "711px";
    iframe.style.border = "none";
    border.appendChild(iframe);

    return { div, iframe };
}

function RemoveMADiv(index) {
    console.log(debugPrefix, "[HandlePopupResponse] Removing Unity Slots Div");
    const div = document.getElementById(extensionDivIdPrefix + index);
    if (div) {
        div.remove();
        return true;
    } else {
        return false;
    }
}

////////////////////////////////////////
// Detecting Question Response Events //
////////////////////////////////////////
const possibleWagers = [1, 1, 1, 1, 1, 1, 2, 3, 3, 5, 5, 10];

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
    timeDelta = endQuestionTimerAndFetchTimeDelta();
    let currentWager = possibleWagers[Math.floor(Math.random() * possibleWagers.length)];
    const isCorrect = !!resultBox.querySelector('.questionWidget-correctText');
    const isIncorrect = !!resultBox.querySelector('.questionWidget-incorrectText');

    if (isCorrect) {
        console.log(debugPrefix, '[HandleResultBox] CORRECT answer detected');
        sendMessageToUnity(div1Id, "SetWager", `${currentWager.toString()}:${timeDelta}`);
        sendMessageToUnity(div2Id, "SetWager", `${currentWager.toString()}:${timeDelta}`);

    } else if (isIncorrect) {
        currentWager = -1;
        console.log(debugPrefix, '[HandleResultBox] INCORRECT answer detected');
        sendMessageToUnity(div1Id, "SetWager", `${currentWager.toString()}:${timeDelta}`);
        sendMessageToUnity(div2Id, "SetWager", `${currentWager.toString()}:${timeDelta}`);

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
        const winAmount = parsedJson?.rtp;
        if (winAmount > 0) {
            // trigger hardware
            if (hwEnabled) {
                send(computeTLNSSignal(timeDelta));
            }

            // update session balance
            sessionBalance += winAmount;
            savePlayerData(sessionBalance, (newBalance) => {
                // callback when save is finished
                console.log(debugPrefix, '[SavePlayerData] Balance Saved: ', sessionBalance);
            });
        } else {
            console.log(debugPrefix, "[HandleUnityMessage] RTP returned not valid, won't update!");
        }

        // send update to other instance!
        const sourceUnityInstance = parsedJson?.instanceId;  // 0 or 1
        sendMessageToUnity(Math.abs(sourceUnityInstance - 1), "SetBalance", sessionBalance.toString());

    } else if (event.data?.type === "unityReady") {
        console.log(debugPrefix, "[HandleUnityLoadResponse] Unity Game Loaded Successfully");

        // go through startup sequence
        loadPlayerData((loadedBalance) => {
            // callback when load is finished
            if (loadedBalance >= 0) {
                sessionBalance = loadedBalance;
                sendMessageToUnity(div1Id, "SetBalance", loadedBalance.toString());
                sendMessageToUnity(div2Id, "SetBalance", loadedBalance.toString());
                console.log(debugPrefix, '[LoadPlayerData] Balance Loaded $', loadedBalance);

                // tell unity instances their instance ID
                sendMessageToUnity(div1Id, "SetInstanceId", div1Id.toString());
                sendMessageToUnity(div2Id, "SetInstanceId", div2Id.toString());
            } else {
                console.log(debugPrefix, '[LoadPlayerData] Balance Failed to Load!\nDefaulting to $', loadedBalance);
            }
        });

        startQuestionTimer();
    } else if (event.data?.type === "toggleSound") {
        // get int and communicate new state to the other board
        const parsedJson = JSON.parse(event.data.payload);
        const sourceUnityInstance = parsedJson?.instanceId;  // 0 or 1
        console.log(debugPrefix, "[HandleUnityLoadResponse] Unity Game Asking to Toggle Sound on instance: ", Math.abs(sourceUnityInstance - 1));
        sendMessageToUnity(Math.abs(sourceUnityInstance - 1), "ToggleSound", "");
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


function sendMessageToUnity(index, method, arg) {
    // calls the given method while passing through the string arg
    const iframe = unityInstances[index];
    iframe.contentWindow.postMessage({
        type: 'UNITY_COMMAND',
        method: method,
        value: arg,
    }, '*');
}

// copy to the client side C# code to compute reel spin time, which we'll use to score the time spent to solve the
// question to compute our signal to send
function computeSpinTime(timeDeltaMeasured) {
    const lbLn = 3.219; // ln(25)
    const ubLn = 5.704; // ln(300)

    timeDeltaMeasured = Math.min(Math.max(timeDeltaMeasured, 25), 300);
    const rate = (Math.log(timeDeltaMeasured) - lbLn) / (ubLn - lbLn);
    return rate * 5 + 5;
}

function computePerformanceScore(spinTime) {
    // spinTime is between 5 and 10
    const normalized = (spinTime - 5) / 5;
    return 1 - normalized;  // invert to make 1 = good
}

function buildWinSignal(score) {
    const freq = 150 + 100 * score;  // 150–250
    const intensity = Math.round(10 + 10 * score);  // 10–20
    const duration = 200 + 300 * score;  // 200–500 ms
    return `F${freq.toFixed(0)}I${intensity}D${duration.toFixed(0)}`;
}

function computeTLNSSignal(timeDeltaMeasured) {
    const spinTime = computeSpinTime(timeDeltaMeasured);
    const score = computePerformanceScore(spinTime);

    return buildWinSignal(score);
}
