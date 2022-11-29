using System;
using System.Threading.Tasks;
using Qkmaxware.Astro.Control.Devices;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Simulated autoguider
/// </summary>
public class SimulatedGuider : IGuider {
    public GuideState State {get; private set;}

    public string Name => ".Net Simulated Autoguider";

    public bool IsConnected => true;

    public void Connect(){}

    public void Disconnect(){}

    public void StartGuiding() {
        State = GuideState.Calibrating;
        Task.Run(() => {
            Task.Delay(TimeSpan.FromSeconds(4)).Wait();
            State = GuideState.Guiding;
        });
    }

    public void StopGuiding(){
        State = GuideState.Idle;
    }
}

}