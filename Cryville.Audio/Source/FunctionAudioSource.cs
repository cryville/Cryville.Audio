using Cryville.Common.Math;
using System;
using System.IO;
using UnsafeIL;

namespace Cryville.Audio.Source {
	/// <summary>
	/// An <see cref="AudioStream" /> that generates sound by a given function.
	/// </summary>
	public abstract class FunctionAudioSource : AudioStream {
		long _pos;
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
		SampleHandler _sampleHandler;

		/// <inheritdoc />
		protected override unsafe void OnSetFormat() {
			switch (Format.SampleFormat) {
				case SampleFormat.U8: _sampleHandler = WriteU8; break;
				case SampleFormat.S16: _sampleHandler = WriteS16; break;
				case SampleFormat.S24: _sampleHandler = WriteS24; break;
				case SampleFormat.S32: _sampleHandler = WriteS32; break;
				case SampleFormat.F32: _sampleHandler = WriteF32; break;
				case SampleFormat.F64: _sampleHandler = WriteF64; break;
				default: throw new NotSupportedException();
			}
		}

		/// <inheritdoc />
		public sealed override unsafe int Read(byte[] buffer, int offset, int count) {
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (buffer.Length - offset < count) throw new ArgumentException("The sum of offset and count is larger than the buffer length.");
			if (Disposed) throw new ObjectDisposedException(null);
			var len = Format.Align(count, true);
			var sampleCount = len / Format.FrameSize;
			fixed (byte* fptr = buffer) {
				byte* ptr = fptr;
				for (int i = 0; i < sampleCount; i++) {
					for (int j = 0; j < Format.Channels; j++) {
						float v = Func(_time, j);
						_sampleHandler(ref ptr, v);
					}
					_time += 1d / Format.SampleRate;
				}
			}
			_pos += len;
			return len;
		}

		static unsafe void WriteU8(ref byte* ptr, double v) {
			Unsafe.Write(ptr, ClampScale.ToByte(v));
			ptr += sizeof(byte);
		}

		static unsafe void WriteS16(ref byte* ptr, double v) {
			Unsafe.Write(ptr, ClampScale.ToInt16(v));
			ptr += sizeof(short);
		}

		static unsafe void WriteS24(ref byte* ptr, double v) {
			int d = ClampScale.ToInt24(v);
			*ptr++ = (byte)d;
			*ptr++ = (byte)(d >> 8);
			*ptr++ = (byte)(d >> 16);
		}

		static unsafe void WriteS32(ref byte* ptr, double v) {
			Unsafe.Write(ptr, ClampScale.ToInt32(v));
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
			long newPos;
			switch (origin) {
				case SeekOrigin.Begin: newPos = offset; break;
				case SeekOrigin.Current: newPos = Position + offset; break;
				case SeekOrigin.End: newPos = Length + offset; break;
				default: throw new ArgumentException("Invalid SeekOrigin.", nameof(origin));
			}
			if (newPos < 0) throw new ArgumentException("Seeking is attempted before the beginning of the stream.");
			_pos = newPos;
			_time = Time;
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
		public override long Position { get => _pos; set => Seek(0, SeekOrigin.Begin); }
		/// <inheritdoc />
		public override void Flush() { }
		/// <inheritdoc />
		public override void SetLength(long value) => throw new NotSupportedException();
		/// <inheritdoc />
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	}
}
