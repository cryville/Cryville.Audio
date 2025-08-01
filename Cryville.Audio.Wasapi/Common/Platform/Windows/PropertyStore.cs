using Microsoft.Windows.PropSys;

namespace Cryville.Common.Platform.Windows {
	internal sealed class PropertyStore(IPropertyStore obj) {
		public object? Get(PROPERTYKEY key) {
			obj.GetValue(ref key, out var result);
			return result.ToObject(null);
		}
	}
}
