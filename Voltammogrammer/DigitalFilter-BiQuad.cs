using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Ref: Cookbook formulae for audio equalizer biquad filter coefficients

namespace Voltammogrammer
{
    class DigitalFilter_BiQuad
    {
        // フィルタの係数
        double a0, a1, a2, b0, b1, b2;
        // バッファ
        double out1, out2;
        double in1, in2;

        public DigitalFilter_BiQuad()
        {
            // メンバー変数を初期化
            a0 = 1.0; // 0以外にしておかないと除算でエラーになる
            a1 = 0.0;
            a2 = 0.0;
            b0 = 1.0;
            b1 = 0.0;
            b2 = 0.0;

            in1 = 0.0;
            in2 = 0.0;

            out1 = 0.0;
            out2 = 0.0;
        }

        public double Process(double in0)
        {
            // 入力信号にフィルタを適用し、出力信号変数に保存。
            double out0 = b0 / a0 * in0 + b1 / a0 * in1 + b2 / a0 * in2 - a1 / a0 * out1 - a2 / a0 * out2;

            in2 = in1; // 2つ前の入力信号を更新
            in1 = in0;  // 1つ前の入力信号を更新

            out2 = out1; // 2つ前の出力信号を更新
            out1 = out0;  // 1つ前の出力信号を更新

            // 出力信号を返す
            return out0;
        }

        public void Notch(double freq, double samplerate, double bw = 1.0)
        {
            // フィルタ係数計算で使用する中間値を求める。
            double omega = 2.0 * Math.PI * freq / samplerate;
            double alpha = Math.Sin(omega) * Math.Sinh(Math.Log(2.0) / 2.0 * bw * omega / Math.Sin(omega));

            // フィルタ係数を求める。
            a0 = 1.0 + alpha;
            a1 = -2.0 * Math.Cos(omega);
            a2 = 1.0 - alpha;
            b0 = 1.0;
            b1 = -2.0 * Math.Cos(omega);
            b2 = 1.0;
        }

        public void LowPass(double freq, double samplerate, double q = 0.707107)
        {
            // フィルタ係数計算で使用する中間値を求める。
            double omega = 2.0 * Math.PI * freq / samplerate;
            double alpha = Math.Sin(omega) / (2.0 * q);

            // フィルタ係数を求める。
            a0 = 1.0 + alpha;
            a1 = -2.0 * Math.Cos(omega);
            a2 = 1.0 - alpha;
            b0 = (1.0 - Math.Cos(omega)) / 2.0;
            b1 = 1.0 - Math.Cos(omega);
            b2 = (1.0 - Math.Cos(omega)) / 2.0;
        }
    }
}
