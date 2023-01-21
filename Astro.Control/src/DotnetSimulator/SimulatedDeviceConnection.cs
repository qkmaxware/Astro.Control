using System.Collections.Generic;
using System.Linq;
using Qkmaxware.Astro.Control;
using Qkmaxware.Astro.Control.Devices;

namespace Qkmaxware.Astro.Control {

/// <summary>
/// Simulated device source with a simulated connection to the source
/// </summary>
public class SimulatedDeviceConnection : IServerConnection, IDeviceSource {

    #region Fake Connection

    public bool IsConnected {get; set;}

    public IConnectionLogger InputLogger { get; set;  }

    public void Connect() {
        IsConnected = true;
    }

    public void Disconnect() {
        IsConnected = false;
    }

    #endregion

    #region Fake Device Source
    public IEnumerable<IDevice> EnumerateAllDevices() => 
                    this.EnumerateAutoGuiders().Cast<IDevice>()
                    .Concat(this.EnumerateCameras().Cast<IDevice>())
                    .Concat(this.EnumerateDomes().Cast<IDevice>())
                    .Concat(this.EnumerateFilterWheels().Cast<IDevice>())
                    .Concat(this.EnumerateFocusers().Cast<IDevice>())
                    .Concat(this.EnumerateTelescopes().Cast<IDevice>());
    private ICamera ccd = new SimulatedCcd();
    public IEnumerable<ICamera> EnumerateCameras() {
        yield return ccd;
    }

    private IDome dome = new SimulatedDome();
    public IEnumerable<IDome> EnumerateDomes() {
        yield return dome; 
    }

    private IFilterWheel wheel = new SimulatedFilterWheel();
    public IEnumerable<IFilterWheel> EnumerateFilterWheels() {
        yield return wheel;
    }

    private IFocuser focuser = new SimulatedFocuser();
    public IEnumerable<IFocuser> EnumerateFocusers() {
        yield return focuser;
    }

    private ITelescope telescope = new SimulatedTelescope();
    public IEnumerable<ITelescope> EnumerateTelescopes() {
        yield return telescope;
    }

    private IGuider guider = new SimulatedGuider();
    public IEnumerable<IGuider> EnumerateAutoGuiders() {
        yield return guider;
    }
    #endregion
}

}