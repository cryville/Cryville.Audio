using System.Runtime.InteropServices;

namespace Cryville.Audio.OpenSLES.Native {
	[Guid("89f6a7e0-beac-11df-5c8b-0002a5d5c51b")]
	[StructLayout(LayoutKind.Sequential)]
	struct SLAndroidConfigurationItf {
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLAndroidConfiguration_SetConfiguration SetConfiguration;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLAndroidConfiguration_GetConfiguration GetConfiguration;
	}
}
