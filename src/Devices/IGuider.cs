namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Simple enum of auto-guider states
/// </summary>
public enum GuideState {
    Idle, Calibrating, Guiding
}

/// <summary>
/// Interface for devices that can perform auto-guiding
/// </summary>
public interface IGuider : IDevice {
    /// <summary>
    /// Check if the auto-guider is currently guiding
    /// </summary>
    /// <value>stateof the auto-guider</value>
    GuideState State {get;}
    /// <summary>
    /// Begin guiding with the auto-guider
    /// </summary>
    void StartGuiding();
    /// <summary>
    /// Stop the auto-guider
    /// </summary>
    void StopGuiding();
}

}