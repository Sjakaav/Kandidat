mergeInto(LibraryManager.library, {
  SocketIO_Connect: function (urlPtr) {
    var url = UTF8ToString(urlPtr);
    // Note: `io` comes from the CDN script you loaded in index.html
    window.unitySocket = io(url);

    unitySocket.on("connect", function() {
      // Calls C# method onSocketIOConnect()
      Module.onSocketIOConnect && Module.onSocketIOConnect();
    });
    unitySocket.on("disconnect", function() {
      Module.onSocketIODisconnect && Module.onSocketIODisconnect();
    });
    unitySocket.on("connect_error", function(err) {
      console.error("Socket.IO connect_error", err);
      Module.onConnectError && Module.onConnectError(err.message);
    });
    unitySocket.on("transcription_ready", function(msg) {
      var json = JSON.stringify(msg);
      Module.onTranscription && Module.onTranscription(json);
    });
    unitySocket.on("ai_response", function(msg) {
      var json = JSON.stringify(msg);
      Module.onAIResponse && Module.onAIResponse(json);
    });
  },

  SocketIO_Emit: function (eventPtr, dataPtr) {
    if (!window.unitySocket) return;
    var ev   = UTF8ToString(eventPtr);
    var data = UTF8ToString(dataPtr);
    unitySocket.emit(ev, JSON.parse(data));
  }
});