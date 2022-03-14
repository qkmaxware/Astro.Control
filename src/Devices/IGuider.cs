namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Interface for devices that can perform auto-guiding
/// </summary>
public interface IGuider {
    /// <summary>
    /// Begin guiding with the auto-guider
    /// </summary>
    void BeginGuiding();
}

}