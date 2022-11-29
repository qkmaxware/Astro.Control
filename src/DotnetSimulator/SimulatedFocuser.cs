using System;
using System.IO;
using System.Threading.Tasks;
using Qkmaxware.Astro.Control.Devices;
using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Simulated focuser
/// </summary>
public class SimulatedFocuser : IFocuser {
    public string Name => ".Net Simulated Focuser";

    public bool IsConnected => true;
    public void Connect() { }
    public void Disconnect() { }

    public bool IsMotionReversed {get; private set;}


    public int FocusPosition {get; private set;}
    public void ResetMotion() => IsMotionReversed = false;
    public void ReverseMotion() => IsMotionReversed = true;


    public int GetMaximumFocusPosition() => 100;
    public int GetMinimumFocusPosition() => 0;

    public void GotoFocusPosition(int speed, int position) {
        resolveMotion();
        FocusPosition = position;
    }

    private int stepsPerMinute;
    private DateTime? startedMovingAt;

    private void resolveMotion() {
        if (startedMovingAt.HasValue) {
            var duration = (DateTime.Now - startedMovingAt.Value).TotalMinutes;
            var delta = duration * stepsPerMinute;
            this.FocusPosition = (int)Math.Max(Math.Min(this.FocusPosition + delta, GetMaximumFocusPosition()), GetMinimumFocusPosition());
            startedMovingAt = null;
        }
    }
    public void StartFocusing(int velocity) {
        resolveMotion();
        this.stepsPerMinute = velocity * 60;
        startedMovingAt = DateTime.Now;
    }

    public void StopFocusing() {
        resolveMotion();
        this.stepsPerMinute = 0;
        startedMovingAt = null;
    }
}

}