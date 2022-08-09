using System;
using System.Runtime.InteropServices;

namespace Cryville.Common.Platform.Windows {
	public abstract class ComInterfaceWrapper<T> : IDisposable where T : class {
		protected T ComObject { get; private set; }
		protected ComInterfaceWrapper(T comObject) {
			ComObject = comObject;
		}

		~ComInterfaceWrapper() {
			Dispose(false);
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (ComObject != null) {
				Marshal.ReleaseComObject(ComObject);
				ComObject = null;
			}
		}
	}
}