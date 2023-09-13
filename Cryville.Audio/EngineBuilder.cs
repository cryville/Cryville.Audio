using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Cryville.Audio {
	/// <summary>
	/// The recommended entry for Cryville.Audio that creates an <see cref="IAudioDeviceManager" />.
	/// </summary>
	public static class EngineBuilder {
		/// <summary>
		/// The list of available engines.
		/// </summary>
		public static readonly IList<Type> Engines = new Collection<Type> { };

		/// <summary>
		/// Creates a <see cref="IAudioDeviceManager" /> in the <see cref="Engines" /> list.
		/// </summary>
		/// <returns>The first <see cref="IAudioDeviceManager" /> that can be successfully created. <see langword="null" /> if no engine is supported.</returns>
		/// <remarks>
		/// <para>Add engines to <see cref="Engines" /> before calling this method.</para>
		/// </remarks>
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
