using Cryville.Common.Platform.Windows;
using Microsoft.Windows.AudioClient;
using Microsoft.Windows.MMDevice;
using System.Collections.Generic;

namespace Cryville.Audio.Wasapi {
	/// <summary>
	/// An <see cref="IAudioDeviceManager" /> that interact with Wasapi.
	/// </summary>
	public class MMDeviceEnumeratorWrapper : ComInterfaceWrapper, IAudioDeviceManager {
		readonly IMMDeviceEnumerator _internal;

		/// <summary>
		/// Creates an instance of the <see cref="MMDeviceEnumeratorWrapper" /> class.
		/// </summary>
		public MMDeviceEnumeratorWrapper() : base(new IMMDeviceEnumerator()) {
			_internal = (IMMDeviceEnumerator)ComObject;
		}

		/// <inheritdoc />
		public IEnumerable<IAudioDevice> GetDevices(DataFlow dataFlow) {
			_internal.EnumAudioEndpoints(
				Util.ToInternalDataFlowEnum(dataFlow),
				(uint)DEVICE_STATE_XXX.DEVICE_STATEMASK_ALL,
				out var result
			);
			return new MMDeviceCollectionWrapper(result);
		}

		/// <inheritdoc />
		public IAudioDevice GetDefaultDevice(DataFlow dataFlow) {
			_internal.GetDefaultAudioEndpoint(
				Util.ToInternalDataFlowEnum(dataFlow),
				ERole.eConsole,
				out var result
			);
			return new MMDeviceWrapper(result);
		}
	}
}
