// trigger the load of Unity build game on a canvas on the screen of the MA website

document.getElementById("startBtn").addEventListener("click", () => {
    const config = {
        dataUrl: "GameBuild/Build/MASpinner_v0.0.1.data",
        frameworkUrl: "GameBuild/Build/MASpinner_v0.0.1.framework.js",
        codeUrl: "GameBuild/Build/MASpinner_v0.0.1.wasm",
        companyName: "FriesInTheBag",
        productName: "UnityUISample",
        productVersion: "1.0"
    };

    createUnityInstance(document.querySelector("#unityContainer"), config)
        .then((unityInstance) => {
            console.log("Unity instance loaded successfully!");
        })
        .catch((error) => {
            console.error("Failed to load Unity instance:", error);
        });
});

const canvas = document.createElement("canvas");
canvas.id = "MAUnityContainer";
canvas.style.width = "450px";
canvas.style.height = "800px";
document.body.appendChild(canvas);

const script = document.createElement("script");
script.src = chrome.runtime.getURL("GameBuild/Build/MASpinner_v0.0.1.loader.js");
document.body.appendChild(script);

script.onload = () => {
    const config = {
        dataUrl: chrome.runtime.getURL("GameBuild/Build/MASpinner_v0.0.1.data"),
        frameworkUrl: chrome.runtime.getURL("GameBuild/Build/MASpinner_v0.0.1.framework.js"),
        codeUrl: chrome.runtime.getURL("GameBuild/Build/MASpinner_v0.0.1.wasm"),
        companyName: "FriesInTheBag",
        productName: "UnityUISample",
        productVersion: "1.0"
    };

    createUnityInstance(canvas, config)
        .then((unityInstance) => console.log("Unity loaded"))
        .catch((err) => console.error("Unity failed", err));
};