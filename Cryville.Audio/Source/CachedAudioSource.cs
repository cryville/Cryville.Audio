using System;
using System.IO;
using UnsafeIL;

namespace Cryville.Audio.Source {
	/// <summary>
	/// An <see cref="AudioStream" /> that caches data for reuse.
	/// </summary>
	/// <remarks>
	/// <para>This stream is not seekable. Use <see cref="Rewind" /> to reset its timestamp to zero.</para>
	/// </remarks>
	/// <param name="source">The <see cref="AudioStream" /> to be cached.</param>
	/// <param name="cacheFrameCount">The duration of the cache in frames.</param>
	/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
	public class CachedAudioSource(AudioStream source, long cacheFrameCount) : AudioStream((source ?? throw new ArgumentNullException(nameof(source))).Format) {
		/// <summary>
		/// Gets a clone of this <see cref="CachedAudioSource" /> with the timestamp reset.
		/// </summary>
		/// <returns>A clone of this <see cref="CachedAudioSource" /> with the timestamp reset.</returns>
		/// <remarks>
		/// Use with object pool is recommended.
		/// </remarks>
		public CachedAudioSource Clone() {
			var result = (CachedAudioSource)MemberwiseClone();
			result.Rewind();
			return result;
		}

		/// <summary>
		/// Resets the timestamp to reuse the instance.
		/// </summary>
		/// <remarks>
		/// Use with object pool is recommended.
		/// </remarks>
		public void Rewind() => FramePosition = 0;

		sealed class Cache(AudioStream source, long cacheFrameCount, WaveFormat format) {
			public readonly AudioStream Source = source;
			public long CacheFrameCount = cacheFrameCount;
			public long FramePosition;
			public byte[] CacheBuffer = new byte[cacheFrameCount * format.FrameSize];

			public void FillBufferTo(long frameOffset) {
				if (frameOffset <= FramePosition) return;
				_ = Source.ReadFramesGreedily(ref CacheBuffer[FramePosition * format.FrameSize], (int)(frameOffset - FramePosition));
				FramePosition = frameOffset;
			}
		}
		readonly Cache _cache = new(source, cacheFrameCount, source.Format);

		/// <inheritdoc />
		public override long FrameLength => _cache.CacheBuffer.Length / Format.FrameSize;

		/// <summary>
		/// Whether this audio stream has been disposed.
		/// </summary>
		public bool Disposed { get; private set; }

		/// <inheritdoc />
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			Disposed = true;
		}

		/// <inheritdoc />
		protected override int EnsureBufferSize(int targetBufferSize) {
			source.BufferSize = targetBufferSize;
			return source.BufferSize;
		}

		/// <inheritdoc />
		protected override unsafe int ReadFramesInternal(ref byte buffer, int frameCount) {
			if (Disposed) throw new ObjectDisposedException(null);
			long frameOffsetToLoad = Math.Min(_cache.CacheFrameCount, FramePosition + frameCount);
			_cache.FillBufferTo(frameOffsetToLoad);
			long rem = _cache.CacheFrameCount - FramePosition;
			int frames = (int)Math.Min(rem, frameCount);
			if (frames > 0) {
				fixed (byte* sptr = _cache.CacheBuffer, dptr = &buffer) {
					Unsafe.CopyBlock(dptr, sptr + Position, (uint)(frames * Format.FrameSize));
				}
			}
			SilentBuffer(Format, ref Unsafe.Add(ref buffer, frames * Format.FrameSize), frameCount - frames);
			return frames;
		}

		/// <inheritdoc />
		protected override long SeekFrameInternal(long frameOffset, SeekOrigin origin) => origin switch {
			SeekOrigin.Begin => frameOffset,
			SeekOrigin.Current => FramePosition + frameOffset,
			SeekOrigin.End => FrameLength + frameOffset,
			_ => throw new ArgumentException("Invalid SeekOrigin.", nameof(origin)),
		};

		/// <inheritdoc />
		public override bool CanRead => !Disposed;
		/// <inheritdoc />
		public override bool CanSeek => false;
		/// <inheritdoc />
		public override bool CanWrite => false;
		/// <inheritdoc />
		public override void Flush() { }
		/// <inheritdoc />
		public override void SetLength(long value) => throw new NotSupportedException();
		/// <inheritdoc />
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	}

	/// <summary>
	/// A builder that builds <see cref="CachedAudioSource" />.
	/// </summary>
	/// <param name="source">The <see cref="AudioStream" /> to be cached.</param>
	public class CachedAudioSourceBuilder(AudioStream source) : AudioStreamBuilder<CachedAudioSource> {
		/// <inheritdoc />
		public override WaveFormat DefaultFormat => source.Format;
		/// <summary>
		/// The duration of the cache in frames.
		/// </summary>
		public long CacheFrameCount { get; set; }

		/// <inheritdoc />
		public override CachedAudioSource Build(WaveFormat format) => new(source, CacheFrameCount);
	}
}
