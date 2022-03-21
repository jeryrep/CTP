using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CTP.Api;
using CTPWPF.Front.Annotations;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace CTPWPF.Front
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window, INotifyPropertyChanged {
        private readonly Service _service;
        private SeriesCollection AnalogSeries { get; set; }
        private double _lastValue;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _service = new Service();
            AnalogSeries = new SeriesCollection {
                new LineSeries {
                    Values = new ChartValues<ObservableValue>()
                },
            };

            Task.Run(() => {
                while (true) {
                    Thread.Sleep(50);
                    Application.Current.Dispatcher.Invoke(() => {
                        AnalogSeries.First().Values.Add(new ObservableValue(Convert.ToDouble(_service.GetAnalogRead())));
                        SetValue();
                    });
                }
            });
            DataContext = this;
        }

        private double LastValue {
            get => _lastValue;
            set {
                _lastValue = value;
                OnPropertyChanged();
            }
        }
        private void SetValue() {
            var target = ((ChartValues<ObservableValue>)AnalogSeries.First().Values).Last().Value;
            var step = (target - _lastValue) / 4;
            Task.Run(() => {
                for (var i = 0; i < 4; i++) {
                    Thread.Sleep(10);
                    LastValue += step;
                }
                LastValue = target;
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e) 
        {
            AnalogRead.Text = _service.GetAnalogRead();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _service.GetAnalogWrite(AnalogWrite.Text);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}