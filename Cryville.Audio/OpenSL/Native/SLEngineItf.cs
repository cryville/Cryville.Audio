using System;
using System.Runtime.InteropServices;

namespace OpenSL.Native {
	[Guid("8d97c260-ddd4-11db-8f95-0002a5d5c51b")]
	[StructLayout(LayoutKind.Sequential)]
	internal struct SLEngineItf {
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLEngineItf_CreateLEDDevice CreateLEDDevice;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLEngineItf_CreateVibraDevice CreateVibraDevice;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLEngineItf_CreateAudioPlayer CreateAudioPlayer;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLEngineItf_CreateAudioRecorder CreateAudioRecorder;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLEngineItf_CreateMidiPlayer CreateMidiPlayer;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLEngineItf_CreateListener CreateListener;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLEngineItf_Create3DGroup Create3DGroup;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLEngineItf_CreateOutputMix CreateOutputMix;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLEngineItf_CreateMetadataExtractor CreateMetadataExtractor;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLEngineItf_CreateExtensionObject CreateExtensionObject;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLEngineItf_QueryNumSupportedInterfaces QueryNumSupportedInterfaces;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLEngineItf_QuerySupportedInterfaces QuerySupportedInterfaces;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLEngineItf_QueryNumSupportedExtensions QueryNumSupportedExtensions;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLEngineItf_QuerySupportedExtension QuerySupportedExtension;
		[MarshalAs(UnmanagedType.FunctionPtr)] public SLEngineItf_IsExtensionSupported IsExtensionSupported;
	}
}
