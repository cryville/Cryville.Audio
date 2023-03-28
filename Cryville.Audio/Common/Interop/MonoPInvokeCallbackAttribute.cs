using System;

namespace Cryville.Common.Interop {
	/// <summary>
	/// Attribute used to annotate functions that will be called back from the unmanaged world.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class MonoPInvokeCallbackAttribute : Attribute { }
}
