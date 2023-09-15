using Microsoft.Windows.PropSys;

namespace Cryville.Common.Platform.Windows {
	internal sealed class PropertyStore : ComInterfaceWrapper {
		public PropertyStore(IPropertyStore obj) : base(obj) { }
		public object Get(PROPERTYKEY key) {
			(ComObject as IPropertyStore).GetValue(ref key, out var result);
			return result.ToObject(null);
		}
	}
}
