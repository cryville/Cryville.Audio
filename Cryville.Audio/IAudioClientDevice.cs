namespace Cryville.Audio {
	/// <summary>
	/// Represents an <see cref="IAudioDevice" /> that holds an underlying "client" as well.
	/// </summary>
	public interface IAudioClientDevice : IAudioDevice {
		/// <summary>
		/// When implemented, reactivates the underlying "client" if it is used in the device class.
		/// </summary>
		/// <remarks>
		/// <para>This method serves as a fast path for reconnection. When reconnecting, this method should be called before calling <see cref="IAudioDevice.Connect(WaveFormat, int, AudioUsage, AudioShareMode)" />. If that even fails, the caller will have to recreate the device.</para>
		/// </remarks>
		void ReactivateClient();
	}
}
