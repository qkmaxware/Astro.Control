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
    /// The absolute position of the focuser
    /// </summary>
    /// <value>the absolute position of the focuser</value>
    public int AbsolutePosition {
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
    /// Focus inwards by the specific number of steps
    /// </summary>
    /// <param name="steps">steps to focus</param>
    public void FocusInwards(int steps) {
        var vector = this.GetPropertyOrThrow<IndiVector<IndiNumberValue>>("REL_FOCUS_POSITION");
        vector.GetItemWithName("FOCUS_RELATIVE_POSITION").Value = steps;
        SetProperty(vector);
    }

    /// <summary>
    /// Focus outwards by the specific number of steps
    /// </summary>
    /// <param name="steps">steps to focus</param>
    public void FocusOutwards(int steps) {
        FocusInwards(-steps);
    }
}

}