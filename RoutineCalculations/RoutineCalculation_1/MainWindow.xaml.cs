using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using BaseTypes;

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
        private DoubleFrequencyFunction _teoreticakFrequencyFunction;
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
        }

        private void BuildRndFrequencyFunction()
        {
            var rnd = new Random();

            var rndValues = new SequenceGenerator<double>(_a, (int) (_b - _a), v => v += 1, x => _function.Calculate(rnd.NextDouble()*_k));

            _rndFrequencyFunction = new DoubleFrequencyFunction(rndValues);
        }

        private void BuildTeoreticalFrequencyFunction()
        {
            var hypodispersionValues = new SequenceGenerator<double>(_a, (int)(_b - _a), v => v += 1, x => _function.Calculate(x));

            _teoreticakFrequencyFunction = new DoubleFrequencyFunction(hypodispersionValues);
        }
    }
}