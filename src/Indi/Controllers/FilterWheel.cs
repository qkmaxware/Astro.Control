using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Qkmaxware.Astro.Control.Controllers {

/// <summary>
/// Controller abstraction for filter wheels
/// </summary>
public class IndiFilterWheelController : IndiDeviceController {
    
    /// <summary>
    /// Number of filters in this wheel (optional)
    /// </summary>
    public int? FilterCount;

    public IndiFilterWheelController(IndiDevice device, int? filterCount = null) : base(device) {
        this.FilterCount = filterCount;
    }

    /// <summary>
    /// Current slot of the filter wheel
    /// </summary>
    /// <returns>slot index from 1 to N</returns>
    public int GetFilterIndex() {
        var value = this.GetProperty<IndiNumberValue>("FILTER_SLOT");
        return (int)value.Value;
    }

    /// <summary>
    /// Send a request to change the current filter
    /// </summary>
    /// <param name="index">index of the filter to change to</param>
    public void ChangeFilterAsync(int index) {
        var prop = "FILTER_SLOT";
        var value = this.GetProperty<IndiNumberValue>(prop);
        value.Value = index;

        if (this.FilterCount.HasValue) {
            var _internal = value.Value;
            var N = this.FilterCount.Value;
            value.Value = (_internal - N * Math.Floor(_internal / N));
        }

        this.SetProperty(prop, value);
    }

    /// <summary>
    /// Change to the next filter
    /// </summary>
    public void NextFilter() {
        this.ChangeFilterAsync(this.GetFilterIndex() + 1);
    }

    /// <summary>
    /// Change to the previous filter
    /// </summary>
    public void PreviousFilter() {
        this.ChangeFilterAsync(this.GetFilterIndex() - 1);
    }

}

}