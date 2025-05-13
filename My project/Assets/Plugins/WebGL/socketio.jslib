mergeInto(LibraryManager.library, {
  // call from C#: SocketIO_Connect("https://…");
  SocketIO_Connect: function(urlPtr) {
    var url = UTF8ToString(urlPtr);
    window.socketQueue = [];
    window.unitySocket = io(url);

    function enqueue(method, data) {
      socketQueue.push({ method: method, data: data });
    }

    window.unitySocket.on("connect", () => enqueue("onSocketIOConnect", ""));
    window.unitySocket.on("disconnect", () => enqueue("onSocketIODisconnect", ""));
    window.unitySocket.on("connect_error", (err) => enqueue("onConnectError", err.message));

    window.unitySocket.on("transcription_ready", (payload) => {
      enqueue("onTranscription", (payload.transcription||""));
    });
    window.unitySocket.on("ai_response", (payload) => {
      enqueue("onAIResponse", (payload.response||""));
    });
  },

  // call from C#: SocketIO_Emit("audio_message", "\"…base64…\"");
  SocketIO_Emit: function(eventPtr, dataPtr) {
    if (!window.unitySocket) return;
    var ev  = UTF8ToString(eventPtr);
    var raw = UTF8ToString(dataPtr);
    try {
      window.unitySocket.emit(ev, JSON.parse(raw));
    } catch(e) {
      window.unitySocket.emit(ev, raw);
    }
  },

  // call each frame from Unity to flush the queue
  PollSocketEvents: function() {
    if (!window.socketQueue || !window.unityInstance) return;
    while (window.socketQueue.length) {
      var e = window.socketQueue.shift();
      window.unityInstance.SendMessage("SocketManager", e.method, e.data);
    }
  },

  // if you ever want to disconnect from C#
  SocketIO_Disconnect: function() {
    if (window.unitySocket) {
      window.unitySocket.disconnect();
      delete window.unitySocket;
    }
  }
});