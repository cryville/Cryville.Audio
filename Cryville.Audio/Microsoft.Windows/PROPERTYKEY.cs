using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Microsoft.Windows.PropSys {
	[StructLayout(LayoutKind.Sequential)]
	internal struct PROPERTYKEY : IEquatable<PROPERTYKEY> {
		public Guid   fmtid;
		public UInt32 pid;
		public PROPERTYKEY(Guid fmt, UInt32 p) {
			fmtid = fmt;
			pid = p;
		}
		public PROPERTYKEY(string fmt, UInt32 p) : this(new Guid(fmt), p) { }

		public override bool Equals(object obj) {
			if (obj == null || !(obj is PROPERTYKEY)) return false;
			PROPERTYKEY other = (PROPERTYKEY)obj;
			return Equals(other);
		}

		public bool Equals(PROPERTYKEY other) {
			return fmtid.Equals(other.fmtid) && pid.Equals(other.pid);
		}

		public override int GetHashCode() {
			return fmtid.GetHashCode() ^ pid.GetHashCode();
		}

		public override string ToString() {
			return String.Format(CultureInfo.InvariantCulture, "fmtid = {0}, pid = {1}", fmtid, pid);
		}

		public static bool operator ==(PROPERTYKEY left, PROPERTYKEY right) {
			return left.Equals(right);
		}

		public static bool operator !=(PROPERTYKEY left, PROPERTYKEY right) {
			return !(left == right);
		}
	}
}
