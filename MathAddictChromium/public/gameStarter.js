// trigger the load of Unity build game when clicking the start button

document.getElementById("startBtn").addEventListener("click", () => {
    const config = {
        dataUrl: "GameBuild/Build/TestBuild.data",
        frameworkUrl: "GameBuild/Build/TestBuild.framework.js",
        codeUrl: "GameBuild/Build/TestBuild.wasm",
        streamingAssetsUrl: "GameBuild/StreamingAssets",
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