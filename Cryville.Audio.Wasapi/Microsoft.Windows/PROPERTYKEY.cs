using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Microsoft.Windows.PropSys {
	[StructLayout(LayoutKind.Sequential)]
	internal struct PROPERTYKEY(Guid fmt, uint p) : IEquatable<PROPERTYKEY> {
		public Guid fmtid = fmt;
		public uint pid = p;

		public PROPERTYKEY(string fmt, uint p) : this(new Guid(fmt), p) { }

		public override bool Equals(object obj) => obj is PROPERTYKEY other && Equals(other);
		public bool Equals(PROPERTYKEY other) => fmtid.Equals(other.fmtid) && pid.Equals(other.pid);
		public override int GetHashCode() => fmtid.GetHashCode() ^ pid.GetHashCode();

		public override readonly string ToString() => string.Format(CultureInfo.InvariantCulture, "fmtid = {0}, pid = {1}", fmtid, pid);

		public static bool operator ==(PROPERTYKEY left, PROPERTYKEY right) => left.Equals(right);
		public static bool operator !=(PROPERTYKEY left, PROPERTYKEY right) => !(left == right);
	}
}
