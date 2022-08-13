using Cryville.Common.Platform.Windows;
using Microsoft.Windows;
using Microsoft.Windows.MMDevice;
using Microsoft.Windows.PropSys;
using System;
using System.Runtime.InteropServices;

namespace Cryville.Audio.Wasapi {
	/// <summary>
	/// An <see cref="IAudioDevice" /> that interacts with Wasapi.
	/// </summary>
	public class MMDevice : ComInterfaceWrapper, IAudioDevice {
		internal MMDevice(IntPtr obj) : base(obj) { }

		/// <summary>
		/// The properties of the device.
		/// </summary>
		public PropertyStore Properties { get; private set; }

		string m_name;
		/// <inheritdoc />
		public string Name {
			get {
				if (m_name == null) {
					EnsureOpenPropertyStore();
					m_name = (string)Properties.Get(new PROPERTYKEY("a45c254e-df1c-4efd-8020-67d146a850e0", 14));
				}
				return m_name;
			}
		}

		static Guid GUID_MM_ENDPOINT = new Guid("1BE09788-6894-4089-8586-9A2A6C265AC5");
		DataFlow? m_dataFlow;
		/// <inheritdoc />
		public DataFlow DataFlow {
			get {
				if (m_dataFlow == null) {
					Marshal.QueryInterface(
						ComObject,
						ref GUID_MM_ENDPOINT,
						out var endpoint
					);
					IMMEndpoint.GetDataFlow(endpoint, out var presult);
					m_dataFlow = Util.FromInternalDataFlowEnum(presult);
					Marshal.ReleaseComObject(Marshal.GetObjectForIUnknown(endpoint));
				}
				return m_dataFlow.Value;
			}
		}

		static Guid GUID_AUDIOCLIENT = new Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2");
		/// <inheritdoc />
		public Audio.AudioClient Connect() {
			IMMDevice.Activate(ComObject, ref GUID_AUDIOCLIENT, (uint)CLSCTX.ALL, IntPtr.Zero, out var result);
			return new AudioClient(result, this);
		}

		private void EnsureOpenPropertyStore() {
			if (Properties != null) return;
			IMMDevice.OpenPropertyStore(ComObject, (uint)STGM.READ, out var result);
			Properties = new PropertyStore(result);
		}
	}
}
