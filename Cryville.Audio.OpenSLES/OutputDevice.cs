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

		/// <inheritdoc />
		public WaveFormat DefaultFormat => new WaveFormat {
			Channels = 2,
			SampleRate = 48000,
			SampleFormat = SampleFormat.S16,
		};

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
