using System;
using UnsafeIL;

namespace Cryville.Audio.Source.Resample {
	sealed unsafe class FilterKit(int Npc) {
		const double IzeroEPSILON = 1E-21;
		static double Izero(double x) {
			double sum = 1, u = 1;
			int n = 1;
			double halfx = x / 2.0;
			do {
				double temp = halfx / n;
				n += 1;
				temp *= temp;
				u *= temp;
				sum += u;
			} while (u >= IzeroEPSILON * sum);
			return sum;
		}

		public static void LrsLpFilter(double* c, int N, double frq, double Beta, int Num) {
			c[0] = 2.0 * frq;
			for (int i = 1; i < N; i++) {
				double temp = Math.PI * i / Num;
				c[i] = Math.Sin(2.0 * temp * frq) / temp;
			}

			double IBeta = 1.0 / Izero(Beta);
			double inm1 = 1.0 / (N - 1);
			for (int i = 1; i < N; i++) {
				double temp = i * inm1;
				double temp1 = 1.0 - temp * temp;
				temp1 = temp1 < 0 ? 0 : temp1;
				c[i] *= Izero(Beta * Math.Sqrt(temp1)) * IBeta;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Imp">impulse response</param>
		/// <param name="ImpD">impulse response deltas</param>
		/// <param name="Nwing">len of one wing of filter</param>
		/// <param name="Interp">Interpolate coefs using deltas?</param>
		/// <param name="Xp">Current sample</param>
		/// <param name="Ph">Phase</param>
		/// <param name="Inc">increment (1 for right wing or -1 for left)</param>
		/// <returns></returns>
		public double LrsFilterUp(double[] Imp, double[] ImpD, int Nwing, bool Interp, ref double Xp, double Ph, int Inc) {
			Ph *= Npc;

			double v = 0.0f;
			fixed (double* cHp = &Imp[(int)Ph], pImp = Imp) {
				double* Hp = cHp;
				double* End = pImp + Nwing;
				if (Interp) {
					fixed (double* cHdp = &ImpD[(int)Ph]) {
						double* Hdp = cHdp;
						double a = Ph - Math.Floor(Ph);

						if (Inc == 1) {
							End--;
							if (Ph == 0) {
								Hp += Npc;
								Hdp += Npc;
							}
						}

						while (Hp < End) {
							double t = *Hp;
							t += *Hdp * a;
							Hdp += Npc;
							t *= Xp;
							v += t;
							Hp += Npc;
							Xp = ref Unsafe.Add(ref Xp, Inc);
						}
					}
				}
				else {
					if (Inc == 1) {
						End--;
						if (Ph == 0) {
							Hp += Npc;
						}
					}

					while (Hp < End) {
						double t = *Hp;
						t *= Xp;
						v += t;
						Hp += Npc;
						Xp = ref Unsafe.Add(ref Xp, Inc);
					}
				}
			}

			return v;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Imp">impulse response</param>
		/// <param name="ImpD">impulse response deltas</param>
		/// <param name="Nwing">len of one wing of filter</param>
		/// <param name="Interp">Interpolate coefs using deltas?</param>
		/// <param name="Xp">Current sample</param>
		/// <param name="Ph">Phase</param>
		/// <param name="Inc">increment (1 for right wing or -1 for left)</param>
		/// <param name="dhb">filter sampling period</param>
		/// <returns></returns>
		public static double LrsFilterUD(double[] Imp, double[] ImpD, int Nwing, bool Interp, ref double Xp, double Ph, int Inc, double dhb) {
			double v = 0.0f;
			double Ho = Ph * dhb;
			int End = Nwing;
			if (Inc == 1) {
				End--;
				if (Ph == 0)
					Ho += dhb;
			}

			int Hp;
			if (Interp)
				while ((Hp = (int)Ho) < End) {
					double t = Imp[Hp];
					double a = Ho - Math.Floor(Ho);
					t += ImpD[Hp] * a;
					t *= Xp;
					v += t;
					Ho += dhb;
					Xp = ref Unsafe.Add(ref Xp, Inc);
				}
			else
				while ((Hp = (int)Ho) < End) {
					double t = Imp[Hp];
					t *= Xp;
					v += t;
					Ho += dhb;
					Xp = ref Unsafe.Add(ref Xp, Inc);
				}

			return v;
		}
	}
}
