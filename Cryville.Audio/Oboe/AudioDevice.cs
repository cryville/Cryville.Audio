using Oboe;
using System;
using BAudioClient = Cryville.Audio.AudioClient;

namespace Cryville.Audio.Oboe {
	public class AudioDevice : IAudioDevice {
		internal AudioStreamBuilder _builder;
		internal AudioDevice(AudioStreamBuilder builder, DataFlow dataFlow) {
			_builder = builder;
			m_dataFlow = dataFlow;
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) { }

		public string Name => "";

		readonly DataFlow m_dataFlow;
		public DataFlow DataFlow => m_dataFlow;

		public BAudioClient Connect() {
			_builder.Internal.setDirection(ToInternalDirection(m_dataFlow));
			return new AudioClient(this);
		}

		static Direction ToInternalDirection(DataFlow dataFlow) {
			switch (dataFlow) {
				case DataFlow.Out: return Direction.Output;
				case DataFlow.In: return Direction.Input;
				default: throw new ArgumentOutOfRangeException(nameof(dataFlow));
			}
		}
	}
}
