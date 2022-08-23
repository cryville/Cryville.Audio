using Microsoft.Windows.PropSys;
using System.Runtime.InteropServices;

namespace Cryville.Common.Platform.Windows {
	public class PropertyStore : ComInterfaceWrapper {
		internal PropertyStore(IPropertyStore obj) : base(Marshal.GetIUnknownForObject(obj)) { }
		public object Get(PROPERTYKEY key) {
			(Marshal.GetObjectForIUnknown(ComObject) as IPropertyStore).GetValue(ref key, out var result);
			return result.ToObject(null);
		}
	}
}
