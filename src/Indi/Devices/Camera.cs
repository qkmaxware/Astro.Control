using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Devices {

using IndiSwitchVector = IndiVector<IndiSwitchValue>;

/// <summary>
/// Images received from an IndiCamera, in FITS format
/// </summary>
public class IndiBlobImage : ICameraImage {
    private IndiBlobValue fitsBlob;
    private bool isCompressed;
    public IndiBlobImage(IndiBlobValue blob, bool isCompressed) {
        this.fitsBlob = blob;
        this.isCompressed = isCompressed;
    }
    public string SaveFile(string path) {
        if (!string.IsNullOrEmpty(path) && fitsBlob != null) {
            var dir = System.IO.Path.GetDirectoryName(path);
            var filename = System.IO.Path.GetFileNameWithoutExtension(path);
            var full_path = System.IO.Path.Combine(dir, filename + (isCompressed ? ".fz" : ".fits"));

            fitsBlob.WriteBlobToFile(full_path);

            return full_path;
        } else {
            return null;
        }
    }
}

/// <summary>
/// Controller abstraction for CCD devices
/// </summary>
public class IndiCameraController : IndiDeviceController, ICamera {
    public IndiCameraController(IndiDevice device) : base(device) {}

    private static readonly string ccd_cooling_property = "CCD_COOLER";
    /// <summary>
    /// Test if the camera's cooling is enabled
    /// </summary>
    /// <value>true if cooling is enabled</value>
    public bool IsCoolingEnabled {
        get {
            var vector = GetPropertyOrThrow<IndiSwitchVector>(ccd_cooling_property);
            var on = vector.GetSwitch("COOLER_ON");
            if (on != null && on.IsOn) {
                return true;
            } else {
                return false;
            }
        }
    }
    private void enableCooling(bool isCooling) {
        var vector = GetPropertyOrThrow<IndiSwitchVector>(ccd_cooling_property);
        vector.SwitchTo(isCooling ? "COOLER_ON" : "COOLER_OFF");
        SetProperty(ccd_cooling_property, vector);
    }
    /// <summary>
    /// Enable camera sensor cooling
    /// </summary>
    public void EnableCooling() {
        enableCooling(true);
    }
    /// <summary>
    /// Disable camera sensor cooling
    /// </summary>
    public void DisableCooling() {
        enableCooling(false);
    }

    public void UseRAW() {
        var property = "CCD_COMPRESSION";
        var vector = GetPropertyOrThrow<IndiSwitchVector>(property);
        vector.SwitchTo("CCD_RAW");
        SetProperty(property, vector);
    }
    
    public void UseCompression() {
        var property = "CCD_COMPRESSION";
        var vector = GetPropertyOrThrow<IndiSwitchVector>(property);
        vector.SwitchTo("CCD_COMPRESS");
        SetProperty(vector);
    }

    public bool IsUsingCompression() {
        var property = "CCD_COMPRESSION";
        var vector = GetPropertyOrThrow<IndiSwitchVector>(property);
        return vector != null && vector.IsOn("CCD_COMPRESS");
    }

    private void enableReceivingImageBlobs(IndiBlobState state = IndiBlobState.Also) {
        var message = new IndiEnableBlobMessage {
            DeviceName = this.Name,
            PropertyName = "CCD1",
            State = state
        };
        this.SendMessageToDevice(message);
    }
    private void setBinning(Binning binning) {
        var property = "CCD_BINNING";
        var vector = GetPropertyOrDefault<IndiVector<IndiNumberValue>>(property);
        if (vector != null) {
            var hbin = vector.GetItemWithName("HOR_BIN");
            var ybin = vector.GetItemWithName("VER_BIN");
            if (hbin != null && ybin != null) {
                hbin.Value = binning.Horizontal;
                ybin.Value = binning.Vertical;
                SetProperty(vector);
            }
        }
    }
    private void abort() {
        var property = "CCD_ABORT_EXPOSURE";
        var vector = GetPropertyOrDefault<IndiSwitchVector>(property);
        if (vector != null) {
            vector.SwitchTo("ABORT");
            SetProperty(vector);
        }
    }

    private object shutter = new object();
    public ICameraImage ExposeSync(Duration timespan, Binning binning) {
        // Compute delay
        double seconds = (double)timespan.TotalSeconds();

        // Only 1 may enter here 
        IndiBlobValue blob = null;
        bool isCompressed = false;
        lock (shutter) { 
            // Abort old photos
            abort();

            // Set the image properties
            setBinning(binning);

            // Enable receiving the image
            enableReceivingImageBlobs(IndiBlobState.Also);

            // Setup property watcher
            var CcdWatch = AwaitPropertyChange("CCD1");

            // Take the image
            var property = "CCD_EXPOSURE";
            var vector = this.GetPropertyOrNew<IndiVector<IndiNumberValue>>(property, () => {
                var v = new IndiVector<IndiNumberValue>(property);
                v.Add(new IndiNumberValue { Name = "CCD_EXPOSURE_VALUE", Value = seconds });
                return v;
            });
            vector.GetItemWithName("CCD_EXPOSURE_VALUE").Value = seconds;
            this.SetProperty(vector);

            // Wait for the image to be exposed
            Task.Delay(TimeSpan.FromSeconds(seconds)).Wait();
            
            // Await arrival of blob
            CcdWatch.Wait();

            // Save the blob value
            var new_image = this.GetPropertyOrDefault<IndiVector<IndiBlobValue>>("CCD1");
            var new_blob = new_image?.GetItemWithName("CCD1");
            blob = new_blob;

            isCompressed = this.IsUsingCompression();

            // Disable receiving of images
            enableReceivingImageBlobs(IndiBlobState.Never);
        }

        if (blob != null) {
            return (ICameraImage)(new IndiBlobImage(blob, isCompressed));
        } else {
            return null;
        }
    }

    public Task<ICameraImage> ExposeAsync(Duration timespan, Binning binning) {
        return Task.Run(() => {
            return ExposeSync(timespan, binning);
        });
    }

}

}