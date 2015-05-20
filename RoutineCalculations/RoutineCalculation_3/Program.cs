using System;
using System.Collections.Generic;
using System.Linq;
using BaseTypes;
using BaseTypes.FrequencyFunction;
using BaseTypes.Interval;

namespace RoutineCalculation_3
{
    internal class Program
    {
        private static readonly PiecewiseFunction<double> Function = new PiecewiseFunction<double>
        {
            Functions = new Dictionary<Interval<double>, Func<double, double>>
            {
                {new DoubleInterval(double.NegativeInfinity, double.PositiveInfinity), x => 1d/(x + 3)}
            }
        };

        private const int ChiSquareVariationalSeriesLength = 200;
        private const int KolmogorovVariationalSeriesLength = 30;
        private const int OmegaSquareVariationalSeriesLength = 50;

        private static void Main(string[] args)
        {
            PearsonTest();
            KolmogorovTest();
            OmegaSquareTest();
            Console.ReadKey();
        }

        private static void PearsonTest(int barCount = 10)
        {
            var rnd = new Random();
            var variationalSeries =
                new SequenceGenerator<double>(0, ChiSquareVariationalSeriesLength,
                    x => x += 10d/ChiSquareVariationalSeriesLength,
                    x => Function.Calculate(rnd.NextDouble()*10)).Select(v => Math.Round(v, 3)).OrderBy(x => x).ToList();

            var histogram = new DoubleHistogram(variationalSeries);

            var equalProbavilityHistigramBars = histogram.EqualProbabilityHistogram(barCount).ToList();

            var pi = new double[barCount];
            var xMax = variationalSeries.Max();
            var xMin = variationalSeries.Min();
            var samplingIntervalLength = xMax - xMin;

            for (var i = 0; i < barCount; i++)
            {
                var bar = equalProbavilityHistigramBars[i];

                pi[i] = (bar.Interval.Right - bar.Interval.Left)/samplingIntervalLength;
            }

            var check = 1 - pi.Sum();
            if (check > 0.01)
            {
                throw new Exception();
            }

            double chiSquare = 0;
            for (var i = 0; i < barCount; i++)
            {
                var bar = equalProbavilityHistigramBars[i];

                chiSquare += Math.Pow(pi[i] - (bar.ValuesCount/ChiSquareVariationalSeriesLength), 2)/pi[i];
            }

            #region Второй вариант

            //double chiSquare2 = 0;
            //for (var i = 0; i < barCount; i++)
            //{
            //    var bar = equalProbavilityHistigramBars[i];

            //    chiSquare2 += Math.Pow(bar.ValuesCount - ChiSquareVariationalSeriesLength*pi[i], 2)/
            //                  (ChiSquareVariationalSeriesLength*pi[i]);
            //}

            //Console.WriteLine(chiSquare2);

            #endregion

            var itemsPerBar = ChiSquareVariationalSeriesLength/barCount;
            chiSquare *= itemsPerBar;
            var k = barCount - 2 - 1;
            Console.WriteLine(new string('=', 80));
            Console.WriteLine("Pearson Test");
            Console.WriteLine("n = {0}", ChiSquareVariationalSeriesLength);
            Console.WriteLine("chiSquare = {0}", chiSquare);
            Console.WriteLine("count of bars = {0}", barCount);
            Console.WriteLine("S = 2 (Uniform distribution)");
            Console.WriteLine("k = barCount - S - 1 = {0} - 2 - 1 = {1}", barCount, k);
            Console.WriteLine("_____________________________________________");
            Console.WriteLine("alpha             | 0.05    | 0,01   | 0,001 ");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("chiS(alpha,{0})     | 14,068  | 18,478 | 24,327", k);
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine();
        }

        private static void KolmogorovTest()
        {
            var rnd = new Random();
            var variationalSeries =
                new SequenceGenerator<double>(0, KolmogorovVariationalSeriesLength,
                    x => x += 10d/KolmogorovVariationalSeriesLength,
                    x => Function.Calculate(rnd.NextDouble()*10 - 0)).Select(v => Math.Round(v, 3)).OrderBy(
                        x => x).ToList();

            var freqFunc = new DoubleFrequencyFunction(variationalSeries);
            var freqFuncPoints = freqFunc.Function.ToList();

            var xMax = variationalSeries.Max();
            var xMin = variationalSeries.Min();
            var samplingIntervalLength = xMax - xMin;

            var kolmogorovTeoreticalFunction = new PiecewiseFunction<double>
            {
                Functions = new Dictionary<Interval<double>, Func<double, double>>
                {
                    {new DoubleInterval(double.NegativeInfinity, xMin), x => 0},
                    {new DoubleInterval(xMin, xMax, true, true), x => (x - xMin)/samplingIntervalLength},
                    {new DoubleInterval(xMax, double.PositiveInfinity), x => 1}
                }
            };


            double absMaxDeviation = 0;

            for (var i = 0; i < freqFuncPoints.Count; i++)
            {
                var temp = Math.Abs(freqFuncPoints[i].Y - kolmogorovTeoreticalFunction.Calculate(freqFuncPoints[i].X));
                if (temp > absMaxDeviation)
                {
                    absMaxDeviation = temp;
                }

                if (i <= 0) continue;
                temp =
                    Math.Abs(freqFuncPoints[i - 1].Y - kolmogorovTeoreticalFunction.Calculate(freqFuncPoints[i].X));
                if (temp > absMaxDeviation)
                {
                    absMaxDeviation = temp;
                }
            }

            var lambda = Math.Sqrt(KolmogorovVariationalSeriesLength)*absMaxDeviation;

            Console.WriteLine(new string('=', 80));
            Console.WriteLine("Kolmogorov test");
            Console.WriteLine("n = {0}", KolmogorovVariationalSeriesLength);
            Console.WriteLine("lambda = {0}", lambda);
            Console.WriteLine("_______________________________");
            Console.WriteLine("alpha         | 0.05   | 0.02");
            Console.WriteLine("-------------------------------");
            Console.WriteLine("lambda(alpha) | 1.358  | 1.520");
            Console.WriteLine("-------------------------------");
        }

        private static void OmegaSquareTest()
        {
            var rnd = new Random();
            var variationalSeries =
                new SequenceGenerator<double>(0, OmegaSquareVariationalSeriesLength,
                    x => x += 10d/OmegaSquareVariationalSeriesLength,
                    x => Function.Calculate(rnd.NextDouble()*10 - 0)).Select(v => Math.Round(v, 6)).OrderBy(
                        x => x).ToList();

            var nOmegaSquare = 1d/(12*OmegaSquareVariationalSeriesLength) +
                               variationalSeries.Select(
                                   (x, i) =>
                                       Math.Pow(
                                           Function.Calculate(x) - ((i + 1) - 0.5)/OmegaSquareVariationalSeriesLength, 2))
                                   .Sum();
            var omegaSquare = nOmegaSquare/OmegaSquareVariationalSeriesLength;

            Console.WriteLine(new string('=', 80));
            Console.WriteLine("OmegaSqure Test");
            Console.WriteLine("n = {0}", OmegaSquareVariationalSeriesLength);
            Console.WriteLine("nOmegaSquare = {0}", omegaSquare);
            Console.WriteLine("_____________________________");
            Console.WriteLine("alpha | 0.1   | 0.05  | 0.01");
            Console.WriteLine("-----------------------------");
            Console.WriteLine("w2    | 0.347 | 0.461 | 0.743");
            Console.WriteLine("-----------------------------");
        }
    }
}
