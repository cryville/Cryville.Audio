using Microsoft.Windows.AudioClient;
using Microsoft.Windows.MMDevice;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cryville.Audio.Wasapi {
	/// <summary>
	/// An <see cref="IAudioDeviceManager" /> that interact with Wasapi.
	/// </summary>
	public class MMDeviceEnumeratorWrapper : IAudioDeviceManager {
		readonly IMMDeviceEnumerator _internal = new();

		/// <summary>
		/// Creates an instance of the <see cref="MMDeviceEnumeratorWrapper" /> class.
		/// </summary>
		public MMDeviceEnumeratorWrapper() {
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				throw new PlatformNotSupportedException();
		}

		/// <inheritdoc />
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing">Whether the method is being called by user.</param>
		protected virtual void Dispose(bool disposing) {
			if (!disposing) return;
			if (_internal != null) {
				Marshal.ReleaseComObject(_internal);
			}
		}

		/// <inheritdoc />
		public IEnumerable<IAudioDevice> GetDevices(DataFlow dataFlow) {
			_internal.EnumAudioEndpoints(
				Helpers.ToInternalDataFlowEnum(dataFlow),
				(uint)DEVICE_STATE_XXX.DEVICE_STATEMASK_ALL,
				out var result
			);
			return new MMDeviceCollectionWrapper(result);
		}

		/// <inheritdoc />
		public IAudioDevice GetDefaultDevice(DataFlow dataFlow) {
			_internal.GetDefaultAudioEndpoint(
				Helpers.ToInternalDataFlowEnum(dataFlow),
				ERole.eConsole,
				out var result
			);
			return new MMDeviceWrapper(result);
		}
	}
}
