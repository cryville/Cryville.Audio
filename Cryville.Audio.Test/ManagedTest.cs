using Cryville.Audio.Source;
using NUnit.Framework;
using System;
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

	[TestFixture(typeof(Wasapi.MMDeviceEnumerator))]
	[TestFixture(typeof(WinMM.WaveDeviceManager))]
	public class ManagedTest<T> where T : IAudioDeviceManager, new() {
		IAudioDeviceManager manager;
		IAudioDevice device;
		AudioClient client;

		[OneTimeSetUp]
		public void OneTimeSetUp() {
			FFmpeg.AutoGen.ffmpeg.RootPath = "";
			manager = new T();
			device = manager.GetDefaultDevice(DataFlow.Out);
			client = device.Connect();
			WaveFormat? format = WaveFormat.Default;
			TestContext.WriteLine("Client Default Format: {0}", format);
			client.IsFormatSupported(format.Value, out format);
			if (format != null) client.Init(format.Value);
			else throw new NotSupportedException("No supported format is found.");
		}

		[Test]
		public virtual void EnumerateDevices() {
			uint count = 0;
			foreach (var dev in manager.GetDevices(DataFlow.Out)) {
				TestContext.WriteLine("Device: {0}", dev.Name);
				dev.Dispose();
				count++;
			}
			TestContext.WriteLine("Count: {0}", count);
		}

		[Test]
		public virtual void GetDeviceInformation() {
			TestContext.WriteLine("Name: {0}", device.Name);
			TestContext.WriteLine("Data Flow: {0}", device.DataFlow);
			TestContext.WriteLine("Default Buffer Duration: {0}", client.DefaultBufferDuration);
			TestContext.WriteLine("Minimum Buffer Duration: {0}", client.MinimumBufferDuration);
			TestContext.WriteLine("Device Default Format: {0}", client.DefaultFormat);
			TestContext.WriteLine("Connection Format: {0}", client.Format);
			TestContext.WriteLine("Buffer Size: {0}B", client.BufferSize);
			TestContext.WriteLine("Maximum Latency: {0}ms", client.MaximumLatency);
			TestContext.WriteLine("Actual Latency: {0}ms", (float)client.BufferSize / client.Format.BytesPerSecond * 1000 + client.MaximumLatency);
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

			source.Muted = true;
			LogPosition("Mute");
			Thread.Sleep(1000);
			source.Muted = false;
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
			TestContext.WriteLine("API: {0}", typeof(T).Namespace);
			var source = new LibavFileAudioSource(file);
			TestContext.WriteLine("Duration: {0}s", source.GetDuration());
			TestContext.WriteLine("Best stream index: {0}", source.BestStreamIndex);
			TestContext.WriteLine("Best stream duration: {0}s", source.GetDuration(source.BestStreamIndex));
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
		[TestCase(ManagedTestCaseResources.AudioFile, ManagedTestCaseResources.AudioFile)]
		public virtual void PlayWithSimpleQueue(string file1, string file2) {
			TestContext.WriteLine("API: {0}", typeof(T).Namespace);
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
			TestContext.WriteLine("API: {0}", typeof(T).Namespace);
			var source = new SimpleSequencerSource();
			client.Source = source;

			var session = source.NewSession();

			var rsource = new LibavFileAudioSource(file);
			rsource.SelectStream();

			var source1 = new CachedAudioSource(rsource, 15);
			session.Sequence(1, source1);

			var source2 = source1.Clone();
			session.Sequence(4, source2);

			client.Start();
			source.Playing = true;

			for (int i = 0; i < 20; i++) {
				LogPosition(string.Format("Polyphony: {0}", source.Polyphony));
				Thread.Sleep(1000);
			}
			client.Source = null;
			client.Pause();
			source2.Dispose();
			source1.Dispose();
			rsource.Dispose();
			source.Dispose();
		}

		void LogPosition(string desc) {
			TestContext.WriteLine(
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
