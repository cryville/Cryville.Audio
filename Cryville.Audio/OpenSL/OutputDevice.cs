using System;

namespace Cryville.Audio.OpenSL {
	public class OutputDevice : IAudioDevice {
		Engine _engine;
		internal OutputDevice(Engine engine) {
			_engine = engine;
		}

		~OutputDevice() {
			Dispose(false);
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

		public string Name => "Default";

		public DataFlow DataFlow => DataFlow.Out;

		public AudioClient Connect() => new OutputClient(_engine, this);
	}
}
