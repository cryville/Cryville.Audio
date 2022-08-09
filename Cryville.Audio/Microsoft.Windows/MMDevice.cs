using Microsoft.Windows.PropSys;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Windows.MMDevice {
	[Guid("D666063F-1587-4E43-81F1-B948E807363F")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMMDevice {
		void Activate(
			ref Guid iid,
			UInt32 dwClsCtx,
			/* ref PROPVARIANT */ IntPtr pActivationParams,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppInterface
		);
		void OpenPropertyStore(
			UInt32 stgmAccess,
			out IPropertyStore ppProperties
		);
		void GetId(
			[MarshalAs(UnmanagedType.LPWStr)] out string ppstrId
		);
		void GetState(
			out UInt32 pdwState
		);
	}

	[Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMMDeviceCollection {
		void GetCount(
			out UInt32 pcDevices
		);
		void Item(
			UInt32 nDevice,
			out IMMDevice ppDevice
		);
	}

	[ComImport]
	[Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
	public class MMDeviceEnumerator { }

	[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMMDeviceEnumerator {
		void EnumAudioEndpoints(
			EDataFlow dataFlow,
			UInt32 dwStateMask,
			out IMMDeviceCollection ppDevices
		);
		void GetDefaultAudioEndpoint(
			EDataFlow dataFlow,
			ERole role,
			out IMMDevice ppEndpoint
		);
		void GetDevice(
			[MarshalAs(UnmanagedType.LPWStr)] string pwstrId,
			out IMMDevice ppDevice
		);
		void RegisterEndpointNotificationCallback(
			IMMNotificationClient pClient
		);
		void UnregisterEndpointNotificationCallback(
			IMMNotificationClient pClient
		);
	}

	[Guid("1BE09788-6894-4089-8586-9A2A6C265AC5")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMMEndpoint {
		void GetDataFlow(out EDataFlow pDataFlow);
	}

	[Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMMNotificationClient {
		// TODO
	}

	public enum EDataFlow : UInt32 {
		eRender = 0,
		eCapture,
		eAll,
		EDataFlow_enum_count,
	}

	public enum ERole : UInt32 {
		eConsole = 0,
		eMultimedia,
		eCommunications,
		ERole_enum_count
	};
}
