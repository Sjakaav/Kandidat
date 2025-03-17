#import <AVFoundation/AVFoundation.h>
#import <Speech/Speech.h>

extern "C" {
    void StartSpeechRecognition() {
        SFSpeechRecognizer *recognizer = [[SFSpeechRecognizer alloc] init];
        SFSpeechAudioBufferRecognitionRequest *request = [[SFSpeechAudioBufferRecognitionRequest alloc] init];

        AVAudioEngine *audioEngine = [[AVAudioEngine alloc] init];
        AVAudioInputNode *inputNode = audioEngine.inputNode;
        AVAudioFormat *format = [inputNode outputFormatForBus:0];

        [audioEngine.inputNode installTapOnBus:0 bufferSize:1024 format:format block:^(AVAudioPCMBuffer * _Nonnull buffer, AVAudioTime * _Nonnull when) {
            [request appendAudioPCMBuffer:buffer];
        }];

        [audioEngine prepare];
        [audioEngine startAndReturnError:nil];

        [recognizer recognitionTaskWithRequest:request resultHandler:^(SFSpeechRecognitionResult * _Nullable result, NSError * _Nullable error) {
            if (result) {
                const char* recognizedText = [result.bestTranscription.formattedString UTF8String];
                UnitySendMessage("SpeechToText", "OnSpeechResult", recognizedText);
            }
        }];
    }
}
