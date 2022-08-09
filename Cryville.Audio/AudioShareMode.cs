namespace Cryville.Audio {
	/// <summary>
	/// The share mode of an audio connection.
	/// </summary>
	public enum AudioShareMode {
		/// <summary>
		/// The device is shared with other connections, at the cost of a higher latency than <see cref="Exclusive"/>. The output data is mixed by the audio service.
		/// </summary>
		Shared,
		/// <summary>
		/// The device is exclusive to the current connection, providing a low latency.
		/// </summary>
		/// <remarks>To initialize an exclusive connection, the device must allow exclusive mode and must not be being used in either modes at the moment.</remarks>
		Exclusive,
	}
}
