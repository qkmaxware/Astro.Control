using System;
using System.IO;
using System.Threading.Tasks;
using Qkmaxware.Astro.Control.Devices;
using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Simulated telescope
/// </summary>
public class SimulatedTelescope : ITelescope {
    public string Name => ".Net Simulated Telescope Mount";

    public bool IsConnected => true;

    public void Connect() { }

    public void Disconnect() { }


    /// <summary>
    /// The current right ascension of the telescope
    /// </summary>
    /// <returns></returns>
    public Angle RightAscension {
        get {
            resolveMotion();
            return ra;
        }
    }
    /// <summary>
    /// The current declination of the telescope
    /// </summary>
    /// <returns></returns>
    public Angle Declination {
        get {
            resolveMotion();
            return dec;
        }
    }

    public bool IsSlewing => motionStartedAt != null;
    public bool IsTracking => false;

    private Angle ra = Angle.Zero;
    private Angle dec = Angle.Zero;
    private Angle targetRa;
    private Angle targetDec;

    private DateTime? motionStartedAt;

    // Default is the sidereal tracking rate
    public static Angle SiderealTrackingRatePerSecond = Angle.Degrees(0.00417805556); 
    public Angle RaRatePerSecond {get; set;}  = SiderealTrackingRatePerSecond;
    public Angle DecRatePerSecond {get; set;} = SiderealTrackingRatePerSecond;
    private double vRpm;
    private double hRpm;

    private static double moveTowards(double current, double target, double maxDelta) {
        if (Math.Abs(target - current) <= maxDelta) {
            return target;
        }
        return current + Math.Sign(target - current) * maxDelta;
    }

    private void resolveMotion() {
        // Goto's
        if (motionStartedAt != null && targetRa != null && targetDec != null) {
            var duration = (DateTime.Now - motionStartedAt.Value).TotalSeconds;
            var startra = (double)this.ra.TotalDegrees();
            var startdec = (double)this.dec.TotalDegrees();
            ra = Angle.Degrees(moveTowards(startra, (double)this.targetRa.TotalDegrees(), (double)this.RaRatePerSecond.TotalDegrees() * duration)).Wrap();
            dec = Angle.Degrees(moveTowards(startdec, (double)this.targetDec.TotalDegrees(), (double)this.DecRatePerSecond.TotalDegrees() * duration)).Wrap();
            var afterra = (double)ra.TotalDegrees();
            var afterdec = (double)dec.TotalDegrees();

            if (startra == afterra && startdec == afterdec) {
                motionStartedAt = null;
                targetDec = null;
                targetRa = null;
            }
        }
        // Manual rotation
        if (motionStartedAt != null) {
            var duration = (DateTime.Now - motionStartedAt.Value).TotalSeconds;
            ra = Angle.Degrees((double)this.ra.TotalDegrees() + hRpm * duration).Wrap();
            dec = Angle.Degrees((double)this.dec.TotalDegrees() + vRpm * duration).Wrap();
        }
    }

    private void cancelMotion() {
        motionStartedAt = null;
        targetDec = null;
        targetRa = null;
        this.vRpm = 0;
        this.hRpm = 0;
    }

    private void startMotion(Angle targetRa, Angle targetDec) {
        motionStartedAt = DateTime.Now;
        this.targetRa = targetRa.Wrap();
        this.targetDec = targetDec.Wrap();
    }

    public void Rotate(float horizontal, float vertical) {
        resolveMotion();
        cancelMotion();
        vRpm = vertical;
        hRpm = horizontal;
        motionStartedAt = DateTime.Now;
    }

    public void Sync(Angle ra, Angle dec) {
        resolveMotion();
        cancelMotion();
        this.ra = ra.Wrap();
        this.dec = dec.Wrap();
    }

    public void Goto(Angle ra, Angle dec) {
        resolveMotion();
        cancelMotion();
        startMotion(ra, dec);
    }

    public void Track(Angle ra, Angle dec, TrackingRate rate) {
        resolveMotion();
        cancelMotion();
        this.ra = ra.Wrap();
        this.dec = dec.Wrap();
    }
}

}