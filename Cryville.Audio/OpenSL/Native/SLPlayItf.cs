using System;
using System.Runtime.InteropServices;

namespace OpenSL.Native {
	internal enum SL_PLAYSTATE : UInt32 {
		STOPPED = 0x00000001,
		PAUSED  = 0x00000002,
		PLAYING = 0x00000003,
	}
	[Guid("ef0bd9c0-ddd7-11db-49bf-0002a5d5c51b")]
	[StructLayout(LayoutKind.Sequential)]
	internal struct SLPlayItf {
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLPlayItf_SetPlayState SetPlayState;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLPlayItf_GetPlayState GetPlayState;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLPlayItf_GetDuration GetDuration;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLPlayItf_GetPosition GetPosition;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLPlayItf_RegisterCallback RegisterCallback;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLPlayItf_SetCallbackEventsMask SetCallbackEventsMask;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLPlayItf_GetCallbackEventsMask GetCallbackEventsMask;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLPlayItf_SetMarkerPosition SetMarkerPosition;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLPlayItf_ClearMarkerPosition ClearMarkerPosition;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLPlayItf_GetMarkerPosition GetMarkerPosition;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLPlayItf_SetPositionUpdatePeriod SetPositionUpdatePeriod;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLPlayItf_GetPositionUpdatePeriod GetPositionUpdatePeriod;
	}
}
