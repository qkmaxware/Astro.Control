using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Interface describing device controls for observation domes
/// </summary>
public interface IDome : IDevice {
    
    #region Shutter
    /// <summary>
    /// Check if the shutter is open
    /// </summary>
    /// <value>true if the shutter is open</value>
    bool IsShutterOpen {get; }
    /// <summary>
    /// Open the dome's shutter
    /// </summary>
    void OpenShutter();
    /// <summary>
    /// Close the dome's shutter
    /// </summary>
    void CloseShutter();
    #endregion

    #region Rotation
    /// <summary>
    /// The current direction the dome is pointing
    /// </summary>
    /// <value>direction</value>
    Angle PointingDirection {get;}
    /// <summary>
    /// Move the dome to the given azimuthal angle
    /// </summary>
    /// <param name="rpm">rotations per minute</param>
    /// <param name="angle">current rotation angle</param>
    void Goto(double rpm, Angle angle);
    /// <summary>
    /// Begin rotating the dome clockwise
    /// </summary>
    /// <param name="rpm">rotations per minute</param>
    void RotateCW(double rpm);
    /// <summary>
    /// Begin rotating the dome counter clockwise
    /// </summary>
    /// <param name="rpm">rotations per minute</param>
    void RotateCCW(double rpm);
    /// <summary>
    /// Stop all rotations
    /// </summary>
    void Stop();
    #endregion
}

}