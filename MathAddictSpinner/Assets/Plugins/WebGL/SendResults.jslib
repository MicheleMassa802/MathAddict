mergeInto(LibraryManager.library, {
  SendResults: function (messagePtr) {
    const message = UTF8ToString(messagePtr);
    window.parent.postMessage({ type: "unityResult", payload: message }, "*");
  }
});