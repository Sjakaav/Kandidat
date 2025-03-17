#import <AVFoundation/AVFoundation.h>

extern "C" {
    void SpeakText(const char* text) {
        NSString *nsText = [NSString stringWithUTF8String:text];
        AVSpeechSynthesizer *synthesizer = [[AVSpeechSynthesizer alloc] init];
        AVSpeechUtterance *utterance = [[AVSpeechUtterance alloc] initWithString:nsText];
        utterance.rate = 0.5;
        utterance.voice = [AVSpeechSynthesisVoice voiceWithLanguage:@"en-US"];
        [synthesizer speakUtterance:utterance];
    }
}
