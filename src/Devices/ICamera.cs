using System;
using System.Threading;
using System.Threading.Tasks;
using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Generic interface for images transferred from an ICamera 
/// </summary>
public interface ICameraImage {
    public string SaveFile(string path);
}

/// <summary>
/// Camera image binning
/// </summary>
public class Binning {
    private int _h;
    public int Horizontal {
        get => _h; 
        set => _h = Math.Max(1, value); // Ensure positive non-zero
    }
    private int _v;
    public int Vertical {
        get => _v; 
        set => _v = Math.Max(1, value); // Ensure positive non-zero
    }
    public Binning() {
        this.Horizontal = 1;
        this.Vertical = 1;
    }
    public Binning(int horizontal, int vertical) {
        this.Horizontal = horizontal;
        this.Vertical = vertical;
    }
}

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
    /// <param name="binning">binning to use</param>
    /// <returns>camera image or null once exposure is done</returns>
    Task<ICameraImage> ExposeAsync(Duration timespan, Binning binning);
}

}