using Cryville.Interop.Java;
using Cryville.Interop.Java.Helper;
using System;

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

		/// <inheritdoc />
		public float DefaultBufferDuration => 20;

		/// <inheritdoc />
		public float MinimumBufferDuration => 0;

		uint _defaultSampleRate;
		/// <inheritdoc />
		public unsafe WaveFormat DefaultFormat {
			get {
				if (JavaVMManager.CurrentVM == null) return WaveFormat.Default;
				if (_defaultSampleRate == 0) {
					var env = JavaVMManager.CurrentEnv;
					using (var frame = new JniLocalFrame(env, 4)) {
						var manager = AndroidHelper.GetSystemService(env, AndroidHelper.GetCurrentApplication(env), "AUDIO_SERVICE");
						var c = env.GetObjectClass(manager);
						if (c == IntPtr.Zero) throw new InvalidOperationException("Could not get the AudioManager class.");
						var f = env.GetStaticFieldID(c, "PROPERTY_OUTPUT_SAMPLE_RATE", "Ljava/lang/String;");
						if (f == IntPtr.Zero) throw new InvalidOperationException("Could not find the static field PROPERTY_OUTPUT_SAMPLE_RATE.");
						var p = env.GetStaticObjectField(c, f);
						var m = env.GetMethodID(c, "getProperty", "(Ljava/lang/String;)Ljava/lang/String;");
						if (m == IntPtr.Zero) throw new InvalidOperationException("Could not find the method getProperty(String).");
						var v = env.CallObjectMethod(manager, m, new JniValue[] { new JniValue(p) });
						if (v == IntPtr.Zero) _defaultSampleRate = WaveFormat.Default.SampleRate;
						else {
							var pstr = env.GetStringChars(v, out _);
							_defaultSampleRate = uint.Parse(new string(pstr, 0, env.GetStringLength(v)));
						}
					}
				}
				var ret = WaveFormat.Default;
				ret.SampleRate = _defaultSampleRate;
				return ret;
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
		public AudioClient Connect(WaveFormat format, float bufferDuration = 0, AudioShareMode shareMode = AudioShareMode.Shared) => new OutputClient(_engine, this, format, bufferDuration, shareMode);
	}
}
