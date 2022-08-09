using System;
using System.Collections.Generic;

namespace Cryville.Audio {
	/// <summary>
	/// Audio device manager that manages <see cref="IAudioDevice" />.
	/// </summary>
	public interface IAudioDeviceManager : IDisposable {
		/// <summary>
		/// Whether this API is supported.
		/// </summary>
		bool IsSupported { get; }

		/// <summary>
		/// Gets all audio devices for the specified <paramref name="dataFlow" />.
		/// </summary>
		/// <param name="dataFlow">The data-flow direction.</param>
		IEnumerable<IAudioDevice> GetDevices(DataFlow dataFlow);

		/// <summary>
		/// Gets the default audio device for the specified <paramref name="dataFlow" />.
		/// </summary>
		/// <param name="dataFlow">The data-flow direction.</param>
		IAudioDevice GetDefaultDevice(DataFlow dataFlow);
	}
}
