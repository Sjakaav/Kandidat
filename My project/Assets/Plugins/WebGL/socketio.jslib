mergeInto(LibraryManager.library, {
  SocketIO_Connect: function (urlPtr) {
    var url = UTF8ToString(urlPtr);
    window.unitySocket = io(url);

    unitySocket.on("connect", function() {
      console.log("📡 JS: socket.connected → calling OnSocketIOConnect");
      unityInstance.SendMessage('SocketManager', 'onSocketIOConnect', '');
    });
    unitySocket.on("disconnect", function() {
      console.log("📴 JS: socket.disconnected → calling onSocketIODisconnect");
      unityInstance.SendMessage('SocketManager', 'onSocketIODisconnect', '');
    });
    unitySocket.on("connect_error", function(err) {
      console.error("Socket.IO connect_error", err);
      unityInstance.SendMessage('SocketManager', 'onConnectError', err.message);
    });
    unitySocket.on("transcription_ready", function(msg) {
      // msg is already an object { transcription: "…" }
      var json = JSON.stringify(msg);
      console.log("📥 JS: transcription_ready →", json);
      unityInstance.SendMessage('SocketManager', 'onTranscription', json);
    });
    unitySocket.on("ai_response", function(msg) {
      var json = JSON.stringify(msg);
      console.log("📥 JS: ai_response →", json);
      unityInstance.SendMessage('SocketManager', 'onAIResponse', json);
    });
  },

  SocketIO_Emit: function (eventPtr, dataPtr) {
    if (!window.unitySocket) return;
    var ev   = UTF8ToString(eventPtr);
    var data = UTF8ToString(dataPtr);
    console.log(`📤 JS: Emitting ${ev}`, data);
    unitySocket.emit(ev, JSON.parse(data));
  }
});