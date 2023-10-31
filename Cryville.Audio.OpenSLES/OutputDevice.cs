using Cryville.Interop.Java;
using Cryville.Interop.Java.Helper;
using System;
using System.Globalization;

namespace Cryville.Audio.OpenSLES {
	/// <summary>
	/// An <see cref="IAudioDevice" /> that interacts with OpenSL ES.
	/// </summary>
	public class OutputDevice : IAudioDevice {
		readonly Engine _engine;
		internal OutputDevice(Engine engine) {
			_engine = engine;
		}

		/// <inheritdoc />
		~OutputDevice() {
			Dispose(false);
		}

		bool _disposed;
		/// <inheritdoc />
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing">Whether the method is being called by user.</param>
		protected virtual void Dispose(bool disposing) {
			if (!_disposed) {
				_disposed = true;
			}
		}

		/// <inheritdoc />
		public string Name => "Default";

		/// <inheritdoc />
		public DataFlow DataFlow => DataFlow.Out;

		int m_burstSize;
		/// <inheritdoc />
		public int BurstSize {
			get {
				GetDefaultParameters();
				return m_burstSize;
			}
		}
		/// <inheritdoc />
		public int MinimumBufferSize => BurstSize;

		/// <inheritdoc />
		public int DefaultBufferSize => MinimumBufferSize + BurstSize;

		uint m_defaultSampleRate;
		/// <inheritdoc />
		public WaveFormat DefaultFormat {
			get {
				GetDefaultParameters();
				var ret = WaveFormat.Default;
				ret.SampleRate = m_defaultSampleRate;
				return ret;
			}
		}

		unsafe void GetDefaultParameters() {
			if (m_defaultSampleRate != 0) return;
			if (JavaVMManager.CurrentVM == null) {
				m_burstSize = 256;
				m_defaultSampleRate = WaveFormat.Default.SampleRate;
			}
			else {
				var env = JavaVMManager.CurrentEnv;
				using (var frame = new JniLocalFrame(env, 6)) {
					var manager = AndroidHelper.GetSystemService(env, AndroidHelper.GetCurrentApplication(env), "AUDIO_SERVICE");
					var c = env.GetObjectClass(manager);
					if (c == IntPtr.Zero) throw new InvalidOperationException("Could not get the AudioManager class.");
					var m = env.GetMethodID(c, "getProperty", "(Ljava/lang/String;)Ljava/lang/String;");
					if (m == IntPtr.Zero) {
						m_burstSize = 256;
						m_defaultSampleRate = WaveFormat.Default.SampleRate;
						return;
					}

					var f1 = env.GetStaticFieldID(c, "PROPERTY_OUTPUT_SAMPLE_RATE", "Ljava/lang/String;");
					if (f1 == IntPtr.Zero) throw new InvalidOperationException("Could not find the static field PROPERTY_OUTPUT_SAMPLE_RATE.");
					var p1 = env.GetStaticObjectField(c, f1);
					var v1 = env.CallObjectMethod(manager, m, new JniValue[] { new JniValue(p1) });
					if (v1 == IntPtr.Zero) m_defaultSampleRate = WaveFormat.Default.SampleRate;
					else {
						var pstr = env.GetStringChars(v1, out _);
						m_defaultSampleRate = uint.Parse(new string(pstr, 0, env.GetStringLength(v1)), CultureInfo.InvariantCulture);
					}

					var f2 = env.GetStaticFieldID(c, "PROPERTY_OUTPUT_FRAMES_PER_BUFFER", "Ljava/lang/String;");
					if (f2 == IntPtr.Zero) throw new InvalidOperationException("Could not find the static field PROPERTY_OUTPUT_FRAMES_PER_BUFFER.");
					var p2 = env.GetStaticObjectField(c, f2);
					var v2 = env.CallObjectMethod(manager, m, new JniValue[] { new JniValue(p2) });
					if (v2 == IntPtr.Zero) m_burstSize = 256;
					else {
						var pstr = env.GetStringChars(v2, out _);
						m_burstSize = int.Parse(new string(pstr, 0, env.GetStringLength(v2)), CultureInfo.InvariantCulture);
					}
				}
			}
		}

		/// <inheritdoc />
		public bool IsFormatSupported(WaveFormat format, out WaveFormat? suggestion, AudioShareMode shareMode = AudioShareMode.Shared) {
			switch (format.Channels) {
				case 1:
				case 2:
					break;
				default:
					format.Channels = 2;
					IsFormatSupported(format, out suggestion, shareMode);
					return false;
			}
			switch (format.SampleRate) {
				case 8000:
				case 11025:
				case 12000:
				case 16000:
				case 22050:
				case 24000:
				case 32000:
				case 44100:
				case 48000:
					break;
				default:
					if (format.SampleRate < 8000) format.SampleRate = 8000;
					else if (format.SampleRate < 11025) format.SampleRate = 11025;
					else if (format.SampleRate < 12000) format.SampleRate = 12000;
					else if (format.SampleRate < 16000) format.SampleRate = 16000;
					else if (format.SampleRate < 22050) format.SampleRate = 22050;
					else if (format.SampleRate < 24000) format.SampleRate = 24000;
					else if (format.SampleRate < 32000) format.SampleRate = 32000;
					else if (format.SampleRate < 44100) format.SampleRate = 44100;
					else format.SampleRate = 48000;
					IsFormatSupported(format, out suggestion, shareMode);
					return false;
			}
			switch (format.SampleFormat) {
				case SampleFormat.U8:
				case SampleFormat.S16:
					suggestion = format;
					return true;
				default:
					format.SampleFormat = SampleFormat.S16;
					suggestion = format;
					return false;
			}
		}

		/// <inheritdoc />
		public AudioClient Connect(WaveFormat format, int bufferSize = 0, AudioShareMode shareMode = AudioShareMode.Shared) => new OutputClient(_engine, this, format, bufferSize, shareMode);
	}
}
