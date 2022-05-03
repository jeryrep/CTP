using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
    private List<double> _realValues;
    public static volatile bool _stop;
    private double _time;

    public ChartView() {
        InitializeComponent();
        IAcCardService cardService = new AcCardService();
        Channel.ItemsSource = cardService.GetChannels();
        Stop.IsEnabled = false;
        AnalogSeries = new SeriesCollection {
            new LineSeries {
                Fill = Brushes.Transparent,
                Values = new ChartValues<ObservablePoint>()
            }
        };
    }

    private void StartClick(object sender, RoutedEventArgs e) {
        DefaultChartParameters();
        _time = 0;
        AnalogSeries.First().Values.Clear();
        if ((int)MinValue.Value >= (int)MaxValue.Value) {
            MessageBox.Show("Minimalna wartość musi być mniejsza od maksymalnej wartości", "Błąd");
            return;
        }

        _values = new List<double>();
        LoadButton.IsEnabled = false;
        MaxValue.IsEnabled = false;
        MinValue.IsEnabled = false;
        Start.IsEnabled = false;
        Stop.IsEnabled = true;
        Save.IsEnabled = false;
        _stop = false;
        _service = new AnalogService(Channel.SelectedItem.ToString()!,
            InputConfig.SelectedItem.ToString()!.Equals("Synchroniczny") ? 10106 : 10083,
            Convert.ToInt32(MinValue.Value), Convert.ToInt32(MaxValue.Value));
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
        LoadButton.IsEnabled = true;
        MaxValue.IsEnabled = true;
        MinValue.IsEnabled = true;
        Stop.IsEnabled = false;
        Start.IsEnabled = true;
        Save.IsEnabled = true;
        _realValues = new List<double>();
        GetRealValue();
        AnalogSeries.First().Values.Clear();
        for (var i = 0; i < _values.Count; i++)
            AnalogSeries.First().Values.Add(new ObservablePoint(SamplingMs * i, _values[i]));
    }

    private void Channel_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
        SwitchChannelsConfiguration();
    }

    private void SwitchChannelsConfiguration() {
        if (Channel.SelectedValue.ToString()![^1] - '0' < 4) {
            InputConfig.ItemsSource = new List<string> {
                "Synchroniczny", "Niesynchroniczny"
            };
            MinValue.Minimum = -20;
            MinValue.Maximum = 20;
            MaxValue.Minimum = -20;
            MaxValue.Maximum = 20;
        }
        else {
            InputConfig.ItemsSource = new List<string> {
                "Niesynchroniczny"
            };
            MinValue.Minimum = -10;
            MinValue.Maximum = 10;
            MaxValue.Minimum = -10;
            MaxValue.Maximum = 10;
        }
        InputConfig.SelectedIndex = 0;
        MinValue.Value = MinValue.Minimum;
        MaxValue.Value = MaxValue.Maximum;
        _time = 0;
        AdjustYAxis();
    }

    private void SwitchChannelVoltageRange() {
        if (InputConfig.SelectedValue == null) return;
        if (InputConfig.SelectedValue.ToString()!.Equals("Synchroniczny")) {
            MinValue.Minimum = -20;
            MinValue.Maximum = 20;
            MaxValue.Minimum = -20;
            MaxValue.Maximum = 20;
        } else {
            MinValue.Minimum = -10;
            MinValue.Maximum = 10;
            MaxValue.Minimum = -10;
            MaxValue.Maximum = 10;
        }
    }

    private void AdjustYAxis()
    {
        ChartYAxis.MinValue = Convert.ToDouble(MinValue.Value);
        ChartYAxis.MaxValue = Convert.ToDouble(MaxValue.Value);
    }
    private void btnSaveFile_Click(object sender, RoutedEventArgs e)
    {
        var saveFileDialog = new SaveFileDialog {
            Filter = "Skoroszyt programu Excel (*.xlsx)|*.xlsx|Skoroszyt programu Excel 97-2003 (*.xls)|*.xls"
        };
        if (saveFileDialog.ShowDialog() != true) return;
        var items = Enumerable.Range(0, _values.Count-1).Select(x => new
        {
            Time = (double)(x * SamplingMs) / 1000,
            Reading = _values[x],
            RealValues = _realValues[x]
        }).ToExcel(x => x.SheetName("DAQMx Reading Session"));
        File.WriteAllBytes(saveFileDialog.FileName, items);
    }

    private void MinValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => AdjustYAxis();

    private void MaxValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => AdjustYAxis();

    private void InputConfig_SelectionChanged(object sender, SelectionChangedEventArgs e) => SwitchChannelVoltageRange();

    //TODO: Zaimplementować wczytywanie danych na wykres z csv z teamsa
    private void LoadButton_Click(object sender, RoutedEventArgs e) {
        string path;
        /*var openFile = new OpenFileDialog();
        if (openFile.ShowDialog()) {
            path = File
        }*/
    }

    private void Calculate_Click(object sender, RoutedEventArgs e)
    {
        ChartYAxis.MinValue = Convert.ToDouble(MinRange.Text);
        ChartYAxis.MaxValue = Convert.ToDouble(MaxRange.Text);

        AnalogSeries.First().Values.Clear();
        for (var i = 0; i < _realValues.Count; i++)
        {
            AnalogSeries.First().Values.Add(new ObservablePoint(SamplingMs * i, _realValues[i]));
        }
        ChartYAxis.Title = "Długość [mm]";

    }

    private void DefaultChartParameters()
    {
        ChartYAxis.Title = "Amplitude [V]";
        AdjustYAxis();
    }

    private (int a, int b) GetAAndB(int maxRange, int minRange, int maxValue, int minValue)
    {
        var a = (maxRange - minRange) / (maxValue - minValue);
        var Y = a * maxValue + 0;
        var b = maxRange - Y;
        return (a, b);
    }
    private void GetRealValue()
    {
        int maxRange = int.Parse(MaxRange.Text);
        int minRange = int.Parse(MinRange.Text);
        int maxValue = (int)MaxValue.Value;
        int minValue = (int)MinValue.Value;
        var AAndB = GetAAndB(maxRange, minRange, maxValue, minValue);


        for (var i = 0; i < _values.Count; i++)
        {
            _realValues.Add(_values[i] * AAndB.a + AAndB.b);
        }

    }

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }
}