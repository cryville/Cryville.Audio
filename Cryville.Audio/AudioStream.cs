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

		long m_position;
		/// <inheritdoc />
		public override long Position {
			get => m_position;
			set => m_position = Seek(value, SeekOrigin.Begin);
		}

		/// <summary>
		/// The time in seconds within the current audio stream.
		/// </summary>
		public virtual double Time => (double)Position / Format.BytesPerSecond;

		void CheckParams(byte[] buffer, int offset, int count) {
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (buffer.Length - offset < count) throw new ArgumentException("The sum of offset and count is larger than the buffer length.");
			if (Format == default) throw new InvalidOperationException("Format not set.");
		}

		/// <inheritdoc />
		public override sealed int Read(byte[] buffer, int offset, int count) {
			CheckParams(buffer, offset, count);
			count = (int)Format.Align(count, true);
			count = ReadInternal(buffer, offset, count);
			m_position += count;
			return count;
		}

		/// <summary>
		/// Reads a sequence of frames from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values started from <paramref name="offset" /> replaced by the frames read from the current audio source.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current audio stream.</param>
		/// <param name="frameCount">The maximum number of frames to be read from the current audio stream.</param>
		/// <returns>The total number of frames read into the buffer. This can be less than the number of frames requested if that many frames are not currently available, or zero (0) if <paramref name="frameCount" /> is 0 or the end of the stream has been reached.</returns>
		public int ReadFrames(byte[] buffer, int offset, int frameCount) {
			CheckParams(buffer, offset, frameCount * Format.FrameSize);
			frameCount = ReadFramesInternal(buffer, offset, frameCount);
			m_position += frameCount * Format.FrameSize;
			return frameCount;
		}

		/// <summary>
		/// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values started from <paramref name="offset" /> replaced by the bytes read from the current audio source.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current audio stream.</param>
		/// <param name="count">The maximum number of bytes to be read from the current audio stream.</param>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if <paramref name="count" /> is 0 or the end of the stream has been reached.</returns>
		protected virtual int ReadInternal(byte[] buffer, int offset, int count) {
			count = (int)Format.Align(count, true);
			count = ReadFramesInternal(buffer, offset, count / Format.FrameSize) * Format.FrameSize;
			m_position += count;
			return count;
		}

		/// <summary>
		/// When overridden in a derived class, reads a sequence of frames from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values started from <paramref name="offset" /> replaced by the frames read from the current audio source.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current audio stream.</param>
		/// <param name="frameCount">The maximum number of frames to be read from the current audio stream.</param>
		/// <returns>The total number of frames read into the buffer. This can be less than the number of frames requested if that many frames are not currently available, or zero (0) if <paramref name="frameCount" /> is 0 or the end of the stream has been reached.</returns>
		protected abstract int ReadFramesInternal(byte[] buffer, int offset, int frameCount);

		/// <summary>
		/// Fills the buffer with silence.
		/// </summary>
		/// <param name="format">The wave format.</param>
		/// <param name="buffer">The buffer to be filled.</param>
		/// <param name="offset">The offset in bytes from the start of the <paramref name="buffer" /> to start filling.</param>
		/// <param name="count">The length in bytes to be filled.</param>
		public static unsafe int SilentBuffer(WaveFormat format, byte[] buffer, int offset, int count) {
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			count = (int)format.Align(count, true);
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
