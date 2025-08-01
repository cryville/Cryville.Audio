using Cryville.Audio.Source;
using Cryville.Audio.Wasapi;
using Cryville.Audio.WaveformAudio;
using NUnit.Framework;
using System.Threading;

namespace Cryville.Audio.Test {
	[TestFixture(typeof(MMDeviceEnumeratorWrapper))]
	[TestFixture(typeof(WaveDeviceManager))]
	public class AutoReconnectSpecificManagedTest<T> where T : IAudioDeviceManager, new() {
		AutoReconnectContextImpl _context;

		[OneTimeSetUp]
		public void OneTimeSetUp() {
			FFmpeg.AutoGen.ffmpeg.RootPath = "";
			_context = new AutoReconnectContextImpl(new T(), DataFlow.Out);
			_context.Init();
		}

		sealed class AutoReconnectContextImpl(IAudioDeviceManager manager, DataFlow dataFlow) : AutoReconnectContext(manager, dataFlow) {
			protected override AudioStream CreateAudioStream() {
				TestContext.WriteLine("Recreating audio stream");
				TestContext.WriteLine("Connection Format: {0}", Format);
				TestContext.WriteLine("Buffer Size: {0}", BufferSize);
				return new SingleToneAudioSource(Format) { Type = ToneType.Sine, Frequency = 440, Amplitude = 1 };
			}
		}

		[Test]
		[Order(1)]
		public virtual void GetDeviceInformation() {
			Log("Name: {0}", _context.Device.Name);
			Log("Data Flow: {0}", _context.Device.DataFlow);
			Log("Burst Size: {0}", _context.Device.BurstSize);
			Log("Minimum Buffer Size: {0}", _context.Device.MinimumBufferSize);
			Log("Default Buffer Size: {0}", _context.Device.DefaultBufferSize);
			Log("Device Default Format: {0}", _context.Device.DefaultFormat);
			Log("Connection Format: {0}", _context.Format);
			Log("Buffer Size: {0}", _context.BufferSize);
			Log("Maximum Latency: {0}ms", _context.MaximumLatency);
			Log("Actual Latency: {0}ms", (float)_context.BufferSize / _context.Format.SampleRate * 1000 + _context.MaximumLatency);
		}

		[Test]
		public virtual void Play() {
			_context.Start();

			for (int i = 0; i < 10; i++) {
				LogPosition("");
				Thread.Sleep(1000);
			}

			_context.Pause();
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
				_context.Position, _context.BufferPosition,
				(_context.BufferPosition - _context.Position) * 1e3, desc
			);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown() {
			_context.Dispose();
		}
	}
}
