using System;

namespace Cryville.Audio.Common.Platform {
	public class ExternalClassWrapper<T> : IDisposable where T : class, IDisposable {
		protected T Internal { get; private set; }
		protected ExternalClassWrapper(T obj) {
			Internal = obj;
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (Internal != null) {
					Internal.Dispose();
					Internal = null;
				}
			}
		}
	}
}
