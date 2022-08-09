using System;

namespace Cryville.Audio {
	/// <summary>
	/// Audio device.
	/// </summary>
	public interface IAudioDevice : IDisposable {
		/// <summary>
		/// The friendly name of the device.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The data-flow direction of the device.
		/// </summary>
		DataFlow DataFlow { get; }

		/// <summary>
		/// Connects to the device.
		/// </summary>
		/// <returns>An <see cref="AudioClient" /> for interaction with the device.</returns>
		AudioClient Connect();
	}
}
