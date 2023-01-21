using System.Linq;
using System.Collections.Generic;
using Qkmaxware.Astro.Control.Devices;

namespace Qkmaxware.Astro.Control {

public partial class Phd2Connection : IDeviceSource {
    public IEnumerable<IDevice> EnumerateAllDevices() => this.EnumerateAutoGuiders().Cast<IDevice>();
    /// <summary>
    /// List all camera that are available for control via this source
    /// </summary>
    /// <returns>enumerable of cameras</returns>
    public IEnumerable<ICamera> EnumerateCameras() => Enumerable.Empty<ICamera>();
    /// <summary>
    /// List all observation domes that are available for control via this source
    /// </summary>
    /// <returns>enumerable of domes</returns>
    public IEnumerable<IDome> EnumerateDomes() => Enumerable.Empty<IDome>();
    /// <summary>
    /// List all filter wheels that are available for control via this source
    /// </summary>
    /// <returns>enumerable of filter wheels</returns>
    public IEnumerable<IFilterWheel> EnumerateFilterWheels() => Enumerable.Empty<IFilterWheel>();
    /// <summary>
    /// List all focusers that are available for control via this source
    /// </summary>
    /// <returns>enumerable of focusers</returns>
    public IEnumerable<IFocuser> EnumerateFocusers() => Enumerable.Empty<IFocuser>();
    /// <summary>
    /// List all telescopes that are available for control via this source
    /// </summary>
    /// <returns>enumerable of telescopes</returns>
    public IEnumerable<ITelescope> EnumerateTelescopes() => Enumerable.Empty<ITelescope>();

    /// <summary>
    /// List all auto-guiders that are available for control via this source
    /// </summary>
    /// <returns>enumerable of guiders</returns>
    public IEnumerable<IGuider> EnumerateAutoGuiders() => this.EnumerateProfiles;
}

}
