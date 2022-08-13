using Cryville.Common.Platform.Windows;
using Microsoft.Windows.MMDevice;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Cryville.Audio.Wasapi {
	internal class MMDeviceCollection : ComInterfaceWrapper, IEnumerable<IAudioDevice> {
		internal MMDeviceCollection(IntPtr obj) : base(obj) {
			IMMDeviceCollection.GetCount(ComObject, out m_count);
		}

		private uint m_count;
		public uint Count { get => m_count; private set => m_count = value; }

		public MMDevice this[int index] {
			get {
				if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
				IMMDeviceCollection.Item(ComObject, (uint)index, out var result);
				return new MMDevice(result);
			}
		}

		public IEnumerator<IAudioDevice> GetEnumerator() => new Enumerator(this);

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		private class Enumerator : IEnumerator<IAudioDevice> {
			int _index = -1;
			readonly MMDeviceCollection _obj;

			public Enumerator(MMDeviceCollection obj) {
				_obj = obj;
			}

			public IAudioDevice Current => _obj[_index];

			object IEnumerator.Current => Current;

			public void Dispose() { }

			public bool MoveNext() => ++_index < _obj.Count;

			public void Reset() => _index = -1;
		}
	}
}
