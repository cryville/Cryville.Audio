using System;

namespace Cryville.Audio.Source {
	/// <summary>
	/// Audio source that provides wave data.
	/// </summary>
	public abstract class AudioSource : IDisposable {
		/// <inheritdoc />
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing">Whether the method is being called by user.</param>
		protected abstract void Dispose(bool disposing);

		/// <summary>
		/// Whether the audio source is muted.
		/// </summary>
		public bool Muted { get; set; }

		/// <summary>
		/// The output wave format.
		/// </summary>
		protected WaveFormat Format { get; private set; }

		/// <summary>
		/// The buffer size in bytes.
		/// </summary>
		protected int BufferSize { get; private set; }

		/// <summary>
		/// Whether if the source has reached the end of data.
		/// </summary>
		public abstract bool EndOfData { get; }

		internal void SetFormat(WaveFormat format, int bufferSize) {
			Format = format;
			BufferSize = bufferSize;
			OnSetFormat();
		}

		/// <summary>
		/// Called when the wave format is set and the buffer size is determined.
		/// </summary>
		protected virtual void OnSetFormat() {
			if (!IsFormatSupported(Format))
				throw new NotSupportedException("Format not supported");
		}

		/// <summary>
		/// Gets whether <paramref name="format" /> is supported by the audio source.
		/// </summary>
		/// <param name="format">The wave format.</param>
		protected internal abstract bool IsFormatSupported(WaveFormat format);

		/// <summary>
		/// Fills the buffer with wave data requested by <see cref="AudioClient" />.
		/// </summary>
		/// <param name="buffer">The buffer to be filled.</param>
		/// <param name="offset">The offset in bytes from the start of the <paramref name="buffer" /> to start filling.</param>
		/// <param name="length">The length in bytes to be filled.</param>
		/// <remarks>
		/// To optimize performance, the caller must ensure <paramref name="buffer" /> is not <see langword="null" /> and <paramref name="length" /> is not greater than the length of <paramref name="buffer" />.
		/// </remarks>
		protected internal abstract void FillBuffer(byte[] buffer, int offset, int length);

		/// <summary>
		/// Fills the buffer with silence.
		/// </summary>
		/// <param name="buffer">The buffer to be filled.</param>
		/// <param name="offset">The offset in bytes from the start of the <paramref name="buffer" /> to start filling.</param>
		/// <param name="length">The length in bytes to be filled.</param>
		/// <remarks>
		/// To optimize performance, the caller must ensure <paramref name="buffer" /> is not <see langword="null" /> and <paramref name="length" /> is not greater than the length of <paramref name="buffer" />.
		/// </remarks>
		protected void SilentBuffer(byte[] buffer, int offset, int length) {
			SilentBuffer(Format, buffer, offset, length);
		}

		/// <summary>
		/// Fills the buffer with silence.
		/// </summary>
		/// <param name="format">The wave format.</param>
		/// <param name="buffer">The buffer to be filled.</param>
		/// <param name="offset">The offset in bytes from the start of the <paramref name="buffer" /> to start filling.</param>
		/// <param name="length">The length in bytes to be filled.</param>
		/// <remarks>
		/// To optimize performance, the caller must ensure <paramref name="buffer" /> is not <see langword="null" /> and <paramref name="length" /> is not greater than the length of <paramref name="buffer" />.
		/// </remarks>
		public static unsafe void SilentBuffer(WaveFormat format, byte[] buffer, int offset, int length) {
			fixed (byte* rptr = buffer) {
				switch (format.SampleFormat) {
					case SampleFormat.Unsigned8:
					case SampleFormat.Signed24:
						for (int i = 0; i < length; i++) {
							*(rptr + i) = 0x80;
						}
						break;
					case SampleFormat.Signed16:
						var ptr16 = (short*)rptr;
						for (int i = 0; i < length / 2; i++) {
							*(ptr16 + i) = 0;
						}
						break;
					case SampleFormat.Signed32:
						var ptr32 = (int*)rptr;
						for (int i = 0; i < length / 4; i++) {
							*(ptr32 + i) = 0;
						}
						break;
					case SampleFormat.Binary32:
						var ptrf32 = (float*)rptr;
						for (int i = 0; i < length / 4; i++) {
							*(ptrf32 + i) = 0;
						}
						break;
					case SampleFormat.Binary64:
						var ptrf64 = (double*)rptr;
						for (int i = 0; i < length / 8; i++) {
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
