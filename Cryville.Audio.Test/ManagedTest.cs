using Cryville.Audio.Source;
using Cryville.Audio.Source.Libav;
using Cryville.Audio.Source.Resample;
using Cryville.Audio.Wasapi;
using Cryville.Audio.WaveformAudio;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading;

namespace Cryville.Audio.Test {
	public static partial class ManagedTestCaseResources {
		/*
		/// <summary>
		/// The path to an audio file of a common format that has only one stream, encoded with a common audio codec.
		/// </summary>
		public const string AudioFile = @"";
		/// <summary>
		/// The path to a video file of a common format that has one audio stream and one video stream. The audio stream is encoded with a common audio codec.
		/// </summary>
		public const string VideoFile = @"";
		*/
	}

	public class DefaultManagedTest : ManagedTest {
		protected override IAudioDeviceManager CreateEngine() {
			var builder = new EngineBuilder();
			builder.Engines.Add(typeof(MMDeviceEnumeratorWrapper));
			builder.Engines.Add(typeof(WaveDeviceManager));
			return builder.Create();
		}
	}

	[TestFixture(typeof(MMDeviceEnumeratorWrapper))]
	[TestFixture(typeof(WaveDeviceManager))]
	public class SpecificManagedTest<T> : ManagedTest where T : IAudioDeviceManager, new() {
		protected override IAudioDeviceManager CreateEngine() {
			return new T();
		}
	}

	public abstract class ManagedTest {
		IAudioDeviceManager manager;
		IAudioDevice device;
		AudioClient client;

		protected abstract IAudioDeviceManager CreateEngine();
		protected virtual AudioUsage Usage => AudioUsage.Media;

		[OneTimeSetUp]
		public void OneTimeSetUp() {
			FFmpeg.AutoGen.ffmpeg.RootPath = "";
			manager = CreateEngine() ?? throw new InvalidOperationException("Cannot create engine.");
			device = manager.GetDefaultDevice(DataFlow.Out);
			client = device.Connect(device.DefaultFormat, device.BurstSize + device.MinimumBufferSize, Usage);
		}

		[Test]
		[Order(0)]
		public virtual void EnumerateDevices() {
			uint count = 0;
			foreach (var dev in manager.GetDevices(DataFlow.Out)) {
				Log("Device: {0}", dev.Name);
				dev.Dispose();
				count++;
			}
			Log("Count: {0}", count);
		}

		[Test]
		[Order(1)]
		public virtual void GetDeviceInformation() {
			Log("Name: {0}", device.Name);
			Log("Data Flow: {0}", device.DataFlow);
			Log("Burst Size: {0}", device.BurstSize);
			Log("Minimum Buffer Size: {0}", device.MinimumBufferSize);
			Log("Default Buffer Size: {0}", device.DefaultBufferSize);
			Log("Device Default Format: {0}", device.DefaultFormat);
			Log("Connection Format: {0}", client.Format);
			Log("Buffer Size: {0}", client.BufferSize);
			Log("Maximum Latency: {0}ms", client.MaximumLatency);
			Log("Actual Latency: {0}ms", (float)client.BufferSize / client.Format.SampleRate * 1000 + client.MaximumLatency);
		}

		[Test]
		public virtual void IsFormatSupported() {
			var testFormat = new WaveFormat() { Channels = 2, SampleFormat = SampleFormat.S16, SampleRate = 44100 };
			Log("Test Format: {0}", testFormat);
			bool isSupported = device.IsFormatSupported(testFormat, out var suggestedFormat);
			Log("Supported: {0}", isSupported);
			Log("Suggested Format: {0}", suggestedFormat);
		}

		[Test]
		public virtual void PlayDummy() {
			client.Start();
			for (int i = 0; i < 10; i++) {
				LogPosition("");
				Thread.Sleep(1000);
			}
			client.Pause();
		}

		[Test]
		public virtual void PlaySingleTone() {
			var source = new SingleToneAudioSource { Type = ToneType.Sine, Frequency = 440, Amplitude = 1f };
			client.Source = source;

			client.Start();

			source.Type = ToneType.Sine;
			LogPosition("Sine 440Hz");
			Thread.Sleep(1000);
			source.Type = ToneType.Triangle;
			LogPosition("Triangle 440Hz");
			Thread.Sleep(1000);
			source.Type = ToneType.Square;
			LogPosition("Square 440Hz");
			Thread.Sleep(1000);

			client.Source = null;
			LogPosition("Mute");
			Thread.Sleep(1000);
			client.Source = source;
			source.Frequency = 880f;

			source.Type = ToneType.Sine;
			LogPosition("Sine 880Hz");
			Thread.Sleep(1000);
			source.Type = ToneType.Triangle;
			LogPosition("Triangle 880Hz");
			Thread.Sleep(1000);
			source.Type = ToneType.Square;
			LogPosition("Square 880Hz");
			Thread.Sleep(1000);

			LogPosition("Playback done");
			client.Source = null;
			client.Pause();

			source.Dispose();
		}

		[Test]
		[TestCase(ManagedTestCaseResources.AudioFile)]
		[TestCase(ManagedTestCaseResources.VideoFile)]
		public virtual void PlayWithLibAV(string file) {
			Log("API: {0}", manager.GetType().Namespace);
			var source = new LibavFileAudioSource(file);
			Log("Duration: {0}s", source.GetStreamDuration());
			Log("Best stream index: {0}", source.BestStreamIndex);
			Log("Best stream duration: {0}s", source.GetStreamDuration(source.BestStreamIndex));
			source.SelectStream();
			client.Source = source;
			client.Start();
			for (int i = 0; i < 10; i++) {
				LogPosition("");
				Thread.Sleep(1000);
			}
			client.Source = null;
			client.Pause();
			source.Dispose();
		}

