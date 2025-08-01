using System;
using System.IO;

namespace Cryville.Audio.Source {
	/// <summary>
	/// An <see cref="AudioStream" /> that generates sound by a given function.
	/// </summary>
	public abstract class FunctionAudioSource(WaveFormat format) : AudioDoubleSampleStream(format) {
		double _time;

		/// <summary>
		/// The channel count of the output format.
		/// </summary>
		protected int Channels => Format.Channels;

		/// <inheritdoc />
		public override long FrameLength => long.MaxValue;

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
		protected sealed override unsafe int ReadFramesInternal(ref double buffer, int frameCount) {
			if (Disposed) throw new ObjectDisposedException(null);
			fixed (double* fptr = &buffer) {
				double* ptr = fptr;
				for (int i = 0; i < frameCount; i++) {
					for (int j = 0; j < Format.Channels; j++) {
						float v = Func(_time, j);
						*ptr++ = v;
					}
					_time += 1d / Format.SampleRate;
				}
			}
			return frameCount;
		}

		/// <summary>
		/// The function used to generate wave.
		/// </summary>
		/// <param name="time">The time position.</param>
		/// <param name="channel">The channel index.</param>
		protected abstract float Func(double time, int channel);

		/// <inheritdoc />
		protected override long SeekFrameInternal(long frameOffset, SeekOrigin origin) {
			if (Disposed) throw new ObjectDisposedException(null);
			var newPos = origin switch {
				SeekOrigin.Begin => frameOffset,
				SeekOrigin.Current => FramePosition + frameOffset,
				SeekOrigin.End => FrameLength + frameOffset,
				_ => throw new ArgumentException("Invalid SeekOrigin.", nameof(origin)),
			};
			if (newPos < 0) throw new ArgumentException("Seeking is attempted before the beginning of the stream.");
			_time = newPos / Format.SampleRate;
			return newPos;
		}

		/// <inheritdoc />
		public override bool CanRead => !Disposed;
		/// <inheritdoc />
		public override bool CanSeek => !Disposed;
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
	/// A builder that builds <see cref="FunctionAudioSource" />.
	/// </summary>
	public abstract class FunctionAudioSourceBuilder<T> : AudioStreamBuilder<T> where T : FunctionAudioSource {
		/// <inheritdoc />
		public override WaveFormat DefaultFormat => WaveFormat.Default;
		/// <inheritdoc />
		public sealed override bool IsFormatSupported(WaveFormat format) => true;
	}
}
