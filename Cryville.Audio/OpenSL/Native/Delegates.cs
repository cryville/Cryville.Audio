using Cryville.Common.Interop;
using System;
using System.Runtime.InteropServices;

namespace OpenSL.Native {
	#region SLAudioIODeviceCapabilitiesItf
	/*public delegate void slAvailableAudioInputsChangedCallback(
		IntPtr caller,
		IntPtr pContext,
		UInt32 deviceID,
		UInt32 numInputs,
		[MarshalAs(UnmanagedType.Bool)] bool isNew
	);
	public delegate void slAvailableAudioOutputsChangedCallback(
		IntPtr caller,
		IntPtr pContext,
		UInt32 deviceID,
		UInt32 numOutputs,
		[MarshalAs(UnmanagedType.Bool)] bool isNew
	);
	public delegate void slDefaultDeviceIDMapChangedCallback(
		IntPtr caller,
		IntPtr pContext,
		[MarshalAs(UnmanagedType.Bool)] bool isOutput,
		UInt32 numDevices
	);
	public delegate SLresult SLAudioIODeviceCapabilitiesItf_GetAvailableAudioInputs(
		IntPtr self,
		ref UInt32 pNumInputs,
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] UInt32[] pInputDeviceIDs
	);
	public delegate SLresult SLAudioIODeviceCapabilitiesItf_QueryAudioInputCapabilities(
		IntPtr self,
		UInt32 deviceId,
		out SLAudioInputDescriptor pDescriptor
	);
	public delegate SLresult SLAudioIODeviceCapabilitiesItf_RegisterAvailableAudioInputsChangedCallback(
		IntPtr self,
		slAvailableAudioInputsChangedCallback callback,
		IntPtr pContext
	);
	public delegate SLresult SLAudioIODeviceCapabilitiesItf_GetAvailableAudioOutputs(
		IntPtr self,
		ref UInt32 pNumOutputs,
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] UInt32[] pOutputDeviceIDs
	);
	public delegate SLresult SLAudioIODeviceCapabilitiesItf_QueryAudioOutputCapabilities(
		IntPtr self,
		UInt32 deviceId,
		out SLAudioOutputDescriptor pDescriptor
	);
	public delegate SLresult SLAudioIODeviceCapabilitiesItf_RegisterAvailableAudioOutputsChangedCallback(
		IntPtr self,
		slAvailableAudioOutputsChangedCallback callback,
		IntPtr pContext
	);
	public delegate SLresult SLAudioIODeviceCapabilitiesItf_RegisterDefaultDeviceIDMapChangedCallback(
		IntPtr self,
		slDefaultDeviceIDMapChangedCallback callback,
		IntPtr pContext
	);
	public delegate SLresult SLAudioIODeviceCapabilitiesItf_GetAssociatedAudioInputs(
		IntPtr self,
		UInt32 deviceId,
		ref UInt32 pNumAudioInputs,
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] UInt32[] pAudioInputDeviceIDs
	);
	public delegate SLresult SLAudioIODeviceCapabilitiesItf_GetAssociatedAudioOutputs(
		IntPtr self,
		UInt32 deviceId,
		ref UInt32 pNumAudioOutputs,
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] UInt32[] pAudioOutputDeviceIDs
	);
	public delegate SLresult SLAudioIODeviceCapabilitiesItf_GetDefaultAudioDevices(
		IntPtr self,
		UInt32 defaultDeviceID,
		ref UInt32 pNumAudioDevices,
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] UInt32[] pAudioDeviceIDs
	);
	public delegate SLresult SLAudioIODeviceCapabilitiesItf_QuerySampleFormatsSupported(
		IntPtr self,
		UInt32 deviceId,
		UInt32 samplingRate,
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] out Int32 pSampleFormats,
		ref UInt32 pNumOfSampleFormats
	);*/
	#endregion
	#region SLBufferQueueItf
	public delegate void slBufferQueueCallback (
		IntPtr caller,
		IntPtr pContext
	);
	public delegate SLresult SLBufferQueueItf_Enqueue(
		IntPtr self,
		IntPtr pBuffer,
		UInt32 size
	);
	public delegate SLresult SLBufferQueueItf_Clear(
		IntPtr self
	);
	public delegate SLresult SLBufferQueueItf_GetState(
		IntPtr self,
		out SLBufferQueueState pState
	);
	public delegate SLresult SLBufferQueueItf_RegisterCallback(
		IntPtr self,
		slBufferQueueCallback callback,
		IntPtr pContext
	);
	#endregion
	#region SLEngineItf
	public delegate SLresult SLEngineItf_CreateLEDDevice(
		IntPtr self,
		out IntPtr pDevice,
		UInt32 deviceID,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)] bool[] pInterfaceRequired
	);
	public delegate SLresult SLEngineItf_CreateVibraDevice(
		IntPtr self,
		out IntPtr pDevice,
		UInt32 deviceID,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)] bool[] pInterfaceRequired
	);
	public delegate SLresult SLEngineItf_CreateAudioPlayer(
		IntPtr self,
		out IntPtr pPlayer,
		ref SLDataSource pAudioSrc,
		ref SLDataSink pAudioSnk,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)] bool[] pInterfaceRequired
	);
	public delegate SLresult SLEngineItf_CreateAudioRecorder(
		IntPtr self,
		out IntPtr pRecorder,
		ref SLDataSource pAudioSrc,
		ref SLDataSink pAudioSnk,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)] bool[] pInterfaceRequired
	);
	public delegate SLresult SLEngineItf_CreateMidiPlayer(
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
	public delegate SLresult SLEngineItf_CreateListener(
		IntPtr self,
		out IntPtr pListener,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)] bool[] pInterfaceRequired
	);
	public delegate SLresult SLEngineItf_Create3DGroup(
		IntPtr self,
		out IntPtr pGroup,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)] bool[] pInterfaceRequired
	);
	public delegate SLresult SLEngineItf_CreateOutputMix(
		IntPtr self,
		out IntPtr pMix,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)] bool[] pInterfaceRequired
	);
	public delegate SLresult SLEngineItf_CreateMetadataExtractor(
		IntPtr self,
		out IntPtr pMetadataExtractor,
		ref SLDataSource pDataSource,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)] bool[] pInterfaceRequired
	);
	public delegate SLresult SLEngineItf_CreateExtensionObject(
		IntPtr self,
		out IntPtr pObject,
		IntPtr pParameters,
		UInt32 objectID,
		UInt32 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIds,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)] bool[] pInterfaceRequired
	);
	public delegate SLresult SLEngineItf_QueryNumSupportedInterfaces(
		IntPtr self,
		UInt32 objectID,
		out UInt32 pNumSupportedInterfaces
	);
	public delegate SLresult SLEngineItf_QuerySupportedInterfaces(
		IntPtr self,
		UInt32 objectID,
		UInt32 index,
		out Guid pInterfaceId
	);
	public delegate SLresult SLEngineItf_QueryNumSupportedExtensions(
		IntPtr self,
		out UInt32 pNumExtensions
	);
	public delegate SLresult SLEngineItf_QuerySupportedExtension(
		IntPtr self,
		UInt32 index,
		[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUTF8StrMarshaler))] string pExtensionName,
		ref UInt16 pNameLength
	);
	public delegate SLresult SLEngineItf_IsExtensionSupported(
		IntPtr self,
		[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUTF8StrMarshaler))] string pExtensionName,
		[MarshalAs(UnmanagedType.Bool)] out bool pSupported
	);
	#endregion
	#region SLObjectItf
	public delegate void slObjectCallback(
		IntPtr caller,
		IntPtr pContext,
		UInt32 @event,
		SLresult result,
		UInt32 param,
		IntPtr pInterface
	);
	public delegate SLresult SLObjectItf_Realize(
		IntPtr self,
		[MarshalAs(UnmanagedType.Bool)] bool async
	);
	public delegate SLresult SLObjectItf_Resume (
		IntPtr self,
		[MarshalAs(UnmanagedType.Bool)] bool async
	);
	public delegate SLresult SLObjectItf_GetState(
		IntPtr self,
		out UInt32 pState
	);
	public delegate SLresult SLObjectItf_GetInterface(
		IntPtr self,
		[MarshalAs(UnmanagedType.LPStruct)] Guid iid,
		out IntPtr pInterface
	);
	public delegate SLresult SLObjectItf_RegisterCallback(
		IntPtr self,
		[MarshalAs(UnmanagedType.FunctionPtr)] slObjectCallback callback,
		IntPtr pContext
	);
	public delegate void SLObjectItf_AbortAsyncOperation(
		IntPtr self
	);
	public delegate void SLObjectItf_Destroy(
		IntPtr self
	);
	public delegate SLresult SLObjectItf_SetPriority(
		IntPtr self,
		UInt32 priority
	);
	public delegate SLresult SLObjectItf_GetPriority(
		IntPtr self,
		out UInt32 pPriority
	);
	public delegate SLresult SLObjectItf_SetLossOfControlInterfaces(
		IntPtr self,
		UInt16 numInterfaces,
		[MarshalAs(UnmanagedType.LPArray)] ref Guid[] pInterfaceIDs,
		[MarshalAs(UnmanagedType.Bool)] bool enabled
	);
	#endregion
	#region SLOutputMix
	public delegate void slMixDeviceChangeCallback (
		SLOutputMixItf caller,
		IntPtr pContext
	);
	public delegate SLresult SLOutputMixItf_GetDestinationOutputDeviceIDs(
		IntPtr self,
		ref int pNumDevices,
		UInt32[] pDeviceIDs
	);
	public delegate SLresult SLOutputMixItf_RegisterDeviceChangeCallback(
		IntPtr self,
		slMixDeviceChangeCallback callback,
		IntPtr pContext
	);
	public delegate SLresult SLOutputMixItf_ReRoute(
		IntPtr self,
		Int32 numOutputDevices,
		UInt32[] pOutputDeviceIDs
	);
	#endregion
	#region SLPlayItf
	public delegate void slPlayCallback(
		SLPlayItf caller,
		IntPtr pContext,
		UInt32 @event
	);
	public delegate SLresult SLPlayItf_SetPlayState(
		IntPtr self,
		UInt32 state
	);
	public delegate SLresult SLPlayItf_GetPlayState(
		IntPtr self,
		out UInt32 pState
	);
	public delegate SLresult SLPlayItf_GetDuration(
		IntPtr self,
		out UInt32 pMsec
	);
	public delegate SLresult SLPlayItf_GetPosition(
		IntPtr self,
		out UInt32 pMsec
	);
	public delegate SLresult SLPlayItf_RegisterCallback(
		IntPtr self,
		slPlayCallback callback,
		IntPtr pContext
	);
	public delegate SLresult SLPlayItf_SetCallbackEventsMask(
		IntPtr self,
		UInt32 eventFlags
	);
	public delegate SLresult SLPlayItf_GetCallbackEventsMask(
		IntPtr self,
		out UInt32 pEventFlags
	);
	public delegate SLresult SLPlayItf_SetMarkerPosition(
		IntPtr self,
		UInt32 msec
	);
	public delegate SLresult SLPlayItf_ClearMarkerPosition(
		IntPtr self
	);
	public delegate SLresult SLPlayItf_GetMarkerPosition(
		IntPtr self,
		out UInt32 pMsec
	);
	public delegate SLresult SLPlayItf_SetPositionUpdatePeriod(
		IntPtr self,
		UInt32 msec
	);
	public delegate SLresult SLPlayItf_GetPositionUpdatePeriod(
		IntPtr self,
		out UInt32 pMsec
	);
	#endregion
}