		[Test]
		[TestCase(ManagedTestCaseResources.AudioFile)]
		[TestCase(ManagedTestCaseResources.VideoFile)]
		public virtual void PlayResampledWithLibAV(string file) {
			Log("API: {0}", manager.GetType().Namespace);
			var source = new LibavFileAudioSource(file);
			Log("Duration: {0}s", source.GetStreamDuration());
			Log("Best stream index: {0}", source.BestStreamIndex);
			Log("Best stream duration: {0}s", source.GetStreamDuration(source.BestStreamIndex));
			source.SelectStream();
			Log("Source wave format: {0}", source.DefaultFormat);
			Log("Output wave format: {0}", client.Format);
			var resampledSource = new ResampledAudioSource(source);
			client.Source = resampledSource;
			client.Start();
			for (int i = 0; i < 10; i++) {
				LogPosition("");
				Thread.Sleep(1000);
			}
			client.Source = null;
			client.Pause();
			resampledSource.Dispose();
			source.Dispose();
		}

		[Test]
		[TestCase(ManagedTestCaseResources.AudioFile)]
		[TestCase(ManagedTestCaseResources.VideoFile)]
		public virtual void PlaySoughtWithLibAV(string file) {
			Log("API: {0}", manager.GetType().Namespace);
			var source = new LibavFileAudioSource(file);
			Log("Duration: {0}s", source.GetStreamDuration());
			Log("Best stream index: {0}", source.BestStreamIndex);
			Log("Best stream duration: {0}s", source.GetStreamDuration(source.BestStreamIndex));
			client.Source = source;
			source.SeekTime(60, SeekOrigin.Begin);
			client.Start();
			for (int i = 0; i < 10; i++) {
				LogPosition(string.Format("Source: {0}s", source.TimePosition));
				Thread.Sleep(1000);
			}
			client.Pause();
			source.SeekTime(-30, SeekOrigin.Current);
			client.Start();
			for (int i = 0; i < 10; i++) {
				LogPosition(string.Format("Source: {0}s", source.TimePosition));
				Thread.Sleep(1000);
			}
			client.Pause();
			client.Source = null;
			source.Dispose();
		}

		[Test]
		[TestCase(ManagedTestCaseResources.AudioFile, ManagedTestCaseResources.AudioFile)]
		public virtual void PlayWithSimpleQueue(string file1, string file2) {
			Log("API: {0}", manager.GetType().Namespace);
			var source = new SimpleSequencerSource();
			client.Source = source;
			client.Start();

			var session = source.NewSession();

			var source1 = new LibavFileAudioSource(file1);
			source1.SelectStream();
			session.Sequence(1, source1);

			var source2 = new LibavFileAudioSource(file2);
			source2.SelectStream();
			session.Sequence(4, source2);

			source.Playing = true;

			for (int i = 0; i < 20; i++) {
				LogPosition(string.Format("Polyphony: {0}", source.Polyphony));
				Thread.Sleep(1000);
			}
			client.Source = null;
			client.Pause();
			source1.Dispose();
			source2.Dispose();
			source.Dispose();
		}

		[Test]
		[TestCase(ManagedTestCaseResources.AudioFile)]
		public virtual void PlayCachedWithSimpleQueue(string file) {
			Log("API: {0}", manager.GetType().Namespace);
			var source = new SimpleSequencerSource();
			client.Source = source;

			var session = source.NewSession();

			var rsource = new LibavFileAudioSource(file);
			rsource.SelectStream();

			var source1 = new CachedAudioSource(rsource, 15);
			session.Sequence(1, source1);

			CachedAudioSource source2 = null;

			client.Start();
			source.Playing = true;

			for (int i = 0; i < 20; i++) {
				LogPosition(string.Format("Polyphony: {0}", source.Polyphony));
				if (i == 3) {
					source2 = source1.Clone();
					session.Sequence(4, source2);
				}
				Thread.Sleep(1000);
			}
			client.Source = null;
			client.Pause();
			source2?.Dispose();
			source1.Dispose();
			rsource.Dispose();
			source.Dispose();
		}

		[Test]
		[TestCase(ManagedTestCaseResources.AudioFile, ManagedTestCaseResources.AudioFile)]
		public virtual void PlayTwoSessions(string file1, string file2) {
			Log("API: {0}", manager.GetType().Namespace);
			var source = new SimpleSequencerSource();
			client.Source = source;
			client.Start();

			var session = source.NewSession();

			var source1 = new LibavFileAudioSource(file1);
			source1.SelectStream();
			session.Sequence(0, source1);

			source.Playing = true;

			for (int i = 0; i < 5; i++) {
				LogPosition(string.Format("Polyphony: {0}", source.Polyphony));
				Thread.Sleep(1000);
			}

			session = source.NewSession();

			var source2 = new LibavFileAudioSource(file2);
			source2.SelectStream();
			session.Sequence(0, source2);

			source.Playing = true;

			for (int i = 0; i < 5; i++) {
				LogPosition(string.Format("Polyphony: {0}", source.Polyphony));
				Thread.Sleep(1000);
			}

			client.Source = null;
			client.Pause();
			source1.Dispose();
			source2.Dispose();
			source.Dispose();
		}

		protected virtual void Log(string msg) {
			TestContext.WriteLine(msg);
		}

		protected void Log(string format, params object[] args) {
			Log(string.Format(format, args));
		}

		protected void LogPosition(string desc) {
			Log(
				"Clock: {0:F6}s | Buffer: {1:F6}s | Latency: {2:F3}ms | {3}",
				client.Position, client.BufferPosition,
				(client.BufferPosition - client.Position) * 1e3, desc
			);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown() {
			client.Dispose();
			device.Dispose();
			manager.Dispose();
		}
	}
}
