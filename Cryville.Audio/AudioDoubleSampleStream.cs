using System;

namespace Cryville.Audio {
	/// <summary>
	/// An audio stream that outputs samples of double-precision floating-point sample format.
	/// </summary>
	/// <param name="format">The wave format.</param>
	/// <exception cref="NotSupportedException">When overridden, <paramref name="format" /> is not supported.</exception>
	public abstract class AudioDoubleSampleStream(WaveFormat format) : AudioStream(format) {
		readonly SampleWriter _sampleWriter = SampleConvert.GetSampleWriter(format.SampleFormat);
		double[] _buffer = [];

		/// <summary>
		/// Ensures the minimum size of the buffer.
		/// </summary>
		/// <param name="targetBufferSize">The requested minimum buffer size in frames.</param>
		/// <remarks>
		/// <para>When overridden, the base method must be called so that the internal buffer of the <see cref="AudioDoubleSampleStream" /> class is checked.</para>
		/// </remarks>
		protected override int EnsureBufferSize(int targetBufferSize) {
			int newBufferSize = AudioStreamHelpers.GetNewBufferSize(targetBufferSize, BufferSize);
			if (newBufferSize <= BufferSize) return BufferSize;
			_buffer = new double[newBufferSize * Format.Channels];
			return newBufferSize;
		}

		/// <inheritdoc />
		public int ReadFrames(ref double buffer, int frameCount) {
			EnsureBufferSize(frameCount);
			frameCount = ReadFramesInternal(ref buffer, frameCount);
			m_framePosition += frameCount;
			return frameCount;
		}

		/// <summary>
		/// When overridden in a derived class, reads a sequence of frames from the current stream.
		/// </summary>
		/// <param name="buffer">A reference to the buffer. When this method returns, the buffer contains the specified data replaced by the frames read from the current audio source.</param>
		/// <param name="frameCount">The maximum number of frames to be read from the current audio stream.</param>
		/// <returns>The total number of frames read into the buffer. This can be less than the number of frames requested if that many frames are not currently available, or zero (0) if <paramref name="frameCount" /> is 0 or the end of the stream has been reached.</returns>
		protected abstract int ReadFramesInternal(ref double buffer, int frameCount);

		/// <inheritdoc />
		protected override unsafe int ReadFramesInternal(ref byte buffer, int frameCount) {
			frameCount = ReadFramesInternal(ref _buffer[0], frameCount);
			int sampleCount = frameCount * Format.Channels;
			fixed (byte* rptr = &buffer) {
				byte* ptr = rptr;
				for (int i = 0; i < sampleCount; i++) {
					_sampleWriter(ref ptr, _buffer[i]);
				}
			}
			return frameCount;
		}
	}
}
