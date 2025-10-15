// unityBridge.js
console.log('unityInstance is:', typeof unityInstance !== 'undefined' ? 'available' : 'missing');

window.addEventListener('message', (event) => {
    if (!event.data || event.data.type !== 'UNITY_COMMAND') return;

    const { method, value } = event.data;
    if (typeof unityInstance !== 'undefined') {
        console.log(`[Unity Iframe] Calling ${method}(${value})`);
        unityInstance.SendMessage("MAUnityManager", method, String(value));
    } else {
        console.warn('[Unity Iframe] Unity instance not ready');
    }
});