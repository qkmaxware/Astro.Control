using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Controller abstraction for filter wheels
/// </summary>
public class IndiFilterWheelController : IndiDeviceController, IFilterWheel {
    
    /// <summary>
    /// Number of filters in this wheel (optional)
    /// </summary>
    public int? FilterCount;

    public IndiFilterWheelController(IndiDevice device, int? filterCount = null) : base(device) {
        this.FilterCount = filterCount;
    }

    /// <summary>
    /// List all the filters supported by this device
    /// </summary>
    /// <returns>list of filter names</returns>
    public List<string> ListFilterNames() {
        var property = "FILTER_NAME";
        var vector = this.GetPropertyOrDefault<IndiVector<IndiTextValue>>(property);
        return vector?.Select(x => x.Value)?.ToList() ?? new List<string>();
    }
    /// <summary>
    /// Change all the filters currently on this device
    /// </summary>
    /// <param name="filters">list of filter names</param>
    public void UpdateFilterNames(List<string> filters) {
        var property = "FILTER_NAME";
        var vector = this.GetPropertyOrDefault<IndiVector<IndiTextValue>>(property);

        if (vector != null) {
            vector.Clear();
            foreach (var filter in filters) {
                vector.Add(new IndiTextValue{ Value = filter });
            }
            this.SetProperty(vector);
        }
    }

    /// <summary>
    /// Current slot of the filter wheel
    /// </summary>
    /// <returns>slot index from 0 to N-1</returns>
    public int CurrentFilterIndex() => (int)(this.GetPropertyOrDefault<IndiVector<IndiNumberValue>>("FILTER_SLOT").GetItemWithName("FILTER_SLOT_VALUE")?.Value ?? 1) - 1;

    /// <summary>
    /// Send a request to change the current filter
    /// </summary>
    /// <param name="index">index of the filter to change to</param>
    public void ChangeFilterAsync(int index) {
        var prop = "FILTER_SLOT";
        var value = this.GetPropertyOrThrow<IndiVector<IndiNumberValue>>(prop);
        var slot = value.GetItemWithName("FILTER_SLOT_VALUE");
        if (slot != null) {
            slot.Value = index + 1; // indexes in INDI are 1->N, indexes in c# are 0->N-1

            if (this.FilterCount.HasValue) {
                var _internal = slot.Value;
                var N = this.FilterCount.Value;
                slot.Value = (_internal - N * Math.Floor(_internal / N));
            }
        }

        this.SetProperty(value);
    }

    /// <summary>
    /// Change to the next filter
    /// </summary>
    public void NextFilter() {
        this.ChangeFilterAsync(this.CurrentFilterIndex() + 1);
    }

    /// <summary>
    /// Change to the previous filter
    /// </summary>
    public void PreviousFilter() {
        this.ChangeFilterAsync(this.CurrentFilterIndex() - 1);
    }

}

}