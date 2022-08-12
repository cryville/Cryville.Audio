namespace Cryville.Common.Math {
	public static class ClampScale {
		public static byte ToByte(double v) {
			v = v * 0x80 + 0x80;
			if (v >= byte.MaxValue) return byte.MaxValue;
			if (v < byte.MinValue) return byte.MinValue;
			return (byte)System.Math.Floor(v);
		}
		public static short ToInt16(double v) {
			v *= 0x8000;
			if (v >= short.MaxValue) return short.MaxValue;
			if (v < short.MinValue) return short.MinValue;
			return (short)System.Math.Floor(v);
		}
		public static int ToInt32(double v) {
			v *= 0x80000000;
			if (v >= int.MaxValue) return int.MaxValue;
			if (v < int.MinValue) return int.MinValue;
			return (int)System.Math.Floor(v);
		}
	}
}
