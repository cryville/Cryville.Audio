using System;
using UnsafeIL;

namespace Cryville.Audio {
	/// <summary>
	/// Provides a set of <see langword="static" /> extension helper methods related to <see cref="AudioStream" />.
	/// </summary>
	public static class AudioStreamExtensions {
		/// <summary>
		/// Reads a sequence of frames from the current stream greedily and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="stream">The audio stream.</param>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values started from <paramref name="offset" /> replaced by the frames read from the current audio source.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current audio stream.</param>
		/// <param name="frameCount">The maximum number of frames to be read from the current audio stream.</param>
		/// <returns>The total number of frames read into the buffer. This can be less than the number of frames requested if that many frames are not currently available, or zero (0) if <paramref name="frameCount" /> is 0 or the end of the stream has been reached.</returns>
		public static int ReadFramesGreedily(this AudioStream stream, byte[] buffer, int offset, int frameCount) {
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			int readCount = 0;
			for (; ; ) {
				int count = stream.ReadFrames(buffer, offset, frameCount);
				if (count == 0) break;
				offset += count * stream.Format.FrameSize;
				frameCount -= count;
				readCount += count;
				if (frameCount == 0) break;
			}
			return readCount;
		}

		/// <summary>
		/// Reads a sequence of frames from the current stream greedily and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="stream">The audio stream.</param>
		/// <param name="buffer">A reference to the buffer. When this method returns, the buffer contains the specified data replaced by the frames read from the current audio source.</param>
		/// <param name="frameCount">The maximum number of frames to be read from the current audio stream.</param>
		/// <returns>The total number of frames read into the buffer. This can be less than the number of frames requested if that many frames are not currently available, or zero (0) if <paramref name="frameCount" /> is 0 or the end of the stream has been reached.</returns>
		public static int ReadFramesGreedily(this AudioStream stream, ref byte buffer, int frameCount) {
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			int readCount = 0;
			for (; ; ) {
				int count = stream.ReadFrames(ref buffer, frameCount);
				if (count == 0) break;
				buffer = ref Unsafe.Add(ref buffer, count * stream.Format.FrameSize);
				frameCount -= count;
				readCount += count;
				if (frameCount == 0) break;
			}
			return readCount;
		}

		/// <summary>
		/// Reads a sequence of frames from the current stream greedily and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="stream">The audio stream.</param>
		/// <param name="buffer">A reference to the buffer. When this method returns, the buffer contains the specified data replaced by the frames read from the current audio source.</param>
		/// <param name="frameCount">The maximum number of frames to be read from the current audio stream.</param>
		/// <returns>The total number of frames read into the buffer. This can be less than the number of frames requested if that many frames are not currently available, or zero (0) if <paramref name="frameCount" /> is 0 or the end of the stream has been reached.</returns>
		public static int ReadFramesGreedily(this AudioDoubleSampleStream stream, ref double buffer, int frameCount) {
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			int readCount = 0;
			for (; ; ) {
				int count = stream.ReadFrames(ref buffer, frameCount);
				if (count == 0) break;
				buffer = ref Unsafe.Add(ref buffer, count * stream.Format.Channels);
				frameCount -= count;
				readCount += count;
				if (frameCount == 0) break;
			}
			return readCount;
		}
	}
}
