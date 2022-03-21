using System;
using NationalInstruments.DAQmx;

namespace CTP.Api
{
    public class Service {
        private readonly Task _analogInput;
        private readonly Task _analogOutput;
        private readonly AIChannel _aiChannel;
        private readonly AOChannel _aoChannel;
        public Service() {
            _analogInput = new Task();
            _analogOutput = new Task();
            _aiChannel = _analogInput.AIChannels.CreateVoltageChannel("Dev1/ai0", "aiChannel", AITerminalConfiguration.Differential, 0, 10, AIVoltageUnits.Volts);
            _aoChannel = _analogOutput.AOChannels.CreateVoltageChannel("Dev1/ao0", "aoChannel", 0, 5, AOVoltageUnits.Volts);
        }
        public string GetAnalogRead() {
            var reader = new AnalogSingleChannelReader(_analogInput.Stream);
            var analogDataIn = reader.ReadSingleSample();
            return analogDataIn.ToString("0.00");
        }

        public void GetAnalogWrite(string value) {
            var writer = new AnalogSingleChannelWriter(_analogOutput.Stream);
            var analogDataOut = Convert.ToDouble(value);
            writer.WriteSingleSample(true, analogDataOut);
        }
    }
}
