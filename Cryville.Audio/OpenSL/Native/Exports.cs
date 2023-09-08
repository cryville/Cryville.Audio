using System;
using System.Runtime.InteropServices;

namespace OpenSL.Native {
	internal enum SLresult : UInt32 {
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
		[DllImport("OpenSLES")]
		public static extern SLresult slCreateEngine(
			out IntPtr pEngine,
			UInt32 numOptions,
			[MarshalAs(UnmanagedType.LPArray)] SLEngineOption[] pEngineOptions,
			UInt32 numInterfaces,
			IntPtr pInterfaceIds,
			IntPtr pInterfaceRequired
		);

		[DllImport("OpenSLES")]
		public static extern SLresult slQueryNumSupportedEngineInterfaces(
            out UInt32 pNumSupportedInterfaces
        );

		[DllImport("OpenSLES")]
		public static extern SLresult slQuerySupportedEngineInterfaces(
            UInt32 index,
            ref Guid pInterfaceId
        );
    }
}
