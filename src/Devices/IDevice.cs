using System.Collections.Generic;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Implementation of a generic device
/// </summary>
public interface IDevice {
    #region Device Information
    /// <summary>
    /// Name of the device being controlled
    /// </summary>
    /// <value>name</value>
    string Name {get;}
    #endregion

    #region Connection Status
    /// <summary>
    /// Connection status of the device
    /// </summary>
    /// <value>true if the device is connected and accessible</value>
    bool IsConnected {get;}
    /// <summary>
    /// Connect the device
    /// </summary>
    void Connect();
    /// <summary>
    /// Disconnect the device
    /// </summary>
    void Disconnect();
    #endregion
}

/// <summary>
/// A generic device that has communicate details
/// </summary>
public interface IHasPortDetails{
    /// <summary>
    /// Port used by the INDI device
    /// </summary>
    public string Port {get; set;}
}

public interface IHasCommunicationsDetails {
    /// <summary>
    /// Communication baud rate
    /// </summary>
    int? BaudRate {get; set;}
    /// <summary>
    /// Enumerate over valid baud rates for the given device
    /// </summary>
    /// <returns>enumerable of baud rates</returns>
    IEnumerable<int> EnumerateBaudRates();
}

}