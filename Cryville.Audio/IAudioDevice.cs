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
		/// The default buffer duration of the device in milliseconds.
		/// </summary>
		float DefaultBufferDuration { get; }

		/// <summary>
		/// The minimum buffer duration of the device in milliseconds.
		/// </summary>
		float MinimumBufferDuration { get; }

		/// <summary>
		/// The default wave format of the device.
		/// </summary>
		WaveFormat DefaultFormat { get; }

		/// <summary>
		/// Gets whether <paramref name="format" /> is supported by the device.
		/// </summary>
		/// <param name="format">The specified wave format.</param>
		/// <param name="suggestion">A wave format suggested by the device. <paramref name="format" /> if it is supported. <see langword="null" /> if no format is supported.</param>
		/// <param name="shareMode">The share mode.</param>
		/// <returns>Whether <paramref name="format" /> is supported.</returns>
		bool IsFormatSupported(WaveFormat format, out WaveFormat? suggestion, AudioShareMode shareMode = AudioShareMode.Shared);

		/// <summary>
		/// Connects to the device.
		/// </summary>
		/// <param name="format">The wave format.</param>
		/// <param name="bufferDuration">The buffer duration of the connection in milliseconds.</param>
		/// <param name="shareMode">The share mode of the connection.</param>
		/// <returns>An <see cref="AudioClient" /> for interaction with the device.</returns>
		AudioClient Connect(WaveFormat format, float bufferDuration = 0, AudioShareMode shareMode = AudioShareMode.Shared);
	}
}
