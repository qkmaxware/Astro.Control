using System.Collections.Generic;
using System.Linq;
using Qkmaxware.Astro.Control.Devices;

namespace Qkmaxware.Astro.Control {

public partial class IndiConnection : IDeviceSource {
    /// <summary>
    /// List all camera that are available for control via this source
    /// </summary>
    /// <returns>enumerable of cameras</returns>
    public IEnumerable<ICamera> EnumerateCameras() => this.Devices.AllCCDs().Select(device => new IndiCameraController(device));
    /// <summary>
    /// List all observation domes that are available for control via this source
    /// </summary>
    /// <returns>enumerable of domes</returns>
    public IEnumerable<IDome> EnumerateDomes() => this.Devices.AllDomes().Select(device => new IndiDomeController(device));
    /// <summary>
    /// List all filter wheels that are available for control via this source
    /// </summary>
    /// <returns>enumerable of filter wheels</returns>
    public IEnumerable<IFilterWheel> EnumerateFilterWheels() => this.Devices.AllFilterWheels().Select(device => new IndiFilterWheelController(device));
    /// <summary>
    /// List all focusers that are available for control via this source
    /// </summary>
    /// <returns>enumerable of focusers</returns>
    public IEnumerable<IFocuser> EnumerateFocusers() => this.Devices.AllFocusers().Select(device => new IndiFocuserController(device));
    /// <summary>
    /// List all telescopes that are available for control via this source
    /// </summary>
    /// <returns>enumerable of telescopes</returns>
    public IEnumerable<ITelescope> EnumerateTelescopes() => this.Devices.AllTelescopes().Select((device) => new IndiTelescopeController(device));

}

}