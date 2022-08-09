using Cryville.Audio.Common.Platform;
using System;
using System.Collections.Generic;
using CAudioStreamBuilder = Oboe.AudioStreamBuilder;

namespace Cryville.Audio.Oboe {
	public class AudioStreamBuilder : ExternalClassWrapper<CAudioStreamBuilder>, IAudioDeviceManager {
		public AudioStreamBuilder() : base(new CAudioStreamBuilder()) { }

		internal new CAudioStreamBuilder Internal => base.Internal;

		public bool IsSupported => false;

		public IAudioDevice GetDefaultDevice(DataFlow dataFlow) {
			return new AudioDevice(this, dataFlow);
		}

		public IEnumerable<IAudioDevice> GetDevices(DataFlow dataFlow) {
			throw new NotImplementedException();
		}
	}
}
