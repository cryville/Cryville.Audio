using System;
using System.Runtime.InteropServices;

namespace Cryville.Audio.OpenSLES.Native {
	internal enum SLResult : uint {
		SUCCESS                  = 0x00000000,
		PRECONDITIONS_VIOLATED   = 0x00000001,
		PARAMETER_INVALID        = 0x00000002,
		MEMORY_FAILURE           = 0x00000003,
		RESOURCE_ERROR           = 0x00000004,
		RESOURCE_LOST            = 0x00000005,
		IO_ERROR                 = 0x00000006,
		BUFFER_INSUFFICIENT      = 0x00000007,
		CONTENT_CORRUPTED        = 0x00000008,
		CONTENT_UNSUPPORTED      = 0x00000009,
		CONTENT_NOT_FOUND        = 0x0000000A,
		PERMISSION_DENIED        = 0x0000000B,
		FEATURE_UNSUPPORTED      = 0x0000000C,
		INTERNAL_ERROR           = 0x0000000D,
		UNKNOWN_ERROR            = 0x0000000E,
		OPERATION_ABORTED        = 0x0000000F,
		CONTROL_LOST             = 0x00000010,
		READONLY                 = 0x00000011,
		ENGINEOPTION_UNSUPPORTED = 0x00000012,
		SOURCE_SINK_INCOMPATIBLE = 0x00000013,
	}

	internal static class Exports {
#if USE_SAFE_DLL_IMPORT
		[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
#endif
		[DllImport("OpenSLES")]
		public static extern SLResult slCreateEngine(
			out IntPtr pEngine,
			UInt32 numOptions,
			[MarshalAs(UnmanagedType.LPArray)] SLEngineOption[]? pEngineOptions,
			UInt32 numInterfaces,
			IntPtr pInterfaceIds,
			IntPtr pInterfaceRequired
		);

#if USE_SAFE_DLL_IMPORT
		[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
#endif
		[DllImport("OpenSLES")]
		public static extern SLResult slQueryNumSupportedEngineInterfaces(
			out UInt32 pNumSupportedInterfaces
		);

#if USE_SAFE_DLL_IMPORT
		[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
#endif
		[DllImport("OpenSLES")]
		public static extern SLResult slQuerySupportedEngineInterfaces(
			UInt32 index,
			ref Guid pInterfaceId
		);
	}

	enum SL_ANDROID_STREAM : UInt32 {
		VOICE        = 0,
		SYSTEM       = 1,
		RING         = 2,
		MEDIA        = 3,
		ALARM        = 4,
		NOTIFICATION = 5,
	}
}
