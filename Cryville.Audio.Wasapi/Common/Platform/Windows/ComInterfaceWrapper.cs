using System;
using System.Runtime.InteropServices;

namespace Cryville.Common.Platform.Windows {
	public abstract class ComInterfaceWrapper : IDisposable {
		protected object ComObject { get; private set; }
		protected ComInterfaceWrapper(object comObject) {
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
			if (disposing) {
				if (ComObject != null) {
					Marshal.ReleaseComObject(ComObject);
					ComObject = null;
				}
			}
		}
	}
}