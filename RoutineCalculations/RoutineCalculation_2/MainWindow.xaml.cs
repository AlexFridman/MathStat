using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BaseTypes;
using BaseTypes.FrequencyFunction;
using BaseTypes.Interval;
using C1.WPF.C1Chart;

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

        private readonly PiecewiseFunction<double> _teoreticalFunc = new PiecewiseFunction<double>
        {
            Functions = new Dictionary<Interval<double>, Func<double, double>>
            {
                {new DoubleInterval(double.NegativeInfinity, 1d/13), x => 0},
                {new DoubleInterval(1d/13, 1d/3, true, true), x => -1d/10/x + 1.3},
                {new DoubleInterval(1d/3, double.PositiveInfinity), x => 1}
            }
        };

        private DoubleFrequencyFunction _frequencyFunctionPoints;
        private IEnumerable<Point> _teoreticalFunctionPoints;
        private DoubleHistogram _histogram;
        protected double[] _rndVariationalSeries;

        private int _n;
        private double _a;
        private double _b;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!TryParseParameters()) return;

            BuildRndFrequencyFunction();
            BuildTeoreticalFunction();
            BuildRndVariationalSeries();
            InitializeRndHistogram();
            DisplayEqualIntervalsHistogram();
            DisplayPolygon();
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
                x => _function.Calculate(rnd.NextDouble()*(_b - _a) + _a));
            var roundedValues = rndValues.Select(v => (double) Math.Round((decimal) v, 2)).ToList();

            _rndVariationalSeries = roundedValues.ToArray();
        }

        private void BuildRndFrequencyFunction()
        {
            var rnd = new Random();

            var rndValues = new SequenceGenerator<double>(0, _n, v => v += 1,
                x => _function.Calculate(rnd.NextDouble()*(_b - _a) + _a));
            var roundedValues = rndValues.Select(v => (double) Math.Round((decimal) v, 2)).ToList();

            _frequencyFunctionPoints = new DoubleFrequencyFunction(roundedValues);
        }


        private void BuildTeoreticalFunction()
        {
            var xValues = new SequenceGenerator<double>(_a, _n, x => x += Math.Abs(_b - _a)/_n, x => x).ToList();
            var yValues = xValues.Select(x => _teoreticalFunc.Calculate(x));

            var points =
                xValues.Zip(yValues,
                    (x, y) => new Point((double) Math.Round((decimal) x, 5), (double) Math.Round((decimal) y, 5)))
                    .ToList();

            _teoreticalFunctionPoints = points;
        }

        #endregion

        private void InitializeRndHistogram()
        {
            _histogram = new DoubleHistogram(_rndVariationalSeries);
        }

        private void DisplayEqualIntervalsHistogram()
        {
            try
            {
                ClearChart(RndHistogram);

                var bars = _histogram.EqualIntervalHistogram().ToList();
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
            var bars = _histogram.EqualIntervalHistogram().ToList();

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

        private void SetAxis(C1Chart chart, IEnumerable<DoubleHistogram.Bar> bars)
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

        private void DisplayHistogramChart(C1Chart chart, IEnumerable<DoubleHistogram.Bar> bars)
        {
            foreach (var bar in bars)
            {
                DisplayBar(chart, bar);
            }
        }

        private void DisplayBar(C1Chart chart, DoubleHistogram.Bar bar)
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
    }
}
