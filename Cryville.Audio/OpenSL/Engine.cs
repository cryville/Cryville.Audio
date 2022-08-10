using OpenSL.Native;
using System;
using System.Collections.Generic;

namespace Cryville.Audio.OpenSL {
	public class Engine : IAudioDeviceManager {
		internal SLItfWrapper<SLObjectItf> ObjEngine;

		public Engine() {
			Util.SLR(Exports.slCreateEngine(out var pObjEngine, 0, null, 0, IntPtr.Zero, IntPtr.Zero), "slCreateEngine");
			ObjEngine = new SLItfWrapper<SLObjectItf>(pObjEngine);
			Util.SLR(ObjEngine.Obj.Realize(ObjEngine, false), "ObjEngine.Realize");
		}

		~Engine() {
			Dispose(false);
		}

		public bool Disposed { get; private set; }
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (!Disposed) {
				if (ObjEngine != null) ObjEngine.Obj.Destroy(ObjEngine);
				Disposed = true;
			}
		}

		public IAudioDevice GetDefaultDevice(DataFlow dataFlow) {
			return new OutputDevice(this);
		}

		public IEnumerable<IAudioDevice> GetDevices(DataFlow dataFlow) {
			switch (dataFlow) {
				case DataFlow.Out: return new List<IAudioDevice> { new OutputDevice(this) };
				case DataFlow.In: throw new NotImplementedException();
				default: throw new NotSupportedException();
			}
		}
	}
}
