using Microsoft.Windows.PropSys;
using System.Runtime.InteropServices;

namespace Cryville.Common.Platform.Windows {
	public class PropertyStore : ComInterfaceWrapper<IPropertyStore> {
		public PropertyStore(IPropertyStore obj) : base(obj) { }
		public object Get(PROPERTYKEY key) {
			ComObject.GetValue(ref key, out var result);
			return result.ToObject(null);
		}
	}
}
