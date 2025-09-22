const iframe = document.createElement("iframe");
iframe.src = chrome.runtime.getURL("GameBuild/popup.html");
iframe.style.position = "fixed";
iframe.style.top = "0";
iframe.style.left = "0";
iframe.style.width = "100vw";
iframe.style.height = "100vh";
iframe.style.border = "none";
document.body.appendChild(iframe);