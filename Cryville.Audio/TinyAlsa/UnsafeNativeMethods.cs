using System;
using System.Runtime.InteropServices;

namespace Cryville.Audio.TinyAlsa {
	internal class UnsafeNativeMethods {
		[DllImport("tinyalsa")]
		public static extern IntPtr pcm_params_get(uint card, uint device, uint flags);
		[DllImport("tinyalsa")]
		public static extern int pcm_params_to_string(IntPtr @params, [MarshalAs(UnmanagedType.LPArray)] byte[] @string, uint size);
	}
}
