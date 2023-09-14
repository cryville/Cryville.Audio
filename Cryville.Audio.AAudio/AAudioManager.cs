using Cryville.Interop.Java;
using Cryville.Interop.Java.Helper;
using System;
using System.Collections.Generic;

namespace Cryville.Audio.AAudio {
	public class AAudioManager : IAudioDeviceManager {
		readonly IntPtr _manager;

		public AAudioManager() {
			var env = JavaVMManager.CurrentEnv;
			if (AndroidHelper.DeviceApiLevel < 26) throw new NotSupportedException("AAudio is not available below API level 26.");
			_manager = env.NewGlobalRef(AndroidHelper.GetSystemService(env, AndroidHelper.GetCurrentApplication(env), "AUDIO_SERVICE"));
		}

		bool _disposed;

		~AAudioManager() {
			Dispose(false);
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (!_disposed) return;
			JavaVMManager.CurrentEnv.DeleteGlobalRef(_manager);
			_disposed = true;
		}

		/// <inheritdoc />
		public IAudioDevice GetDefaultDevice(DataFlow dataFlow) => new AAudioStreamBuilder(dataFlow);

		/// <inheritdoc />
		public IEnumerable<IAudioDevice> GetDevices(DataFlow dataFlow) {
			var env = JavaVMManager.CurrentEnv;
			using (var frame0 = new JniLocalFrame(env, 2)) {
				var devs = IntPtr.Zero;
				using (var frame1 = new JniLocalFrame(env, 2)) {
					var c = env.GetObjectClass(_manager);
					if (c == IntPtr.Zero) throw new InvalidOperationException("Could not get the AudioManager class.");
					var m = env.GetMethodID(c, "getDevices", "(I)[Landroid/media/AudioDeviceInfo;");
					if (m == IntPtr.Zero) throw new InvalidOperationException("Could not find the method getDevices(int).");
					devs = frame1.Pop(env.CallObjectMethod(_manager, m, new JniValue[] { ToInternalDataFlow(dataFlow) }));
				}
				var count = env.GetArrayLength(devs);
				var result = new AAudioStreamBuilder[count];
				for (int i = 0; i < count; i++) {
					using (var frame2 = new JniLocalFrame(env, 1)) {
						result[i] = new AAudioStreamBuilder(env, env.GetObjectArrayElement(devs, i));
					}
				}
				return result;
			}
		}

		static JniValue ToInternalDataFlow(DataFlow dataFlow) {
			int result = 0;
			if ((dataFlow & DataFlow.Out) != 0) result |= 2;
			if ((dataFlow & DataFlow.In) != 0) result |= 1;
			return new JniValue(result);
		}
	}
}
