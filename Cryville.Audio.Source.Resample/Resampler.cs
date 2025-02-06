using System;
using UnsafeIL;

namespace Cryville.Audio.Source.Resample {
	unsafe sealed class Resampler {
		readonly int Npc;

		readonly double[] Imp, ImpD;
		float LpScl;
		readonly int Nmult, Nwing;
		readonly double minFactor, maxFactor;
		readonly int XSize;
		readonly double[] X;
		int Xp, Xread;
		readonly int Xoff, YSize;
		readonly double[] Y;
		int Yp;
		double Time;

		public int FilterWidth => Xoff;

		readonly FilterKit _filterKit;

		public Resampler(bool highQuality, double minFactor, double maxFactor, int Npc = 4096) {
			if (minFactor <= 0.0) 
				throw new ArgumentOutOfRangeException(nameof(minFactor));
			if (maxFactor <= 0.0) 
				throw new ArgumentOutOfRangeException(nameof(maxFactor));
			if (maxFactor < minFactor) 
				throw new ArgumentException("maxFactor is less than minFactor.");

			this.Npc = Npc;
			this.minFactor = minFactor;
			this.maxFactor = maxFactor;

			Nmult = highQuality ? 35 : 11;

			LpScl = 1.0f;
			Nwing = Npc * (Nmult - 1) / 2;

			const double Rolloff = 0.90;
			const double Beta = 6;

			{
				double* Imp64 = stackalloc double[Nwing];

				_filterKit = new(Npc);

				FilterKit.LrsLpFilter(Imp64, Nwing, 0.5 * Rolloff, Beta, Npc);

				Imp = new double[Nwing];
				ImpD = new double[Nwing];
				for (int i = 0; i < Nwing; i++)
					Imp[i] = Imp64[i];

				for (int i = 0; i < Nwing - 1; i++)
					ImpD[i] = Imp[i + 1] - Imp[i];

				ImpD[Nwing - 1] = -Imp[Nwing - 1];
			}

			int Xoff_min = (int)((Nmult + 1) / 2.0 * Math.Max(1.0, 1.0 / minFactor) + 10);
			int Xoff_max = (int)((Nmult + 1) / 2.0 * Math.Max(1.0, 1.0 / maxFactor) + 10);
			Xoff = Math.Max(Xoff_min, Xoff_max);

			XSize = Math.Max(2 * Xoff + 10, 4096);
			X = new double[XSize + Xoff];
			Xp = Xoff;
			Xread = Xoff;

			Array.Clear(X, 0, Xoff);

			YSize = (int)(XSize * maxFactor + 2.0);
			Y = new double[YSize];
			Yp = 0;

			Time = Xoff;
		}

		public int Process(double factor, double* inBuffer, int inBufferLen, bool lastFlag, out int inBufferUsed, double* outBuffer, int outBufferLen) {
			if (factor < minFactor || factor > maxFactor)
				throw new ArgumentOutOfRangeException(nameof(factor));

			inBufferUsed = 0;
			int outSampleCount = 0;

			if (Yp != 0 && outBufferLen > 0) {
				int len = Math.Min(outBufferLen, Yp);
				for (int i = 0; i < len; i++)
					outBuffer[i] = Y[i];
				outSampleCount = len;
				for (int i = 0; i < Yp - len; i++)
					Y[i] = Y[i + len];
				Yp -= len;
			}

			if (Yp != 0)
				return outSampleCount;

			if (factor < 1)
				LpScl = (float)(LpScl * factor);

			for (; ; ) {
				int len = XSize - Xread;

				if (len >= (inBufferLen - inBufferUsed))
					len = inBufferLen - inBufferUsed;

				for (int i = 0; i < len; i++)
					X[Xread + i] = inBuffer[inBufferUsed + i];

				inBufferUsed += len;
				Xread += len;

				int Nx;
				if (lastFlag && (inBufferUsed == inBufferLen)) {
					Nx = Xread - Xoff;
					for (int i = 0; i < Xoff; i++)
						X[Xread + i] = 0;
				}
				else
					Nx = Xread - 2 * Xoff;

				if (Nx <= 0)
					break;

				int Nout;
				if (factor >= 1) {
					Nout = LrsSrcUp(factor, Nx, false);
				}
				else {
					Nout = LrsSrcUD(factor, Nx, false);
				}

				Time -= Nx;
				Xp += Nx;

				int Ncreep = (int)Time - Xoff;
				if (Ncreep != 0) {
					Time -= Ncreep;
					Xp += Ncreep;
				}

				int Nreuse = Xread - (Xp - Xoff);

				for (int i = 0; i < Nreuse; i++)
					X[i] = X[i + (Xp - Xoff)];

				Xread = Nreuse;
				Xp = Xoff;

				if (Nout > YSize) {
					throw new InvalidOperationException("Output array overflow.");
				}

				Yp = Nout;

				if (Yp != 0 && (outBufferLen - outSampleCount) > 0) {
					len = Math.Min(outBufferLen - outSampleCount, Yp);
					for (int i = 0; i < len; i++)
						outBuffer[outSampleCount + i] = Y[i];
					outSampleCount += len;
					for (int i = 0; i < Yp - len; i++)
						Y[i] = Y[i + len];
					Yp -= len;
				}

				if (Yp != 0)
					break;
			}

			return outSampleCount;
		}
		int LrsSrcUp(double factor, int Nx, bool Interp) {
			int Yi = 0;

			double CurrentTime = Time;
			double dt = 1.0 / factor;
			double endTime = CurrentTime + Nx;
			while (CurrentTime < endTime) {
				double LeftPhase = CurrentTime - Math.Floor(CurrentTime);
				double RightPhase = 1.0 - LeftPhase;

				ref double Xp = ref X[(int)CurrentTime];
				double v = _filterKit.LrsFilterUp(Imp, ImpD, Nwing, Interp, ref Xp, LeftPhase, -1);
				v += _filterKit.LrsFilterUp(Imp, ImpD, Nwing, Interp, ref Unsafe.Add(ref Xp, 1), RightPhase, 1);

				v *= LpScl;

				Y[Yi++] = v;
				CurrentTime += dt;
			}

			Time = CurrentTime;
			return Yi;
		}
		int LrsSrcUD(double factor, int Nx, bool Interp) {
			int Yi = 0;

			double CurrentTime = Time;
			double dt = 1.0 / factor;
			double dh = Math.Min(Npc, factor * Npc);
			double endTime = CurrentTime + Nx;
			while (CurrentTime < endTime) {
				double LeftPhase = CurrentTime - Math.Floor(CurrentTime);
				double RightPhase = 1.0 - LeftPhase;

				ref double Xp = ref X[(int)CurrentTime];
				double v = FilterKit.LrsFilterUD(Imp, ImpD, Nwing, Interp, ref Xp, LeftPhase, -1, dh);
				v += FilterKit.LrsFilterUD(Imp, ImpD, Nwing, Interp, ref Unsafe.Add(ref Xp, 1), RightPhase, 1, dh);

				v *= LpScl;
				Y[Yi++] = v;

				CurrentTime += dt;
			}

			Time = CurrentTime;
			return Yi;
		}
	}
}
