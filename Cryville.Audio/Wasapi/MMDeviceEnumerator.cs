using Cryville.Common.Platform.Windows;
using Microsoft.Windows.AudioClient;
using Microsoft.Windows.MMDevice;
using System;
using System.Collections.Generic;

namespace Cryville.Audio.Wasapi {
	/// <summary>
	/// An <see cref="IAudioDeviceManager" /> that interact with Wasapi.
	/// </summary>
	/// <remarks>
	/// <c>Cryville.Audio.WasapiWrapper.dll</c> is required.
	/// </remarks>
	public class MMDeviceEnumerator : ComInterfaceWrapper, IAudioDeviceManager {
		/// <summary>
		/// Creates an instance of the <see cref="MMDeviceEnumerator" /> class.
		/// </summary>
		public MMDeviceEnumerator() : base(CoCreate()) { }

		static IntPtr CoCreate() {
			IMMDeviceEnumerator._ctor(out var result);
			return result;
		}
		
		/// <inheritdoc />
		public IEnumerable<IAudioDevice> GetDevices(DataFlow dataFlow) {
			IMMDeviceEnumerator.EnumAudioEndpoints(
				ComObject,
				Util.ToInternalDataFlowEnum(dataFlow),
				(uint)DEVICE_STATE_XXX.DEVICE_STATEMASK_ALL,
				out var result
			);
			return new MMDeviceCollection(result);
		}

		/// <inheritdoc />
		public IAudioDevice GetDefaultDevice(DataFlow dataFlow) {
			IMMDeviceEnumerator.GetDefaultAudioEndpoint(
				ComObject,
				Util.ToInternalDataFlowEnum(dataFlow),
				ERole.eConsole,
				out var result
			);
			return new MMDevice(result);
		}
	}
}
