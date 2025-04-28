// Assets/Plugins/WebGL/mic.js
mergeInto(LibraryManager.library, {
    RequestMicPermission: function () {
      navigator.mediaDevices.getUserMedia({ audio: true })
        .then(function (stream) {
          console.log("Microphone access granted.");
        })
        .catch(function (err) {
          console.log("Microphone access denied: " + err);
        });
    }
  });
  