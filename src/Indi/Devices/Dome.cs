// Open/Close Dome
// Rotate CW/CCW

using System;
using System.Linq;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Controller abstraction for dome devices
/// </summary>
public class IndiDomeController : IndiDeviceController, IDome {

    public IndiDomeController(IndiDevice device) : base(device) {}
    
    /// <summary>
    /// Check or set if the dome's shutter is open or closed
    /// </summary>
    /// <value>true if the dome's shutter is open</value>
    public bool IsShutterOpen {
        get {
            var vec = GetPropertyOrDefault<IndiVector<IndiSwitchValue>>("DOME_SHUTTER");
            var open = vec?.GetItemWithName("SHUTTER_OPEN");
            return open != null && open.IsOn;
        } set {
            var vec = GetPropertyOrDefault<IndiVector<IndiSwitchValue>>("DOME_SHUTTER");
            if (value) {
                vec.SwitchTo("SHUTTER_OPEN");
            } else {
                vec.SwitchTo("SHUTTER_CLOSE");
            }
            SetProperty(vec);
        }
    }

    /// <summary>
    /// Set the rotational speed of the dome
    /// </summary>
    /// <param name="rpm">rotational speed in Revolutions Per Minute</param>
    public void SetSpeed(double rpm) {
        var vec = this.GetPropertyOrThrow<IndiVector<IndiNumberValue>>("DOME_SPEED");
        var val = vec.GetItemWithName("DOME_SPEED_VALUE");
        if (val != null) {
            val.Value = rpm;
        }
        SetProperty(vec);
    }

    /// <summary>
    /// Start rotating the dome clockwise at set speed
    /// </summary>
    public void StartRotatingClockwise() {
        var vec = this.GetPropertyOrThrow<IndiVector<IndiSwitchValue>>("DOME_MOTION");
        vec.SwitchTo("DOME_CW");
        SetProperty(vec);
    }

    /// <summary>
    /// Start rotating the dome counter clockwise at set speed
    /// </summary>
    public void StartRotatingCClockwise() {
        var vec = this.GetPropertyOrThrow<IndiVector<IndiSwitchValue>>("DOME_MOTION");
        vec.SwitchTo("DOME_CW");
        SetProperty(vec);
    }

    /// <summary>
    /// Stop rotating the dome
    /// </summary>
    public void StopRotating() {
        var vec = this.GetPropertyOrThrow<IndiVector<IndiSwitchValue>>("DOME_ABORT_MOTION");
        vec.SwitchTo("ABORT");
        SetProperty(vec);
    }

}

}