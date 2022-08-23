// Set focuser speed
// Focus Infinity
// Focus Near

using System;
using System.Linq;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Controller abstraction for focuser devices
/// </summary>
public class IndiFocuserController : IndiDeviceController, IFocuser {

    public IndiFocuserController(IndiDevice device) : base(device) {}

    /// <summary>
    /// Check if this focuser's direction is reversed
    /// </summary>
    /// <value>true if the focuser's movement direction is reversed</value>
    public bool IsMotionReversed {
        get {
            var vector = this.GetPropertyOrDefault<IndiVector<IndiSwitchValue>>("FOCUS_REVERSE_MOTION");
            if (vector != null) {
                return vector.IsOn("ENABLED");
            } else {
                return false;
            }
        }
        set {
            var vector = this.GetPropertyOrDefault<IndiVector<IndiSwitchValue>>("FOCUS_REVERSE_MOTION");
            if (vector != null) {
                if (value) {
                    vector.SwitchTo("ENABLED");
                } else {
                    vector.SwitchTo("DISABLED");
                }
                this.SetProperty(vector);
            }
        }
    }

    /// <summary>
    /// Reverse the focuser movement motion 
    /// </summary>
    public void ReverseMotion() => IsMotionReversed = true;
    /// <summary>
    /// Reset the focuser movement motion
    /// </summary>
    public void ResetMotion() => IsMotionReversed = false;

    /// <summary>
    /// The absolute position of the focuser
    /// </summary>
    /// <value>the absolute position of the focuser</value>
    public int FocusPosition {
        get {
            var vector = this.GetPropertyOrDefault<IndiVector<IndiNumberValue>>("ABS_FOCUS_POSITION");
            if (vector != null) {
                return (int)(vector.GetItemWithName("FOCUS_ABSOLUTE_POSITION")?.Value ?? 0);
            } else {
                return 0;
            }
        } set {
            var vector = this.GetPropertyOrDefault<IndiVector<IndiNumberValue>>("FOCUS_SYNC");
            if (vector != null) {
                var v = vector.GetItemWithName("FOCUS_SYNC_VALUE");
                if (v != null)
                    v.Value = value;
                SetProperty(vector);
            }
        }
    }

    /// <summary>
    /// Maximum focus position
    /// </summary>
    /// <returns>maximum focus position</returns>
    public int GetMaximumFocusPosition() => (int)(this.GetPropertyOrDefault<IndiVector<IndiNumberValue>>("FOCUS_MAX")?.GetItemWithName("FOCUS_MAX_VALUE")?.Value ?? 0);
    /// <summary>
    /// Minimum focus position
    /// </summary>
    /// <returns>minimum focus position</returns>
    public int GetMinimumFocusPosition() => 0;
    /// <summary>
    /// Goto the given focus position
    /// </summary>
    /// <param name="speed">speed of the focusing</param>
    /// <param name="position">focus position</param>
    public void GotoFocusPosition(int speed, int position) {
        this.Speed = Math.Abs(speed);
        this.FocusPosition = position;
    }

    /// <summary>
    /// The speed of the focuser
    /// </summary>
    /// <value>the speed of the focuser</value>
    public int Speed {
        get {
            var vector = this.GetPropertyOrDefault<IndiVector<IndiNumberValue>>("FOCUS_SPEED");
            if (vector != null) {
                return (int)(vector.GetItemWithName("FOCUS_SPEED_VALUE")?.Value ?? 0);
            } else {
                return 0;
            }
        } set {
            var vector = this.GetPropertyOrDefault<IndiVector<IndiNumberValue>>("FOCUS_SPEED");
            if (vector != null) {
                var v = vector.GetItemWithName("FOCUS_SPEED_VALUE");
                if (v != null)
                    v.Value = value;
                SetProperty(vector);
            }
        }
    }

    /// <summary>
    /// Start focusing
    /// </summary>
    /// <param name="velocity">velocity of focusing; magnitude is speed, sign is direction</param>
    public void StartFocusing(int velocity) {
        var property = "FOCUS_MOTION";
        var vector = this.GetPropertyOrDefault<IndiVector<IndiSwitchValue>>(property);
        var speed = Math.Abs(velocity);
        var dir = Math.Sign(velocity);

        if (vector != null) {
            this.Speed = speed;
            if (dir < 0) {
                vector.SwitchTo("FOCUS_OUTWARD");
            } else {
                vector.SwitchTo("FOCUS_INWARD");
            }
            this.SetProperty(vector);
        }
    }
    /// <summary>
    /// Stop focusing
    /// </summary>
    public void StopFocusing() {
        this.Speed = 0;

        var property = "FOCUS_MOTION";
        var vector = this.GetPropertyOrDefault<IndiVector<IndiSwitchValue>>(property);
        if (vector != null) {
            foreach (var dir in vector) {
                dir.Value = false;
            }
            this.SetProperty(vector);
        }
    }
}

}