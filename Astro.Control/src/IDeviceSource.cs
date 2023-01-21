using System.Collections.Generic;
using Qkmaxware.Astro.Control.Devices;

namespace Qkmaxware.Astro.Control {

/// <summary>
/// Interface for sources that control astronomical equipment
/// </summary>
public interface IDeviceSource {
    /// <summary>
    /// Enumerable of all devices available on the source
    /// </summary>
    /// <returns>enumerable of all devices</returns>
    IEnumerable<IDevice> EnumerateAllDevices();
    /// <summary>
    /// List all camera that are available for control via this source
    /// </summary>
    /// <returns>enumerable of cameras</returns>
    IEnumerable<ICamera> EnumerateCameras();
    /// <summary>
    /// List all observation domes that are available for control via this source
    /// </summary>
    /// <returns>enumerable of domes</returns>
    IEnumerable<IDome> EnumerateDomes();
    /// <summary>
    /// List all filter wheels that are available for control via this source
    /// </summary>
    /// <returns>enumerable of filter wheels</returns>
    IEnumerable<IFilterWheel> EnumerateFilterWheels();
    /// <summary>
    /// List all focusers that are available for control via this source
    /// </summary>
    /// <returns>enumerable of focusers</returns>
    IEnumerable<IFocuser> EnumerateFocusers();
    /// <summary>
    /// List all telescopes that are available for control via this source
    /// </summary>
    /// <returns>enumerable of telescopes</returns>
    IEnumerable<ITelescope> EnumerateTelescopes();

    /// <summary>
    /// List all auto-guiders that are available for control via this source
    /// </summary>
    /// <returns>enumerable of guiders</returns>
    IEnumerable<IGuider> EnumerateAutoGuiders();
}

}