using System;
using CTP.Api.Interfaces;
using NationalInstruments.DAQmx;

namespace CTP.Api.Services;

public class AnalogService : IAnalogService {
    private readonly Task _analogInput;
    private readonly AIChannel _aiChannel;

    public AnalogService(string channel, int terminalConfiguration, int minValue, int maxValue) {
        _analogInput = new Task();
        _aiChannel = _analogInput.AIChannels.CreateVoltageChannel(channel, "aiChannel",
            (AITerminalConfiguration)terminalConfiguration, minValue, maxValue, AIVoltageUnits.Volts);
    }

    public double GetAnalogReading() {
        var reader = new AnalogSingleChannelReader(_analogInput.Stream);
        var analogDataIn = reader.ReadSingleSample();
        return analogDataIn;
    }
}