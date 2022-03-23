using CTP.Api.Interfaces;
using NationalInstruments.DAQmx;

namespace CTP.Api.Services; 

public class AcCardService : IAcCardService {
    public string[] GetChannels() => DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External);
}