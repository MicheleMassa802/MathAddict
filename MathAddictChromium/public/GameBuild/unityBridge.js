// unityBridge.js
const debugPrefix = "[MathAddict][UnityBridge]";
console.warn(debugPrefix, '[StartUp] UnityInstance is:', typeof unityInstance !== 'undefined' ? 'available' : 'missing');

window.addEventListener('message', (event) => {
    if (!event.data || event.data.type !== 'UNITY_COMMAND') return;

    const { method, value } = event.data;
    if (typeof unityInstance !== 'undefined') {
        console.log(debugPrefix, `[SendMessageToUnity] Calling ${method}(${value})`);
        unityInstance.SendMessage("MAUnityManager", method, String(value));
    } else {
        console.warn(debugPrefix, '[SendMessageToUnity] Unity instance not ready');
    }
});