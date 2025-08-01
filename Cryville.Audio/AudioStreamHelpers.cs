namespace Cryville.Audio {
	/// <summary>
	/// Provides a set of <see langword="static" /> methods related to <see cref="AudioStream" />.
	/// </summary>
	public static class AudioStreamHelpers {
		/// <summary>
		/// Gets a recommended new buffer size, provided the current buffer size and the target buffer size.
		/// </summary>
		/// <param name="targetBufferSize">The requested minimum buffer size in frames.</param>
		/// <param name="currentBufferSize">The current buffer size in frames.</param>
		/// <returns>The new buffer size in frames.</returns>
		public static int GetNewBufferSize(int targetBufferSize, int currentBufferSize) {
			if (targetBufferSize <= 0)
				targetBufferSize = 256;
			if (targetBufferSize <= currentBufferSize)
				return currentBufferSize;
			currentBufferSize *= 2;
			if (targetBufferSize > currentBufferSize)
				currentBufferSize = targetBufferSize;
			return currentBufferSize;
		}
	}
}
