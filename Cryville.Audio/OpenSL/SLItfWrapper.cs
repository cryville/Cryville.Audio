using System;
using System.Runtime.InteropServices;

namespace Cryville.Audio.OpenSL {
	public class SLItfWrapper<T> where T : struct {
		private readonly IntPtr _p;
		public IntPtr Ptr => _p;

		readonly T _o;
		public T Obj => _o;

		public SLItfWrapper(IntPtr p) {
			_p = p;
			IntPtr q = (IntPtr)Marshal.PtrToStructure(p, typeof(IntPtr));
			_o = (T)Marshal.PtrToStructure(q, typeof(T));
		}

		public static implicit operator IntPtr(SLItfWrapper<T> w) => w._p;
		public static implicit operator T(SLItfWrapper<T> w) => w._o;
	}
}
