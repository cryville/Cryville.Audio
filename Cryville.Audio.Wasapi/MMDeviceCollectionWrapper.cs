using Microsoft.Windows.MMDevice;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Cryville.Audio.Wasapi {
	internal sealed class MMDeviceCollectionWrapper : IEnumerable<IAudioDevice> {
		readonly IMMDeviceCollection _internal;

		internal MMDeviceCollectionWrapper(IMMDeviceCollection obj) {
			_internal = obj;
			_internal.GetCount(out m_count);
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

		public Enumerator GetEnumerator() => new(this);
		IEnumerator<IAudioDevice> IEnumerable<IAudioDevice>.GetEnumerator() => GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public struct Enumerator(MMDeviceCollectionWrapper obj) : IEnumerator<IAudioDevice> {
			int _index = -1;

			public readonly IAudioDevice Current => obj[_index];
			readonly object IEnumerator.Current => Current;

			public readonly void Dispose() { }

			public bool MoveNext() => ++_index < obj.Count;

			public void Reset() => _index = -1;
		}
	}
}
