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
	public class MMDeviceWrapper : IAudioDevice {
		bool _connected;
		readonly IMMDevice _internal;
		readonly IAudioClient? _client;

		internal MMDeviceWrapper(IMMDevice obj) {
			_internal = obj;
			_internal.GetState(out var state);
			if (state == (uint)DEVICE_STATE_XXX.DEVICE_STATE_ACTIVE) {
				_internal.Activate(typeof(IAudioClient).GUID, (uint)CLSCTX.ALL, IntPtr.Zero, out var client);
				_client = (IAudioClient)client;
				_client.GetDevicePeriod(out m_defaultBufferDuration, out m_minimumBufferDuration);
			}
		}

		/// <inheritdoc />
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc />
		protected virtual void Dispose(bool disposing) {
			if (!disposing) return;
			m_properties?.Dispose();
			if (_client != null && !_connected) Marshal.ReleaseComObject(_client);
		}

		PropertyStore? m_properties;
		/// <summary>
		/// The properties of the device.
		/// </summary>
		internal PropertyStore Properties {
			get {
				if (m_properties == null) {
					_internal.OpenPropertyStore((uint)STGM.READ, out var result);
					m_properties = new PropertyStore(result);
				}
				return m_properties;
			}
		}

		string? m_name;
		/// <inheritdoc />
		public string Name => m_name ??= ((string?)Properties.Get(new PROPERTYKEY("a45c254e-df1c-4efd-8020-67d146a850e0", 14)) ?? "");

		static Guid GUID_MM_ENDPOINT = typeof(IMMEndpoint).GUID;
		DataFlow? m_dataFlow;
		/// <inheritdoc />
		public DataFlow DataFlow {
			get {
				if (m_dataFlow == null) {
					Marshal.QueryInterface(
						Marshal.GetIUnknownForObject(_internal),
						ref GUID_MM_ENDPOINT,
						out var pendpoint
					);
					var endpoint = (IMMEndpoint)Marshal.GetObjectForIUnknown(pendpoint);
					endpoint.GetDataFlow(out var presult);
					m_dataFlow = Util.FromInternalDataFlowEnum(presult);
					Marshal.ReleaseComObject(endpoint);
				}
				return m_dataFlow.Value;
			}
		}

		static int GCD(int a, int b) => b == 0 ? a : GCD(b, a % b);
		/// <inheritdoc />
		public int BurstSize => GCD(MinimumBufferSize, DefaultBufferSize);

		private readonly long m_minimumBufferDuration;
		/// <inheritdoc />
		public int MinimumBufferSize => Util.FromReferenceTime(DefaultFormat.SampleRate, m_minimumBufferDuration);

		private readonly long m_defaultBufferDuration;
		/// <inheritdoc />
		public int DefaultBufferSize => Util.FromReferenceTime(DefaultFormat.SampleRate, m_defaultBufferDuration);

		/// <inheritdoc />
		public WaveFormat DefaultFormat {
			get {
				if (_client == null) throw new InvalidOperationException("The device is not available.");
				_client.GetMixFormat(out var presult);
				var result = (WAVEFORMATEX)Marshal.PtrToStructure(presult, typeof(WAVEFORMATEX));
				return Util.FromInternalFormat(result);
			}
		}

		/// <inheritdoc />
		public bool IsFormatSupported(WaveFormat format, out WaveFormat? suggestion, AudioShareMode shareMode = AudioShareMode.Shared) {
			if (_client == null) throw new InvalidOperationException("The device is not available.");
			var iformat = Util.ToInternalFormat(format);
			int hr = _client.IsFormatSupported(ToInternalShareModeEnum(shareMode), ref iformat, out var presult);
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

		/// <inheritdoc />
		public AudioClient Connect(WaveFormat format, int bufferSize = 0, AudioShareMode shareMode = AudioShareMode.Shared) {
			if (_client == null) throw new InvalidOperationException("The device is not available.");
			_connected = true;
			return new AudioClientWrapper(_client, this, format, bufferSize, shareMode);
		}

		static AUDCLNT_SHAREMODE ToInternalShareModeEnum(AudioShareMode value) => value switch {
			AudioShareMode.Shared => AUDCLNT_SHAREMODE.SHARED,
			AudioShareMode.Exclusive => AUDCLNT_SHAREMODE.EXCLUSIVE,
			_ => throw new ArgumentOutOfRangeException(nameof(value)),
		};
	}
}
