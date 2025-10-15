// runs in page context
// listens for messages to be sent to Unity and sends them to the instance created by the unityLoader
window.addEventListener('message', (event) => {
    if (event.source !== window || !event.data || event.data.type !== 'UNITY_COMMAND') return;

    const { method, value } = event.data;
    if (typeof unityInstance !== 'undefined') {
        console.log(`[Unity Relay] Sending ${method}(${value})`);
        unityInstance.SendMessage("MAUnityManager", method, String(value));
    } else {
        console.warn('[Unity Relay] Unity instance not ready');
    }
});
