using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;

namespace Cryville.Audio.Source.Libav {
	/// <summary>
	/// An <see cref="AudioStream" /> that uses Libav to demux and decode audio files.
	/// </summary>
	/// <param name="file">The audio file.</param>
	public class LibavFileAudioSource(string file) : AudioStream {
		internal sealed unsafe class Internal {
			readonly AVFormatContext* formatCtx;
			AVCodec* codec;
			AVCodecContext* codecCtx;
			AVPacket* packet;
			AVFrame* frame;
			readonly SwrContext* swrContext;

			public readonly int BestStreamIndex;
			public readonly ReadOnlyCollection<int> Streams;
			public int SelectedStream;
			long _pts;
			WaveFormat? OutFormat;
			WaveFormat? InFormat;
			int BufferSize;
			int frameSize;
			public bool EOF;
			public Internal(string file) {
				if (!File.Exists(file)) throw new FileNotFoundException();
				formatCtx = ffmpeg.avformat_alloc_context();
				fixed (AVFormatContext** formatCtxPtr = &formatCtx) {
					HR(ffmpeg.avformat_open_input(formatCtxPtr, file, null, null));
				}
				HR(ffmpeg.avformat_find_stream_info(formatCtx, null));
				BestStreamIndex = HR(ffmpeg.av_find_best_stream(formatCtx, AVMediaType.AVMEDIA_TYPE_AUDIO, -1, -1, null, 0));
				List<int> streams = [];
				for (int i = 0; i < formatCtx->nb_streams; i++)
					if (formatCtx->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
						streams.Add(i);
				Streams = streams.AsReadOnly();
			}

			public double GetDuration(int index) {
				if (formatCtx == null) throw new ObjectDisposedException(null);
				if (index >= formatCtx->nb_streams || index < -1)
					throw new ArgumentOutOfRangeException(nameof(index));
				if (index == -1) return (double)formatCtx->duration / ffmpeg.AV_TIME_BASE;
				else {
					var stream = formatCtx->streams[index];
					return (double)stream->duration * stream->time_base.num / stream->time_base.den;
				}
			}

			public double GetPresentationTimestamp() {
				if (swrContext == null) throw new ObjectDisposedException(null);
				var timeBase = formatCtx->streams[SelectedStream]->time_base;
				return (double)_pts * timeBase.num / timeBase.den;
			}

			public void OpenStream(int index) {
				if (formatCtx == null) throw new ObjectDisposedException(null);
				if (codecCtx != null)
					throw new InvalidOperationException("Stream already opened.");
				if (index >= formatCtx->nb_streams)
					throw new ArgumentOutOfRangeException(nameof(index));
				SelectedStream = index;

				var param = formatCtx->streams[index]->codecpar;
				codec = ffmpeg.avcodec_find_decoder(param->codec_id);
				if (codec == null) throw new LibavException("Codec not found.");
				codecCtx = ffmpeg.avcodec_alloc_context3(codec);
				ffmpeg.avcodec_parameters_to_context(codecCtx, param);
				HR(ffmpeg.avcodec_open2(codecCtx, codec, null));
				InFormat = new() {
					Channels = (ushort)codecCtx->ch_layout.nb_channels,
					SampleFormat = FromInternalSampleFormat(codecCtx->sample_fmt),
					SampleRate = (uint)codecCtx->sample_rate,
					ChannelMask = FromInternalChannelMask(codecCtx->ch_layout),
				};

				packet = ffmpeg.av_packet_alloc();
			}

			public WaveFormat GetInFormat() {
				if (formatCtx == null) throw new ObjectDisposedException(null);
				if (codecCtx == null) OpenStream(BestStreamIndex);
				return InFormat!.Value;
			}

			public void SetFormat(WaveFormat format, int bufferSize) {
				if (formatCtx == null) throw new ObjectDisposedException(null);
				if (OutFormat != null) throw new InvalidOperationException("Format already set.");
				if (codecCtx == null) OpenStream(BestStreamIndex);
				OutFormat = format;
				BufferSize = bufferSize;
				var outFormat = OutFormat.Value;
				frameSize = OutFormat.Value.FrameSize;

				AVChannelLayout outLayout;
				if (outFormat.ChannelMask == 0) ffmpeg.av_channel_layout_default(&outLayout, outFormat.Channels);
				else outLayout = ToInternalChannelLayout(outFormat);

				frame = ffmpeg.av_frame_alloc();

				fixed (SwrContext** pSwrContext = &swrContext)
					HR(ffmpeg.swr_alloc_set_opts2(
						pSwrContext,
						&outLayout, ToInternalSampleFormat(outFormat.SampleFormat), (int)outFormat.SampleRate,
						&codecCtx->ch_layout, codecCtx->sample_fmt, codecCtx->sample_rate,
						0, null
					));

				HR(ffmpeg.swr_init(swrContext));
			}

			public int FillBuffer(ref byte buffer, int frameCount) {
				if (swrContext == null) throw new ObjectDisposedException(null);
				if (EOF) return 0;
				fixed (byte* rptr = &buffer) {
					byte* ptr = rptr;
					int decoded = 0;
					while (decoded < frameCount) {
						int frame_count;
						int out_samples = HR(ffmpeg.swr_get_out_samples(swrContext, 0));
						if (out_samples < frameCount) {
							// Samples in the buffer are not sufficient. Read and decode a new frame.
							int ret = ffmpeg.avcodec_receive_frame(codecCtx, frame);
							if (ret == -0xb) {
								while (true) {
									ret = ffmpeg.av_read_frame(formatCtx, packet);
									if (ret == -0x20464f45) {
										EOF = true;
										goto eof;
									}
									else if (ret < 0) HR(ret);
									if (packet->stream_index == SelectedStream) {
										_pts = packet->pts;
										break;
									}
									ffmpeg.av_packet_unref(packet);
								}
								HR(ffmpeg.avcodec_send_packet(codecCtx, packet));
								ffmpeg.av_packet_unref(packet);
								continue;
							}
							frame_count = HR(ffmpeg.swr_convert(swrContext, &ptr, frameCount - decoded, frame->extended_data, frame->nb_samples));
							ffmpeg.av_frame_unref(frame);
						}
						else {
							// Samples in the buffer are sufficient. Flush them.
							frame_count = HR(ffmpeg.swr_convert(swrContext, &ptr, frameCount - decoded, null, 0));
							if (frame_count == 0) goto eof; // Don't know why this happens but dumb fix anyway
						}
						ptr += frame_count * frameSize;
						decoded += frame_count;
					}
				eof:
					return decoded;
				}
			}

			public void Seek(double time) {
				if (formatCtx == null) throw new ObjectDisposedException(null);
				var timeBase = formatCtx->streams[SelectedStream]->time_base;
				var ts = (long)(time / timeBase.num * timeBase.den);
				HR(ffmpeg.avformat_seek_file(formatCtx, SelectedStream, 0, ts, ts, 0));
				HR(ffmpeg.swr_drop_output(swrContext, HR(ffmpeg.swr_get_out_samples(swrContext, 0))));
				if (!EOF) {
					while (true) {
						int ret = ffmpeg.avcodec_receive_frame(codecCtx, frame);
						if (ret == -0xb) {
							while (true) {
								ret = ffmpeg.av_read_frame(formatCtx, packet);
								if (ret == -0x20464f45) {
									EOF = true;
									return;
								}
								else if (ret < 0) HR(ret);
								if (packet->stream_index == SelectedStream) {
									_pts = packet->pts;
									break;
								}
								ffmpeg.av_packet_unref(packet);
							}
							HR(ffmpeg.avcodec_send_packet(codecCtx, packet));
							ffmpeg.av_packet_unref(packet);
							continue;
						}
						long epts = _pts + frame->pkt_duration;
						if (epts >= ts) {
							HR(ffmpeg.swr_convert(swrContext, null, 0, frame->extended_data, frame->nb_samples));
							var outSamples = HR(ffmpeg.swr_get_out_samples(swrContext, 0));
							HR(ffmpeg.swr_drop_output(swrContext, (int)(outSamples * ((double)(ts - _pts) / (epts - _pts)))));
							ffmpeg.av_frame_unref(frame);
							break;
						}
						ffmpeg.av_frame_unref(frame);
					}
				}
			}

			public void Close() {
				if (swrContext != null) {
					fixed (SwrContext** swrContextPtr = &swrContext) {
						ffmpeg.swr_free(swrContextPtr);
					}
				}
				if (frame != null) {
					fixed (AVFrame** framePtr = &frame) {
						ffmpeg.av_frame_free(framePtr);
					}
				}
				if (packet != null) {
					fixed (AVPacket** packetPtr = &packet) {
						ffmpeg.av_packet_free(packetPtr);
					}
				}
				if (codecCtx != null) {
					fixed (AVCodecContext** codecCtxPtr = &codecCtx) {
						ffmpeg.avcodec_free_context(codecCtxPtr);
					}
				}
				if (formatCtx != null) {
					fixed (AVFormatContext** formatCtxPtr = &formatCtx) {
						ffmpeg.avformat_close_input(formatCtxPtr);
					}
					ffmpeg.avformat_free_context(formatCtx);
				}
			}

			static int HR(int value) {
				if (value < 0) throw new LibavException(string.Format(CultureInfo.InvariantCulture, "An external error occured: 0x{0:x8}", -value));
				return value;
			}

			static SampleFormat FromInternalSampleFormat(AVSampleFormat value) => value switch {
				AVSampleFormat.AV_SAMPLE_FMT_U8 or AVSampleFormat.AV_SAMPLE_FMT_U8P => SampleFormat.U8,
				AVSampleFormat.AV_SAMPLE_FMT_S16 or AVSampleFormat.AV_SAMPLE_FMT_S16P => SampleFormat.S16,
				AVSampleFormat.AV_SAMPLE_FMT_S32 or AVSampleFormat.AV_SAMPLE_FMT_S32P => SampleFormat.S32,
				AVSampleFormat.AV_SAMPLE_FMT_FLT or AVSampleFormat.AV_SAMPLE_FMT_FLTP => SampleFormat.F32,
				AVSampleFormat.AV_SAMPLE_FMT_DBL or AVSampleFormat.AV_SAMPLE_FMT_DBLP => SampleFormat.F64,
				_ => throw new NotSupportedException(),
			};
			static AVSampleFormat ToInternalSampleFormat(SampleFormat value) => value switch {
				SampleFormat.U8 => AVSampleFormat.AV_SAMPLE_FMT_U8,
				SampleFormat.S16 => AVSampleFormat.AV_SAMPLE_FMT_S16,
				SampleFormat.S32 => AVSampleFormat.AV_SAMPLE_FMT_S32,
				SampleFormat.F32 => AVSampleFormat.AV_SAMPLE_FMT_FLT,
				SampleFormat.F64 => AVSampleFormat.AV_SAMPLE_FMT_DBL,
				_ => throw new NotSupportedException(),
			};
			static ChannelMask FromInternalChannelMask(AVChannelLayout value) {
				if (value.order == AVChannelOrder.AV_CHANNEL_ORDER_UNSPEC) return 0;
				if (value.order != AVChannelOrder.AV_CHANNEL_ORDER_NATIVE) throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The channel order {0} is not supported.", value.order));

				ulong internalChannelMask = value.u.mask;
				ulong channelMask = internalChannelMask & 0x01f9_8003_ffff;
				if (channelMask != internalChannelMask) throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The Libav channel mask {0} contains a channel that is not supported.", internalChannelMask));
				return (ChannelMask)(
					(channelMask & 0x3ffff) |
					((channelMask & 0x0001_8000_0000) >> 7) |
					((channelMask & 0x0008_0000_0000) >> 12) |
					((channelMask & 0x0030_0000_0000) >> 18) |
					((channelMask & 0x0040_0000_0000) >> 17) |
					((channelMask & 0x0080_0000_0000) >> 19) |
					((channelMask & 0x0100_0000_0000) >> 18)
				);
			}
			static AVChannelLayout ToInternalChannelLayout(WaveFormat value) {
				value.ValidateChannelMask();
				ulong channelMask = (ulong)value.ChannelMask;
				ulong internalChannelMask = channelMask & 0x3ffffff;
				if (internalChannelMask != channelMask) throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The channel mask {0} contains a channel that is not supported by Libav.", channelMask));
				return new AVChannelLayout {
					order = AVChannelOrder.AV_CHANNEL_ORDER_NATIVE,
					nb_channels = value.Channels,
					u = { mask =
						(internalChannelMask & 0x3ffff) |
						((internalChannelMask & 0x000c0000) << 18) |
						((internalChannelMask & 0x00100000) << 19) |
						((internalChannelMask & 0x00200000) << 17) |
						((internalChannelMask & 0x00400000) << 18) |
						((internalChannelMask & 0x00800000) << 12) |
						((internalChannelMask & 0x03000000) << 7)
					},
				};
			}
		}
		readonly Internal _internal = new(file);

