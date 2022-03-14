using System.Threading;
using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Interface describing device controls for cameras
/// </summary>
public interface ICamera : IDevice {
    /// <summary>
    /// Test if the camera's cooling is enabled
    /// </summary>
    /// <value>true if cooling is enabled</value>
    public bool IsCoolingEnabled {get;}
    /// <summary>
    /// Enable camera sensor cooling
    /// </summary>
    public void EnableCooling();
    /// <summary>
    /// Disable camera sensor cooling
    /// </summary>
    public void DisableCooling();
    /// <summary>
    /// Take an image by exposing the camera's sensor for the given amount of time
    /// </summary>
    /// <param name="timespan">duration of exposure</param>
    public CancellationTokenSource Expose(Duration timespan);
}

}