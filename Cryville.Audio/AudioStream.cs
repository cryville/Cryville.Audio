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

		/// <inheritdoc />
		public sealed override long Seek(long offset, SeekOrigin origin) {
			return m_framePosition = SeekFrameInternal(offset / Format.FrameSize, origin);
		}

		/// <summary>
		/// Sets the time in frames within the current audio stream.
		/// </summary>
		/// <param name="frameOffset">An offset in frames relative to the <paramref name="origin" /> parameter.</param>
		/// <param name="origin">A value of type <see cref="SeekOrigin" /> indicating the reference point used to obtain the new time.</param>
		/// <returns>The new time in frames within the current audio stream.</returns>
		public double SeekFrame(long frameOffset, SeekOrigin origin) {
			m_framePosition = SeekFrameInternal(frameOffset, origin);
			return m_framePosition;
		}

		/// <summary>
		/// Sets the time in seconds within the current audio stream.
		/// </summary>
		/// <param name="timeOffset">An offset in seconds relative to the <paramref name="origin" /> parameter.</param>
		/// <param name="origin">A value of type <see cref="SeekOrigin" /> indicating the reference point used to obtain the new time.</param>
		/// <returns>The new time in seconds within the current audio stream.</returns>
		public double SeekTime(double timeOffset, SeekOrigin origin) {
			var time = SeekTimeInternal(timeOffset, origin);
			m_framePosition = (long)(time * Format.SampleRate);
			return time;
		}

		/// <summary>
		/// When overridden in a derived class, sets the time in seconds within the current audio stream.
		/// </summary>
		/// <param name="timeOffset">An offset in seconds relative to the <paramref name="origin" /> parameter.</param>
		/// <param name="origin">A value of type <see cref="SeekOrigin" /> indicating the reference point used to obtain the new time.</param>
		/// <returns>The new time in seconds within the current audio stream.</returns>
		protected virtual double SeekTimeInternal(double timeOffset, SeekOrigin origin) {
			return (double)SeekFrameInternal((long)(timeOffset * Format.SampleRate), origin) / Format.SampleRate;
		}

		/// <summary>
		/// When overridden in a derived class, sets the time in frames within the current audio stream.
		/// </summary>
		/// <param name="frameOffset">An offset in frames relative to the <paramref name="origin" /> parameter.</param>
		/// <param name="origin">A value of type <see cref="SeekOrigin" /> indicating the reference point used to obtain the new time.</param>
		/// <returns>The new time in frames within the current audio stream.</returns>
		protected abstract long SeekFrameInternal(long frameOffset, SeekOrigin origin);

		/// <inheritdoc />
		public sealed override long Length => FrameLength * Format.FrameSize;

		/// <summary>
		/// The length of the audio stream in frames.
		/// </summary>
		public abstract long FrameLength { get; }

		/// <summary>
		/// The length of the audio stream in seconds.
		/// </summary>
		public virtual double TimeLength => (double)FrameLength / Format.SampleRate;

		long m_framePosition;

		/// <inheritdoc />
		public sealed override long Position {
			get => m_framePosition * Format.FrameSize;
			set => Seek(value, SeekOrigin.Begin);
		}

		/// <summary>
		/// The time in frames within the current audio stream.
		/// </summary>
		public long FramePosition {
			get => m_framePosition;
			set => SeekFrame(value, SeekOrigin.Begin);
		}

		/// <summary>
		/// The time in seconds within the current audio stream.
		/// </summary>
		public virtual double TimePosition => (double)FramePosition / Format.SampleRate;

		void CheckParams(byte[] buffer, int offset, int count) {
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (buffer.Length - offset < count) throw new ArgumentException("The sum of offset and count is larger than the buffer length.");
			if (Format == default) throw new InvalidOperationException("Format not set.");
		}

		/// <inheritdoc />
		public sealed override int Read(byte[] buffer, int offset, int count) => ReadFrames(buffer, offset, count / Format.FrameSize) * Format.FrameSize;

		/// <summary>
		/// Reads a sequence of frames from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values started from <paramref name="offset" /> replaced by the frames read from the current audio source.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current audio stream.</param>
		/// <param name="frameCount">The maximum number of frames to be read from the current audio stream.</param>
		/// <returns>The total number of frames read into the buffer. This can be less than the number of frames requested if that many frames are not currently available, or zero (0) if <paramref name="frameCount" /> is 0 or the end of the stream has been reached.</returns>
		public int ReadFrames(byte[] buffer, int offset, int frameCount) {
			CheckParams(buffer, offset, frameCount * Format.FrameSize);
			frameCount = ReadFramesInternal(ref buffer[offset], frameCount);
			m_framePosition += frameCount;
			return frameCount;
		}

		/// <summary>
		/// Reads a sequence of frames from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">A reference to the buffer. When this method returns, the buffer contains the specified data replaced by the frames read from the current audio source.</param>
		/// <param name="frameCount">The maximum number of frames to be read from the current audio stream.</param>
		/// <returns>The total number of frames read into the buffer. This can be less than the number of frames requested if that many frames are not currently available, or zero (0) if <paramref name="frameCount" /> is 0 or the end of the stream has been reached.</returns>
		public int ReadFrames(ref byte buffer, int frameCount) {
			frameCount = ReadFramesInternal(ref buffer, frameCount);
			m_framePosition += frameCount;
			return frameCount;
		}

		/// <summary>
		/// When overridden in a derived class, reads a sequence of frames from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">A reference to the buffer. When this method returns, the buffer contains the specified data replaced by the frames read from the current audio source.</param>
		/// <param name="frameCount">The maximum number of frames to be read from the current audio stream.</param>
		/// <returns>The total number of frames read into the buffer. This can be less than the number of frames requested if that many frames are not currently available, or zero (0) if <paramref name="frameCount" /> is 0 or the end of the stream has been reached.</returns>
		protected abstract int ReadFramesInternal(ref byte buffer, int frameCount);

		/// <summary>
		/// Fills the buffer with silence.
		/// </summary>
		/// <param name="format">The wave format.</param>
		/// <param name="buffer">The buffer to be filled.</param>
		/// <param name="frameCount">The length in frames to be filled.</param>
		public static unsafe void SilentBuffer(WaveFormat format, ref byte buffer, int frameCount) {
			int sampleCount = frameCount * format.Channels;
			fixed (byte* rptr = &buffer) {
				switch (format.SampleFormat) {
					case SampleFormat.U8:
						for (int i = 0; i < sampleCount; i++) {
							*(rptr + i) = 0x80;
						}
						break;
					case SampleFormat.S16:
						var ptr16 = (short*)rptr;
						for (int i = 0; i < sampleCount; i++) {
							*(ptr16 + i) = 0;
						}
						break;
					case SampleFormat.S24:
						for (int i = 0; i < sampleCount * 3; i++) {
							*(rptr + i) = 0;
						}
						break;
					case SampleFormat.S32:
						var ptr32 = (int*)rptr;
						for (int i = 0; i < sampleCount; i++) {
							*(ptr32 + i) = 0;
						}
						break;
					case SampleFormat.F32:
						var ptrf32 = (float*)rptr;
						for (int i = 0; i < sampleCount; i++) {
							*(ptrf32 + i) = 0;
						}
						break;
					case SampleFormat.F64:
						var ptrf64 = (double*)rptr;
						for (int i = 0; i < sampleCount; i++) {
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
