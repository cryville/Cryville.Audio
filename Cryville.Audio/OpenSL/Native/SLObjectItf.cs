using System;
using System.Runtime.InteropServices;

namespace OpenSL.Native {
	[Guid("79216360-ddd7-11db-16ac-0002a5d5c51b")]
	[StructLayout(LayoutKind.Sequential)]
	public struct SLObjectItf {
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLObjectItf_Realize Realize;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLObjectItf_Resume Resume;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLObjectItf_GetState GetState;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLObjectItf_GetInterface GetInterface;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLObjectItf_RegisterCallback RegisterCallback;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLObjectItf_AbortAsyncOperation AbortAsyncOperation;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLObjectItf_Destroy Destroy;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLObjectItf_SetPriority SetPriority;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLObjectItf_GetPriority GetPriority;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLObjectItf_SetLossOfControlInterfaces SetLossOfControlInterfaces;
	}
}