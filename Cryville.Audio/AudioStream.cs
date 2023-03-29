using System;
using System.IO;

namespace Cryville.Audio {
	/// <summary>
	/// Audio stream.
	/// </summary>
	public abstract class AudioStream : Stream {
		/// <summary>
		/// The wave format.
		/// </summary>
		protected WaveFormat Format { get; private set; }

		/// <summary>
		/// The buffer size in bytes.
		/// </summary>
		protected int BufferSize { get; private set; }

		/// <summary>
		/// Whether if the stream has reached the end of data.
		/// </summary>
		public abstract bool EndOfData { get; }

		/// <summary>
		/// Sets the wave format and the buffer size of this audio stream.
		/// </summary>
		/// <param name="format">The wave format.</param>
		/// <param name="bufferSize">The buffer size in bytes.</param>
		/// <exception cref="NotSupportedException"><paramref name="format" /> is not supported by the audio stream.</exception>
		public void SetFormat(WaveFormat format, int bufferSize) {
			if (!IsFormatSupported(Format))
				throw new NotSupportedException("Format not supported");
			Format = format;
			BufferSize = bufferSize;
			OnSetFormat();
		}

		/// <summary>
		/// Called when the wave format and the buffer size is determined.
		/// </summary>
		protected abstract void OnSetFormat();

		/// <summary>
		/// Gets whether <paramref name="format" /> is supported by the audio stream.
		/// </summary>
		/// <param name="format">The wave format.</param>
		protected internal abstract bool IsFormatSupported(WaveFormat format);

		/// <summary>
		/// Sets the time within the current audio stream.
		/// </summary>
		/// <param name="offset">An offset in seconds relative to the <paramref name="origin" /> parameter.</param>
		/// <param name="origin">A value of type <see cref="SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
		/// <returns>The new time in seconds within the current audio stream.</returns>
		public double SeekToTime(double offset, SeekOrigin origin)
			=> Seek(Format.Align(offset * Format.BytesPerSecond), origin) / Format.BytesPerSecond;

		/// <summary>
		/// Sets the duration of the current audio stream.
		/// </summary>
		/// <param name="value">The desired duration of the current audio stream in seconds.</param>
		public void SetDuration(double value)
			=> SetLength(Format.Align(value * Format.BytesPerSecond));

		/// <summary>
		/// The duration in seconds of the audio stream.
		/// </summary>
		public double Duration => (double)Length / Format.BytesPerSecond;

		/// <summary>
		/// The time within the current audio stream.
		/// </summary>
		public double Time => (double)Position / Format.BytesPerSecond;

		/// <summary>
		/// Fills the buffer with silence.
		/// </summary>
		/// <param name="format">The wave format.</param>
		/// <param name="buffer">The buffer to be filled.</param>
		/// <param name="offset">The offset in bytes from the start of the <paramref name="buffer" /> to start filling.</param>
		/// <param name="count">The length in bytes to be filled.</param>
		public static unsafe void SilentBuffer(WaveFormat format, byte[] buffer, int offset, int count) {
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			fixed (byte* rptr = buffer) {
				switch (format.SampleFormat) {
					case SampleFormat.U8:
						for (int i = 0; i < count; i++) {
							*(rptr + i) = 0x80;
						}
						break;
					case SampleFormat.S16:
						var ptr16 = (short*)rptr;
						for (int i = 0; i < count / 2; i++) {
							*(ptr16 + i) = 0;
						}
						break;
					case SampleFormat.S24:
						for (int i = 0; i < count; i++) {
							*(rptr + i) = 0;
						}
						break;
					case SampleFormat.S32:
						var ptr32 = (int*)rptr;
						for (int i = 0; i < count / 4; i++) {
							*(ptr32 + i) = 0;
						}
						break;
					case SampleFormat.F32:
						var ptrf32 = (float*)rptr;
						for (int i = 0; i < count / 4; i++) {
							*(ptrf32 + i) = 0;
						}
						break;
					case SampleFormat.F64:
						var ptrf64 = (double*)rptr;
						for (int i = 0; i < count / 8; i++) {
							*(ptrf64 + i) = 0;
						}
						break;
					default:
						throw new NotSupportedException();
				}
			}
		}
	}
}
