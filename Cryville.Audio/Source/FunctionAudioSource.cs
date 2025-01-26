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

		unsafe delegate void SampleHandler(ref byte* ptr, double v);
		SampleHandler? _sampleHandler;

		/// <inheritdoc />
		protected override unsafe void OnSetFormat() => _sampleHandler = Format.SampleFormat switch {
			SampleFormat.U8 => WriteU8,
			SampleFormat.S16 => WriteS16,
			SampleFormat.S24 => WriteS24,
			SampleFormat.S32 => WriteS32,
			SampleFormat.F32 => WriteF32,
			SampleFormat.F64 => WriteF64,
			_ => throw new NotSupportedException(),
		};

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

		static unsafe void WriteU8(ref byte* ptr, double v) {
			Unsafe.Write(ptr, SampleClipping.ToByte(v));
			ptr += sizeof(byte);
		}

		static unsafe void WriteS16(ref byte* ptr, double v) {
			Unsafe.Write(ptr, SampleClipping.ToInt16(v));
			ptr += sizeof(short);
		}

		static unsafe void WriteS24(ref byte* ptr, double v) {
			int d = SampleClipping.ToInt24(v);
			*ptr++ = (byte)d;
			*ptr++ = (byte)(d >> 8);
			*ptr++ = (byte)(d >> 16);
		}

		static unsafe void WriteS32(ref byte* ptr, double v) {
			Unsafe.Write(ptr, SampleClipping.ToInt32(v));
			ptr += sizeof(int);
		}

		static unsafe void WriteF32(ref byte* ptr, double v) {
			Unsafe.Write(ptr, (float)v);
			ptr += sizeof(float);
		}

		static unsafe void WriteF64(ref byte* ptr, double v) {
			Unsafe.Write(ptr, v);
			ptr += sizeof(double);
		}

		/// <summary>
		/// The function used to generate wave.
		/// </summary>
		/// <param name="time">The time position.</param>
		/// <param name="channel">The channel index.</param>
		protected abstract float Func(double time, int channel);

		/// <inheritdoc />
		public override long Seek(long offset, SeekOrigin origin) {
			if (Disposed) throw new ObjectDisposedException(null);
			var newPos = origin switch {
				SeekOrigin.Begin => offset,
				SeekOrigin.Current => Position + offset,
				SeekOrigin.End => Length + offset,
				_ => throw new ArgumentException("Invalid SeekOrigin.", nameof(origin)),
			};
			if (newPos < 0) throw new ArgumentException("Seeking is attempted before the beginning of the stream.");
			_time = newPos / Format.BytesPerSecond;
			return newPos;
		}

		/// <inheritdoc />
		public override bool CanRead => !Disposed;
		/// <inheritdoc />
		public override bool CanSeek => !Disposed;
		/// <inheritdoc />
		public override bool CanWrite => false;
		/// <inheritdoc />
		public override long Length => long.MaxValue;
		/// <inheritdoc />
		public override void Flush() { }
		/// <inheritdoc />
		public override void SetLength(long value) => throw new NotSupportedException();
		/// <inheritdoc />
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	}
}
