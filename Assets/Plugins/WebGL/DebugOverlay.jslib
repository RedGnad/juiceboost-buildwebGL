mergeInto(LibraryManager.library, {
  ShowDebugOverlay: function (messagePtr) {
    var message = UTF8ToString(messagePtr);

    // Crée ou met à jour l'overlay de debug
    var overlay = document.getElementById("unity-debug-overlay");
    if (!overlay) {
      overlay = document.createElement("div");
      overlay.id = "unity-debug-overlay";
      overlay.style.position = "fixed";
      overlay.style.top = "10px";
      overlay.style.left = "10px";
      overlay.style.background = "rgba(0,0,0,0.9)";
      overlay.style.color = "lime";
      overlay.style.padding = "10px";
      overlay.style.fontSize = "14px";
      overlay.style.zIndex = "9999";
      overlay.style.maxWidth = "90%";
      overlay.style.borderRadius = "5px";
      overlay.style.fontFamily = "monospace";
      document.body.appendChild(overlay);
    }

    overlay.innerHTML = message;
    console.log("[DEBUG OVERLAY]", message);
  },
});
