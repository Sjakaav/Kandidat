mergeInto(LibraryManager.library, {
  SocketIO_Connect: function(urlPtr) {
    var url = UTF8ToString(urlPtr);
    window.unitySocket = io(url);

    window.unitySocket.on("connect",        ()=> window.unityInstance.SendMessage("SocketManager","onSocketIOConnect",""));
    window.unitySocket.on("disconnect",     ()=> window.unityInstance.SendMessage("SocketManager","onSocketIODisconnect",""));
    window.unitySocket.on("connect_error",  err=> window.unityInstance.SendMessage("SocketManager","onConnectError",err.message));

    window.unitySocket.on("transcription_ready", data=>{
      var msg = (typeof data==="string")?data:JSON.stringify(data);
      window.unityInstance.SendMessage("SocketManager","onTranscription",msg);
    });
    window.unitySocket.on("ai_response", data=>{
      var msg = (typeof data==="string")?data:JSON.stringify(data);
      window.unityInstance.SendMessage("SocketManager","onAIResponse",msg);
    });
  },

  SocketIO_Emit: function(ePtr, dPtr) {
    if (!window.unitySocket) return;
    var ev  = UTF8ToString(ePtr);
    var raw = UTF8ToString(dPtr);
    var obj = JSON.parse(raw);
    window.unitySocket.emit(ev, obj);
  }
});