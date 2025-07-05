using System;

namespace Cryville.Audio {
	/// <summary>
	/// Audio channel mask describing the samples and their arrangement in the audio frame.
	/// </summary>
	[Flags]
	public enum ChannelMask {
#pragma warning disable format
		/// <summary>
		/// Front left (FL) channel.
		/// </summary>
		FrontLeft          = 0x00000001,
		/// <summary>
		/// Front right (FR) channel.
		/// </summary>
		FrontRight         = 0x00000002,
		/// <summary>
		/// Front center (FC) channel.
		/// </summary>
		FrontCenter        = 0x00000004,
		/// <summary>
		/// "Low frequency effect" (LFE) channel.
		/// </summary>
		LowFrequency       = 0x00000008,
		/// <summary>
		/// Back left (BL) channel.
		/// </summary>
		BackLeft           = 0x00000010,
		/// <summary>
		/// Back right (BR) channel.
		/// </summary>
		BackRight          = 0x00000020,
		/// <summary>
		/// Front left of center channel.
		/// </summary>
		FrontLeftOfCenter  = 0x00000040,
		/// <summary>
		/// Front right of center channel.
		/// </summary>
		FrontRightOfCenter = 0x00000080,
		/// <summary>
		/// Back center (BC) channel.
		/// </summary>
		BackCenter         = 0x00000100,
		/// <summary>
		/// Side left (SL) channel.
		/// </summary>
		SideLeft           = 0x00000200,
		/// <summary>
		/// Side right (SR) channel.
		/// </summary>
		SideRight          = 0x00000400,
		/// <summary>
		/// Top center (TC) channel.
		/// </summary>
		TopCenter          = 0x00000800,
		/// <summary>
		/// Top front left (TFL) channel.
		/// </summary>
		TopFrontLeft       = 0x00001000,
		/// <summary>
		/// Top front center (TFC) channel.
		/// </summary>
		TopFrontCenter     = 0x00002000,
		/// <summary>
		/// Top front right (TFR) channel.
		/// </summary>
		TopFrontRight      = 0x00004000,
		/// <summary>
		/// Top back left (TBL) channel.
		/// </summary>
		TopBackLeft        = 0x00008000,
		/// <summary>
		/// Top back center (TBC) channel.
		/// </summary>
		TopBackCenter      = 0x00010000,
		/// <summary>
		/// Top back right (TBR) channel.
		/// </summary>
		TopBackRight       = 0x00020000,
		/// <summary>
		/// Top side left (TSL) channel.
		/// </summary>
		TopSideLeft        = 0x00040000,
		/// <summary>
		/// Top side right (TSR) channel.
		/// </summary>
		TopSideRight       = 0x00080000,
		/// <summary>
		/// Bottom front left (BFL) channel.
		/// </summary>
		BottomFrontLeft    = 0x00100000,
		/// <summary>
		/// Bottom front center (BFC) channel.
		/// </summary>
		BottomFrontCenter  = 0x00200000,
		/// <summary>
		/// Bottom front right (BFR) channel.
		/// </summary>
		BottomFrontRight   = 0x00400000,
		/// <summary>
		/// The second "low frequency effect" (LFE) channel.
		/// </summary>
		LowFrequency2      = 0x00800000,
		/// <summary>
		/// Front wide left (FWL) channel.
		/// </summary>
		FrontWideLeft      = 0x01000000,
		/// <summary>
		/// Front wide right (FWR) channel.
		/// </summary>
		FrontWideRight     = 0x02000000,

		/// <summary>
		/// Mono channel layout.
		/// </summary>
		Mono      = FrontCenter,
		/// <summary>
		/// Stereo channel layout.
		/// </summary>
		Stereo    = FrontLeft | FrontRight,
		/// <summary>
		/// 3 channel layout.
		/// </summary>
		Tri       = Stereo | FrontCenter,
		/// <summary>
		/// 3 channel layout, with the third channel placed on the back.
		/// </summary>
		TriBack   = Stereo | BackCenter,
		/// <summary>
		/// 4 channel layout.
		/// </summary>
		Four      = Tri | BackCenter,
		/// <summary>
		/// Quad channel layout.
		/// </summary>
		Quad      = Stereo | BackLeft | BackRight,
		/// <summary>
		/// 5 channel layout, with surround channels placed on the back.
		/// </summary>
		FiveBack  = Tri | BackLeft | BackRight,
		/// <summary>
		/// 5 channel layout, with surround channels placed on the side.
		/// </summary>
		FiveSide  = Tri | SideLeft | SideRight,
		/// <summary>
		/// 6 channel layout, with surround channels placed on the back.
		/// </summary>
		SixBack   = FiveBack | BackCenter,
		/// <summary>
		/// 6 channel layout, with surround channels placed on the side.
		/// </summary>
		SixSide   = FiveSide | BackCenter,
		/// <summary>
		/// 7 channel layout.
		/// </summary>
		Seven     = FiveBack | SideLeft | SideRight,
		/// <summary>
		/// Octagonal channel layout.
		/// </summary>
		Octagonal = Seven | BackCenter,
		/// <summary>
		/// 9 channel layout.
		/// </summary>
		Nine      = Seven | FrontWideLeft | FrontWideRight,

		/// <summary>
		/// x.1 channel layout, adding one LFE channel.
		/// </summary>
		LFPoint1 = LowFrequency,
		/// <summary>
		/// x.2 channel layout, adding two LFE channels.
		/// </summary>
		LFPoint2 = LowFrequency | LowFrequency2,

		/// <summary>
		/// x.2 channel layout, adding two top front channels.
		/// </summary>
		TopPoint2Front = TopFrontLeft | TopFrontRight,
		/// <summary>
		/// x.2 channel layout, adding two top side channels.
		/// </summary>
		TopPoint2Side = TopSideLeft | TopSideRight,
		/// <summary>
		/// x.4 channel layout, adding four top channels.
		/// </summary>
		TopPoint4 = TopPoint2Front | TopBackLeft | TopBackRight,
		/// <summary>
		/// x.6 channel layout, adding six top channels.
		/// </summary>
		TopPoint6 = TopPoint2Side | TopPoint4,
#pragma warning restore format
	}
}
