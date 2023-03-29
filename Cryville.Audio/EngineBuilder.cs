using System;
using System.Collections.ObjectModel;

namespace Cryville.Audio {
	/// <summary>
	/// The recommended entry for Cryville.Audio that creates an <see cref="IAudioDeviceManager" />.
	/// </summary>
	public static class EngineBuilder {
		/// <summary>
		/// The list of available engines.
		/// </summary>
		public static readonly Collection<Type> Engines = new Collection<Type> {
			typeof(Wasapi.MMDeviceEnumerator),
			typeof(OpenSL.Engine),
			typeof(WinMM.WaveDeviceManager),
		};

		/// <summary>
		/// Creates a recommended <see cref="IAudioDeviceManager" />.
		/// </summary>
		/// <returns>A recommended <see cref="IAudioDeviceManager" />. <see langword="null" /> if no engine is supported.</returns>
		public static IAudioDeviceManager Create() {
			foreach (var type in Engines) {
				if (!typeof(IAudioDeviceManager).IsAssignableFrom(type)) continue;
				try {
					return (IAudioDeviceManager)Activator.CreateInstance(type);
				}
				catch (Exception) { }
			}
			return null;
		}
	}
}
