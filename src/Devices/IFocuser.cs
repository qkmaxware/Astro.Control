namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Interface describing device controls for focusers
/// </summary>
public interface IFocuser : IDevice {
    /// <summary>
    /// Test if the focuser motion is reversed
    /// </summary>
    /// <value>true if the motion is reversed</value>
    bool IsMotionReversed {get;}
    /// <summary>
    /// Reverse the focuser movement motion 
    /// </summary>
    void ReverseMotion();
    /// <summary>
    /// Reset the focuser movement motion
    /// </summary>
    void ResetMotion();

    /// <summary>
    /// Start focusing
    /// </summary>
    /// <param name="velocity">velocity of focusing in steps/second; magnitude is speed, sign is direction</param>
    void StartFocusing(int velocity);
    /// <summary>
    /// Stop focusing
    /// </summary>
    void StopFocusing();

    /// <summary>
    /// Current position of the focuser
    /// </summary>
    /// <value></value>
    public int FocusPosition {get;}
    /// <summary>
    /// Maximum focus position
    /// </summary>
    /// <returns>maximum focus position</returns>
    public int GetMaximumFocusPosition();
    /// <summary>
    /// Minimum focus position
    /// </summary>
    /// <returns>minimum focus position</returns>
    public int GetMinimumFocusPosition();
    /// <summary>
    /// Goto the given focus position
    /// </summary>
    /// <param name="speed">speed of the focusing</param>
    /// <param name="position">focus position</param>
    void GotoFocusPosition(int speed, int position);
}

}