using Cryville.Audio.AAudio.Native;
using Cryville.Interop.Java;
using Cryville.Interop.Java.Helper;
using System;

namespace Cryville.Audio.AAudio {
	public class AAudioStreamBuilder : IAudioDevice {
		static IntPtr m_id;
		static IntPtr m_name;
		static IntPtr m_sink;
		static IntPtr m_source;
		static IntPtr m_toString;
		static void GetMethods(IJniEnv env) {
			if (m_name != IntPtr.Zero) return;
			using (var frame = new JniLocalFrame(env, 2)) {
				var c_deviceInfo = env.FindClass("android/media/AudioDeviceInfo");
				m_id = env.GetMethodID(c_deviceInfo, "getId", "()I");
				m_name = env.GetMethodID(c_deviceInfo, "getProductName", "()Ljava/lang/CharSequence;");
				m_sink = env.GetMethodID(c_deviceInfo, "isSink", "()Z");
				m_source = env.GetMethodID(c_deviceInfo, "isSource", "()Z");
				var c_charSequence = env.FindClass("java/lang/CharSequence");
				m_toString = env.GetMethodID(c_charSequence, "toString", "()Ljava/lang/String;");
			}
		}
		static readonly JniValue[] _args0 = new JniValue[0];
		internal unsafe AAudioStreamBuilder(IJniEnv env, IntPtr deviceInfo) {
			GetMethods(env);
			using (var frame = new JniLocalFrame(env, 2)) {
				_id = env.CallIntMethod(deviceInfo, m_id, _args0);
				var cs = env.CallObjectMethod(deviceInfo, m_name, _args0);
				var str = env.CallObjectMethod(cs, m_toString, _args0);
				var pstr = env.GetStringChars(str, out _);
				Name = new string(pstr, 0, env.GetStringLength(str));
				env.ReleaseStringChars(str, pstr);
				if (env.CallBooleanMethod(deviceInfo, m_sink, _args0)) DataFlow = DataFlow.Out;
				if (env.CallBooleanMethod(deviceInfo, m_source, _args0)) DataFlow = DataFlow.In;
			}
		}
		internal unsafe AAudioStreamBuilder(DataFlow dataFlow) {
			_id = 0;
			Name = "Default";
			DataFlow = dataFlow;
		}

		private bool _disposed;
		~AAudioStreamBuilder() {
			Dispose(disposing: false);
		}
		public void Dispose() {
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
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

		/// <inheritdoc />
		public float DefaultBufferDuration => 0;

		/// <inheritdoc />
		public float MinimumBufferDuration => 0;

		/// <inheritdoc />
		public WaveFormat DefaultFormat => WaveFormat.Default;

		static aaudio_direction_t ToInternalDataFlow(DataFlow dataFlow) {
			switch (dataFlow) {
				case DataFlow.Out: return aaudio_direction_t.AAUDIO_DIRECTION_OUTPUT;
				case DataFlow.In: return aaudio_direction_t.AAUDIO_DIRECTION_INPUT;
				default: throw new ArgumentOutOfRangeException(nameof(dataFlow));
			}
		}

		/// <inheritdoc />
		public bool IsFormatSupported(WaveFormat format, out WaveFormat? suggestion, AudioShareMode shareMode = AudioShareMode.Shared) {
			suggestion = format;
			return true;
		}

		/// <inheritdoc />
		public AudioClient Connect(WaveFormat format, float bufferDuration = 0, AudioShareMode shareMode = AudioShareMode.Shared) {
			UnsafeNativeMethods.AAudio_createStreamBuilder(out var builder);
			UnsafeNativeMethods.AAudioStreamBuilder_setDeviceId(builder, _id);
			UnsafeNativeMethods.AAudioStreamBuilder_setDirection(builder, ToInternalDataFlow(DataFlow));
			UnsafeNativeMethods.AAudioStreamBuilder_setPerformanceMode(builder, aaudio_performance_mode_t.AAUDIO_PERFORMANCE_MODE_LOW_LATENCY);
			UnsafeNativeMethods.AAudioStreamBuilder_setUsage(builder, aaudio_usage_t.AAUDIO_USAGE_GAME);
			UnsafeNativeMethods.AAudioStreamBuilder_openStream(builder, out var stream);
			UnsafeNativeMethods.AAudioStreamBuilder_delete(builder);
			return new AAudioStream(this, stream);
		}
	}
}