		/// <summary>
		/// Whether this audio stream has been disposed.
		/// </summary>
		public bool Disposed { get; private set; }

		/// <inheritdoc />
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			_internal.Close();
			Disposed = true;
		}

		/// <inheritdoc />
		public override bool EndOfData => _internal.EOF;

		/// <inheritdoc />
		/// <remarks>
		/// <para>This property may be inaccurate.</para>
		/// </remarks>
		public override long FrameLength => (long)(TimeLength * Format.SampleRate);
		/// <inheritdoc />
		/// <remarks>
		/// <para>This property may be inaccurate.</para>
		/// </remarks>
		public override double TimeLength => GetStreamDuration(_internal.SelectedStream);
		/// <inheritdoc />
		public override double TimePosition => _time;

		/// <summary>
		/// The index to the best audio stream.
		/// </summary>
		public int BestStreamIndex => _internal.BestStreamIndex;
		/// <summary>
		/// The collection of indices to all audio streams.
		/// </summary>
		public ReadOnlyCollection<int> Streams => _internal.Streams;

		/// <summary>
		/// Selects the best stream as the source.
		/// </summary>
		/// <exception cref="InvalidOperationException">The stream has been selected.</exception>
		/// <remarks>
		/// <para>This method can only be called before <see cref="AudioStream.SetFormat(WaveFormat, int)" /> is called, which is called while setting <see cref="AudioClient.Source" />.</para>
		/// </remarks>
		public void SelectStream() => SelectStream(BestStreamIndex);

