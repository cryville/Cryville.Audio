using Cryville.Common.Platform.Windows;
using Microsoft.Windows;
using Microsoft.Windows.AudioClient;
using Microsoft.Windows.AudioSessionTypes;
using Microsoft.Windows.MMDevice;
using Microsoft.Windows.PropSys;
using System;
using System.Runtime.InteropServices;

namespace Cryville.Audio.Wasapi {
	/// <summary>
	/// An <see cref="IAudioDevice" /> that interacts with Wasapi.
	/// </summary>
	public class MMDeviceWrapper : IAudioClientDevice {
		readonly IMMDevice _internal;
		IAudioClient? _client;

		internal MMDeviceWrapper(IMMDevice obj) {
			_internal = obj;
			_internal.GetState(out var state);
			if (state != (uint)DEVICE_STATE_XXX.DEVICE_STATE_ACTIVE) {
				return;
			}
			ReactivateClient();
		}

		/// <inheritdoc />
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc />
		protected virtual void Dispose(bool disposing) { }

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
				}
				return m_dataFlow.Value;
			}
		}

		static int GCD(int a, int b) => b == 0 ? a : GCD(b, a % b);
		/// <inheritdoc />
		public int BurstSize => GCD(MinimumBufferSize, DefaultBufferSize);

		private long m_minimumBufferDuration;
		/// <inheritdoc />
		public int MinimumBufferSize => Helpers.FromReferenceTime(DefaultFormat.SampleRate, m_minimumBufferDuration);

		private long m_defaultBufferDuration;
		/// <inheritdoc />
		public int DefaultBufferSize => Helpers.FromReferenceTime(DefaultFormat.SampleRate, m_defaultBufferDuration);

		/// <inheritdoc />
		public WaveFormat DefaultFormat {
			get {
				if (_client == null) throw new InvalidOperationException("The device is not available.");
				try {
					_client.GetMixFormat(out var pResult);
					var result = Helpers.FromInternalFormat(pResult);
					Marshal.FreeCoTaskMem(pResult);
					return result;
				}
				catch (COMException ex) when ((uint)ex.ErrorCode == 0x88890004) {
					ReactivateClient();
					return DefaultFormat;
				}
			}
		}

		/// <inheritdoc />
		public bool IsFormatSupported(WaveFormat format, out WaveFormat? suggestion, AudioUsage usage = AudioUsage.Media, AudioShareMode shareMode = AudioShareMode.Shared) {
			try {
				if (_client == null) throw new InvalidOperationException("The device is not available.");
				var iFormat = Helpers.ToInternalFormat(format);
				int hr = _client.IsFormatSupported(ToInternalShareModeEnum(shareMode), ref iFormat, out var pResult);
				if (hr == 0) { // S_OK
					suggestion = format;
					if (pResult != IntPtr.Zero) Marshal.FreeCoTaskMem(pResult);
					return true;
				}
				else if (hr == 1) { // S_FALSE
					suggestion = Helpers.FromInternalFormat(pResult);
					if (pResult != IntPtr.Zero) Marshal.FreeCoTaskMem(pResult);
					return false;
				}
				else if ((hr & 0x7fffffff) == 0x08890008) { // AUDCLNT_E_UNSUPPORTED_FORMAT
					suggestion = null;
					if (pResult != IntPtr.Zero) Marshal.FreeCoTaskMem(pResult);
					return false;
				}
				else Marshal.ThrowExceptionForHR(hr);
				throw new NotSupportedException("Theoretically unreachable.");
			}
			catch (COMException ex) when ((uint)ex.ErrorCode == 0x88890004) {
				ReactivateClient();
				return IsFormatSupported(format, out suggestion, usage, shareMode);
			}
			catch (PlatformNotSupportedException) when (format.ChannelMask != 0) {
				return IsFormatSupported(format with { ChannelMask = 0 }, out suggestion, usage, shareMode);
			}
		}

		/// <inheritdoc />
		public void ReactivateClient() {
			try {
				_internal.Activate(typeof(IAudioClient2).GUID, (uint)CLSCTX.ALL, IntPtr.Zero, out var client);
				_client = (IAudioClient2)client;
			}
			catch (InvalidCastException) {
				_internal.Activate(typeof(IAudioClient).GUID, (uint)CLSCTX.ALL, IntPtr.Zero, out var client);
				_client = (IAudioClient)client;
			}
			_client?.GetDevicePeriod(out m_defaultBufferDuration, out m_minimumBufferDuration);
		}

		/// <inheritdoc />
		public AudioClient Connect(WaveFormat format, int bufferSize = 0, AudioUsage usage = AudioUsage.Media, AudioShareMode shareMode = AudioShareMode.Shared) {
			format.ValidateChannelMask();
			if (_client == null)
				throw new InvalidOperationException("The device is not available.");
			return new AudioClientWrapper(_client, this, format, bufferSize, usage, shareMode);
		}

		static AUDCLNT_SHAREMODE ToInternalShareModeEnum(AudioShareMode value) => value switch {
			AudioShareMode.Shared => AUDCLNT_SHAREMODE.SHARED,
			AudioShareMode.Exclusive => AUDCLNT_SHAREMODE.EXCLUSIVE,
			_ => throw new ArgumentOutOfRangeException(nameof(value)),
		};
	}
}
