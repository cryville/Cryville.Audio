using System;
using System.Runtime.InteropServices;

namespace Cryville.Audio.OpenSLES {
	internal sealed class SLItfWrapper<T> where T : struct {
		private readonly IntPtr _p;
		public IntPtr Ptr => _p;

		readonly T _o;
		public T Obj => _o;

		public SLItfWrapper(IntPtr p) {
			_p = p;
#if NET451_OR_GREATER || NETSTANDARD1_2_OR_GREATER || NETCOREAPP1_0_OR_GREATER
			IntPtr q = Marshal.PtrToStructure<IntPtr>(p);
			_o = Marshal.PtrToStructure<T>(q);
#else
			IntPtr q = (IntPtr)Marshal.PtrToStructure(p, typeof(IntPtr));
			_o = (T)Marshal.PtrToStructure(q, typeof(T));
#endif
		}

		public static implicit operator IntPtr(SLItfWrapper<T> w) => w._p;
		public static implicit operator T(SLItfWrapper<T> w) => w._o;
	}
}
