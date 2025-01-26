using System;
using System.Runtime.Serialization;

namespace Cryville.Audio.OpenSLES {
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
