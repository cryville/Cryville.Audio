using System;
using UnsafeIL;

namespace Cryville.Audio {
	/// <summary>
	/// Represents the method that reads a sample of a specific sample format as a <see cref="double" /> value.
	/// </summary>
	/// <param name="ptr">The pointer to the buffer to be read, advanced to the next sample when the method returns.</param>
	/// <returns>The sample value as a <see cref="double" /> value.</returns>
	public unsafe delegate double SampleReader(ref byte* ptr);
	/// <summary>
	/// Represents the method that writes a <see cref="double" /> sample value as a specific sample format.
	/// </summary>
	/// <param name="ptr">The pointer to the buffer to be written, advanced to the next sample when the method returns.</param>
	/// <param name="v">The <see cref="double" /> sample value to be written.</param>
	public unsafe delegate void SampleWriter(ref byte* ptr, double v);
	/// <summary>
	/// Provides a set of <see cref="SampleReader" /> and <see cref="SampleWriter" /> methods for various sample formats.
	/// </summary>
	public static unsafe class SampleConvert {
		/// <summary>
		/// Gets a <see cref="SampleReader" /> for a sample format.
		/// </summary>
		/// <param name="sampleFormat">The sample format.</param>
		/// <returns>A <see cref="SampleReader" /> for the specified sample format.</returns>
		/// <exception cref="NotSupportedException">The specified sample format is not supported.</exception>
		public static SampleReader GetSampleReader(SampleFormat sampleFormat) => sampleFormat switch {
			SampleFormat.U8 => ReadU8,
			SampleFormat.S16 => ReadS16,
			SampleFormat.S24 => BitConverter.IsLittleEndian ? ReadS24LE : ReadS24BE,
			SampleFormat.S32 => ReadS32,
			SampleFormat.F32 => ReadF32,
			SampleFormat.F64 => ReadF64,
			_ => throw new NotSupportedException()
		};
		static double ReadU8(ref byte* ptr) {
			return *ptr++ / (double)0x80 - 1;
		}
		static double ReadS16(ref byte* ptr) {
			double ret = Unsafe.Read<short>(ptr) / (double)0x8000;
			ptr += sizeof(short);
			return ret;
		}
		static double ReadS24LE(ref byte* ptr) {
			return (*ptr++ | *ptr++ << 8 | *ptr++ << 16) / (double)0x800000;
		}
		static double ReadS24BE(ref byte* ptr) {
			return (*ptr++ << 16 | *ptr++ << 8 | *ptr++) / (double)0x800000;
		}
		static double ReadS32(ref byte* ptr) {
			double ret = Unsafe.Read<int>(ptr) / (double)0x80000000;
			ptr += sizeof(int);
			return ret;
		}
		static double ReadF32(ref byte* ptr) {
			double ret = Unsafe.Read<float>(ptr);
			ptr += sizeof(float);
			return ret;
		}
		static double ReadF64(ref byte* ptr) {
			double ret = Unsafe.Read<double>(ptr);
			ptr += sizeof(double);
			return ret;
		}

		/// <summary>
		/// Gets a <see cref="SampleWriter" /> for a sample format.
		/// </summary>
		/// <param name="sampleFormat">The sample format.</param>
		/// <returns>A <see cref="SampleWriter" /> for the specified sample format.</returns>
		/// <exception cref="NotSupportedException">The specified sample format is not supported.</exception>
		public static SampleWriter GetSampleWriter(SampleFormat sampleFormat) => sampleFormat switch {
			SampleFormat.U8 => WriteU8,
			SampleFormat.S16 => WriteS16,
			SampleFormat.S24 => BitConverter.IsLittleEndian ? WriteS24LE : WriteS24BE,
			SampleFormat.S32 => WriteS32,
			SampleFormat.F32 => WriteF32,
			SampleFormat.F64 => WriteF64,
			_ => throw new NotSupportedException()
		};
		static void WriteU8(ref byte* ptr, double v) {
			Unsafe.Write(ptr, SampleClipping.ToU8(v));
			ptr += sizeof(byte);
		}
		static void WriteS16(ref byte* ptr, double v) {
			Unsafe.Write(ptr, SampleClipping.ToS16(v));
			ptr += sizeof(short);
		}
		static void WriteS24LE(ref byte* ptr, double v) {
			int d = SampleClipping.ToS24(v);
			*ptr++ = (byte)d;
			*ptr++ = (byte)(d >> 8);
			*ptr++ = (byte)(d >> 16);
		}
		static void WriteS24BE(ref byte* ptr, double v) {
			int d = SampleClipping.ToS24(v);
			*ptr++ = (byte)(d >> 16);
			*ptr++ = (byte)(d >> 8);
			*ptr++ = (byte)d;
		}
		static void WriteS32(ref byte* ptr, double v) {
			Unsafe.Write(ptr, SampleClipping.ToS32(v));
			ptr += sizeof(int);
		}
		static void WriteF32(ref byte* ptr, double v) {
			Unsafe.Write(ptr, (float)v);
			ptr += sizeof(float);
		}
		static void WriteF64(ref byte* ptr, double v) {
			Unsafe.Write(ptr, v);
			ptr += sizeof(double);
		}
	}
}
