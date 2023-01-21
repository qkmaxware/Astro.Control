using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Qkmaxware.Astro.Control.Devices;
using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Simulated filter wheel
/// </summary>
public class SimulatedFilterWheel : IFilterWheel {
    public string Name => ".Net Simulated Filter Wheel";

    public bool IsConnected => true;
    public void Connect() { }
    public void Disconnect() { }

    private int filterIndex = 0;
    public void ChangeFilterAsync(int index) {
        this.filterIndex = index % filters.Count;
    }
    public int CurrentFilterIndex() {
        return filterIndex;
    }

    private List<string> filters = new List<string>() {
        "Red",
        "Blue",
        "Green",
        "Luminance",
    };
    public List<string> ListFilterNames() {
        return this.filters;
    }
    public void UpdateFilterNames(List<string> filters) {
        this.filters = filters ?? new List<string>();
    }
}

}