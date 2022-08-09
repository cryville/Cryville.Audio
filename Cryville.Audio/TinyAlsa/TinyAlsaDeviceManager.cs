using System;
using System.Collections.Generic;

namespace Cryville.Audio.TinyAlsa {
	public class TinyAlsaDeviceManager : IAudioDeviceManager {
		public bool IsSupported => Environment.OSVersion.Platform == PlatformID.Unix;

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) { }

		public IAudioDevice GetDefaultDevice(DataFlow dataFlow) {
			return new TinyAlsaDevice(0, 0, dataFlow);
		}

		public IEnumerable<IAudioDevice> GetDevices(DataFlow dataFlow) {
			throw new NotImplementedException();
		}
	}
}
