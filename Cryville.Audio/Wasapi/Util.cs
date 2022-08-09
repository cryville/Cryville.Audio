using Microsoft.Windows.MMDevice;
using System;

namespace Cryville.Audio.Wasapi {
	internal static class Util {
		public static DataFlow FromInternalDataFlowEnum(EDataFlow value) {
			switch (value) {
				case EDataFlow.eRender: return DataFlow.Out;
				case EDataFlow.eCapture: return DataFlow.In;
				case EDataFlow.eAll: return DataFlow.All;
				default: throw new ArgumentOutOfRangeException(nameof(value));
			}
		}
		public static EDataFlow ToInternalDataFlowEnum(DataFlow value) {
			switch (value) {
				case DataFlow.Out: return EDataFlow.eRender;
				case DataFlow.In: return EDataFlow.eCapture;
				case DataFlow.All: return EDataFlow.eAll;
				default: throw new ArgumentOutOfRangeException(nameof(value));
			}
		}
	}
}
