namespace Cryville.Audio {
	/// <summary>
	/// Status of the <see cref="AudioClient" />.
	/// </summary>
	public enum AudioClientStatus {
		/// <summary>
		/// The <see cref="AudioClient" /> is open but not yet started, or paused.
		/// </summary>
		Idle,
		/// <summary>
		/// The <see cref="AudioClient" /> is being started.
		/// </summary>
		Starting,
		/// <summary>
		/// The <see cref="AudioClient" /> is started and playing.
		/// </summary>
		Playing,
		/// <summary>
		/// The <see cref="AudioClient" /> is being paused.
		/// </summary>
		Pausing,
		/// <summary>
		/// The <see cref="AudioClient" /> is being closed.
		/// </summary>
		Closing,
		/// <summary>
		/// The <see cref="AudioClient" /> is closed.
		/// </summary>
		Closed,
	}
}
