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
        protected set => _h = Math.Max(1, value); // Ensure positive non-zero
    }
    private int _v;
    public int Vertical {
        get => _v; 
        protected set => _v = Math.Max(1, value); // Ensure positive non-zero
    }
    public Binning() {
        this.Horizontal = 1;
        this.Vertical = 1;
    }
    public Binning(int horizontal, int vertical) {
        this.Horizontal = horizontal;
        this.Vertical = vertical;
    }

    /// <summary>
    /// 1x1 binning or "no" binning
    /// </summary>
    public static readonly Binning None = new Binning(1, 1);
    /// <summary>
    /// 2x2 binning
    /// </summary>
    public static readonly Binning X2 = new Binning(2, 2);
    /// <summary>
    /// 3x3 binning
    /// </summary>
    public static readonly Binning X3 = new Binning(3, 3);
    /// <summary>
    /// 4x4 binning
    /// </summary>
    public static readonly Binning X4 = new Binning(4, 4);
    /// <summary>
    /// 5x5 binning
    /// </summary>
    public static readonly Binning X5 = new Binning(5, 5);
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