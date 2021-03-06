﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BaseTypes;
using BaseTypes.FrequencyFunction;
using BaseTypes.Interval;
using C1.WPF.C1Chart;
using Bar = BaseTypes.Bar;

namespace RoutineCalculation_2
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PiecewiseFunction<double> _function = new PiecewiseFunction<double>
        {
            Functions = new Dictionary<Interval<double>, Func<double, double>>
            {
                {new DoubleInterval(double.NegativeInfinity, double.PositiveInfinity), x => 1d/(x + 3)}
            }
        };

        private readonly PiecewiseFunction<double> _teoreticalDensityFunc = new PiecewiseFunction<double>
        {
            Functions = new Dictionary<Interval<double>, Func<double, double>>
            {
                {new DoubleInterval(double.NegativeInfinity, 1d/13), x => 0},
                {new DoubleInterval(1d/13, 1d/3, true, true), x => 1d/(10*x*x)},
                {new DoubleInterval(1d/3, double.PositiveInfinity), x => 0}
            }
        };

        private IEnumerable<Point> _teoreticalFunctionPoints;
        private double[] _rndVariationalSeries;

        private int _n;
        private double _a;
        private double _b;
        private List<Point> _teoreticalDensityFunctionPoints;
        private Bar[] _equalIntervalHistogramBars;
        private DoubleHistogram _histogram;
        private const int TeorFncPointsCount = 10000;
        private const double TeorFuncInterval = 0.0001;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!TryParseParameters()) return;
            bool isWorkDoneSucceed = false;

            while (!isWorkDoneSucceed)
            {
                try
                {
                    isWorkDoneSucceed = DoWork();
                }
                catch { }
            }
        }

        private bool DoWork()
        {
            BuildRndFrequencyFunction();
            BuildTeoreticalFunction();
            BuildRndVariationalSeries();
            BuildRndHistogram();
            DisplayEqualIntervalsHistogram();
            DisplayPolygon();
            DisplayHistogramTable();
            DisplayPolygonPointsTable();
            //BuildGroupStatFunc();
            BuildTeoreticalDensityFunction();
            DisplayTeorDensFunction();
            DisplayCompareChart();
            DisplayTeorDensFuncPoints();
            DisplayGroupStatFunc();
            return true;
        }

        #region reoutine_calc_1

        private bool TryParseParameters()
        {
            if (!int.TryParse(nTextBox.Text, out _n))
            {
                MessageBox.Show("Введите число в поле n.");
                return false;
            }

            if (_n < 1 || _n > 10000)
            {
                MessageBox.Show("n должно быть больше 0, но меньше 10000");
                return false;
            }

            if (!double.TryParse(aTextBox.Content.ToString(), out _a))
            {
                MessageBox.Show("Введите число в поле a.");
                return false;
            }

            if (!double.TryParse(bTextBox.Content.ToString(), out _b))
            {
                MessageBox.Show("Введите число в поле a.");
                return false;
            }

            if (_a >= _b)
            {
                MessageBox.Show("Параметр а должен быть строго меньше b.");
                return false;
            }
            return true;
        }

        private void BuildRndVariationalSeries()
        {
            var rnd = new Random();

            var rndValues = new SequenceGenerator<double>(0, _n, v => v += 1,
                x => _function.Calculate(rnd.NextDouble() * (_b - _a) + _a));
            var roundedValues = rndValues.Select(v => (double)Math.Round((decimal)v, 6)).ToList();

            _rndVariationalSeries = roundedValues.ToArray();
        }

        private void BuildRndFrequencyFunction()
        {
            var rnd = new Random();

            var rndValues = new SequenceGenerator<double>(0, _n, v => v += 1,
                x => _function.Calculate(rnd.NextDouble() * (_b - _a) + _a));
            var roundedValues = rndValues.Select(v => (double)Math.Round((decimal)v, 6)).ToList();
        }


        private void BuildTeoreticalFunction()
        {
            var xValues = new SequenceGenerator<double>(_a, TeorFncPointsCount, x => x +=TeorFuncInterval, x => x).ToList();
            var yValues = xValues.Select(x => _teoreticalDensityFunc.Calculate(x));

            var points =
                xValues.Zip(yValues,
                    (x, y) => new Point((double)Math.Round((decimal)x, 5), (double)Math.Round((decimal)y, 5)))
                    .ToList();

            _teoreticalFunctionPoints = points;
        }

        #endregion

        private void BuildRndHistogram()
        {
            _histogram = new DoubleHistogram(_rndVariationalSeries);

            _equalIntervalHistogramBars = _histogram.EqualIntervalHistogram().ToArray();
        }

        private void DisplayEqualIntervalsHistogram()
        {
            try
            {
                ClearChart(RndHistogram);

                var bars = _equalIntervalHistogramBars;
                SetAxis(RndHistogram, bars);
                DisplayHistogramChart(RndHistogram, bars);
            }
            catch
            {
                MessageBox.Show("Выбрано неверное кол-во интервалов.");
            }
        }

        private void DisplayPolygon()
        {
            try
            {
                var bars = _equalIntervalHistogramBars;

                var polygonPts = _histogram.BuildPolygon(bars).Select(p => new Point(p.X, p.Y));

                RndHistogram.Data.Children.Add(new XYDataSeries
                {
                    ItemsSource = new ObservableCollection<Point>(polygonPts),
                    XValueBinding = new Binding("X"),
                    ValueBinding = new Binding("Y"),
                    ChartType = ChartType.Line,
                    Name = "Polygon"
                });
            }
            catch
            {
                //MessageBox.Show("Выбрано неверное кол-во интервалов.");
            }
        }

        private void SetAxis(C1Chart chart, IEnumerable<Bar> bars)
        {
            var minX = bars.Min(b => b.Interval.Left);
            var maxX = bars.Max(b => b.Interval.Right);
            var minY = 0;
            var maxY = bars.Max(b => b.F);

            chart.View.AxisX.Min = minX - 0.1;
            chart.View.AxisX.Max = maxX + 0.1;
            chart.View.AxisY.Min = 0;
            chart.View.AxisY.Max = maxY + 1;
        }

        private void ClearChart(C1Chart chart)
        {
            chart.Data.Children.Clear();
        }

        private void DisplayHistogramChart(C1Chart chart, IEnumerable<Bar> bars)
        {
            foreach (var bar in bars)
            {
                DisplayBar(chart, bar);
            }
        }

        private void DisplayHistogramTable()
        {
            try
            {
                HistogramGrid.Children.Clear();
                HistogramGrid.RowDefinitions.Clear();

                var indexHeadeLabel = CreateLabel("i", 0);
                var aiHeaderLabel = CreateLabel("A(i)", 0, 1);
                var biHeaderLabel = CreateLabel("B(i)", 0, 2);
                var viHeaderLabel = CreateLabel("v(i)", 0, 3);
                var hiHeaderLabel = CreateLabel("h(i)", 0, 4);
                var fiHeaderLabel = CreateLabel("f*(i)", 0, 5);

                HistogramGrid.RowDefinitions.Add(new RowDefinition());
                HistogramGrid.Children.Add(indexHeadeLabel);
                HistogramGrid.Children.Add(aiHeaderLabel);
                HistogramGrid.Children.Add(biHeaderLabel);
                HistogramGrid.Children.Add(viHeaderLabel);
                HistogramGrid.Children.Add(hiHeaderLabel);
                HistogramGrid.Children.Add(fiHeaderLabel);

                var bars = _equalIntervalHistogramBars;

                var index = 1;
                foreach (var bar in bars)
                {
                    var indexLabel = CreateLabel(index.ToString(), index);
                    var aiLabel = CreateLabel(bar.Interval.Left.ToString("F3"), index, 1);
                    var biLabel = CreateLabel(bar.Interval.Right.ToString("F3"), index, 2);
                    var viLabel = CreateLabel(bar.ValuesCount.ToString("F3"), index, 3);
                    var hiLabel = CreateLabel(bar.Interval.Length.ToString("F3"), index, 4);
                    var fiLabel = CreateLabel(bar.F.ToString("F3"), index, 5);

                    HistogramGrid.RowDefinitions.Add(new RowDefinition());

                    HistogramGrid.Children.Add(indexLabel);
                    HistogramGrid.Children.Add(aiLabel);
                    HistogramGrid.Children.Add(biLabel);
                    HistogramGrid.Children.Add(viLabel);
                    HistogramGrid.Children.Add(hiLabel);
                    HistogramGrid.Children.Add(fiLabel);

                    index++;
                }
            }
            catch
            {
                // MessageBox.Show("Выбрано неверное кол-во интервалов.");
            }
        }

        private void DisplayPolygonPointsTable()
        {
            try
            {
                PolygonGrid.ColumnDefinitions.Clear();
                PolygonGrid.Children.Clear();

                PolygonGrid.ColumnDefinitions.Add(new ColumnDefinition());
                PolygonGrid.Children.Add(CreateLabel("X"));
                PolygonGrid.Children.Add(CreateLabel("Y", 1));

                var bars = _equalIntervalHistogramBars;

                var polygonPts = _histogram.BuildPolygon(bars).Select(p => new Point(p.X, p.Y));

                var column = 1;
                foreach (var point in polygonPts)
                {
                    PolygonGrid.ColumnDefinitions.Add(new ColumnDefinition());

                    var xLabel = CreateLabel(point.X.ToString("F3"), 0, column);
                    var yLabel = CreateLabel(point.Y.ToString("F3"), 1, column);

                    PolygonGrid.Children.Add(xLabel);
                    PolygonGrid.Children.Add(yLabel);

                    column++;
                }
            }
            catch
            {
                //  MessageBox.Show("Выбрано неверное кол-во интервалов.");
            }
        }


        private void DisplayGroupStatFunc()
        {
            try
            {
                var bars = _equalIntervalHistogramBars;
                var groupDistFunc = new DoubleGroupDistributionFunction(bars, _n);

                var functionPoints = groupDistFunc.Function;

                ClearChart(GroupDistChsrt);

                GroupDistChsrt.Data.Children.Add(new XYDataSeries
                {
                    ItemsSource = functionPoints,
                    XValueBinding = new Binding("X"),
                    ValueBinding = new Binding("Y"),
                    ChartType = ChartType.Step
                });
            }
            catch
            {
                // ignored
            }
        }

        #region Density function

        private void BuildTeoreticalDensityFunction()
        {
            var xValues = new SequenceGenerator<double>(_a, TeorFncPointsCount, x => x += TeorFuncInterval, x => x).ToList();
            var yValues = xValues.Select(x => _teoreticalDensityFunc.Calculate(x));

            var points =
                xValues.Zip(yValues,
                    (x, y) => new Point((double)Math.Round((decimal)x, 5), (double)Math.Round((decimal)y, 5)))
                    .ToList();

            _teoreticalDensityFunctionPoints = points;
        }

        private void DisplayTeorDensFuncPoints()
        {
            AnalyticsDensityFunctionGrid.ColumnDefinitions.Clear();
            AnalyticsDensityFunctionGrid.Children.Clear();

            AnalyticsDensityFunctionGrid.ColumnDefinitions.Add(new ColumnDefinition());
            AnalyticsDensityFunctionGrid.Children.Add(CreateLabel("X"));
            AnalyticsDensityFunctionGrid.Children.Add(CreateLabel("Y", 1));

            var column = 1;
            foreach (var point in _teoreticalDensityFunctionPoints.Where(p => p.Y != 0))
            {
                AnalyticsDensityFunctionGrid.ColumnDefinitions.Add(new ColumnDefinition());

                var valueLabel = CreateLabel(point.X.ToString(CultureInfo.InvariantCulture), 0, column);
                var countLabel = CreateLabel(point.Y.ToString(CultureInfo.InvariantCulture), 1, column);

                AnalyticsDensityFunctionGrid.Children.Add(valueLabel);
                AnalyticsDensityFunctionGrid.Children.Add(countLabel);

                column++;
            }
        }

        private void DisplayTeorDensFunction()
        {
            var points = new ObservableCollection<Point>(_teoreticalDensityFunctionPoints);

            DensityFunctionChart.View.AxisX.Min = 0;
            DensityFunctionChart.View.AxisX.Max = 1;

            DensityFunctionChart.Data.Children.Clear();
            DensityFunctionChart.Data.Children.Add(new XYDataSeries
            {
                ItemsSource = points,
                XValueBinding = new Binding("X"),
                ValueBinding = new Binding("Y"),
                ChartType = ChartType.Line
            });
        }

        #endregion

        private void DisplayCompareChart()
        {
            var points = new ObservableCollection<Point>(_teoreticalDensityFunctionPoints);

            CompareChart.View.AxisX.Min = 0;
            CompareChart.View.AxisX.Max = 1;

            CompareChart.Data.Children.Clear();
            CompareChart.Data.Children.Add(new XYDataSeries
            {
                ItemsSource = points,
                XValueBinding = new Binding("X"),
                ValueBinding = new Binding("Y"),
                ChartType = ChartType.Line
            });
            try
            {
                //ClearChart(CompareChart);

                var bars = _equalIntervalHistogramBars;
                SetAxis(CompareChart, bars);
                DisplayHistogramChart(CompareChart, bars);
            }
            catch
            {
                //MessageBox.Show("Выбрано неверное кол-во интервалов.");
            }
        }

        private void DisplayBar(C1Chart chart, Bar bar)
        {
            var uiElement = CreateBar(bar.Interval.Left, bar.Interval.Right, bar.F);

            chart.Data.Children.Add(uiElement);
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

        private XYDataSeries CreateBar(double x0, double x1, double y)
        {
            var pts = new[]
            {
                new Point(x0, 0),
                new Point(x0, y),
                new Point(x1, y),
                new Point(x1, 0)
            };

            var points = new ObservableCollection<Point>(pts);
            return new XYDataSeries
            {
                ItemsSource = new ObservableCollection<Point>(points),
                XValueBinding = new Binding("X"),
                ValueBinding = new Binding("Y"),
                ChartType = ChartType.Line,
                Name = "Bar"
            };
        }

        private void HistogramCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            var bars = RndHistogram.Data.Children.Where(ch => ch.Name == "Bar");
            foreach (var bar in bars)
            {
                bar.Visibility = Visibility.Visible;
            }
        }

        private void HistogramCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var bars = RndHistogram.Data.Children.Where(ch => ch.Name == "Bar");
            foreach (var bar in bars)
            {
                bar.Visibility = Visibility.Hidden;
            }
        }

        private void PolygoneCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            var polygon = RndHistogram.Data.Children.FirstOrDefault(ch => ch.Name == "Polygon");

            if (polygon != null)
            {
                polygon.Visibility = Visibility.Visible;
            }
        }

        private void PolygoneCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var polygon = RndHistogram.Data.Children.FirstOrDefault(ch => ch.Name == "Polygon");

            if (polygon != null)
            {
                polygon.Visibility = Visibility.Hidden;
            }
        }

        private void ChangeTaskButton_OnClick(object sender, RoutedEventArgs e)
        {
            var w2 = new MainWindow2();
            w2.Top = Top;
            w2.Left = Left;
            w2.Show();
            Close();
        }
    }
}
