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
        _realValues = new List<double>();
        LoadButton.IsEnabled = false;
        MaxValue.IsEnabled = false;
        MinValue.IsEnabled = false;
        Start.IsEnabled = false;
        Stop.IsEnabled = true;
        Save.IsEnabled = false;
        Calculate.IsEnabled = false;
        VoltageValue.IsChecked = true;
        _stop = false;
        _service = new AnalogService(Channel.SelectedItem.ToString()!,
            InputConfig.SelectedItem.ToString()!.Equals("Synchroniczny") ? 10106 : 10083,
            Convert.ToInt32(MinValue.Value), Convert.ToInt32(MaxValue.Value));
        var AAndB = GetAAndB();
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
                    var realReading = reading * AAndB.a + AAndB.b;
                    if (VoltageValue.IsChecked == true)
                    {
                        AnalogSeries.First().Values.Add(new ObservablePoint(_time, reading));
                    } else
                    {
                        AnalogSeries.First().Values.Add(new ObservablePoint(_time, realReading));
                    }
                    
                    _values.Add(reading);
                    if (AnalogSeries.First().Values.Count >= 100)
                        AnalogSeries.First().Values.RemoveAt(0);
                    _realValues.Add(realReading);
                });
            }
        });
        DataContext = this;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void StopClick(object sender, RoutedEventArgs e)
    {
        _stop = true;
        LoadButton.IsEnabled = true;
        MaxValue.IsEnabled = true;
        MinValue.IsEnabled = true;
        Stop.IsEnabled = false;
        Start.IsEnabled = true;
        Save.IsEnabled = true;
        Calculate.IsEnabled = true;
        DrawChart();

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
        var AAndB = GetAAndB();

        _realValues = new List<double>();
        for (var i = 0; i < _values.Count; i++)
        {
            _realValues.Add(_values[i] * AAndB.a + AAndB.b);
        }
        DrawChart();
    }

    private void DefaultChartParameters()
    {
        ChartYAxis.Title = "Amplituda [V]";
        AdjustYAxis();
    }

    private (double a, double b) GetAAndB()
    {
        var a = (int.Parse(MaxRange.Text) - int.Parse(MinRange.Text)) / ((double)MaxValue.Value - (double)MinValue.Value);
        var Y = a * MaxValue.Value + 0;
        var b = int.Parse(MaxRange.Text) - Y;
        return (a, b);
    }

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }

    private void ChangeAxis(string mess)
    {
        ChartYAxis.Title = mess;
        if(VoltageValue.IsChecked!=true)
        {
            ChartYAxis.MinValue = Convert.ToDouble(MinRange.Text);
            ChartYAxis.MaxValue = Convert.ToDouble(MaxRange.Text);
        } else
        {
            ChartYAxis.MinValue = Convert.ToDouble(MinValue.Value);
            ChartYAxis.MaxValue = Convert.ToDouble(MaxValue.Value);
        }    
    }

    private void DrawChart()
    {
        AnalogSeries.First().Values.Clear();
        if (VoltageValue.IsChecked == true)
        {
            ChangeAxis("Amplituda [V]");
            for (var i = 0; i < _values.Count; i++)
            {
                AnalogSeries.First().Values.Add(new ObservablePoint(SamplingMs * i, _values[i]));
            }
        }
        else
        {
            ChangeAxis("Długość [mm]");
            for (var i = 0; i < _realValues.Count; i++)
            {
                AnalogSeries.First().Values.Add(new ObservablePoint(SamplingMs * i, _realValues[i]));
            }
        }
    }

    private void PhysicalValue_Checked(object sender, RoutedEventArgs e)
    {
        if(_stop==true)
        {
            DrawChart();
        }
    }

    private void VoltageValue_Checked(object sender, RoutedEventArgs e)
    {
        if(_stop==true)
        {
            DrawChart();
        }
    }
}