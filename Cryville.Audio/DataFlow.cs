using System;

namespace Cryville.Audio {
	/// <summary>
	/// The data-flow direction of an audio connection.
	/// </summary>
	[Flags]
	public enum DataFlow {
		/// <summary>
		/// None.
		/// </summary>
		None = 0,
		/// <summary>
		/// Data flows from software to hardware.
		/// </summary>
		Out = 1,
		/// <summary>
		/// Data flows from hardware to software.
		/// </summary>
		In = 2,
		/// <summary>
		/// Any data-flow direction.
		/// </summary>
		All = 3,
	}
}
