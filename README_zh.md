[English](README.md)

# Cryville.Audio
项目 #A012 [cau] Cryville.Audio 是一个 .NET 下的实时音频播放库。

本项目于 2022-04-08 创建，于 2022-08-09 开源。

## 支持框架
- .NET Framework 3.5
- .NET Standard 2.0

如果需要在更早的框架使用本项目，请提交 issue。

## 使用
```cs
// 将引擎加入构建器中
EngineBuilder.Engines.Add(typeof(MMDeviceEnumeratorWrapper));
EngineBuilder.Engines.Add(typeof(WaveDeviceManager));

AudioManager = EngineBuilder.Create();
if (AudioManager == null) {
	// 初始化失败。在此处处理错误。
}
else {
	AudioClient = AudioManager.GetDefaultDevice(DataFlow.Out).Connect(device.DefaultFormat, device.DefaultBufferSize);
	AudioClient.Source = new AudioSource(); // 在此处设置音频源，所有可用的音频源参见 Cryville.Audio.Source 命名空间。
	AudioClient.Start();
}
```

## 支持音频源
- 以给定的函数生成波形数据
  - 生成纯调
  - 使用自定义函数（扩展）
- 使用 FFmpeg 从音频或视频文件中读取音频数据（需要 FFmpeg，参见下方段落）
- 缓存其它音频源的数据并复用
- 将多个音频源以指定的时间戳序列播放
- 使用自定义音频源（扩展）

## 支持编码库
使用这些库需要自己进行下载或构建。

### FFmpeg（跨平台）
不需要可执行二进制文件。需要以下的库：
- libavcodec
- libavfilter
- libavformat
- libavutil
- libswresample

本项目只使用 `file` 协议、解流器、解析器和解码器。另外，重采样需要 `aresample` 滤波器。

## 支持 API
当前支持：
- WASAPI（Windows Vista+）
- WinMM（Windows 2000+）
- OpenSL ES（Android 2.3+）
- AAudio（Android 8+）

开发中：
- Core Audio（iOS 2.0+；iPadOS 2.0+；MacOS 10.0+）

取消支持：
- ~~TinyALSA（Android 3.2+）~~
- ~~Oboe（Android 4.4+）~~
