#import <Foundation/Foundation.h>
#import <AVFoundation/AVFoundation.h>

extern "C" void _SpeakIOS(const char* message)
{
    @autoreleasepool {
        NSString *text = [NSString stringWithUTF8String:message];
        AVSpeechSynthesizer *synthesizer = [[AVSpeechSynthesizer alloc] init];
        AVSpeechUtterance *utterance = [AVSpeechUtterance speechUtteranceWithString:text];
        utterance.rate = AVSpeechUtteranceDefaultSpeechRate;
        [synthesizer speakUtterance:utterance];
    }
}
