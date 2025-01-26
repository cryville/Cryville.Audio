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
		/// The burst size of the device in frames.
		/// </summary>
		int BurstSize { get; }

		/// <summary>
		/// The minimum buffer size of the device in frames.
		/// </summary>
		int MinimumBufferSize { get; }

		/// <summary>
		/// The default buffer size of the device in frames.
		/// </summary>
		int DefaultBufferSize { get; }

		/// <summary>
		/// The default wave format of the device for shared-mode streams.
		/// </summary>
		/// <remarks>
		/// <para>For exclusive-mode streams, call <see cref="IsFormatSupported(WaveFormat, out WaveFormat?, AudioUsage, AudioShareMode)" /> to determine an eligible format.</para>
		/// </remarks>
		WaveFormat DefaultFormat { get; }

		/// <summary>
		/// Gets whether <paramref name="format" /> is supported by the device.
		/// </summary>
		/// <param name="format">The specified wave format.</param>
		/// <param name="suggestion">A wave format suggested by the device. <paramref name="format" /> if it is supported. <see langword="null" /> if no format is supported.</param>
		/// <param name="usage">The audio usage.</param>
		/// <param name="shareMode">The share mode.</param>
		/// <returns>Whether <paramref name="format" /> is supported.</returns>
		bool IsFormatSupported(WaveFormat format, out WaveFormat? suggestion, AudioUsage usage = AudioUsage.Media, AudioShareMode shareMode = AudioShareMode.Shared);

		/// <summary>
		/// Connects to the device.
		/// </summary>
		/// <param name="format">The wave format.</param>
		/// <param name="bufferSize">The buffer size of the connection in frames.</param>
		/// <param name="usage">The audio usage.</param>
		/// <param name="shareMode">The share mode of the connection.</param>
		/// <returns>An <see cref="AudioClient" /> for interaction with the device.</returns>
		AudioClient Connect(WaveFormat format, int bufferSize = 0, AudioUsage usage = AudioUsage.Media, AudioShareMode shareMode = AudioShareMode.Shared);
	}
}
