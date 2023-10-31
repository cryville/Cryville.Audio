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
		/// The buffer size in frames.
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
		/// <param name="bufferSize">The buffer size in frames.</param>
		/// <exception cref="InvalidOperationException">This method has already been called successfully once on the audio stream.</exception>
		/// <exception cref="NotSupportedException"><paramref name="format" /> is not supported by the audio stream.</exception>
		public void SetFormat(WaveFormat format, int bufferSize) {
			if (!IsFormatSupported(format))
				throw new NotSupportedException("Format not supported.");
			if (format == Format && bufferSize == BufferSize) return;
			if (Format != default || BufferSize != 0)
				throw new InvalidOperationException("Format already set.");
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
		/// <param name="origin">A value of type <see cref="SeekOrigin" /> indicating the reference point used to obtain the new time.</param>
		/// <returns>The new time in seconds within the current audio stream.</returns>
		public virtual double SeekTime(double offset, SeekOrigin origin) {
			Seek(Format.Align(offset * Format.BytesPerSecond), origin);
			return Time;
		}

		/// <summary>
		/// Sets the duration of the current audio stream.
		/// </summary>
		/// <param name="value">The duration in seconds.</param>
		public virtual void SetDuration(double value)
			=> SetLength(Format.Align(value * Format.BytesPerSecond));

		/// <summary>
		/// The duration in seconds of the audio stream.
		/// </summary>
		public virtual double Duration => (double)Length / Format.BytesPerSecond;

		/// <summary>
		/// The time in seconds within the current audio stream.
		/// </summary>
		public virtual double Time => (double)Position / Format.BytesPerSecond;

		/// <summary>
		/// Fills the buffer with silence.
		/// </summary>
		/// <param name="format">The wave format.</param>
		/// <param name="buffer">The buffer to be filled.</param>
		/// <param name="offset">The offset in bytes from the start of the <paramref name="buffer" /> to start filling.</param>
		/// <param name="count">The length in bytes to be filled.</param>
		public static unsafe int SilentBuffer(WaveFormat format, byte[] buffer, int offset, int count) {
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			count = format.Align(count, true);
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
			return count;
		}
	}
}
