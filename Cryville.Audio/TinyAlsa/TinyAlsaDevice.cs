using System;
using System.Text;

namespace Cryville.Audio.TinyAlsa {
	public class TinyAlsaDevice : IAudioDevice {
		IntPtr param;

		internal TinyAlsaDevice(uint card, uint device, DataFlow flow) {
			param = UnsafeNativeMethods.pcm_params_get(card, device, flow == DataFlow.Out ? 0x00000000u : 0x10000000u);
			if (param == IntPtr.Zero) throw new InvalidOperationException("Cannot get device info");
		}

		public bool Disposed { get; private set; }

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (!Disposed) {
				Disposed = true;
			}
		}

		public string Name => "";

		public DataFlow DataFlow => throw new NotImplementedException();

		public string Format {
			get {
				var buf = new byte[256];
				_ = UnsafeNativeMethods.pcm_params_to_string(param, buf, (uint)buf.Length);
				return Encoding.UTF8.GetString(buf);
			}
		}

		public AudioClient Connect() {
			throw new NotImplementedException();
		}
	}
}
