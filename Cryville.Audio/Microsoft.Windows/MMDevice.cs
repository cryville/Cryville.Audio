using Microsoft.Windows.PropSys;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Windows.MMDevice {
	internal static class IMMDevice {
		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IMMDevice_Activate")]
		public static extern void Activate(
			IntPtr self,
			ref Guid iid,
			UInt32 dwClsCtx,
			/* ref PROPVARIANT */ IntPtr pActivationParams,
			out IntPtr ppInterface
		);
		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IMMDevice_OpenPropertyStore")]
		public static extern void OpenPropertyStore(
			IntPtr self,
			UInt32 stgmAccess,
			out IPropertyStore ppProperties
		);
		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IMMDevice_GetId")]
		public static extern void GetId(
			IntPtr self,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppstrId
		);
		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IMMDevice_GetState")]
		public static extern void GetState(
			IntPtr self,
			out UInt32 pdwState
		);
	}

	internal static class IMMDeviceCollection {
		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IMMDeviceCollection_GetCount")]
		public static extern void GetCount(
			IntPtr self,
			out UInt32 pcDevices
		);
		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IMMDeviceCollection_Item")]
		public static extern void Item(
			IntPtr self,
			UInt32 nDevice,
			out IntPtr ppDevice
		);
	}

	internal static class IMMDeviceEnumerator {

		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "_ctor_IMMDeviceEnumerator")]
		public static extern void _ctor(
			out IntPtr @out
		);
		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IMMDeviceEnumerator_EnumAudioEndpoints")]
		public static extern void EnumAudioEndpoints(
			IntPtr self,
			EDataFlow dataFlow,
			UInt32 dwStateMask,
			out IntPtr ppDevices
		);
		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IMMDeviceEnumerator_GetDefaultAudioEndpoint")]
		public static extern void GetDefaultAudioEndpoint(
			IntPtr self,
			EDataFlow dataFlow,
			ERole role,
			out IntPtr ppEndpoint
		);
		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IMMDeviceEnumerator_GetDevice")]
		public static extern void GetDevice(
			IntPtr self,
			[MarshalAs(UnmanagedType.LPWStr)] string pwstrId,
			out IntPtr ppDevice
		);
		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IMMDeviceEnumerator_RegisterEndpointNotificationCallback")]
		public static extern void RegisterEndpointNotificationCallback(
			IntPtr self,
			IntPtr pClient
		);
		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IMMDeviceEnumerator_UnregisterEndpointNotificationCallback")]
		public static extern void UnregisterEndpointNotificationCallback(
			IntPtr self,
			IntPtr pClient
		);
	}

	internal static class IMMEndpoint {
		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IMMEndpoint_GetDataFlow")]
		public static extern void GetDataFlow(
			IntPtr self,
			out EDataFlow pDataFlow
		);
	}

	internal enum EDataFlow : UInt32 {
		eRender = 0,
		eCapture,
		eAll,
		EDataFlow_enum_count,
	}

	internal enum ERole : UInt32 {
		eConsole = 0,
		eMultimedia,
		eCommunications,
		ERole_enum_count
	};
}