		/// <summary>
		/// Selects a stream as the source.
		/// </summary>
		/// <param name="index">The index of the stream.</param>
		/// <exception cref="InvalidOperationException">The stream has been selected.</exception>
		/// <remarks>
		/// <para>This method can only be called before <see cref="AudioStream.SetFormat(WaveFormat, int)" /> is called, which is called while setting <see cref="AudioClient.Source" />.</para>
		/// </remarks>
		public void SelectStream(int index) => _internal.OpenStream(index);

		/// <summary>
		/// Gets the duration of a stream or the file.
		/// </summary>
		/// <param name="streamId">The stream index. The duration of the file is retrieved if <c>-1</c> is specified.</param>
		/// <returns>The duration in seconds.</returns>
		public double GetStreamDuration(int streamId = -1) => _internal.GetDuration(streamId);
		/// <inheritdoc />
		public override WaveFormat DefaultFormat => _internal.GetInFormat();
		/// <inheritdoc />
		public override bool IsFormatSupported(WaveFormat format)
			=> format.SampleFormat == SampleFormat.U8
			|| format.SampleFormat == SampleFormat.S16
			|| format.SampleFormat == SampleFormat.S32
			|| format.SampleFormat == SampleFormat.F32
			|| format.SampleFormat == SampleFormat.F64;

