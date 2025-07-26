using System;
using System.IO;

namespace Cryville.Audio.Source.Resample {
	/// <summary>
	/// An <see cref="AudioStream" /> that resamples another <see cref="AudioStream" />.
	/// </summary>
	/// <remarks>
	/// <para>Call <see cref="AudioStream.SetFormat(WaveFormat, int)" /> on a <see cref="ResampledAudioSource" /> to set the destination format.</para>
	/// </remarks>
	public class ResampledAudioSource : AudioStream {
		readonly AudioStream _source;
		readonly bool _highQuality;

		/// <summary>
		/// Creates an instance of the <see cref="ResampledAudioSource" /> class.
		/// </summary>
		/// <param name="source">The source <see cref="AudioStream" />.</param>
		/// <param name="sourceFormat">The wave format set to the source, if set, must be a supported wave format of the source audio stream.</param>
		/// <param name="highQuality">Whether to resample with high quality.</param>
		public ResampledAudioSource(AudioStream source, WaveFormat? sourceFormat = null, bool highQuality = true) {
			_source = source ?? throw new ArgumentNullException(nameof(source));
			_highQuality = highQuality;

			DefaultFormat = sourceFormat ?? source.DefaultFormat;
			if (!source.IsFormatSupported(DefaultFormat)) {
				throw new NotSupportedException("Format not supported.");
			}
		}

		/// <inheritdoc />
		public override bool EndOfData => _source.EndOfData;

		/// <inheritdoc />
		public override long FrameLength => (long)(_source.FrameLength * _internal?._factor ?? 0);

		/// <inheritdoc />
		public override WaveFormat DefaultFormat { get; }
		/// <inheritdoc />
		public override bool IsFormatSupported(WaveFormat format) => format.Channels == DefaultFormat.Channels && format.ChannelMask == DefaultFormat.ChannelMask;
		/// <inheritdoc />
		protected override void OnSetFormat() {
			_internal = new(DefaultFormat, Format, BufferSize, _highQuality);
			_source.SetFormat(DefaultFormat, _internal._inBufferFrameLength);
		}

		sealed class Internal {
			readonly WaveFormat _inFormat;
			public readonly double _factor;
			public readonly int _inBufferFrameLength;
			readonly int _filterWidth;
			readonly int _channelCount;
			int _inFrameOffset;
			readonly byte[] _inBuffer;
			readonly Resampler[] _resampler;
			readonly double[][] _inSampleBuffer;
			readonly double[][] _outSampleBuffer;
			readonly SampleReader _sampleReader;
			readonly SampleWriter _sampleWriter;

			public Internal(WaveFormat inFormat, WaveFormat outFormat, int outBufferFrameLength, bool highQuality) {
				_inFormat = inFormat;
				_channelCount = outFormat.Channels;
				_factor = (double)outFormat.SampleRate / inFormat.SampleRate;

				_resampler = new Resampler[_channelCount];
				for (int i = 0; i < _channelCount; i++) _resampler[i] = new(highQuality, _factor, _factor);
				_filterWidth = _resampler[0].FilterWidth;

				_inBufferFrameLength = (int)(outBufferFrameLength / _factor) + _filterWidth;
				_inBuffer = new byte[_inBufferFrameLength * inFormat.FrameSize];

				_inSampleBuffer = new double[_channelCount][];
				for (int i = 0; i < _channelCount; i++) _inSampleBuffer[i] = new double[_inBufferFrameLength];
				_outSampleBuffer = new double[_channelCount][];
				for (int i = 0; i < _channelCount; i++) _outSampleBuffer[i] = new double[outBufferFrameLength];

				_sampleReader = SampleConvert.GetSampleReader(inFormat.SampleFormat);
				_sampleWriter = SampleConvert.GetSampleWriter(outFormat.SampleFormat);
			}
			public unsafe int ReadFramesInternal(AudioStream source, ref byte buffer, int frameCount) {
				int inFrameCount = _inBufferFrameLength - _inFrameOffset;
				_inFrameOffset += inFrameCount = source.ReadFrames(_inBuffer, _inFrameOffset * _inFormat.FrameSize, inFrameCount);
				fixed (byte* rptr = _inBuffer) {
					byte* ptr = rptr;
					for (int i = _inFrameOffset - inFrameCount; i < _inFrameOffset; i++) {
						for (int c = 0; c < _channelCount; c++) {
							_inSampleBuffer[c][i] = _sampleReader(ref ptr);
						}
					}
				}
				int usedInFrameCount = 0, outFrameCount = 0;
				for (int c = 0; c < _channelCount; c++) {
					fixed (double* iptr = _inSampleBuffer[c], optr = _outSampleBuffer[c]) {
						outFrameCount = _resampler[c].Process(_factor, iptr, _inFrameOffset, false, out usedInFrameCount, optr, frameCount);
					}
				}
				fixed (byte* rptr = &buffer) {
					byte* ptr = rptr;
					for (int i = 0; i < frameCount; i++) {
						for (int c = 0; c < _channelCount; c++) {
							_sampleWriter(ref ptr, _outSampleBuffer[c][i]);
						}
					}
				}
				_inFrameOffset -= usedInFrameCount;
				if (_inFrameOffset > 0) {
					for (int c = 0; c < _channelCount; c++) {
						double[] sampleBuffer = _inSampleBuffer[c];
						Array.Copy(sampleBuffer, usedInFrameCount, sampleBuffer, 0, _inFrameOffset);
					}
				}
				return outFrameCount;
			}
		}

		Internal? _internal;
		/// <inheritdoc />
		protected override unsafe int ReadFramesInternal(ref byte buffer, int frameCount) {
			return _internal?.ReadFramesInternal(_source, ref buffer, frameCount) ?? 0;
		}

		/// <inheritdoc />
		protected override long SeekFrameInternal(long frameOffset, SeekOrigin origin) {
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override bool CanRead => true;
		/// <inheritdoc />
		public override bool CanSeek => true;
		/// <inheritdoc />
		public override bool CanWrite => false;
		/// <inheritdoc />
		public override void Flush() { }
		/// <inheritdoc />
		public override void SetLength(long value) => throw new NotSupportedException();
		/// <inheritdoc />
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	}
}
