using Cryville.Common.Platform.Windows;
using Microsoft.Windows.AudioClient;
using Microsoft.Windows.MMDevice;
using System;
using System.Collections.Generic;
using CMMDeviceEnumerator = Microsoft.Windows.MMDevice.MMDeviceEnumerator;

namespace Cryville.Audio.Wasapi {
	public class MMDeviceEnumerator : ComInterfaceWrapper<IMMDeviceEnumerator>, IAudioDeviceManager {
		public MMDeviceEnumerator() : base(new CMMDeviceEnumerator() as IMMDeviceEnumerator) { }

		public bool IsSupported
			=> Environment.OSVersion.Platform == PlatformID.Win32NT
			&& Environment.OSVersion.Version.Major >= 6;

		public IEnumerable<IAudioDevice> GetDevices(DataFlow dataFlow) {
			ComObject.EnumAudioEndpoints(
				Util.ToInternalDataFlowEnum(dataFlow),
				(uint)DEVICE_STATE_XXX.DEVICE_STATEMASK_ALL,
				out var result
			);
			return new MMDeviceCollection(result);
		}

		public IAudioDevice GetDefaultDevice(DataFlow dataFlow) {
			ComObject.GetDefaultAudioEndpoint(
				Util.ToInternalDataFlowEnum(dataFlow),
				ERole.eConsole,
				out var result
			);
			return new MMDevice(result);
		}
	}
}