		/// <inheritdoc />
		protected override void OnSetFormat() => _internal.SetFormat(Format, BufferSize);

		double _time;
		/// <inheritdoc />
		protected override int ReadFramesInternal(ref byte buffer, int frameCount) {
			if (Disposed) throw new ObjectDisposedException(null);
			frameCount = _internal.FillBuffer(ref buffer, frameCount);
			_time = _internal.GetPresentationTimestamp();
			return frameCount;
		}

		/// <inheritdoc />
		protected override long SeekFrameInternal(long frameOffset, SeekOrigin origin)
			=> (long)(SeekTimeInternal((double)frameOffset / Format.SampleRate, origin) * Format.SampleRate);
		/// <inheritdoc />
		protected override double SeekTimeInternal(double timeOffset, SeekOrigin origin) {
			if (Disposed) throw new ObjectDisposedException(null);
			var newTime = origin switch {
				SeekOrigin.Begin => timeOffset,
				SeekOrigin.Current => _time + timeOffset,
				SeekOrigin.End => TimeLength + timeOffset,
				_ => throw new ArgumentException("Invalid SeekOrigin.", nameof(origin)),
			};
			if (newTime < 0) throw new ArgumentException("Seeking is attempted before the beginning of the stream.");
			_internal.Seek(newTime);
			_time = _internal.GetPresentationTimestamp();
			return _time;
		}

		/// <inheritdoc />
		public override bool CanRead => !Disposed;
		/// <inheritdoc />
		public override bool CanSeek => !Disposed;
		/// <inheritdoc />
		public override bool CanWrite => false;
		/// <inheritdoc />
		public override void Flush() { }
		/// <inheritdoc />
		public override void SetLength(long value) => throw new NotSupportedException();
		/// <inheritdoc />
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	}

	/// <summary>
	/// The exception that is thrown by Libav.
	/// </summary>
	[Serializable]
	public class LibavException : Exception {
		/// <inheritdoc />
		public LibavException() { }
		/// <inheritdoc />
		public LibavException(string message) : base(message) { }
		/// <inheritdoc />
		public LibavException(string message, Exception innerException) : base(message, innerException) { }
		/// <inheritdoc />
		protected LibavException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
