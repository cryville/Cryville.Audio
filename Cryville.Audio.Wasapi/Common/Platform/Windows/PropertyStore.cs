using Microsoft.Windows.PropSys;
using System;
using System.Runtime.InteropServices;

namespace Cryville.Common.Platform.Windows {
	internal sealed class PropertyStore(IPropertyStore obj) : IDisposable {
		public void Dispose() {
			Marshal.ReleaseComObject(obj);
		}
		public object? Get(PROPERTYKEY key) {
			obj.GetValue(ref key, out var result);
			return result.ToObject(null);
		}
	}
}
