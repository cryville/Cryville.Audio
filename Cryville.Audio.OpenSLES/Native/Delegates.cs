using Cryville.Common.Compat;
using System;
using System.Runtime.InteropServices;

namespace OpenSLES.Native {
	#region SLBufferQueueItf
	internal delegate void slBufferQueueCallback(
		IntPtr caller,
		IntPtr pContext
	);
	internal delegate SLresult SLBufferQueueItf_Enqueue(
		IntPtr self,
		IntPtr pBuffer,
		UInt32 size
	);
	internal delegate SLresult SLBufferQueueItf_Clear(
		IntPtr self
	);
	internal delegate SLresult SLBufferQueueItf_GetState(
		IntPtr self,
		out SLBufferQueueState pState
	);
	internal delegate SLresult SLBufferQueueItf_RegisterCallback(
		IntPtr self,
		slBufferQueueCallback callback,
		IntPtr pContext
	);
	#endregion
	#region SLEngineItf
	internal delegate SLresult SLEngineItf_CreateLEDDevice(
		IntPtr self,
		out IntPtr pDevice,
		UInt32 deviceID,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)] bool[] pInterfaceRequired
	);
	internal delegate SLresult SLEngineItf_CreateVibraDevice(
		IntPtr self,
		out IntPtr pDevice,
		UInt32 deviceID,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)] bool[] pInterfaceRequired
	);
	internal delegate SLresult SLEngineItf_CreateAudioPlayer(
		IntPtr self,
		out IntPtr pPlayer,
		ref SLDataSource pAudioSrc,
		ref SLDataSink pAudioSnk,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)] bool[] pInterfaceRequired
	);
	internal delegate SLresult SLEngineItf_CreateAudioRecorder(
		IntPtr self,
		out IntPtr pRecorder,
		ref SLDataSource pAudioSrc,
		ref SLDataSink pAudioSnk,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)] bool[] pInterfaceRequired
	);
	internal delegate SLresult SLEngineItf_CreateMidiPlayer(
		IntPtr self,
		out IntPtr pPlayer,
		ref SLDataSource pMIDISrc,
		ref SLDataSource pBankSrc,
		ref SLDataSink pAudioOutput,
		ref SLDataSink pVibra,
		ref SLDataSink pLEDArray,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)] bool[] pInterfaceRequired
	);
	internal delegate SLresult SLEngineItf_CreateListener(
		IntPtr self,
		out IntPtr pListener,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)] bool[] pInterfaceRequired
	);
	internal delegate SLresult SLEngineItf_Create3DGroup(
		IntPtr self,
		out IntPtr pGroup,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)] bool[] pInterfaceRequired
	);
	internal delegate SLresult SLEngineItf_CreateOutputMix(
		IntPtr self,
		out IntPtr pMix,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)] bool[] pInterfaceRequired
	);
	internal delegate SLresult SLEngineItf_CreateMetadataExtractor(
		IntPtr self,
		out IntPtr pMetadataExtractor,
		ref SLDataSource pDataSource,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)] bool[] pInterfaceRequired
	);
	internal delegate SLresult SLEngineItf_CreateExtensionObject(
		IntPtr self,
		out IntPtr pObject,
		IntPtr pParameters,
		UInt32 objectID,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)] bool[] pInterfaceRequired
	);
	internal delegate SLresult SLEngineItf_QueryNumSupportedInterfaces(
		IntPtr self,
		UInt32 objectID,
		out UInt32 pNumSupportedInterfaces
	);
	internal delegate SLresult SLEngineItf_QuerySupportedInterfaces(
		IntPtr self,
		UInt32 objectID,
		UInt32 index,
		out Guid pInterfaceId
	);
	internal delegate SLresult SLEngineItf_QueryNumSupportedExtensions(
		IntPtr self,
		out UInt32 pNumExtensions
	);
	internal delegate SLresult SLEngineItf_QuerySupportedExtension(
		IntPtr self,
		UInt32 index,
		[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUTF8StrMarshaler))] string pExtensionName,
		ref UInt16 pNameLength
	);
	internal delegate SLresult SLEngineItf_IsExtensionSupported(
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
		SLresult result,
		UInt32 param,
		IntPtr pInterface
	);
	internal delegate SLresult SLObjectItf_Realize(
		IntPtr self,
		[MarshalAs(UnmanagedType.Bool)] bool async
	);
	internal delegate SLresult SLObjectItf_Resume(
		IntPtr self,
		[MarshalAs(UnmanagedType.Bool)] bool async
	);
	internal delegate SLresult SLObjectItf_GetState(
		IntPtr self,
		out UInt32 pState
	);
	internal delegate SLresult SLObjectItf_GetInterface(
		IntPtr self,
		[MarshalAs(UnmanagedType.LPStruct)] Guid iid,
		out IntPtr pInterface
	);
	internal delegate SLresult SLObjectItf_RegisterCallback(
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
	internal delegate SLresult SLObjectItf_SetPriority(
		IntPtr self,
		UInt32 priority
	);
	internal delegate SLresult SLObjectItf_GetPriority(
		IntPtr self,
		out UInt32 pPriority
	);
	internal delegate SLresult SLObjectItf_SetLossOfControlInterfaces(
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
	internal delegate SLresult SLOutputMixItf_GetDestinationOutputDeviceIDs(
		IntPtr self,
		ref int pNumDevices,
		UInt32[] pDeviceIDs
	);
	internal delegate SLresult SLOutputMixItf_RegisterDeviceChangeCallback(
		IntPtr self,
		slMixDeviceChangeCallback callback,
		IntPtr pContext
	);
	internal delegate SLresult SLOutputMixItf_ReRoute(
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
	internal delegate SLresult SLPlayItf_SetPlayState(
		IntPtr self,
		UInt32 state
	);
	internal delegate SLresult SLPlayItf_GetPlayState(
		IntPtr self,
		out UInt32 pState
	);
	internal delegate SLresult SLPlayItf_GetDuration(
		IntPtr self,
		out UInt32 pMsec
	);
	internal delegate SLresult SLPlayItf_GetPosition(
		IntPtr self,
		out UInt32 pMsec
	);
	internal delegate SLresult SLPlayItf_RegisterCallback(
		IntPtr self,
		slPlayCallback callback,
		IntPtr pContext
	);
	internal delegate SLresult SLPlayItf_SetCallbackEventsMask(
		IntPtr self,
		UInt32 eventFlags
	);
	internal delegate SLresult SLPlayItf_GetCallbackEventsMask(
		IntPtr self,
		out UInt32 pEventFlags
	);
	internal delegate SLresult SLPlayItf_SetMarkerPosition(
		IntPtr self,
		UInt32 msec
	);
	internal delegate SLresult SLPlayItf_ClearMarkerPosition(
		IntPtr self
	);
	internal delegate SLresult SLPlayItf_GetMarkerPosition(
		IntPtr self,
		out UInt32 pMsec
	);
	internal delegate SLresult SLPlayItf_SetPositionUpdatePeriod(
		IntPtr self,
		UInt32 msec
	);
	internal delegate SLresult SLPlayItf_GetPositionUpdatePeriod(
		IntPtr self,
		out UInt32 pMsec
	);
	#endregion
}
