mergeInto(LibraryManager.library, {
  micStream: null,
  mediaRecorder: null,
  audioChunks: [],
  
  StartMicRecording: function () {
    navigator.mediaDevices.getUserMedia({ audio: true })
      .then(function (stream) {
        Module.micStream = stream;
        Module.audioChunks = [];
        Module.mediaRecorder = new MediaRecorder(stream);
        
        Module.mediaRecorder.ondataavailable = function (e) {
          Module.audioChunks.push(e.data);
        };
        
        Module.mediaRecorder.onstop = function () {
          var blob = new Blob(Module.audioChunks, { type: 'audio/wav' });
          var reader = new FileReader();
          reader.onloadend = function () {
            var base64Audio = reader.result.split(',')[1];
            // Optional: call a Unity method here
            SendMessage('MicManager', 'OnRecordingComplete', base64Audio);
          };
          reader.readAsDataURL(blob);
        };
        
        Module.mediaRecorder.start();
      })
      .catch(function (err) {
        console.error('Mic access error:', err);
      });
  },

  StopMicRecording: function () {
    if (Module.mediaRecorder && Module.mediaRecorder.state === 'recording') {
      Module.mediaRecorder.stop();
      Module.micStream.getTracks().forEach(track => track.stop());
    }
  }
});
