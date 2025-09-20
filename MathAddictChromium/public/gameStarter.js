// trigger the load of Unity build game when clicking the start button

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