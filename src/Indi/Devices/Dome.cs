// Open/Close Dome
// Rotate CW/CCW

using System;
using System.Linq;
using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Controller abstraction for dome devices
/// </summary>
public class IndiDomeController : IndiDeviceController, IDome {

    public IndiDomeController(IndiDevice device) : base(device) {}

    /// <summary>
    /// Check if the shutter is open
    /// </summary>
    /// <value>true if the shutter is open</value>
    public bool IsShutterOpen {
        get {
            var vec = GetPropertyOrDefault<IndiVector<IndiSwitchValue>>("DOME_SHUTTER");
            var open = vec?.GetItemWithName("SHUTTER_OPEN");
            return open != null && open.IsOn;
        }
    }
    /// <summary>
    /// Open the dome's shutter
    /// </summary>
    public void OpenShutter() {
        var vec = GetPropertyOrDefault<IndiVector<IndiSwitchValue>>("DOME_SHUTTER");
        vec.SwitchTo("SHUTTER_OPEN");
        SetProperty(vec);
    }
    /// <summary>
    /// Close the dome's shutter
    /// </summary>
    public void CloseShutter() {
        var vec = GetPropertyOrDefault<IndiVector<IndiSwitchValue>>("DOME_SHUTTER");
        vec.SwitchTo("SHUTTER_CLOSE");
        SetProperty(vec);
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
    /// Move the dome to the given azimuthal angle
    /// </summary>
    /// <param name="rpm">rotations per minute</param>
    /// <param name="angle">current rotation angle</param>
    public void Goto(double rpm, Angle angle) {
        var vec = this.GetPropertyOrThrow<IndiVector<IndiNumberValue>>("ABS_DOME_POSITION");
        var pos = vec.GetItemWithName("DOME_ABSOLUTE_POSITION");
        if (pos != null) {
            this.SetSpeed(rpm);
            pos.Value = (double)angle.TotalDegrees();
            SetProperty(vec);
        }
    }
    /// <summary>
    /// Begin rotating the dome clockwise
    /// </summary>
    /// <param name="rpm">rotations per minute</param>
    public void RotateCW(double rpm) {
        this.SetSpeed(rpm);

        var vec = this.GetPropertyOrThrow<IndiVector<IndiSwitchValue>>("DOME_MOTION");
        vec.SwitchTo("DOME_CW");
        SetProperty(vec);
    }
    /// <summary>
    /// Begin rotating the dome counter clockwise
    /// </summary>
    /// <param name="rpm">rotations per minute</param>
    public void RotateCCW(double rpm) {
        this.SetSpeed(rpm);

        var vec = this.GetPropertyOrThrow<IndiVector<IndiSwitchValue>>("DOME_MOTION");
        vec.SwitchTo("DOME_CW");
        SetProperty(vec);
    }
    /// <summary>
    /// Stop all rotations
    /// </summary>
    public void Stop() {
        var vec = this.GetPropertyOrThrow<IndiVector<IndiSwitchValue>>("DOME_ABORT_MOTION");
        vec.SwitchTo("ABORT");
        SetProperty(vec);
    }

}

}