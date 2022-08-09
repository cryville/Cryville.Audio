namespace Cryville.Audio {
	/// <summary>
	/// The data-flow direction of an audio connection.
	/// </summary>
	public enum DataFlow {
		/// <summary>
		/// Data flows from software to hardware.
		/// </summary>
		Out,
		/// <summary>
		/// Data flows from hardware to software.
		/// </summary>
		In,
		/// <summary>
		/// Any data-flow direction.
		/// </summary>
		All,
	}
}
