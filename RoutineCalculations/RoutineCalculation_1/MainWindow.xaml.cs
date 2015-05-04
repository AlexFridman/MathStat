using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BaseTypes;
using C1.WPF.C1Chart;
using Ciloci.Flee;

namespace RoutineCalculation_1
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PiecewiseFunction<double> _function;

        private DoubleFrequencyFunction _rndFrequencyFunction;
        private DoubleFrequencyFunction _teoreticalFrequencyFunction;
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
            if (!TryBuildFunction()) return;

            CalculateK();
            DisplayK();
            BuildRndFrequencyFunction();
            BuildTeoreticalFrequencyFunction();
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

            if (!double.TryParse(aTextBox.Text, out _a))
            {
                MessageBox.Show("Введите число в поле a.");
                return false;
            }

            if (!double.TryParse(bTextBox.Text, out _b))
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

        private bool TryBuildFunction()
        {
            var func = ParseFunction();

            if (func == null)
            {
                MessageBox.Show("Не удалось распознать функцию.");
                return false;
            }

            _function = new PiecewiseFunction<double>
            {
                Functions = new Dictionary<Interval<double>, Func<double, double>>
                {
                    {new DoubleInterval(double.NegativeInfinity, double.PositiveInfinity, true, true), func}
                }
            };

            return true;
        }

        private Func<double, double> ParseFunction()
        {
            try
            {
                return CreateExpressionForX(funcTextBox.Text);
            }
            catch (Exception)
            {                          
                return null;
            }
        }

        private Func<double, double> CreateExpressionForX(string expression)
        {
            ExpressionContext context = new ExpressionContext();
            context.Variables["x"] = 0.0d;

            IGenericExpression<double> e = context.CompileGeneric<double>(expression);

            Func<double, double> expressionEvaluator = (double x) =>
            {
                context.Variables["x"] = x;
                var result = e.Evaluate();
                return result;
            };

            return expressionEvaluator;
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

            var rndValues = new SequenceGenerator<double>(_a, _n, v => v += 1, x => _function.Calculate(rnd.NextDouble() * _k));
            var roundedValues = rndValues.Select(v => (double) Math.Round((decimal) v, 2)).ToList();

            _rndFrequencyFunction = new DoubleFrequencyFunction(roundedValues);
        }

        private void BuildTeoreticalFrequencyFunction()
        {
            var hypodispersionValues = new SequenceGenerator<double>(_a, _n, v => v += 1, x => _function.Calculate(x));
            var roundedValues = hypodispersionValues.Select(v => (double)Math.Round((decimal)v, 2)).ToList();

            _teoreticalFrequencyFunction = new DoubleFrequencyFunction(roundedValues);
        }

        private void DisplayVariationalSeries()
        {
            int columnCount = VariationalSeriesGrid.ColumnDefinitions.Count;
            VariationalSeriesGrid.ColumnDefinitions.Clear();
            VariationalSeriesGrid.Children.Clear();

            VariationalSeriesGrid.ColumnDefinitions.Add(new ColumnDefinition());
            VariationalSeriesGrid.Children.Add(CreateLabel("Значение", 0, 0));
            VariationalSeriesGrid.Children.Add(CreateLabel("Кол-во", 1, 0));

            int column = 1;
            foreach (Item<double> item in _rndFrequencyFunction.Frequencies)
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
            var points = new ObservableCollection<Point>(_rndFrequencyFunction.Function.Select(p => new Point(p.X, p.Y)));

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
            int columnCount = AnalyticsFunctionGrid.ColumnDefinitions.Count;
            AnalyticsFunctionGrid.ColumnDefinitions.Clear();
            AnalyticsFunctionGrid.Children.Clear();

            AnalyticsFunctionGrid.ColumnDefinitions.Add(new ColumnDefinition());
            AnalyticsFunctionGrid.Children.Add(CreateLabel("X", 0, 0));
            AnalyticsFunctionGrid.Children.Add(CreateLabel("Y", 1, 0));

            int column = 1;
            foreach (Point<double> point in _teoreticalFrequencyFunction.Function)
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
            var points = new ObservableCollection<Point>(_teoreticalFrequencyFunction.Function.Select(p => new Point(p.X, p.Y)));

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
            var rndPoints = new ObservableCollection<Point>(_rndFrequencyFunction.Function.Select(p => new Point(p.X, p.Y)));
            var teorPoints = new ObservableCollection<Point>(_teoreticalFrequencyFunction.Function.Select(p => new Point(p.X, p.Y)));

            CompareChart.Data.Children.Clear();

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