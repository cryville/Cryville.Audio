using Cryville.Common.Compat;
using System;
using System.Runtime.InteropServices;

namespace Cryville.Audio.OpenSLES.Native {
	#region SLBufferQueueItf
	internal delegate void slBufferQueueCallback(
		IntPtr caller,
		IntPtr pContext
	);
	internal delegate SLResult SLBufferQueueItf_Enqueue(
		IntPtr self,
		IntPtr pBuffer,
		UInt32 size
	);
	internal delegate SLResult SLBufferQueueItf_Clear(
		IntPtr self
	);
	internal delegate SLResult SLBufferQueueItf_GetState(
		IntPtr self,
		out SLBufferQueueState pState
	);
	internal delegate SLResult SLBufferQueueItf_RegisterCallback(
		IntPtr self,
		slBufferQueueCallback callback,
		IntPtr pContext
	);
	#endregion
	#region SLEngineItf
	internal delegate SLResult SLEngineItf_CreateLEDDevice(
		IntPtr self,
		out IntPtr pDevice,
		UInt32 deviceID,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] IntPtr[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool, SizeParamIndex = 3)] bool[] pInterfaceRequired
	);
	internal delegate SLResult SLEngineItf_CreateVibraDevice(
		IntPtr self,
		out IntPtr pDevice,
		UInt32 deviceID,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] IntPtr[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool, SizeParamIndex = 3)] bool[] pInterfaceRequired
	);
	internal delegate SLResult SLEngineItf_CreateAudioPlayer(
		IntPtr self,
		out IntPtr pPlayer,
		ref SLDataSource pAudioSrc,
		ref SLDataSink pAudioSnk,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] IntPtr[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool, SizeParamIndex = 4)] bool[] pInterfaceRequired
	);
	internal delegate SLResult SLEngineItf_CreateAudioRecorder(
		IntPtr self,
		out IntPtr pRecorder,
		ref SLDataSource pAudioSrc,
		ref SLDataSink pAudioSnk,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] IntPtr[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool, SizeParamIndex = 4)] bool[] pInterfaceRequired
	);
	internal delegate SLResult SLEngineItf_CreateMidiPlayer(
		IntPtr self,
		out IntPtr pPlayer,
		ref SLDataSource pMIDISrc,
		ref SLDataSource pBankSrc,
		ref SLDataSink pAudioOutput,
		ref SLDataSink pVibra,
		ref SLDataSink pLEDArray,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 7)] IntPtr[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool, SizeParamIndex = 7)] bool[] pInterfaceRequired
	);
	internal delegate SLResult SLEngineItf_CreateListener(
		IntPtr self,
		out IntPtr pListener,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] IntPtr[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool, SizeParamIndex = 2)] bool[] pInterfaceRequired
	);
	internal delegate SLResult SLEngineItf_Create3DGroup(
		IntPtr self,
		out IntPtr pGroup,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] IntPtr[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool, SizeParamIndex = 2)] bool[] pInterfaceRequired
	);
	internal delegate SLResult SLEngineItf_CreateOutputMix(
		IntPtr self,
		out IntPtr pMix,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] IntPtr[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool, SizeParamIndex = 2)] bool[] pInterfaceRequired
	);
	internal delegate SLResult SLEngineItf_CreateMetadataExtractor(
		IntPtr self,
		out IntPtr pMetadataExtractor,
		ref SLDataSource pDataSource,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] IntPtr[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool, SizeParamIndex = 3)] bool[] pInterfaceRequired
	);
	internal delegate SLResult SLEngineItf_CreateExtensionObject(
		IntPtr self,
		out IntPtr pObject,
		IntPtr pParameters,
		UInt32 objectID,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] IntPtr[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool, SizeParamIndex = 4)] bool[] pInterfaceRequired
	);
	internal delegate SLResult SLEngineItf_QueryNumSupportedInterfaces(
		IntPtr self,
		UInt32 objectID,
		out UInt32 pNumSupportedInterfaces
	);
	internal delegate SLResult SLEngineItf_QuerySupportedInterfaces(
		IntPtr self,
		UInt32 objectID,
		UInt32 index,
		out IntPtr pInterfaceId
	);
	internal delegate SLResult SLEngineItf_QueryNumSupportedExtensions(
		IntPtr self,
		out UInt32 pNumExtensions
	);
	internal delegate SLResult SLEngineItf_QuerySupportedExtension(
		IntPtr self,
		UInt32 index,
		[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUTF8StrMarshaler))] string pExtensionName,
		ref UInt16 pNameLength
	);
	internal delegate SLResult SLEngineItf_IsExtensionSupported(
		IntPtr self,
		[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUTF8StrMarshaler))] string pExtensionName,
		[MarshalAs(UnmanagedType.Bool)] out bool pSupported
	);
	#endregion
	#region SLObjectItf
	internal delegate void slObjectCallback(
		IntPtr caller,
		IntPtr pContext,
		UInt32 @event,
		SLResult result,
		UInt32 param,
		IntPtr pInterface
	);
	internal delegate SLResult SLObjectItf_Realize(
		IntPtr self,
		[MarshalAs(UnmanagedType.Bool)] bool async
	);
	internal delegate SLResult SLObjectItf_Resume(
		IntPtr self,
		[MarshalAs(UnmanagedType.Bool)] bool async
	);
	internal delegate SLResult SLObjectItf_GetState(
		IntPtr self,
		out UInt32 pState
	);
	internal delegate SLResult SLObjectItf_GetInterface(
		IntPtr self,
		[MarshalAs(UnmanagedType.LPStruct)] Guid iid,
		out IntPtr pInterface
	);
	internal delegate SLResult SLObjectItf_RegisterCallback(
		IntPtr self,
		[MarshalAs(UnmanagedType.FunctionPtr)] slObjectCallback callback,
		IntPtr pContext
	);
	internal delegate void SLObjectItf_AbortAsyncOperation(
		IntPtr self
	);
	internal delegate void SLObjectItf_Destroy(
		IntPtr self
	);
	internal delegate SLResult SLObjectItf_SetPriority(
		IntPtr self,
		UInt32 priority
	);
	internal delegate SLResult SLObjectItf_GetPriority(
		IntPtr self,
		out UInt32 pPriority
	);
	internal delegate SLResult SLObjectItf_SetLossOfControlInterfaces(
		IntPtr self,
		UInt16 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIDs,
		[MarshalAs(UnmanagedType.Bool)] bool enabled
	);
	#endregion
	#region SLOutputMix
	internal delegate void slMixDeviceChangeCallback(
		SLOutputMixItf caller,
		IntPtr pContext
	);
	internal delegate SLResult SLOutputMixItf_GetDestinationOutputDeviceIDs(
		IntPtr self,
		ref int pNumDevices,
		UInt32[] pDeviceIDs
	);
	internal delegate SLResult SLOutputMixItf_RegisterDeviceChangeCallback(
		IntPtr self,
		slMixDeviceChangeCallback callback,
		IntPtr pContext
	);
	internal delegate SLResult SLOutputMixItf_ReRoute(
		IntPtr self,
		Int32 numOutputDevices,
		UInt32[] pOutputDeviceIDs
	);
	#endregion
	#region SLPlayItf
	internal delegate void slPlayCallback(
		SLPlayItf caller,
		IntPtr pContext,
		UInt32 @event
	);
	internal delegate SLResult SLPlayItf_SetPlayState(
		IntPtr self,
		UInt32 state
	);
	internal delegate SLResult SLPlayItf_GetPlayState(
		IntPtr self,
		out UInt32 pState
	);
	internal delegate SLResult SLPlayItf_GetDuration(
		IntPtr self,
		out UInt32 pMsec
	);
	internal delegate SLResult SLPlayItf_GetPosition(
		IntPtr self,
		out UInt32 pMsec
	);
	internal delegate SLResult SLPlayItf_RegisterCallback(
		IntPtr self,
		slPlayCallback callback,
		IntPtr pContext
	);
	internal delegate SLResult SLPlayItf_SetCallbackEventsMask(
		IntPtr self,
		UInt32 eventFlags
	);
	internal delegate SLResult SLPlayItf_GetCallbackEventsMask(
		IntPtr self,
		out UInt32 pEventFlags
	);
	internal delegate SLResult SLPlayItf_SetMarkerPosition(
		IntPtr self,
		UInt32 msec
	);
	internal delegate SLResult SLPlayItf_ClearMarkerPosition(
		IntPtr self
	);
	internal delegate SLResult SLPlayItf_GetMarkerPosition(
		IntPtr self,
		out UInt32 pMsec
	);
	internal delegate SLResult SLPlayItf_SetPositionUpdatePeriod(
		IntPtr self,
		UInt32 msec
	);
	internal delegate SLResult SLPlayItf_GetPositionUpdatePeriod(
		IntPtr self,
		out UInt32 pMsec
	);
	#endregion
}
