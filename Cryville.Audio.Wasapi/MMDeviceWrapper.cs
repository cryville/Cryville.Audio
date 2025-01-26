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
		readonly IAudioClient2? _client2;

		internal MMDeviceWrapper(IMMDevice obj) {
			_internal = obj;
			_internal.GetState(out var state);
			if (state == (uint)DEVICE_STATE_XXX.DEVICE_STATE_ACTIVE) {
				_internal.Activate(typeof(IAudioClient).GUID, (uint)CLSCTX.ALL, IntPtr.Zero, out var client);
				_client = (IAudioClient)client;
				_client.GetDevicePeriod(out m_defaultBufferDuration, out m_minimumBufferDuration);
				try {
					_internal.Activate(typeof(IAudioClient2).GUID, (uint)CLSCTX.ALL, IntPtr.Zero, out var client2);
					_client2 = (IAudioClient2)client2;
				}
				catch (COMException) { }
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
						out var pEndpoint
					);
					var endpoint = (IMMEndpoint)Marshal.GetObjectForIUnknown(pEndpoint);
					endpoint.GetDataFlow(out var pResult);
					m_dataFlow = Helpers.FromInternalDataFlowEnum(pResult);
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
		public int MinimumBufferSize => Helpers.FromReferenceTime(DefaultFormat.SampleRate, m_minimumBufferDuration);

		private readonly long m_defaultBufferDuration;
		/// <inheritdoc />
		public int DefaultBufferSize => Helpers.FromReferenceTime(DefaultFormat.SampleRate, m_defaultBufferDuration);

		/// <inheritdoc />
		public WaveFormat DefaultFormat {
			get {
				if (_client == null) throw new InvalidOperationException("The device is not available.");
				_client.GetMixFormat(out var pResult);
#if NET451_OR_GREATER || NETSTANDARD1_2_OR_GREATER || NETCOREAPP1_0_OR_GREATER
				var result = Marshal.PtrToStructure<WAVEFORMATEX>(pResult);
#else
				var result = (WAVEFORMATEX)Marshal.PtrToStructure(pResult, typeof(WAVEFORMATEX));
#endif
				return Helpers.FromInternalFormat(result);
			}
		}

		/// <inheritdoc />
		public bool IsFormatSupported(WaveFormat format, out WaveFormat? suggestion, AudioUsage usage = AudioUsage.Media, AudioShareMode shareMode = AudioShareMode.Shared) {
			if (_client == null) throw new InvalidOperationException("The device is not available.");
			var iFormat = Helpers.ToInternalFormat(format);
			int hr = _client.IsFormatSupported(ToInternalShareModeEnum(shareMode), ref iFormat, out var pResult);
			if (hr == 0) { // S_OK
				suggestion = format;
				if (pResult != IntPtr.Zero) Marshal.FreeCoTaskMem(pResult);
				return true;
			}
			else if (hr == 1) { // S_FALSE
#if NET451_OR_GREATER || NETSTANDARD1_2_OR_GREATER || NETCOREAPP1_0_OR_GREATER
				suggestion = Helpers.FromInternalFormat(Marshal.PtrToStructure<WAVEFORMATEX>(pResult));
#else
				suggestion = Helpers.FromInternalFormat((WAVEFORMATEX)Marshal.PtrToStructure(pResult, typeof(WAVEFORMATEX)));
#endif
				if (pResult != IntPtr.Zero) Marshal.FreeCoTaskMem(pResult);
				return false;
			}
			else if ((hr & 0x7fffffff) == 0x08890008) { // AUDCLNT_E_UNSUPPORTED_FORMAT
				suggestion = null;
				if (pResult != IntPtr.Zero) Marshal.FreeCoTaskMem(pResult);
				return false;
			}
			else Marshal.ThrowExceptionForHR(hr);
			throw new NotSupportedException("Theoretically unreachable");
		}

		/// <inheritdoc />
		public AudioClient Connect(WaveFormat format, int bufferSize = 0, AudioUsage usage = AudioUsage.Media, AudioShareMode shareMode = AudioShareMode.Shared) {
			if (_client == null) throw new InvalidOperationException("The device is not available.");
			_connected = true;
			return new AudioClientWrapper(_client, _client2, this, format, bufferSize, usage, shareMode);
		}

		static AUDCLNT_SHAREMODE ToInternalShareModeEnum(AudioShareMode value) => value switch {
			AudioShareMode.Shared => AUDCLNT_SHAREMODE.SHARED,
			AudioShareMode.Exclusive => AUDCLNT_SHAREMODE.EXCLUSIVE,
			_ => throw new ArgumentOutOfRangeException(nameof(value)),
		};
	}
}
