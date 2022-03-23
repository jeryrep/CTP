﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CTP.Api.Interfaces;
using CTP.Api.Services;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace CTP.FrontEnd.Views
{
    /// <summary>
    /// Logika interakcji dla klasy ChartView.xaml
    /// </summary>
    public partial class ChartView : INotifyPropertyChanged
    {
        private IAnalogService _service;
        private readonly IAcCardService _cardService;
        public SeriesCollection AnalogSeries { get; set; }
        private volatile bool _stop;
        private double _lastValue;
        public ChartView() {
            InitializeComponent();
            _cardService = new AcCardService();
            Channel.ItemsSource = _cardService.GetChannels();
            AnalogSeries = new SeriesCollection {
                new LineSeries {
                    Values = new ChartValues<ObservableValue>()
                }
            };
        }

        public double LastValue
        {
            get => _lastValue;
            set
            {
                _lastValue = value;
                OnPropertyChanged();
            }
        }
        private void SetValue()
        {
            var target = ((ChartValues<ObservableValue>)AnalogSeries.First().Values).Last().Value;
            var step = (target - _lastValue) / 4;
            Task.Run(() => {
                for (var i = 0; i < 4; i++)
                {
                    Thread.Sleep(10);
                    LastValue += step;
                }
                LastValue = target;
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            _stop = false;
            _service = new AnalogService(Channel.SelectedItem.ToString()!, InputConfig.SelectedItem.ToString().Equals("Synchroniczny") ? 10106 : 10083, Convert.ToInt32(MinValue.SelectedItem), Convert.ToInt32(MaxValue.SelectedItem));
            try {
                AnalogSeries.First().Values.RemoveAt(0);
            }
            catch (Exception) {
                // ignored
            }

            Task.Run(() => {
                while (!_stop)
                {
                    Thread.Sleep(50);
                    Application.Current.Dispatcher.Invoke(() => {
                        AnalogSeries.First().Values.Add(new ObservableValue(_service.GetAnalogReading()));
                        SetValue();
                    });
                }
            });
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _stop = true;
            AnalogSeries.First().Values = new ChartValues<ObservableValue>();
        }

        private void Channel_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            SwitchChannelsConfiguration();
        }

        private void SwitchChannelsConfiguration() {
            if (Channel.SelectedIndex < 4)
            {
                MinValue.ItemsSource = Enumerable.Range(-20, 41);
                MaxValue.ItemsSource = Enumerable.Range(-20, 41).Reverse();
                InputConfig.ItemsSource = new List<string> {
                    "Synchroniczny", "Niesynchroniczny"
                };
            } else {
                MinValue.ItemsSource = Enumerable.Range(-10, 21);
                MaxValue.ItemsSource = Enumerable.Range(-10, 21).Reverse();
                InputConfig.ItemsSource = new List<string> {
                    "Niesynchroniczny"
                };
            }
            ChartYAxis.MinValue = Convert.ToDouble(MinValue.SelectedItem);
            ChartYAxis.MaxValue = Convert.ToDouble(MaxValue.SelectedItem);
        }
    }
}
