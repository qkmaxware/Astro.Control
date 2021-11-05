// Set focuser speed
// Focus Infinity
// Focus Near

using System;
using System.Linq;

namespace Qkmaxware.Astro.Control.Controllers {

public class IndiFocuserController : IndiDeviceController {

    public IndiFocuserController(IndiDevice device) : base(device) {}

    /// <summary>
    /// Begin focusing inwards
    /// </summary>
    /// <param name="speed">focusing speed from 0 to Max</param>
    public void StartFocusingInward(int speed) {
        // Set direction
        var direction = this.GetProperty<IndiVector<IndiSwitchValue>>("FOCUS_MOTION");
        direction.SwitchTo("FOCUS_INWARD");
        this.SetProperty("FOCUS_MOTION", direction);

        // Set speed
        var speedValue = this.GetProperty<IndiNumberValue>("FOCUS_SPEED");
        speedValue.Value = speed;
        this.SetProperty("FOCUS_SPEED", speedValue);
    }

    /// <summary>
    /// Begin focusing outwards
    /// </summary>
    /// <param name="speed">focusing speed from 0 to Max</param>
    public void StartFocusingOutward(int speed) {
        // Set direction
        var direction = this.GetProperty<IndiVector<IndiSwitchValue>>("FOCUS_MOTION");
        direction.SwitchTo("FOCUS_OUTWARD");
        this.SetProperty("FOCUS_MOTION", direction);

        // Set speed
        var speedValue = this.GetProperty<IndiNumberValue>("FOCUS_SPEED");
        speedValue.Value = speed;
        this.SetProperty("FOCUS_SPEED", speedValue);
    }

    /// <summary>
    /// Stop all focusing motion
    /// </summary>
    public void StopFocusing() {
        var abort = this.GetProperty<IndiVector<IndiSwitchValue>>("FOCUS_ABORT_MOTION");
        abort.SwitchTo("ABORT");
        this.SetProperty("FOCUS_ABORT_MOTION", abort);
    }

}

}