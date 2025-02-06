using System;
using System.IO;
using UnsafeIL;

namespace Cryville.Audio.Source {
	/// <summary>
	/// An <see cref="AudioStream" /> that generates sound by a given function.
	/// </summary>
	public abstract class FunctionAudioSource : AudioStream {
		double _time;

		/// <summary>
		/// The channel count of the output format.
		/// </summary>
		protected int Channels => Format.Channels;

		/// <inheritdoc />
		public override bool EndOfData => false;

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
		protected internal sealed override bool IsFormatSupported(WaveFormat format) {
			return format.SampleFormat == SampleFormat.U8
				|| format.SampleFormat == SampleFormat.S16
				|| format.SampleFormat == SampleFormat.S24
				|| format.SampleFormat == SampleFormat.S32
				|| format.SampleFormat == SampleFormat.F32
				|| format.SampleFormat == SampleFormat.F64;
		}

		SampleWriter? _sampleHandler;

		/// <inheritdoc />
		protected override unsafe void OnSetFormat() => _sampleHandler = SampleConvert.GetSampleWriter(Format.SampleFormat);

		/// <inheritdoc />
		protected sealed override unsafe int ReadFramesInternal(ref byte buffer, int frameCount) {
			if (Disposed) throw new ObjectDisposedException(null);
			fixed (byte* fptr = &buffer) {
				byte* ptr = fptr;
				for (int i = 0; i < frameCount; i++) {
					for (int j = 0; j < Format.Channels; j++) {
						float v = Func(_time, j);
						_sampleHandler!(ref ptr, v);
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
}
