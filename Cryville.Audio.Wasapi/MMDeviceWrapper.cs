using Cryville.Common.Platform.Windows;
using Microsoft.Windows;
using Microsoft.Windows.AudioClient;
using Microsoft.Windows.AudioSessionTypes;
using Microsoft.Windows.MMDevice;
using Microsoft.Windows.Mme;
using Microsoft.Windows.PropSys;
using System;
using System.Runtime.InteropServices;

namespace Cryville.Audio.Wasapi {
	/// <summary>
	/// An <see cref="IAudioDevice" /> that interacts with Wasapi.
	/// </summary>
	public class MMDeviceWrapper : ComInterfaceWrapper, IAudioDevice {
		bool _connected;
		readonly IntPtr _client;

		internal MMDeviceWrapper(IntPtr obj) : base(obj) {
			IMMDevice.GetState(ComObject, out var state);
			if (state == (uint)DEVICE_STATE_XXX.DEVICE_STATE_ACTIVE) {
				IMMDevice.Activate(ComObject, ref GUID_AUDIOCLIENT, (uint)CLSCTX.ALL, IntPtr.Zero, out _client);
				IAudioClient.GetDevicePeriod(_client, out m_defaultBufferDuration, out m_minimumBufferDuration);
			}
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			if (disposing) {
				m_properties.Dispose();
				if (_client != IntPtr.Zero && !_connected) Marshal.ReleaseComObject(Marshal.GetObjectForIUnknown(_client));
			}
		}

		PropertyStore m_properties;
		/// <summary>
		/// The properties of the device.
		/// </summary>
		internal PropertyStore Properties {
			get {
				EnsureOpenPropertyStore();
				return m_properties;
			}
		}

		string m_name;
		/// <inheritdoc />
		public string Name {
			get {
				if (m_name == null)
					m_name = (string)Properties.Get(new PROPERTYKEY("a45c254e-df1c-4efd-8020-67d146a850e0", 14));
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

		private readonly long m_defaultBufferDuration;
		/// <inheritdoc />
		public float DefaultBufferDuration => FromReferenceTime(m_defaultBufferDuration);

		private readonly long m_minimumBufferDuration;
		/// <inheritdoc />
		public float MinimumBufferDuration => FromReferenceTime(m_minimumBufferDuration);

		/// <inheritdoc />
		public WaveFormat DefaultFormat {
			get {
				if (_client == IntPtr.Zero) throw new InvalidOperationException("The device is not available.");
				IAudioClient.GetMixFormat(_client, out var presult);
				var result = (WAVEFORMATEX)Marshal.PtrToStructure(presult, typeof(WAVEFORMATEX));
				return Util.FromInternalFormat(result);
			}
		}

		private void EnsureOpenPropertyStore() {
			if (m_properties != null) return;
			IMMDevice.OpenPropertyStore(ComObject, (uint)STGM.READ, out var result);
			m_properties = new PropertyStore(result);
		}

		/// <inheritdoc />
		public bool IsFormatSupported(WaveFormat format, out WaveFormat? suggestion, AudioShareMode shareMode = AudioShareMode.Shared) {
			if (_client == IntPtr.Zero) throw new InvalidOperationException("The device is not available.");
			var iformat = Util.ToInternalFormat(format);
			int hr = IAudioClient.IsFormatSupported(_client, ToInternalShareModeEnum(shareMode), ref iformat, out var presult);
			if (hr == 0) { // S_OK
				suggestion = format;
				if (presult != IntPtr.Zero) Marshal.FreeCoTaskMem(presult);
				return true;
			}
			else if (hr == 1) { // S_FALSE
				suggestion = Util.FromInternalFormat((WAVEFORMATEX)Marshal.PtrToStructure(presult, typeof(WAVEFORMATEX)));
				if (presult != IntPtr.Zero) Marshal.FreeCoTaskMem(presult);
				return false;
			}
			else if ((hr & 0x7fffffff) == 0x08890008) { // AUDCLNT_E_UNSUPPORTED_FORMAT
				suggestion = null;
				if (presult != IntPtr.Zero) Marshal.FreeCoTaskMem(presult);
				return false;
			}
			else Marshal.ThrowExceptionForHR(hr);
			throw new NotSupportedException("Theoretically unreachable");
		}

		static Guid GUID_AUDIOCLIENT = new Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2");
		/// <inheritdoc />
		public AudioClient Connect(WaveFormat format, float bufferDuration = 0, AudioShareMode shareMode = AudioShareMode.Shared) {
			if (_client == IntPtr.Zero) throw new InvalidOperationException("The device is not available.");
			_connected = true;
			return new AudioClientWrapper(_client, this, format, bufferDuration, shareMode);
		}

		static AUDCLNT_SHAREMODE ToInternalShareModeEnum(AudioShareMode value) {
			switch (value) {
				case AudioShareMode.Shared: return AUDCLNT_SHAREMODE.SHARED;
				case AudioShareMode.Exclusive: return AUDCLNT_SHAREMODE.EXCLUSIVE;
				default: throw new ArgumentOutOfRangeException(nameof(value));
			}
		}
		static float FromReferenceTime(long value) {
			return value / 1e4f;
		}
	}
}
