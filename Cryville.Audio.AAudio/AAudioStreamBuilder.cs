using Cryville.Audio.AAudio.Native;
using Cryville.Interop.Java;
using Cryville.Interop.Java.Helper;
using System;

namespace Cryville.Audio.AAudio {
	/// <summary>
	/// An <see cref="IAudioDevice" /> that interacts with AAudio.
	/// </summary>
	public class AAudioStreamBuilder : IAudioDevice {
		static IntPtr m_id;
		static IntPtr m_name;
		static IntPtr m_sink;
		static IntPtr m_source;
		static IntPtr m_toString;
		static void GetMethodIds(IJniEnv env) {
			if (m_name != IntPtr.Zero) return;
			using var frame = new JniLocalFrame(env, 2);
			var c_deviceInfo = env.FindClass("android/media/AudioDeviceInfo");
			m_id = env.GetMethodID(c_deviceInfo, "getId", "()I");
			m_name = env.GetMethodID(c_deviceInfo, "getProductName", "()Ljava/lang/CharSequence;");
			m_sink = env.GetMethodID(c_deviceInfo, "isSink", "()Z");
			m_source = env.GetMethodID(c_deviceInfo, "isSource", "()Z");
			var c_charSequence = env.FindClass("java/lang/CharSequence");
			m_toString = env.GetMethodID(c_charSequence, "toString", "()Ljava/lang/String;");
		}
		static readonly JniValue[] _args0 = [];
		internal unsafe AAudioStreamBuilder(IJniEnv env, IntPtr deviceInfo) {
			GetMethodIds(env);
			using var frame = new JniLocalFrame(env, 2);
			_id = env.CallIntMethod(deviceInfo, m_id, _args0);
			var cs = env.CallObjectMethod(deviceInfo, m_name, _args0);
			var str = env.CallObjectMethod(cs, m_toString, _args0);
			var pStr = env.GetStringChars(str, out _);
			Name = new string(pStr, 0, env.GetStringLength(str));
			env.ReleaseStringChars(str, pStr);
			if (env.CallBooleanMethod(deviceInfo, m_sink, _args0)) DataFlow = DataFlow.Out;
			if (env.CallBooleanMethod(deviceInfo, m_source, _args0)) DataFlow = DataFlow.In;
		}
		internal unsafe AAudioStreamBuilder(DataFlow dataFlow) {
			_id = 0;
			Name = "Default";
			DataFlow = dataFlow;
		}

		private bool _disposed;
		/// <summary>
		/// Releases all the unmanaged resources used by this instance.
		/// </summary>
		~AAudioStreamBuilder() {
			Dispose(disposing: false);
		}
		/// <summary>
		/// Releases all the resources used by this instance.
		/// </summary>
		public void Dispose() {
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		/// <summary>
		/// Releases all the resources used by this instance.
		/// </summary>
		/// <param name="disposing">Whether to release managed resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (_disposed) return;
			if (disposing) {
			}
			_disposed = true;
		}

		readonly int _id;

		/// <inheritdoc />
		public string Name { get; private set; }

		/// <inheritdoc />
		public DataFlow DataFlow { get; private set; }

		void GetDefaultParameters() {
			if (m_defaultBufferSize != 0) return;
			var builder = CreateStreamBuilder();
			Helpers.ThrowIfError(UnsafeNativeMethods.AAudioStreamBuilder_openStream(builder, out var stream));
			Helpers.ThrowIfError(UnsafeNativeMethods.AAudioStreamBuilder_delete(builder));
			m_defaultFormat = Helpers.FromInternalWaveFormat(stream);
			m_defaultBufferSize = UnsafeNativeMethods.AAudioStream_getBufferSizeInFrames(stream);
			m_burstSize = UnsafeNativeMethods.AAudioStream_getFramesPerBurst(stream);
			Helpers.ThrowIfError(UnsafeNativeMethods.AAudioStream_close(stream));
		}

		int m_burstSize;
		/// <inheritdoc />
		public int BurstSize {
			get {
				GetDefaultParameters();
				return m_burstSize;
			}
		}
		/// <inheritdoc />
		public int MinimumBufferSize => BurstSize;

		int m_defaultBufferSize;
		/// <inheritdoc />
		public int DefaultBufferSize {
			get {
				GetDefaultParameters();
				return m_defaultBufferSize;
			}
		}

		WaveFormat m_defaultFormat;
		/// <inheritdoc />
		public WaveFormat DefaultFormat {
			get {
				GetDefaultParameters();
				return m_defaultFormat;
			}
		}

		/// <inheritdoc />
		public bool IsFormatSupported(WaveFormat format, out WaveFormat? suggestion, AudioUsage usage = AudioUsage.Media, AudioShareMode shareMode = AudioShareMode.Shared) {
			var builder = CreateStreamBuilder();
			Helpers.SetWaveFormatUsageAndShareMode(builder, format, usage, shareMode);
			Helpers.ThrowIfError(UnsafeNativeMethods.AAudioStreamBuilder_openStream(builder, out var stream));
			Helpers.ThrowIfError(UnsafeNativeMethods.AAudioStreamBuilder_delete(builder));
			suggestion = Helpers.FromInternalWaveFormat(stream);
			Helpers.ThrowIfError(UnsafeNativeMethods.AAudioStream_close(stream));
			return format == suggestion;
		}

		/// <inheritdoc />
		public AudioClient Connect(WaveFormat format, int bufferSize = 0, AudioUsage usage = AudioUsage.Media, AudioShareMode shareMode = AudioShareMode.Shared) {
			var builder = CreateStreamBuilder();
			Helpers.SetWaveFormatUsageAndShareMode(builder, format, usage, shareMode);
			Helpers.ThrowIfError(UnsafeNativeMethods.AAudioStreamBuilder_openStream(builder, out var stream));
			Helpers.ThrowIfError(UnsafeNativeMethods.AAudioStreamBuilder_delete(builder));
			if (bufferSize > 0) {
				UnsafeNativeMethods.AAudioStream_setBufferSizeInFrames(stream, bufferSize);
			}
			return new AAudioStream(this, stream);
		}

		IntPtr CreateStreamBuilder() {
			if (DataFlow != DataFlow.Out) throw new NotSupportedException();
			UnsafeNativeMethods.AAudio_createStreamBuilder(out var builder);
			UnsafeNativeMethods.AAudioStreamBuilder_setDataCallback(builder, AAudioStream.DataCallback, IntPtr.Zero);
			UnsafeNativeMethods.AAudioStreamBuilder_setDeviceId(builder, _id);
			UnsafeNativeMethods.AAudioStreamBuilder_setDirection(builder, Helpers.ToInternalDataFlow(DataFlow));
			UnsafeNativeMethods.AAudioStreamBuilder_setPerformanceMode(builder, aaudio_performance_mode_t.AAUDIO_PERFORMANCE_MODE_LOW_LATENCY);
			if (AndroidHelper.DeviceApiLevel >= 32) {
				try {
					UnsafeNativeMethods.AAudioStreamBuilder_setIsContentSpatialized(builder, true);
					UnsafeNativeMethods.AAudioStreamBuilder_setSpatializationBehavior(builder, aaudio_spatialization_behavior_t.AAUDIO_SPATIALIZATION_BEHAVIOR_NEVER);
				}
				catch (EntryPointNotFoundException) { }
			}
			return builder;
		}
	}
}
