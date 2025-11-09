// Runs in page context -- listens for messages to be sent to Unity and relays them to the instance
// created by the unityLoader.js script

const debugPrefix = "[MathAddict][UnityRelay]";

window.addEventListener('message', (event) => {
    if (event.source !== window || !event.data || event.data.type !== 'UNITY_COMMAND') return;

    const { method, value } = event.data;
    if (typeof unityInstance !== 'undefined') {
        console.log(debugPrefix, `[RelayUnityMessage] Sending ${method}(${value})`);
        unityInstance.SendMessage("MAUnityManager", method, String(value));
    } else {
        console.warn(debugPrefix, '[RelayUnityMessage] Unity instance not ready');
    }
});
