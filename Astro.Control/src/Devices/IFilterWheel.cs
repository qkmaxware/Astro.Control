using System.Collections.Generic;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Interface describing device controls for filter wheels
/// </summary>
public interface IFilterWheel : IDevice {

    /// <summary>
    /// Get the index to the currently selected filter
    /// </summary>
    /// <returns>filter index</returns>
    public int CurrentFilterIndex();
    /// <summary>
    /// Change the current filter
    /// </summary>
    /// <param name="index">index of the filter</param>
    public void ChangeFilterAsync(int index);

    /// <summary>
    /// List all the filters supported by this device
    /// </summary>
    /// <returns>list of filter names</returns>
    List<string> ListFilterNames();
    /// <summary>
    /// Change all the filters currently on this device
    /// </summary>
    /// <param name="filters">list of filter names</param>
    void UpdateFilterNames(List<string> filters);

}

}