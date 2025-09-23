console.log("Injecting Unity script loaded");

function injectUnityIFrame() {
    const iframe = document.createElement("iframe");
    iframe.src = chrome.runtime.getURL("GameBuild/index.html");
    iframe.style.position = "fixed";
    iframe.style.top = "0";
    iframe.style.left = "0";
    iframe.style.width = "100vw";
    iframe.style.height = "100vh";
    iframe.style.border = "none";
    iframe.style.zIndex = "999999";  // need to be on top
    document.body.appendChild(iframe);
}

if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", injectUnityIFrame);
} else {
    // load right away
    injectUnityIFrame();
}