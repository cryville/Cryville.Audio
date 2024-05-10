using OpenSLES.Native;
using System;
using System.Runtime.Serialization;

namespace Cryville.Audio.OpenSLES {
	internal static class Util {
		public static void SLR(SLresult result, string? desc = null) {
			if (result == SLresult.SUCCESS) return;
			var msg = result.ToString();
			if (desc != null) msg = desc + ": " + msg;
			throw new OpenSLException(msg);
		}

		public static SLDataFormat_PCM ToInternalFormat(WaveFormat value) {
			return new SLDataFormat_PCM(value.Channels, value.SampleRate * 1000, value.BitsPerSample, value.BitsPerSample, ~(0xffffffff << value.Channels), (UInt32)SL_BYTEORDER.LITTLEENDIAN);
		}
	}

	/// <summary>
	/// Exception occurring in OpenSL ES.
	/// </summary>
	[Serializable]
	public class OpenSLException : Exception {
		/// <summary>
		/// Creates an instance of the <see cref="OpenSLException" /> class.
		/// </summary>
		public OpenSLException() { }
		/// <summary>
		/// Creates an instance of the <see cref="OpenSLException" /> class.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		public OpenSLException(string message) : base(message) { }
		/// <summary>
		/// Creates an instance of the <see cref="OpenSLException" /> class.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		public OpenSLException(string message, Exception innerException) : base(message, innerException) { }
		/// <summary>
		/// Creates an instance of the <see cref="OpenSLException" /> class with serialized data.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
		protected OpenSLException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
