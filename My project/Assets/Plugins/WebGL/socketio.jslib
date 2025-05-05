mergeInto(LibraryManager.library, {
  SocketIO_Connect: function(urlPtr) {
    var url = UTF8ToString(urlPtr);
    window.unitySocket = io(url);

    window.unitySocket.on("connect", function() {
      window.unityInstance.SendMessage("SocketManager", "onSocketIOConnect", "");
    });
    window.unitySocket.on("disconnect", function() {
      window.unityInstance.SendMessage("SocketManager", "onSocketIODisconnect", "");
    });
    window.unitySocket.on("connect_error", function(err) {
      window.unityInstance.SendMessage("SocketManager", "onConnectError", err.message);
    });

    // unwrap the JSON on the JS side and pass only the raw string
    window.unitySocket.on("transcription_ready", function(payload) {
      // payload.transcription is a JS string
      window.unityInstance.SendMessage("SocketManager", "onTranscription", payload.transcription || "");
    });
    window.unitySocket.on("ai_response", function(payload) {
      window.unityInstance.SendMessage("SocketManager", "onAIResponse", payload.response || "");
    });
  },

  SocketIO_Emit: function(eventPtr, dataPtr) {
    if (!window.unitySocket) return;
    var ev   = UTF8ToString(eventPtr);
    var raw  = UTF8ToString(dataPtr);
    // dataPtr will already be a JSON-encoded string or a quoted string
    try {
      var obj = JSON.parse(raw);
      window.unitySocket.emit(ev, obj);
    } catch (e) {
      // if parse fails, emit raw
      window.unitySocket.emit(ev, raw);
    }
  },

  SocketIO_Disconnect: function() {
    if (window.unitySocket) {
      window.unitySocket.disconnect();
      delete window.unitySocket;
    }
  }
});