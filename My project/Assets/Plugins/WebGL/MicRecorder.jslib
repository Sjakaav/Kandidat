mergeInto(LibraryManager.library, {
  micStream: null,
  mediaRecorder: null,
  audioChunks: [],

  StartMicRecording: function () {
    navigator.mediaDevices.getUserMedia({ audio: true })
      .then(function (stream) {
        Module.micStream = stream;
        Module.audioChunks = [];

        // Set MIME type explicitly to audio/wav if supported
        let options = { mimeType: 'audio/wav' };
        try {
          Module.mediaRecorder = new MediaRecorder(stream, options);
        } catch (e) {
          console.warn('audio/wav not supported, falling back to default codec');
          Module.mediaRecorder = new MediaRecorder(stream);
        }

        Module.mediaRecorder.ondataavailable = function (e) {
          if (e.data.size > 0) {
            Module.audioChunks.push(e.data);
          }
        };

        Module.mediaRecorder.onstop = function () {
          var blob = new Blob(Module.audioChunks, { type: 'audio/wav' });
          var reader = new FileReader();
          reader.onloadend = function () {
            var base64Audio = reader.result.split(',')[1];
            SendMessage('MicManager', 'OnRecordingComplete', base64Audio);
          };
          reader.readAsDataURL(blob);
        };

        Module.mediaRecorder.start();
      })
      .catch(function (err) {
        console.error('Microphone access error:', err);
      });
  },

  StopMicRecording: function () {
    if (Module.mediaRecorder && Module.mediaRecorder.state === 'recording') {
      Module.mediaRecorder.stop();
      if (Module.micStream) {
        Module.micStream.getTracks().forEach(track => track.stop());
      }
    }
  }
});