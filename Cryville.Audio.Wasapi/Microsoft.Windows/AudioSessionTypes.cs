using System;

namespace Microsoft.Windows.AudioSessionTypes {
	[Flags]
	internal enum AUDCLNT_SESSIONFLAGS : UInt32 {
		EXPIREWHENUNOWNED       = 0x10000000,
		DISPLAY_HIDE            = 0x20000000,
		DISPLAY_HIDEWHENEXPIRED = 0x40000000,
	}
	internal enum AUDCLNT_SHAREMODE : UInt32 {
		SHARED,
		EXCLUSIVE
	}
	[Flags]
	internal enum AUDCLNT_STREAMFLAGS : UInt32 {
		CROSSPROCESS        = 0x00010000,
		LOOPBACK            = 0x00020000,
		EVENTCALLBACK       = 0x00040000,
		NOPERSIST           = 0x00080000,
		RATEADJUST          = 0x00100000,
		SRC_DEFAULT_QUALITY = 0x08000000,
		AUTOCONVERTPCM      = 0x80000000,
	}
	enum AUDIO_STREAM_CATEGORY {
		Other = 0,
		ForegroundOnlyMedia = 1,
		BackgroundCapableMedia = 2,
		Communications = 3,
		Alerts = 4,
		SoundEffects = 5,
		GameEffects = 6,
		GameMedia = 7,
		GameChat = 8,
		Speech = 9,
		Movie = 10,
		Media = 11,
		FarFieldSpeech = 12,
		UniformSpeech = 13,
		VoiceTyping = 14,
	}
}
