using System.Collections.Generic;
using Qkmaxware.Astro.Control.Devices;

namespace Qkmaxware.Astro.Control {

/// <summary>
/// Interface for sources that control astronomical equipment
/// </summary>
public interface IDeviceSource {
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
}

}