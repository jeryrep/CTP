using System;
using CTP.Api.Interfaces;
using NationalInstruments.DAQmx;

namespace CTP.Api.Services;

public class AnalogService : IAnalogService {
    private readonly Task _analogInput;
    private readonly Task _analogOutput;
    private readonly AIChannel _aiChannel;
    private readonly AOChannel _aoChannel;

    public AnalogService(string channel, int terminalConfiguration, int minValue, int maxValue) {
        _analogInput = new Task();
        _analogOutput = new Task();
        _aiChannel = _analogInput.AIChannels.CreateVoltageChannel(channel, "aiChannel",
            (AITerminalConfiguration)terminalConfiguration, minValue, maxValue, AIVoltageUnits.Volts);
        _aoChannel =
            _analogOutput.AOChannels.CreateVoltageChannel("Dev1/ao0", "aoChannel", 0, 5, AOVoltageUnits.Volts);
    }

    public double GetAnalogReading() {
        var reader = new AnalogSingleChannelReader(_analogInput.Stream);
        var analogDataIn = reader.ReadSingleSample();
        return analogDataIn;
    }

    public void GetAnalogWrite(string value) {
        var writer = new AnalogSingleChannelWriter(_analogOutput.Stream);
        var analogDataOut = Convert.ToDouble(value);
        writer.WriteSingleSample(true, analogDataOut);
    }
}