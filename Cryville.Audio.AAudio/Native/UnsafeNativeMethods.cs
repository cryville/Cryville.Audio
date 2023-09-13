using System;
using System.Runtime.InteropServices;

#pragma warning disable IDE1006
namespace Cryville.Audio.AAudio.Native {
	internal static class UnsafeNativeMethods {
		private const string LibraryName = "aaudio";

		[DllImport(LibraryName, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string AAudio_convertResultToText(aaudio_result_t returnCode);
		[DllImport(LibraryName, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string AAudio_convertStreamStateToText(aaudio_stream_state_t state);
		[DllImport(LibraryName)]
		public static extern void AAudio_createStreamBuilder(out IntPtr builder);
		#region AAudioStreamBuilder
		[DllImport(LibraryName)]
		public static extern aaudio_result_t AAudioStreamBuilder_delete(IntPtr builder);
		[DllImport(LibraryName)]
		public static extern aaudio_result_t AAudioStreamBuilder_openStream(IntPtr builder, out IntPtr stream);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setAllowedCapturePolicy(IntPtr builder, aaudio_allowed_capture_policy_t capturePolicy);
		[DllImport(LibraryName, CharSet = CharSet.Unicode)]
		public static extern void AAudioStreamBuilder_setAttributionTag(IntPtr builder, [MarshalAs(UnmanagedType.LPWStr)] string attributionTag);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setBufferCapacityInFrames(IntPtr builder, int numFrames);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setChannelCount(IntPtr builder, int channelCount);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setChannelMask(IntPtr builder, aaudio_channel_mask_t channelMask);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setContentType(IntPtr builder, aaudio_content_type_t contentType);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setDataCallback(IntPtr builder, AAudioStream_dataCallback callback, IntPtr userData);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setDeviceId(IntPtr builder, int deviceId);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setDirection(IntPtr builder, aaudio_direction_t direction);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setErrorCallback(IntPtr builder, AAudioStream_errorCallback callback, IntPtr userData);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setFormat(IntPtr builder, aaudio_format_t format);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setFramesPerDataCallback(IntPtr builder, int numFrames);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setInputPreset(IntPtr builder, aaudio_input_preset_t inputPreset);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setIsContentSpatialized(IntPtr builder, bool isSpatialized);
		[DllImport(LibraryName, CharSet = CharSet.Unicode)]
		public static extern void AAudioStreamBuilder_setPackageName(IntPtr builder, [MarshalAs(UnmanagedType.LPWStr)] string packageName);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setPerformanceMode(IntPtr builder, aaudio_performance_mode_t mode);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setPrivacySensitive(IntPtr builder, bool privacySensitive);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setSampleRate(IntPtr builder, int sampleRate);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setSamplesPerFrame(IntPtr builder, int samplesPerFrame);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setSessionId(IntPtr builder, aaudio_session_id_t sessionId);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setSharingMode(IntPtr builder, aaudio_sharing_mode_t sharingMode);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setSpatializationBehavior(IntPtr builder, aaudio_spatialization_behavior_t spatializationBehavior);
		[DllImport(LibraryName)]
		public static extern void AAudioStreamBuilder_setUsage(IntPtr builder, aaudio_usage_t usage);
		#endregion
		#region AAudioStream
		[DllImport(LibraryName)]
		public static extern aaudio_result_t AAudioStream_close(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern aaudio_allowed_capture_policy_t AAudioStream_getAllowedCapturePolicy(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern int AAudioStream_getBufferCapacityInFrames(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern int AAudioStream_getBufferSizeInFrames(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern int AAudioStream_getChannelCount(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern aaudio_channel_mask_t AAudioStream_getChannelMask(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern aaudio_content_type_t AAudioStream_getContentType(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern int AAudioStream_getDeviceId(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern aaudio_direction_t AAudioStream_getDirection(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern aaudio_format_t AAudioStream_getFormat(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern int AAudioStream_getFramesPerBurst(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern int AAudioStream_getFramesPerDataCallback(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern long AAudioStream_getFramesRead(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern long AAudioStream_getFramesWritten(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern aaudio_input_preset_t AAudioStream_getInputPreset(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern aaudio_performance_mode_t AAudioStream_getPerformanceMode(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern int AAudioStream_getSampleRate(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern int AAudioStream_getSamplesPerFrame(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern aaudio_session_id_t AAudioStream_getSessionId(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern aaudio_sharing_mode_t AAudioStream_getSharingMode(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern aaudio_spatialization_behavior_t AAudioStream_getSpatializationBehavior(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern aaudio_stream_state_t AAudioStream_getState(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern aaudio_result_t AAudioStream_getTimestamp(IntPtr stream, clockid_t clockid, out long framePosition, out long timeNanoseconds);
		[DllImport(LibraryName)]
		public static extern aaudio_usage_t AAudioStream_getUsage(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern int AAudioStream_getXRunCount(IntPtr stream);
		[DllImport(LibraryName)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool AAudioStream_isContentSpatialized(IntPtr stream);
		[DllImport(LibraryName)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool AAudioStream_isPrivacySensitive(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern aaudio_result_t AAudioStream_read(IntPtr stream, [MarshalAs(UnmanagedType.LPArray)] byte[] buffer, int numFrames, long timeoutNanoseconds);
		[DllImport(LibraryName)]
		public static extern aaudio_result_t AAudioStream_release(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern aaudio_result_t AAudioStream_requestFlush(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern aaudio_result_t AAudioStream_requestPause(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern aaudio_result_t AAudioStream_requestStart(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern aaudio_result_t AAudioStream_requestStop(IntPtr stream);
		[DllImport(LibraryName)]
		public static extern aaudio_result_t AAudioStream_setBufferSizeInFrames(IntPtr stream, int numFrames);
		[DllImport(LibraryName)]
		public static extern aaudio_result_t AAudioStream_waitForStateChange(IntPtr stream, aaudio_stream_state_t inputState, out aaudio_stream_state_t nextState, long timeoutNanoseconds);
		[DllImport(LibraryName)]
		public static extern aaudio_result_t AAudioStream_write(IntPtr stream, [MarshalAs(UnmanagedType.LPArray)] byte[] buffer, int numFrames, long timeoutNanoseconds);
		#endregion
	}
	internal delegate aaudio_data_callback_result_t AAudioStream_dataCallback(IntPtr stream, IntPtr userData, IntPtr audioData, int numFrames);
	internal delegate void AAudioStream_errorCallback(IntPtr stream, IntPtr userData, aaudio_result_t error);
	internal enum aaudio_allowed_capture_policy_t : int {
		AAUDIO_ALLOW_CAPTURE_BY_ALL = 1,
		AAUDIO_ALLOW_CAPTURE_BY_SYSTEM = 2,
		AAUDIO_ALLOW_CAPTURE_BY_NONE = 3,
	}
	internal enum aaudio_channel_mask_t : int {
		AAUDIO_CHANNEL_INVALID = -1,
		AAUDIO_UNSPECIFIED = 0,
		AAUDIO_CHANNEL_FRONT_LEFT = 1 << 0,
		AAUDIO_CHANNEL_FRONT_RIGHT = 1 << 1,
		AAUDIO_CHANNEL_FRONT_CENTER = 1 << 2,
		AAUDIO_CHANNEL_LOW_FREQUENCY = 1 << 3,
		AAUDIO_CHANNEL_BACK_LEFT = 1 << 4,
		AAUDIO_CHANNEL_BACK_RIGHT = 1 << 5,
		AAUDIO_CHANNEL_FRONT_LEFT_OF_CENTER = 1 << 6,
		AAUDIO_CHANNEL_FRONT_RIGHT_OF_CENTER = 1 << 7,
		AAUDIO_CHANNEL_BACK_CENTER = 1 << 8,
		AAUDIO_CHANNEL_SIDE_LEFT = 1 << 9,
		AAUDIO_CHANNEL_SIDE_RIGHT = 1 << 10,
		AAUDIO_CHANNEL_TOP_CENTER = 1 << 11,
		AAUDIO_CHANNEL_TOP_FRONT_LEFT = 1 << 12,
		AAUDIO_CHANNEL_TOP_FRONT_CENTER = 1 << 13,
		AAUDIO_CHANNEL_TOP_FRONT_RIGHT = 1 << 14,
		AAUDIO_CHANNEL_TOP_BACK_LEFT = 1 << 15,
		AAUDIO_CHANNEL_TOP_BACK_CENTER = 1 << 16,
		AAUDIO_CHANNEL_TOP_BACK_RIGHT = 1 << 17,
		AAUDIO_CHANNEL_TOP_SIDE_LEFT = 1 << 18,
		AAUDIO_CHANNEL_TOP_SIDE_RIGHT = 1 << 19,
		AAUDIO_CHANNEL_BOTTOM_FRONT_LEFT = 1 << 20,
		AAUDIO_CHANNEL_BOTTOM_FRONT_CENTER = 1 << 21,
		AAUDIO_CHANNEL_BOTTOM_FRONT_RIGHT = 1 << 22,
		AAUDIO_CHANNEL_LOW_FREQUENCY_2 = 1 << 23,
		AAUDIO_CHANNEL_FRONT_WIDE_LEFT = 1 << 24,
		AAUDIO_CHANNEL_FRONT_WIDE_RIGHT = 1 << 25,
		AAUDIO_CHANNEL_MONO = AAUDIO_CHANNEL_FRONT_LEFT,
		AAUDIO_CHANNEL_STEREO = AAUDIO_CHANNEL_FRONT_LEFT | AAUDIO_CHANNEL_FRONT_RIGHT,
		AAUDIO_CHANNEL_2POINT1 = AAUDIO_CHANNEL_FRONT_LEFT | AAUDIO_CHANNEL_FRONT_RIGHT | AAUDIO_CHANNEL_LOW_FREQUENCY,
		AAUDIO_CHANNEL_TRI = AAUDIO_CHANNEL_FRONT_LEFT | AAUDIO_CHANNEL_FRONT_RIGHT | AAUDIO_CHANNEL_FRONT_CENTER,
		AAUDIO_CHANNEL_TRI_BACK = AAUDIO_CHANNEL_FRONT_LEFT | AAUDIO_CHANNEL_FRONT_RIGHT | AAUDIO_CHANNEL_BACK_CENTER,
		AAUDIO_CHANNEL_3POINT1 = AAUDIO_CHANNEL_FRONT_LEFT | AAUDIO_CHANNEL_FRONT_RIGHT | AAUDIO_CHANNEL_FRONT_CENTER | AAUDIO_CHANNEL_LOW_FREQUENCY,
		AAUDIO_CHANNEL_2POINT0POINT2 = AAUDIO_CHANNEL_FRONT_LEFT | AAUDIO_CHANNEL_FRONT_RIGHT | AAUDIO_CHANNEL_TOP_SIDE_LEFT | AAUDIO_CHANNEL_TOP_SIDE_RIGHT,
		AAUDIO_CHANNEL_2POINT1POINT2 = AAUDIO_CHANNEL_2POINT0POINT2 | AAUDIO_CHANNEL_LOW_FREQUENCY,
		AAUDIO_CHANNEL_3POINT0POINT2 = AAUDIO_CHANNEL_FRONT_LEFT | AAUDIO_CHANNEL_FRONT_RIGHT | AAUDIO_CHANNEL_FRONT_CENTER | AAUDIO_CHANNEL_TOP_SIDE_LEFT | AAUDIO_CHANNEL_TOP_SIDE_RIGHT,
		AAUDIO_CHANNEL_3POINT1POINT2 = AAUDIO_CHANNEL_3POINT0POINT2 | AAUDIO_CHANNEL_LOW_FREQUENCY,
		AAUDIO_CHANNEL_QUAD = AAUDIO_CHANNEL_FRONT_LEFT | AAUDIO_CHANNEL_FRONT_RIGHT | AAUDIO_CHANNEL_BACK_LEFT | AAUDIO_CHANNEL_BACK_RIGHT,
		AAUDIO_CHANNEL_QUAD_SIDE = AAUDIO_CHANNEL_FRONT_LEFT | AAUDIO_CHANNEL_FRONT_RIGHT | AAUDIO_CHANNEL_SIDE_LEFT | AAUDIO_CHANNEL_SIDE_RIGHT,
		AAUDIO_CHANNEL_SURROUND = AAUDIO_CHANNEL_FRONT_LEFT | AAUDIO_CHANNEL_FRONT_RIGHT | AAUDIO_CHANNEL_FRONT_CENTER | AAUDIO_CHANNEL_BACK_CENTER,
		AAUDIO_CHANNEL_PENTA = AAUDIO_CHANNEL_QUAD | AAUDIO_CHANNEL_FRONT_CENTER,
		AAUDIO_CHANNEL_5POINT1 = AAUDIO_CHANNEL_FRONT_LEFT | AAUDIO_CHANNEL_FRONT_RIGHT | AAUDIO_CHANNEL_FRONT_CENTER | AAUDIO_CHANNEL_LOW_FREQUENCY | AAUDIO_CHANNEL_BACK_LEFT | AAUDIO_CHANNEL_BACK_RIGHT,
		AAUDIO_CHANNEL_5POINT1_SIDE = AAUDIO_CHANNEL_FRONT_LEFT | AAUDIO_CHANNEL_FRONT_RIGHT | AAUDIO_CHANNEL_FRONT_CENTER | AAUDIO_CHANNEL_LOW_FREQUENCY | AAUDIO_CHANNEL_SIDE_LEFT | AAUDIO_CHANNEL_SIDE_RIGHT,
		AAUDIO_CHANNEL_6POINT1 = AAUDIO_CHANNEL_FRONT_LEFT | AAUDIO_CHANNEL_FRONT_RIGHT | AAUDIO_CHANNEL_FRONT_CENTER | AAUDIO_CHANNEL_LOW_FREQUENCY | AAUDIO_CHANNEL_BACK_LEFT | AAUDIO_CHANNEL_BACK_RIGHT | AAUDIO_CHANNEL_BACK_CENTER,
		AAUDIO_CHANNEL_7POINT1 = AAUDIO_CHANNEL_5POINT1 | AAUDIO_CHANNEL_SIDE_LEFT | AAUDIO_CHANNEL_SIDE_RIGHT,
		AAUDIO_CHANNEL_5POINT1POINT2 = AAUDIO_CHANNEL_5POINT1 | AAUDIO_CHANNEL_TOP_SIDE_LEFT | AAUDIO_CHANNEL_TOP_SIDE_RIGHT,
		AAUDIO_CHANNEL_5POINT1POINT4 = AAUDIO_CHANNEL_5POINT1 | AAUDIO_CHANNEL_TOP_FRONT_LEFT | AAUDIO_CHANNEL_TOP_FRONT_RIGHT | AAUDIO_CHANNEL_TOP_BACK_LEFT | AAUDIO_CHANNEL_TOP_BACK_RIGHT,
		AAUDIO_CHANNEL_7POINT1POINT2 = AAUDIO_CHANNEL_7POINT1 | AAUDIO_CHANNEL_TOP_SIDE_LEFT | AAUDIO_CHANNEL_TOP_SIDE_RIGHT,
		AAUDIO_CHANNEL_7POINT1POINT4 = AAUDIO_CHANNEL_7POINT1 | AAUDIO_CHANNEL_TOP_FRONT_LEFT | AAUDIO_CHANNEL_TOP_FRONT_RIGHT | AAUDIO_CHANNEL_TOP_BACK_LEFT | AAUDIO_CHANNEL_TOP_BACK_RIGHT,
		AAUDIO_CHANNEL_9POINT1POINT4 = AAUDIO_CHANNEL_7POINT1POINT4 | AAUDIO_CHANNEL_FRONT_WIDE_LEFT | AAUDIO_CHANNEL_FRONT_WIDE_RIGHT,
		AAUDIO_CHANNEL_9POINT1POINT6 = AAUDIO_CHANNEL_9POINT1POINT4 | AAUDIO_CHANNEL_TOP_SIDE_LEFT | AAUDIO_CHANNEL_TOP_SIDE_RIGHT,
		AAUDIO_CHANNEL_FRONT_BACK = AAUDIO_CHANNEL_FRONT_CENTER | AAUDIO_CHANNEL_BACK_CENTER,
	}
	internal enum aaudio_content_type_t : int {
		AAUDIO_CONTENT_TYPE_SPEECH = 1,
		AAUDIO_CONTENT_TYPE_MUSIC = 2, // default
		AAUDIO_CONTENT_TYPE_MOVIE = 3,
		AAUDIO_CONTENT_TYPE_SONIFICATION = 4
	}
	internal enum aaudio_data_callback_result_t : int {
		AAUDIO_CALLBACK_RESULT_CONTINUE = 0,
		AAUDIO_CALLBACK_RESULT_STOP,
	}
	internal enum aaudio_direction_t : int {
		AAUDIO_DIRECTION_OUTPUT,
		AAUDIO_DIRECTION_INPUT,
	}
	internal enum aaudio_format_t : int {
		AAUDIO_FORMAT_INVALID = -1,
		AAUDIO_FORMAT_UNSPECIFIED = 0,
		AAUDIO_FORMAT_PCM_I16,
		AAUDIO_FORMAT_PCM_FLOAT,
		AAUDIO_FORMAT_PCM_I24_PACKED,
		AAUDIO_FORMAT_PCM_I32,
	}
	internal enum aaudio_input_preset_t : int {
		AAUDIO_UNSPECIFIED = 0,
		AAUDIO_INPUT_PRESET_GENERIC = 1,
		AAUDIO_INPUT_PRESET_CAMCORDER = 5,
		AAUDIO_INPUT_PRESET_VOICE_RECOGNITION = 6, // default
		AAUDIO_INPUT_PRESET_VOICE_COMMUNICATION = 7,
		AAUDIO_INPUT_PRESET_UNPROCESSED = 9,
		AAUDIO_INPUT_PRESET_VOICE_PERFORMANCE = 10,
	}
	internal enum aaudio_performance_mode_t : int {
		AAUDIO_PERFORMANCE_MODE_NONE = 10,
		AAUDIO_PERFORMANCE_MODE_POWER_SAVING,
		AAUDIO_PERFORMANCE_MODE_LOW_LATENCY,
	}
	internal enum aaudio_result_t : int {
		AAUDIO_OK,
		AAUDIO_ERROR_BASE = -900,
		AAUDIO_ERROR_DISCONNECTED,
		AAUDIO_ERROR_ILLEGAL_ARGUMENT,
		AAUDIO_ERROR_INTERNAL = AAUDIO_ERROR_ILLEGAL_ARGUMENT + 2,
		AAUDIO_ERROR_INVALID_STATE,
		AAUDIO_ERROR_INVALID_HANDLE = AAUDIO_ERROR_INVALID_STATE + 3,
		AAUDIO_ERROR_UNIMPLEMENTED = AAUDIO_ERROR_INVALID_HANDLE + 2,
		AAUDIO_ERROR_UNAVAILABLE,
		AAUDIO_ERROR_NO_FREE_HANDLES,
		AAUDIO_ERROR_NO_MEMORY,
		AAUDIO_ERROR_NULL,
		AAUDIO_ERROR_TIMEOUT,
		AAUDIO_ERROR_WOULD_BLOCK,
		AAUDIO_ERROR_INVALID_FORMAT,
		AAUDIO_ERROR_OUT_OF_RANGE,
		AAUDIO_ERROR_NO_SERVICE,
		AAUDIO_ERROR_INVALID_RATE,
	}
	internal enum aaudio_session_id_t : int {
		AAUDIO_SESSION_ID_NONE = -1,
		AAUDIO_SESSION_ID_ALLOCATE = 0,
	}
	internal enum aaudio_sharing_mode_t : int {
		AAUDIO_SHARING_MODE_EXCLUSIVE,
		AAUDIO_SHARING_MODE_SHARED, // default
	}
	internal enum aaudio_spatialization_behavior_t : int {
		AAUDIO_SPATIALIZATION_BEHAVIOR_AUTO = 1,
		AAUDIO_SPATIALIZATION_BEHAVIOR_NEVER = 2,
	}
	internal enum aaudio_stream_state_t : int {
		AAUDIO_STREAM_STATE_UNINITIALIZED = 0,
		AAUDIO_STREAM_STATE_UNKNOWN,
		AAUDIO_STREAM_STATE_OPEN,
		AAUDIO_STREAM_STATE_STARTING,
		AAUDIO_STREAM_STATE_STARTED,
		AAUDIO_STREAM_STATE_PAUSING,
		AAUDIO_STREAM_STATE_PAUSED,
		AAUDIO_STREAM_STATE_FLUSHING,
		AAUDIO_STREAM_STATE_FLUSHED,
		AAUDIO_STREAM_STATE_STOPPING,
		AAUDIO_STREAM_STATE_STOPPED,
		AAUDIO_STREAM_STATE_CLOSING,
		AAUDIO_STREAM_STATE_CLOSED,
		AAUDIO_STREAM_STATE_DISCONNECTED,
	}
	internal enum aaudio_usage_t : int {
		AAUDIO_USAGE_MEDIA = 1,
		AAUDIO_USAGE_VOICE_COMMUNICATION = 2,
		AAUDIO_USAGE_VOICE_COMMUNICATION_SIGNALLING = 3,
		AAUDIO_USAGE_ALARM = 4,
		AAUDIO_USAGE_NOTIFICATION = 5,
		AAUDIO_USAGE_NOTIFICATION_RINGTONE = 6,
		AAUDIO_USAGE_NOTIFICATION_EVENT = 10,
		AAUDIO_USAGE_ASSISTANCE_ACCESSIBILITY = 11,
		AAUDIO_USAGE_ASSISTANCE_NAVIGATION_GUIDANCE = 12,
		AAUDIO_USAGE_ASSISTANCE_SONIFICATION = 13,
		AAUDIO_USAGE_GAME = 14,
		AAUDIO_USAGE_ASSISTANT = 16,
		AAUDIO_SYSTEM_USAGE_OFFSET = 1000,
		AAUDIO_SYSTEM_USAGE_EMERGENCY = AAUDIO_SYSTEM_USAGE_OFFSET,
		AAUDIO_SYSTEM_USAGE_SAFETY = AAUDIO_SYSTEM_USAGE_OFFSET + 1,
		AAUDIO_SYSTEM_USAGE_VEHICLE_STATUS = AAUDIO_SYSTEM_USAGE_OFFSET + 2,
		AAUDIO_SYSTEM_USAGE_ANNOUNCEMENT = AAUDIO_SYSTEM_USAGE_OFFSET + 3,
	}
	internal enum clockid_t {
		CLOCK_REALTIME = 0,
		CLOCK_MONOTONIC = 1,
		CLOCK_PROCESS_CPUTIME_ID = 2,
		CLOCK_THREAD_CPUTIME_ID = 3,
		CLOCK_MONOTONIC_RAW = 4,
		CLOCK_REALTIME_COARSE = 5,
		CLOCK_MONOTONIC_COARSE = 6,
		CLOCK_BOOTTIME = 7,
		CLOCK_REALTIME_ALARM = 8,
		CLOCK_BOOTTIME_ALARM = 9,
	}
}
