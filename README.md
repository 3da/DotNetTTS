# DotNetTTS
This is wrapper for python TTS models:
- https://github.com/alphacep/vosk-tts
- https://github.com/snakers4/silero-models

It just starts python.exe process to send input text through stdin and then read result wav file.

Usage:
```csharp
using var speaker1 = new SileroSpeaker("Data\\model.pt", speaker: "eugene", sampleRate: 48000);
using var speaker2 = new VoskSpeaker("vosk-model-tts-ru-0.1-natasha");

await speaker1.InitializeAsync();
await speaker2.InitializeAsync();

//Simple dialog
await speaker1.SpeakAsync("приветик, как дела?");
await speaker2.SpeakAsync("привет, хорошо");
await speaker1.SpeakAsync("что делаешь?");
await speaker2.SpeakAsync("пью чай");
```

See Examples project for more details.

You need installed python with packages:
```
numpy
torch
torchaudio
tqdm
vosk-tts
```
