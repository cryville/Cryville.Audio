[中文](README_zh.md)

# Cryville.Audio
Project #A012 [cau] Cryville.Audio is a realtime audio playback library under .NET.

This project was created on 2022-04-08. It has been open-sourced since 2022-08-09.

## Supported frameworks
- .NET Framework 3.5
- .NET Standard 2.0

Submit an issue if you want to use this library in an older framework.

## Usage
```cs
// Add engines into the builder
EngineBuilder.Engines.Add(typeof(MMDeviceEnumeratorWrapper));
EngineBuilder.Engines.Add(typeof(WaveDeviceManager));

AudioManager = EngineBuilder.Create();
if (AudioManager == null) {
	// Initialization failed. Handle the error here.
}
else {
	AudioClient = AudioManager.GetDefaultDevice(DataFlow.Out).Connect(device.DefaultFormat, device.DefaultBufferSize);
	AudioClient.Source = new AudioSource(); // Set an audio source here, see the Cryville.Audio.Source namespace for all available audio sources.
	AudioClient.Start();
}
```

## Supported audio sources
- Generate wave data from a given function
  - Generate a pure tone
  - Use your own function (extend)
- Read audio data from an audio or video file using FFmpeg (FFmpeg is required, see below)
- Cache another audio source for reuse
- Sequence multiple audio sources at precise timestamps
- Use your own audio source (extend)

## Supported codec libraries
If you want to use these libraries, you need to download or build them by yourself.

### FFmpeg (Cross-platform)
Executable binaries are not required. The following libraries are required:
- libavcodec
- libavfilter
- libavformat
- libavutil
- libswresample

Only `file` protocol, demuxers, parsers, and decoders are required. Besides, `aresample` filter is required for resampling.

## Supported API
Current support:
- WASAPI (Windows Vista+)
- WaveformAudio (Windows 2000+)
- OpenSL ES (Android 2.3+)
- AAudio (Android 8+)

Working in progress:
- Core Audio (iOS 2.0+, iPadOS 2.0+, MacOS 10.0+)

Dropped support:
- ~~TinyALSA (Android 3.2+)~~
- ~~Oboe (Android 4.4+)~~
