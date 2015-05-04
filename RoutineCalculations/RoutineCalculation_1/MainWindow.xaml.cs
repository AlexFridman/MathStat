using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BaseTypes;
using C1.WPF.C1Chart;

namespace RoutineCalculation_1
{
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

        private int _n;
        private double _a;
        private double _b;
        private double _k;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!TryParseParameters()) return;

            CalculateK();
            DisplayK();
            BuildRndFrequencyFunction();
            BuildTeoreticalFunction();

            DisplayVariationalSeries();
            DisplayStatFunction();
            DisplayTeorFuncPoints();
            DisplayTeorStatFunction();
            DisplayCompareGraph();
        }

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


        private void CalculateK()
        {
            _k = (_b - _a) + _a;
        }

        private void DisplayK()
        {
            kLabel.Content = _k.ToString(CultureInfo.InvariantCulture);
        }

        private void BuildRndFrequencyFunction()
        {
            var rnd = new Random();

            var rndValues = new SequenceGenerator<double>(0, _n, v => v += 1,
                x => _function.Calculate(rnd.NextDouble()*_k));
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

        private void DisplayVariationalSeries()
        {
            VariationalSeriesGrid.ColumnDefinitions.Clear();
            VariationalSeriesGrid.Children.Clear();

            VariationalSeriesGrid.ColumnDefinitions.Add(new ColumnDefinition());
            VariationalSeriesGrid.Children.Add(CreateLabel("Значение"));
            VariationalSeriesGrid.Children.Add(CreateLabel("Кол-во", 1));

            var column = 1;
            foreach (var item in _frequencyFunctionPoints.Frequencies)
            {
                VariationalSeriesGrid.ColumnDefinitions.Add(new ColumnDefinition());

                var valueLabel = CreateLabel(item.Value.ToString(CultureInfo.InvariantCulture), 0, column);
                var countLabel = CreateLabel(item.Count.ToString(), 1, column);

                VariationalSeriesGrid.Children.Add(valueLabel);
                VariationalSeriesGrid.Children.Add(countLabel);

                column++;
            }
        }

        private void DisplayStatFunction()
        {
            var points =
                new ObservableCollection<Point>(_frequencyFunctionPoints.Function.Select(p => new Point(p.X, p.Y)));

            StatFuncChart.View.AxisX.Min = 0;
            StatFuncChart.View.AxisX.Max = 1;

            StatFuncChart.Data.Children.Clear();
            StatFuncChart.Data.Children.Add(new XYDataSeries
            {
                ItemsSource = points,
                XValueBinding = new Binding("X"),
                ValueBinding = new Binding("Y"),
                ChartType = ChartType.Step
            });
        }

        private void DisplayTeorFuncPoints()
        {
            AnalyticsFunctionGrid.ColumnDefinitions.Clear();
            AnalyticsFunctionGrid.Children.Clear();

            AnalyticsFunctionGrid.ColumnDefinitions.Add(new ColumnDefinition());
            AnalyticsFunctionGrid.Children.Add(CreateLabel("X"));
            AnalyticsFunctionGrid.Children.Add(CreateLabel("Y", 1));

            var column = 1;
            foreach (var point in _teoreticalFunctionPoints)
            {
                AnalyticsFunctionGrid.ColumnDefinitions.Add(new ColumnDefinition());

                var valueLabel = CreateLabel(point.X.ToString(CultureInfo.InvariantCulture), 0, column);
                var countLabel = CreateLabel(point.Y.ToString(CultureInfo.InvariantCulture), 1, column);

                AnalyticsFunctionGrid.Children.Add(valueLabel);
                AnalyticsFunctionGrid.Children.Add(countLabel);

                column++;
            }
        }

        private void DisplayTeorStatFunction()
        {
            var points = new ObservableCollection<Point>(_teoreticalFunctionPoints);

            TeoreticalStatFunctionChart.View.AxisX.Min = 0;
            TeoreticalStatFunctionChart.View.AxisX.Max = 1;

            TeoreticalStatFunctionChart.Data.Children.Clear();
            TeoreticalStatFunctionChart.Data.Children.Add(new XYDataSeries
            {
                ItemsSource = points,
                XValueBinding = new Binding("X"),
                ValueBinding = new Binding("Y"),
                ChartType = ChartType.Step
            });
        }

        private void DisplayCompareGraph()
        {
            var rndPoints =
                new ObservableCollection<Point>(_frequencyFunctionPoints.Function.Select(p => new Point(p.X, p.Y)));
            var teorPoints = new ObservableCollection<Point>(_teoreticalFunctionPoints);

            CompareChart.Data.Children.Clear();

            CompareChart.View.AxisX.Min = 0;
            CompareChart.View.AxisX.Max = 1;

            CompareChart.Data.Children.Add(new XYDataSeries
            {
                ItemsSource = rndPoints,
                XValueBinding = new Binding("X"),
                ValueBinding = new Binding("Y"),
                ChartType = ChartType.Step,
                Label = "стат. ф-я"
            });

            CompareChart.Data.Children.Add(new XYDataSeries
            {
                ItemsSource = teorPoints,
                XValueBinding = new Binding("X"),
                ValueBinding = new Binding("Y"),
                ChartType = ChartType.Step,
                Label = "теор. ф-я"
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
    }
}