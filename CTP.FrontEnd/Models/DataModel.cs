using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using CTP.Api.Interfaces;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;

namespace CTP.FrontEnd.Models;

public class DataModel : INotifyPropertyChanged
{
    private const int SamplingMs = 10;
    private IAnalogService _service;
    private List<double> _values;
    private List<double> _realValues;
    public static volatile bool _stop;
    private double _time;
    public DataModel(IAnalogService service)
    {
        _service = service;
        ChartValues = new ChartValues<ObservablePoint>();
        XMax = 10000;
        XMin = 0;

        // Initialize the sine graph
        for (double x = 0; x <= 40; x++)
        {
            ChartValues.Add(new ObservablePoint(x, _service.GetAnalogReading()));
        }

        // Setup the data mapper
        DataMapper = new CartesianMapper<ObservablePoint>()
          .X(point => point.X)
          .Y(point => point.Y)
          .Stroke(point => point.Y > 0.3 ? Brushes.Red : Brushes.LightGreen)
          .Fill(_ => Brushes.Transparent);

        // Setup the IProgress<T> instance in order to update the chart (UI thread)
        // from the background thread 
        var progressReporter = new Progress<double>(newValue => ShiftValuesToTheLeft(newValue, CancellationToken.None));

        // Generate the new data points on a background thread 
        // and use the IProgress<T> instance to update the chart on the UI thread
        Task.Run(async () => await StartSineGenerator(progressReporter, CancellationToken.None));
    }

    // Dynamically add new data
    private void ShiftValuesToTheLeft(double newValue, CancellationToken cancellationToken)
    {
        // Shift item data (and not the items) to the left
        for (var index = 0; index < ChartValues.Count - 1; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ObservablePoint currentPoint = ChartValues[index];
            ObservablePoint nextPoint = ChartValues[index + 1];
            currentPoint.X = nextPoint.X;
            currentPoint.Y = nextPoint.Y;
        }

        // Add the new reading
        ObservablePoint newPoint = ChartValues[^1];
        newPoint.X = newValue;
        newPoint.Y = _service.GetAnalogReading();

        // Update axis min/max
        XMax = newValue;
        XMin = ChartValues[0].X;
    }

    private async Task StartSineGenerator(IProgress<double> progressReporter, CancellationToken cancellationToken)
    {
        while (true)
        {
            // Add the new reading by posting the callback to the UI thread
            ObservablePoint newPoint = ChartValues[^1];
            var newXValue = newPoint.X + 1;
            progressReporter.Report(newXValue);

            // Check if CancellationToken.Cancel() was called 
            cancellationToken.ThrowIfCancellationRequested();

            // Plot at 1/10ms
            await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
        }
    }

    private double xMax;
    public double XMax
    {
        get => xMax;
        set
        {
            xMax = value;
            OnPropertyChanged();
        }
    }

    private double xMin;
    public double XMin
    {
        get => xMin;
        set
        {
            xMin = value;
            OnPropertyChanged();
        }
    }

    private object dataMapper;
    public object DataMapper
    {
        get => dataMapper;
        set
        {
            dataMapper = value;
            OnPropertyChanged();
        }
    }

    public ChartValues<ObservablePoint> ChartValues { get; set; }
    public Func<double, string> LabelFormatter => value => value.ToString("F");

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}