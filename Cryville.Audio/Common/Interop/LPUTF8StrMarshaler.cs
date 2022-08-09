using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Cryville.Common.Interop {
	public class LPUTF8StrMarshaler : ICustomMarshaler {
		public static ICustomMarshaler GetInstance(string cookie) => new LPUTF8StrMarshaler();

		public void CleanUpManagedData(object ManagedObj) {
			// Do nothing
		}

		public void CleanUpNativeData(IntPtr pNativeData) {
			if (pNativeData == IntPtr.Zero) return;
			Marshal.FreeHGlobal(pNativeData);
		}

		public unsafe int GetNativeDataSize() {
			return sizeof(byte*);
		}

		public unsafe IntPtr MarshalManagedToNative(object ManagedObj) {
			if (ManagedObj == null) return IntPtr.Zero;
			var obj = (string)ManagedObj;
			var buffer = Encoding.UTF8.GetBytes(obj);
			var result = Marshal.AllocHGlobal(buffer.Length + 1);
			Marshal.Copy(buffer, 0, result, buffer.Length);
			var ptr = (byte*)result.ToPointer();
			ptr[buffer.Length] = 0;
			return result;
		}

		public unsafe object MarshalNativeToManaged(IntPtr pNativeData) {
			if (pNativeData == IntPtr.Zero) return null;
			var ptr = (byte*)pNativeData.ToPointer();
			var buffer = new List<byte>();
			while (*ptr != 0) {
				buffer.Add(*ptr);
				ptr++;
			}
			return Encoding.UTF8.GetString(buffer.ToArray());
		}
	}
}
