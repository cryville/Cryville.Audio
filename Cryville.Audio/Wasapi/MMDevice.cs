using Cryville.Common.Platform.Windows;
using Microsoft.Windows;
using Microsoft.Windows.MMDevice;
using Microsoft.Windows.PropSys;
using System;
using System.Runtime.InteropServices;
using CIAudioClient = Microsoft.Windows.AudioClient.IAudioClient;

namespace Cryville.Audio.Wasapi {
	public class MMDevice : ComInterfaceWrapper<IMMDevice>, IAudioDevice {
		internal MMDevice(IMMDevice obj) : base(obj) { }

		public PropertyStore Properties { get; private set; }

		string m_name;
		public string Name {
			get {
				if (m_name == null) {
					EnsureOpenPropertyStore();
					m_name = (string)Properties.Get(new PROPERTYKEY("a45c254e-df1c-4efd-8020-67d146a850e0", 14));
				}
				return m_name;
			}
		}

		static Guid GUID_MM_ENDPOINT = typeof(IMMEndpoint).GUID;
		DataFlow? m_dataFlow;
		public DataFlow DataFlow {
			get {
				if (m_dataFlow == null) {
					Marshal.QueryInterface(
						Marshal.GetIUnknownForObject(ComObject),
						ref GUID_MM_ENDPOINT,
						out var pendpoint
					);
					var endpoint = Marshal.GetObjectForIUnknown(pendpoint) as IMMEndpoint;
					endpoint.GetDataFlow(out var presult);
					m_dataFlow = Util.FromInternalDataFlowEnum(presult);
					Marshal.ReleaseComObject(endpoint);
				}
				return m_dataFlow.Value;
			}
		}

		static Guid GUID_AUDIOCLIENT = typeof(CIAudioClient).GUID;
		public Audio.AudioClient Connect() {
			ComObject.Activate(ref GUID_AUDIOCLIENT, (uint)CLSCTX.ALL, IntPtr.Zero, out var result);
			return new AudioClient(result as CIAudioClient, this);
		}

		private void EnsureOpenPropertyStore() {
			if (Properties != null) return;
			ComObject.OpenPropertyStore((uint)STGM.READ, out var result);
			Properties = new PropertyStore(result);
		}
	}
}
