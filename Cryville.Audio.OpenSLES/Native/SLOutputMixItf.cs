using System;
using System.Runtime.InteropServices;

namespace OpenSLES.Native {
	[Guid("97750f60-ddd7-11db-b192-0002a5d5c51b")]
	[StructLayout(LayoutKind.Sequential)]
	internal struct SLOutputMixItf {
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLOutputMixItf_GetDestinationOutputDeviceIDs GetDestinationOutputDeviceIDs;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLOutputMixItf_RegisterDeviceChangeCallback RegisterDeviceChangeCallback;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLOutputMixItf_ReRoute ReRoute;
	}
}
