using Microsoft.Windows.MMDevice;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cryville.Audio.Wasapi {
	internal sealed class MMDeviceCollectionWrapper : IEnumerable<IAudioDevice>, IDisposable {
		readonly IMMDeviceCollection _internal;

		internal MMDeviceCollectionWrapper(IMMDeviceCollection obj) {
			_internal = obj;
			_internal.GetCount(out m_count);
		}

		public void Dispose() {
			Marshal.ReleaseComObject(_internal);
		}

		private uint m_count;
		public uint Count { get => m_count; private set => m_count = value; }

		public MMDeviceWrapper this[int index] {
			get {
				if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
				_internal.Item((uint)index, out var result);
				return new MMDeviceWrapper(result);
			}
		}

		public Enumerator GetEnumerator() => new Enumerator(this);

		IEnumerator<IAudioDevice> IEnumerable<IAudioDevice>.GetEnumerator() => GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public struct Enumerator : IEnumerator<IAudioDevice> {
			readonly MMDeviceCollectionWrapper _obj;
			int _index;

			public Enumerator(MMDeviceCollectionWrapper obj) {
				_obj = obj;
				_index = -1;
			}

			public IAudioDevice Current => _obj[_index];

			object IEnumerator.Current => Current;

			public void Dispose() { }

			public bool MoveNext() => ++_index < _obj.Count;

			public void Reset() => _index = -1;
		}
	}
}
