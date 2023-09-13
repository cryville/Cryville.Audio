using System;
using System.Runtime.InteropServices;

namespace OpenSLES.Native {
	[Guid("2bc99cc0-ddd4-11db-998d-0002a5d5c51b")]
	[StructLayout(LayoutKind.Sequential)]
	internal struct SLBufferQueueItf {
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLBufferQueueItf_Enqueue Enqueue;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLBufferQueueItf_Clear Clear;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLBufferQueueItf_GetState GetState;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLBufferQueueItf_RegisterCallback RegisterCallback;
	}
}
