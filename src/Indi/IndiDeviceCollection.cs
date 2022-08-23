using System;
using System.Linq;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Dynamic;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace Qkmaxware.Astro.Control {

/// <summary>
/// A thread safe collection of INDI devices
/// </summary>
public class IndiDeviceCollection {

    public int Count => propertiesByDeviceName.Count;

    private ConcurrentDictionary<string, ConcurrentDictionary<string, IndiValue>> propertiesByDeviceName = new ConcurrentDictionary<string, ConcurrentDictionary<string, IndiValue>>();
    private IndiConnection source;

    public IndiDeviceCollection(IndiConnection source) {
        this.source = source;
    }

    internal void ReserveDeviceNameIfNotExists(string name) {
        propertiesByDeviceName.AddOrUpdate(
            name, 
            (id) => new ConcurrentDictionary<string, IndiValue>(), 
            (id, @old) => @old
        );
    }

    /// <summary>
    /// Get the properties for a given device by name, or null
    /// </summary>
    /// <param name="name">device name</param>
    /// <returns>properties container</returns>
    internal ConcurrentDictionary<string,IndiValue> GetPropertiesForDevice(string name) {
        ConcurrentDictionary<string, IndiValue> properties;
        if (propertiesByDeviceName.TryGetValue(name, out properties)) {
            return properties;
        } else {
            return null;
        }
    }

    /// <summary>
    /// Fetch a device with the given name or null if the device does not exist
    /// </summary>
    /// <param name="name">device name</param>
    /// <returns>device or null if no device with the given name exists</returns>
    public IndiDevice GetDeviceByNameOrNull(string name) {
        if (this.propertiesByDeviceName.ContainsKey(name)) {
            return new IndiDevice(name, this.source);
        } else {
            return null;
        }
    }

    /// <summary>
    /// Fetch a device with the given name or throw exception
    /// </summary>
    /// <param name="name">device name</param>
    /// <returns>device</returns>
    public IndiDevice GetDeviceByNameOrThrow(string name) {
        if (this.propertiesByDeviceName.ContainsKey(name)) {
            return new IndiDevice(name, this.source);
        } else {
            throw new ArgumentException($"Device '{name}' not found over this connection");
        }
    }

    /// <summary>
    /// Try to get a device by the given name, if it exists
    /// </summary>
    /// <param name="name">device name</param>
    /// <param name="device">device if the device exists</param>
    /// <returns>true if the device exists</returns>
    public bool TryGetDeviceByName(string name, out IndiDevice device) {
        device = this.GetDeviceByNameOrNull(name);
        return device != null;
    }

    /// <summary>
    /// All devices on this server
    /// </summary>
    /// <returns>list of devices</returns>
    public IEnumerable<IndiDevice> AllDevices() {
        return this.propertiesByDeviceName.Select(x => new IndiDevice(x.Key, this.source));
    }

    /// <summary>
    /// All devices currently with an established connection
    /// </summary>
    /// <returns>list of devices</returns>
    public IEnumerable<IndiDevice> ConnectedDevices() {
        return this.AllDevices().Where(device => device.IsConnected);
    }

    /// <summary>
    /// All devices currently without an established connection
    /// </summary>
    /// <returns>list of devices</returns>
    public IEnumerable<IndiDevice> DisconnectedDevices() {
        return this.AllDevices().Where(device => !device.IsConnected);
    }

    /// <summary>
    /// All telescopes on this server as identified by the INDI property "TELESCOPE_INFO" 
    /// </summary>
    /// <returns>list of devices</returns>
    public IEnumerable<IndiDevice> AllTelescopes() {
        return AllDevices().Where((d) => d.Properties.Exists("TELESCOPE_INFO"));
    }

    /// <summary>
    /// All CCD devices on this server as identified by the INDI property "CCD_INFO" 
    /// </summary>
    /// <returns>list of devices</returns>
    public IEnumerable<IndiDevice> AllCCDs() {
        return AllDevices().Where((d) => d.Properties.Exists("CCD_INFO"));
    }

    /// <summary>
    /// All filter wheel devices on this server as identified by the INDI property "FILTER_SLOT" that are not cameras (as they can share the filter properties)
    /// </summary>
    /// <returns>list of devices</returns>
    public IEnumerable<IndiDevice> AllFilterWheels() {
        return AllDevices().Where((d) => d.Properties.Exists("FILTER_SLOT") && !d.Properties.Exists("CCD_INFO"));
    }

    /// <summary>
    /// All focuser devices on this server as identified by the INDI property "FOCUS_MOTION" 
    /// </summary>
    /// <returns>list of devices</returns>
    public IEnumerable<IndiDevice> AllFocusers() {
        return AllDevices().Where((d) => d.Properties.Exists("FOCUS_MOTION"));
    }

    /// <summary>
    /// All observation domes on this server as identified by the INDI property "DOME_MEASUREMENTS" 
    /// </summary>
    /// <returns>list of devices</returns>
    public IEnumerable<IndiDevice> AllDomes() {
        return AllDevices().Where((d) => d.Properties.Exists("DOME_MEASUREMENTS"));
    }
}

}