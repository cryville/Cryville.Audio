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
	/// <param name="duration">The duration of the cache in seconds.</param>
	public class CachedAudioSource(AudioStream source, double duration) : AudioStream {
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

		sealed class Cache(AudioStream source, double duration) {
			public readonly AudioStream Source = source;
			public readonly double Duration = duration;
			WaveFormat _format;
			public long FrameLength = -1;
			public int FramePosition;
			public byte[]? Buffer;

			public void SetFormat(WaveFormat format, int bufferSize) {
				_format = format;
				Source.SetFormat(format, bufferSize);
				FrameLength = (long)(Duration * format.SampleRate);
				Buffer = new byte[FrameLength * format.FrameSize];
			}
			public void FillBufferTo(int frameOffset) {
				_ = Source.ReadFrames(ref Buffer![FramePosition * _format.FrameSize], frameOffset - FramePosition);
				FramePosition = frameOffset;
			}
		}
		readonly Cache _cache = new(source, duration);

		/// <inheritdoc />
		public override bool EndOfData => Position >= (_cache.Buffer ?? throw new InvalidOperationException("Format not set.")).Length;

		/// <inheritdoc />
		public override long FrameLength => (_cache.Buffer ?? throw new InvalidOperationException("Format not set.")).Length / Format.FrameSize;

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
		protected internal override bool IsFormatSupported(WaveFormat format) {
			return _cache.Source.IsFormatSupported(format);
		}

		/// <inheritdoc />
		protected override void OnSetFormat() {
			if (_cache.Buffer != null) return;
			_cache.SetFormat(Format, BufferSize);
		}

		/// <inheritdoc />
		protected override unsafe int ReadFramesInternal(ref byte buffer, int frameCount) {
			if (Disposed) throw new ObjectDisposedException(null);
			int frameOffsetToLoad = (int)Math.Min(_cache.Buffer!.Length, FramePosition + frameCount);
			if (frameOffsetToLoad > _cache.FramePosition) _cache.FillBufferTo(frameOffsetToLoad);
			long rem = _cache.FrameLength - FramePosition;
			int frames = (int)Math.Min(rem, frameCount);
			if (frames > 0) {
				fixed (byte* sptr = _cache.Buffer, dptr = &buffer) {
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
}
