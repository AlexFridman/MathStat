﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BaseTypes;
using BaseTypes.Interval;
using C1.WPF.C1Chart;

namespace RoutineCalculation_4
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow2 : Window
    {
        private readonly int _a = 0;
        private readonly double[] _alphas = {0.001, 0.005, 0.01, 0.05, 0.1, 0.15, 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5};
        private readonly int _b = 10;

        private readonly PiecewiseFunction<double> _function = new PiecewiseFunction<double>
        {
            Functions = new Dictionary<Interval<double>, Func<double, double>>
            {
                {new DoubleInterval(double.NegativeInfinity, double.PositiveInfinity), x => 1d/(x + 3)}
            }
        };

        private readonly double _teorDeviation = 1.797;
        private int _decimals = 3;
        private List<Point> _firstEstimateFuncPoints;
        private Dictionary<double, DoubleInterval> _firstEstimates;
        private int _n;
        private List<Point> _secondEstimateFuncPoints;
        private Dictionary<double, DoubleInterval> _secondEstimates;
        private double _teorExpectation = 5.6;
        private List<double> _variationalSeries;

        public MainWindow2()
        {
            InitializeComponent();
            BuildGraph();
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            if (!TryReadParameters())
            {
                MessageBox.Show("Ввеите корректное n.");
                return;
            }

            BuildVarSeries();
            BuildFirstEstimates();
            DisplayDeviationEstimate();
            DisplayEstimatesTable1();
            BuildFirstEstimateFunc();
            DisplayFirstEstimateFunc();

            BuildSecondEstimates();
            DisplayEstimatesTable2();
            BuildSecondEstimateFunc();
            DisplaySecondEstimateFunc();

            DisplayCompareChart();
        }

        private bool TryReadParameters()
        {
            if (!int.TryParse(NTextBox.Text, out _n))
            {
                return false;
            }

            if (_n < 1 || _n > 1000000)
            {
                return false;
            }

            if (!int.TryParse(DecimalsTextBox.Text, out _decimals))
            {
                return false;
            }

            if (_decimals < 1 || _decimals > 10)
            {
                return false;
            }

            return true;
        }

        private void BuildVarSeries()
        {
            var rnd = new Random();

            var rndValues = new SequenceGenerator<double>(0, _n, v => v += 1,
                x => _function.Calculate(rnd.NextDouble()*(_b - _a) + _a));
            var roundedValues = rndValues.Select(v => (double) Math.Round((decimal) v, 6)).ToList();

            _variationalSeries = roundedValues;
        }

        private void DisplayDeviationEstimate()
        {
            var deviationEstimate = Task2.DeviationEstimate(_variationalSeries);
            DeviationEstimateLabel.Content = deviationEstimate;
        }

        private void BuildFirstEstimates()
        {
            var estimates = new Dictionary<double, DoubleInterval>();

            foreach (var alpha in _alphas)
            {
                var estimateInterval = Task2.DeviationConfidenceInterval(_variationalSeries.Count,
                    Task2.DeviationEstimate(_variationalSeries),
                    1 - alpha);
                estimates.Add(alpha, estimateInterval);
            }

            _firstEstimates = estimates;
        }

        private void BuildGraph()
        {
            var firstPoints = new List<Point>();
            for (var n = 5; n < 1000; n += 5)
            {
                var rnd = new Random();
                var varSeries = new SequenceGenerator<double>(0, n, x => x += 1,
                    x => _function.Calculate(rnd.NextDouble()*10)).OrderBy(x => x).ToList();


                var estimateInterval = Task2.DeviationConfidenceInterval(varSeries.Count,
                    Task1.DeviationEstimate(varSeries), 0.95);
                firstPoints.Add(new Point(n, estimateInterval.Length));
            }

            var secondPoints = new List<Point>();
            for (var n = 5; n < 1000; n += 5)
            {
                var rnd = new Random();
                var varSeries = new SequenceGenerator<double>(0, n, x => x += 1,
                    x => _function.Calculate(rnd.NextDouble()*10)).OrderBy(x => x).ToList();


                var estimateInterval = Task2.DeviationConfidenceInterval(varSeries.Count, _teorDeviation, 0.95);
                secondPoints.Add(new Point(n, estimateInterval.Length));
            }

            using (var stream = File.CreateText("points.txt"))
            {
                foreach (var firstPoint in firstPoints)
                {
                    stream.WriteLine(firstPoint);
                }
                stream.WriteLine("============================");
                foreach (var firstPoint in secondPoints)
                {
                    stream.WriteLine(firstPoint);
                }
            }

            NCompare.Data.Children.Add(new XYDataSeries
            {
                ItemsSource = firstPoints,
                XValueBinding = new Binding("X"),
                ValueBinding = new Binding("Y"),
                ChartType = ChartType.Line,
                Label = "эмпирич МО"
            });

            NCompare.Data.Children.Add(new XYDataSeries
            {
                ItemsSource = secondPoints,
                XValueBinding = new Binding("X"),
                ValueBinding = new Binding("Y"),
                ChartType = ChartType.Line,
                Label = "теоретич МО"
            });
        }


        private void DisplayEstimatesTable1()
        {
            ExpectationEstimateTable1.ColumnDefinitions.Clear();
            ExpectationEstimateTable1.Children.Clear();
            ExpectationEstimateTable1.ColumnDefinitions.Add(new ColumnDefinition());
            var alphaLabel = CreateLabel("alpha");
            var mLabel = CreateLabel("m*", 1);
            var intervalLabel = CreateLabel("[,]", 2);

            ExpectationEstimateTable1.Children.Add(alphaLabel);
            ExpectationEstimateTable1.Children.Add(mLabel);
            ExpectationEstimateTable1.Children.Add(intervalLabel);

            var coulmn = 1;
            foreach (var estimate in _firstEstimates)
            {
                ExpectationEstimateTable1.ColumnDefinitions.Add(new ColumnDefinition());
                var estimateInterval = estimate.Value;

                var left = Math.Round(estimateInterval.Left, _decimals);
                var rigth = Math.Round(estimateInterval.Right, _decimals);
                var interval = string.Format("[{0}, {1}]", left.ToString(CultureInfo.InvariantCulture),
                    rigth.ToString(CultureInfo.InvariantCulture));


                intervalLabel = CreateLabel(interval, 2, coulmn);

                var tetta = Math.Round(estimateInterval.Middle, _decimals).ToString(CultureInfo.InvariantCulture);
                var eps =
                    Math.Round((estimateInterval.Right - estimateInterval.Left)/2, _decimals)
                        .ToString(CultureInfo.InvariantCulture);
                var result = string.Format("{0}±{1}", tetta, eps);

                alphaLabel = CreateLabel(estimate.Key.ToString(CultureInfo.InvariantCulture), 0, coulmn);
                mLabel = CreateLabel(result, 1, coulmn);

                ExpectationEstimateTable1.Children.Add(alphaLabel);
                ExpectationEstimateTable1.Children.Add(mLabel);
                ExpectationEstimateTable1.Children.Add(intervalLabel);
                coulmn++;
            }
        }

        private void BuildFirstEstimateFunc()
        {
            var points = new List<Point>();
            foreach (var estimate in _firstEstimates)
            {
                var point = new Point(estimate.Key, estimate.Value.Length);

                points.Add(point);
            }

            _firstEstimateFuncPoints = points;
        }

        private void DisplayFirstEstimateFunc()
        {
            FirstEstimateChart.Data.Children.Clear();
            FirstEstimateChart.View.AxisX.Title = "alpha";
            FirstEstimateChart.View.AxisY.Title = "interval length";
            FirstEstimateChart.Data.Children.Add(new XYDataSeries
            {
                ItemsSource = _firstEstimateFuncPoints,
                XValueBinding = new Binding("X"),
                ValueBinding = new Binding("Y"),
                ChartType = ChartType.Line
            });
        }

        private void DisplayCompareChart()
        {
            CompareChart.Data.Children.Clear();
            CompareChart.View.AxisX.Title = "alpha";
            CompareChart.View.AxisY.Title = "interval length";
            CompareChart.Data.Children.Add(new XYDataSeries
            {
                ItemsSource = _secondEstimateFuncPoints,
                XValueBinding = new Binding("X"),
                ValueBinding = new Binding("Y"),
                ChartType = ChartType.Line,
                Label = "с теор. МО"
            });
            CompareChart.Data.Children.Add(new XYDataSeries
            {
                ItemsSource = _firstEstimateFuncPoints,
                XValueBinding = new Binding("X"),
                ValueBinding = new Binding("Y"),
                ChartType = ChartType.Line,
                Label = "с эмпирич. МО"
            });
        }

        private Label CreateLabel(string content, int gridRow = 0, int gridColumn = 0)
        {
            var label = new Label
            {
                Content = content
            };

            Grid.SetColumn(label, gridColumn);
            Grid.SetRow(label, gridRow);

            return label;
        }

        private void ChangeTaskButton_OnClick(object sender, RoutedEventArgs e)
        {
            var w1 = new MainWindow
            {
                Top = Top,
                Left = Left
            };
            w1.Show();
            Close();
        }

        #region second estimates

        private void BuildSecondEstimates()
        {
            var estimates = new Dictionary<double, DoubleInterval>();
            foreach (var alpha in _alphas)
            {
                var estimateInterval = Task2.DeviationConfidenceInterval(_variationalSeries.Count,
                    _teorDeviation, 1 - alpha);
                estimates.Add(alpha, estimateInterval);
            }

            _secondEstimates = estimates;
        }

        private void DisplayEstimatesTable2()
        {
            ExpectationEstimateTable2.ColumnDefinitions.Clear();
            ExpectationEstimateTable2.Children.Clear();
            ExpectationEstimateTable2.ColumnDefinitions.Add(new ColumnDefinition());
            var alphaLabel = CreateLabel("alpha");
            var mLabel = CreateLabel("m*", 1);
            var intervalLabel = CreateLabel("[,]", 2);

            ExpectationEstimateTable2.Children.Add(alphaLabel);
            ExpectationEstimateTable2.Children.Add(mLabel);
            ExpectationEstimateTable2.Children.Add(intervalLabel);

            var coulmn = 1;
            foreach (var estimate in _secondEstimates)
            {
                ExpectationEstimateTable2.ColumnDefinitions.Add(new ColumnDefinition());
                var estimateInterval = estimate.Value;

                var left = Math.Round(estimateInterval.Left, _decimals);
                var rigth = Math.Round(estimateInterval.Right, _decimals);
                var interval = string.Format("[{0}, {1}]", left.ToString(CultureInfo.InvariantCulture),
                    rigth.ToString(CultureInfo.InvariantCulture));


                intervalLabel = CreateLabel(interval, 2, coulmn);

                var tetta = Math.Round(estimateInterval.Middle, _decimals).ToString(CultureInfo.InvariantCulture);
                var eps =
                    Math.Round((estimateInterval.Right - estimateInterval.Left)/2, _decimals)
                        .ToString(CultureInfo.InvariantCulture);
                var result = string.Format("{0}±{1}", tetta, eps);

                alphaLabel = CreateLabel(estimate.Key.ToString(CultureInfo.InvariantCulture), 0, coulmn);
                mLabel = CreateLabel(result, 1, coulmn);

                ExpectationEstimateTable2.Children.Add(alphaLabel);
                ExpectationEstimateTable2.Children.Add(mLabel);
                ExpectationEstimateTable2.Children.Add(intervalLabel);
                coulmn++;
            }
        }

        private void BuildSecondEstimateFunc()
        {
            var points = new List<Point>();
            foreach (var estimate in _secondEstimates)
            {
                var point = new Point(estimate.Key, estimate.Value.Length);

                points.Add(point);
            }

            _secondEstimateFuncPoints = points;
        }

        private void DisplaySecondEstimateFunc()
        {
            SecondEstimateChart.Data.Children.Clear();
            SecondEstimateChart.View.AxisX.Title = "alpha";
            SecondEstimateChart.View.AxisY.Title = "interval length";
            SecondEstimateChart.Data.Children.Add(new XYDataSeries
            {
                ItemsSource = _secondEstimateFuncPoints,
                XValueBinding = new Binding("X"),
                ValueBinding = new Binding("Y"),
                ChartType = ChartType.Line
            });
        }

        #endregion
    }

    public static class Task2
    {
        public static DoubleInterval DeviationConfidenceInterval(int seriesLength, double deviation,
            double significanceLevel)
        {
            var k = seriesLength - 1;
            var alpha = 1 - significanceLevel;
            var a = alpha/2;
            var chi1 = alglib.invchisquaredistribution(k, a);
            var chi2 = alglib.invchisquaredistribution(k, significanceLevel + a);

            return new DoubleInterval(k*deviation/(chi2 > chi1 ? chi2 : chi1), k*deviation/(chi1 > chi2 ? chi2 : chi1));
        }

        private static double GetKByVariationalSeriesLength(int seriesLength, double significanceLevel)
        {
            if (seriesLength < 10)
            {
                return alglib.invstudenttdistribution(seriesLength - 1, significanceLevel);
            }
            return alglib.invnormaldistribution(significanceLevel);
        }

        public static double RootMeanSquareDeflection(List<double> variationalSeries)
        {
            return Math.Sqrt(DeviationEstimate(variationalSeries));
        }

        public static double ExpectationEstimate(List<double> variationalSeries)
        {
            return variationalSeries.Average();
        }

        public static double DeviationEstimate(List<double> variationalSeries)
        {
            var averageValue = ExpectationEstimate(variationalSeries);
            var deviation = 1d/(variationalSeries.Count - 1)*
                            variationalSeries.Select(x => Math.Pow(x - averageValue, 2)).Sum();

            //var deviation = variationalSeries.GroupBy(value => value, value => variationalSeries.Where(x => x == value),
            //    (value, values) => new {value, count = values.Count()})
            //    .Select(t => t.count*(t.value - averageValue))
            //    .Sum()/
            //                variationalSeries.Count;

            return deviation;
        }
    }
}