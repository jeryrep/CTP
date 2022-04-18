using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ArrayToExcel;
using CTP.Api.Interfaces;
using CTP.Api.Services;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Microsoft.Win32;

namespace CTP.FrontEnd.Views; 

/// <summary>
/// Logika interakcji dla klasy ChartView.xaml
/// </summary>
public partial class ChartView : INotifyPropertyChanged {
    private const int SamplingMs = 10;
    private IAnalogService _service;
    public SeriesCollection AnalogSeries { get; set; }
    private List<double> _values;
    private volatile bool _stop;
    private double _time;

    public ChartView() {
        InitializeComponent();
        IAcCardService cardService = new AcCardService();
        Channel.ItemsSource = cardService.GetChannels();
        Stop.IsEnabled = false;
        AnalogSeries = new SeriesCollection {
            new LineSeries {
                Values = new ChartValues<ObservablePoint>()
            }
        };
    }

    private void StartClick(object sender, RoutedEventArgs e) {
        if ((int)MinValue.SelectedItem >= (int)MaxValue.SelectedItem) {
            MessageBox.Show("Minimalna wartość musi być mniejsza od maksymalnej wartości", "Błąd");
            return;
        }

        _values = new List<double>();

        Start.IsEnabled = false;
        Stop.IsEnabled = true;
        Save.IsEnabled = false;
        _stop = false;
        _service = new AnalogService(Channel.SelectedItem.ToString()!,
            InputConfig.SelectedItem.ToString()!.Equals("Synchroniczny") ? 10106 : 10083,
            Convert.ToInt32(MinValue.SelectedItem), Convert.ToInt32(MaxValue.SelectedItem));
        try {
            AnalogSeries.First().Values.RemoveAt(0);
        }
        catch (Exception) {
            // ignored
        }

        Task.Run(() => {
            while (!_stop) {
                Thread.Sleep(SamplingMs);
                _time += SamplingMs;
                Application.Current.Dispatcher.Invoke(() => {
                    var reading = _service.GetAnalogReading();
                    AnalogSeries.First().Values.Add(new ObservablePoint(_time, reading));
                    _values.Add(reading);
                    if (AnalogSeries.First().Values.Count >= 100)
                        AnalogSeries.First().Values.RemoveAt(0);
                });
            }
        });
        DataContext = this;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void StopClick(object sender, RoutedEventArgs e) {
        _stop = true;
        _time = 0;
        AnalogSeries.First().Values.Clear();
        Stop.IsEnabled = false;
        Start.IsEnabled = true;
        Save.IsEnabled = true;
        //var items = Enumerable.Range(0, _values.Count).Select(x => new {
        //    Time = (double) (x * SamplingMs) / 1000,
        //    Reading = _values[x]
        //}).ToExcel(x => x.SheetName("DAQMx Reading Session"));
        //File.WriteAllBytes("daqmx.xlsx", items);
    }

    private void Channel_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
        SwitchChannelsConfiguration();
    }

    private void SwitchChannelsConfiguration() {
        if (Channel.SelectedIndex < 4) {
            MinValue.ItemsSource = Enumerable.Range(-20, 41);
            MaxValue.ItemsSource = Enumerable.Range(-20, 41).Reverse();
            InputConfig.ItemsSource = new List<string> {
                "Synchroniczny", "Niesynchroniczny"
            };
        }
        else {
            MinValue.ItemsSource = Enumerable.Range(-10, 21);
            MaxValue.ItemsSource = Enumerable.Range(-10, 21).Reverse();
            InputConfig.ItemsSource = new List<string> {
                "Niesynchroniczny"
            };
        }

        MinValue.SelectedIndex = 0;
        MaxValue.SelectedIndex = 0;
        InputConfig.SelectedIndex = 0;
        _time = 0;
        AdjustYAxis();
    }

    private void AdjustYAxis()
    {
        ChartYAxis.MinValue = Convert.ToDouble(MinValue.SelectedItem);
        ChartYAxis.MaxValue = Convert.ToDouble(MaxValue.SelectedItem);
    }
    private void btnSaveFile_Click(object sender, RoutedEventArgs e)
    {

        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "Skoroszyt programu Excel (*.xlsx)|*.xlsx|Skoroszyt programu Excel 97-2003 (*.xls)|*.xls";
        if (saveFileDialog.ShowDialog() == true)
        {
            var items = Enumerable.Range(0, _values.Count).Select(x => new
            {
                Time = (double)(x * SamplingMs) / 1000,
                Reading = _values[x]
            }).ToExcel(x => x.SheetName("DAQMx Reading Session"));
            File.WriteAllBytes(saveFileDialog.FileName, items);
        }
    }

    private void MinValue_SelectionChanged(object sender, SelectionChangedEventArgs e) => AdjustYAxis();
    private void MaxValue_SelectionChanged(object sender, SelectionChangedEventArgs e) => AdjustYAxis();

    private void InputConfig_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }
}