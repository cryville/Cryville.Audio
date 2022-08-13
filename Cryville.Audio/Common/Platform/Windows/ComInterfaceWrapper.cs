using System;
using System.Runtime.InteropServices;

namespace Cryville.Common.Platform.Windows {
	public abstract class ComInterfaceWrapper : IDisposable {
		protected IntPtr ComObject { get; private set; }
		protected ComInterfaceWrapper(IntPtr comObject) {
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
			if (ComObject != default(IntPtr)) {
				Marshal.ReleaseComObject(Marshal.GetObjectForIUnknown(ComObject));
				ComObject = default(IntPtr);
			}
		}
	}
}