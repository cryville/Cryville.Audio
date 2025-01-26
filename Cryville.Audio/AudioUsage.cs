using System.Diagnostics.CodeAnalysis;

namespace Cryville.Audio {
	/// <summary>
	/// Audio usage.
	/// </summary>
	[SuppressMessage("CodeQuality", "IDE0079", Justification = "False report")]
	[SuppressMessage("Design", "CA1027", Justification = "Not flags")]
	public enum AudioUsage {
		/// <summary>
		/// Unknown usage.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// The usage is media, such as music, or movie soundtracks.
		/// </summary>
		Media = 1,
		/// <summary>
		/// The usage is voice communications, such as telephony or VoIP.
		/// </summary>
		Communication = 2,
		/// <summary>
		/// The usage is an alarm (e.g. wake-up alarm).
		/// </summary>
		Alarm = 3,

		/// <summary>
		/// The usage is notification.
		/// </summary>
		Notification = 4,
		/// <summary>
		/// The usage is telephony ringtone.
		/// </summary>
		NotificationRingtone,
		/// <summary>
		/// The usage is to attract the user's attention, such as a reminder or low battery warning.
		/// </summary>
		NotificationEvent,

		/// <summary>
		/// The usage is for accessibility, such as with a screen reader.
		/// </summary>
		AssistanceAccessibility = 8,
		/// <summary>
		/// The usage is driving or navigation directions.
		/// </summary>
		AssistanceNavigation,
		/// <summary>
		/// The usage is sonification, such as with user interface sounds.
		/// </summary>
		AssistanceSonification,

		/// <summary>
		/// The usage is for game audio.
		/// </summary>
		Game = 16,
	}
}
