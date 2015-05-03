﻿using System;
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

namespace RoutineCalculation_1
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PiecewiseFunction<double> _function = new PiecewiseFunction<double>
        {
            Functions = new Dictionary<Interval<double>, Func<double, double>>
            {
                {new DoubleInterval(double.NegativeInfinity, double.PositiveInfinity, true, true), x => 1d/(x + 3)}
            }
        };

        private DoubleFrequencyFunction _rndFrequencyFunction;
        private DoubleFrequencyFunction _teoreticalFrequencyFunction;
        private int _n;
        private static double _a = 0;
        private static double _b = 10;
        private double _k = _b - _a + _a;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(nTextBox.Text, out _n))
            {
                MessageBox.Show("Введите число в поле n.");
                return;
            }

            if (_n < 1 || _n > 10000)
            {
                MessageBox.Show("n должно быть больше 0, но меньше 10000");
                return;
            }

            BuildRndFrequencyFunction();
            BuildTeoreticalFrequencyFunction();
            DrawVariationalSeries();
            DrawStatFunction();
            DrawTeorFuncPoints();
            DrawTeorStatFunction();
            DrawCompareGraph();
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

        private void DrawVariationalSeries()
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

        private void DrawStatFunction()
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

        private void DrawTeorFuncPoints()
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

        private void DrawTeorStatFunction()
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

        private void DrawCompareGraph()
        {
            var rndPoints = new ObservableCollection<Point>(_rndFrequencyFunction.Function.Select(p => new Point(p.X, p.Y)));
            var teorPoints = new ObservableCollection<Point>(_teoreticalFrequencyFunction.Function.Select(p => new Point(p.X, p.Y)));

            CompareChart.Data.Children.Clear();
            CompareChart.Data.Children.Add(new XYDataSeries
            {
                ItemsSource = rndPoints,
                XValueBinding = new Binding("X"),
                ValueBinding = new Binding("Y"),
                ChartType = ChartType.Step
            });
            CompareChart.Data.Children.Add(new XYDataSeries
            {
                ItemsSource = teorPoints,
                XValueBinding = new Binding("X"),
                ValueBinding = new Binding("Y"),
                ChartType = ChartType.Step
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