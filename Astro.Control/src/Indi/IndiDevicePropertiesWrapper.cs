using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Dynamic;
using System.Collections.Generic;
using System.Collections;
using Qkmaxware.Astro.Control.Devices;
using System.Linq;

namespace Qkmaxware.Astro.Control {

/// <summary>
/// Wrapper for accessing the properties of a given INDI device
/// </summary>
public class IndiDevicePropertiesWrapper : IEnumerable<KeyValuePair<string, IndiValue>> {
    private IndiConnection Connection;
    private string Name;
    internal IndiDevicePropertiesWrapper(IndiConnection connection, string deviceName) {
        this.Connection = connection;
        this.Name = deviceName;
    }
    /// <summary>
    /// Request that all properties be refreshed asynchronously
    /// </summary>
    public void RefreshAsync() {
        Connection.Send(new IndiGetPropertiesMessage(Name));
    }

    /// <summary>
    /// Request that a specific property be refreshed asynchronously
    /// </summary>
    /// <param name="property">name of the property to refresh</param>
    public void RefreshAsync(string property) {
        Connection.Send(new IndiGetPropertiesMessage(Name));
    }

    /// <summary>
    /// Check if the given property exists
    /// </summary>
    /// <param name="name">property name</param>
    /// <returns>true if property exists</returns>
    public bool Exists(string name) {
        var dict = Connection.Devices.GetPropertiesForDevice(Name); 
        if (dict == null) {
            return false;
        }   
        return dict.ContainsKey(name);
    }
    
    /// <summary>
    /// Check if the given property exists and is of the given type
    /// </summary>
    /// <param name="name">property name</param>
    /// <returns>true if property exists and is of the given type</returns>
    public bool Exists<T>(string name) where T:IndiValue {
        var dict = Connection.Devices.GetPropertiesForDevice(Name); 
        if (dict == null) {
            return false;
        }   
        IndiValue val;
        if (dict.TryGetValue(name, out val)) {
            return val is T;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Get the cached local value of the given property
    /// </summary>
    /// <param name="name">property whose value to get</param>
    /// <returns>value of the given property if it exists</returns>
    public IndiValue GetValueOrNull(string name) {
        var dict = Connection.Devices.GetPropertiesForDevice(Name); 
        if (dict == null) {
            return null;
        }   
        IndiValue value;
        if (dict.TryGetValue(name, out value)) {
            return value;
        } else {
            return null;
        }
    }   

    /// <summary>
    /// Get the cached local value of the given property
    /// </summary>
    /// <param name="name">property whose value to get</param>
    /// <param name="value">value of the given property if it exists</param>
    /// <returns>true if the value exists</returns>
    public bool TryGetValue(string name, out IndiValue value) {
        value = this.GetValueOrNull(name);
        return value != null;
    }

    /// <summary>
    /// Get the cached local value of the given property if it is of the given generic type
    /// </summary>
    /// <param name="property">property whose value to get</param>
    /// <returns>value of the given property if it exists and is of the given generic type</returns>
    public T GetValueOrNull<T>(string name) where T:IndiValue {
        var dict = Connection.Devices.GetPropertiesForDevice(Name); 
        if (dict == null) {
            return null;
        }   
        IndiValue value;
        if (dict.TryGetValue(name, out value)) {
            if (value is T) {
                return (T)value;
            } else {
                return null;
            }
        } else {
            return null;
        }
    }

    /// <summary>
    /// Get the cached local value of the given property
    /// </summary>
    /// <param name="name">property whose value to get</param>
    /// <param name="value">value of the given property if it exists</param>
    /// <returns>true if the value exists</returns>
    public bool TryGetValue<T>(string name, out T value) where T:IndiValue {
        value = this.GetValueOrNull<T>(name);
        return value != null;
    }

    /// <summary>
    /// Set the value of a property, sending the change to the INDI server asynchronously. It may take a few moments to be updated locally.
    /// </summary>
    /// <param name="property">name of the property to change</param>
    /// <param name="value">value to change the property to</param>
    public void SetRemoteValue(string property, IndiValue value) {
        // Create NewProperty client message
        // Populate device name, property name, and value
        var message = new IndiNewPropertyMessage(Name, property, value);
        // Encode and send
        Connection.Send(message);
    }

    /// <summary>
    /// Set the value of a property locally. This change will not be sent to the INDI server.
    /// </summary>
    /// <param name="property">name of the property to change</param>
    /// <param name="value">value to change the property to</param>
    internal void SetLocalValue(string property, IndiValue value) {
        var dict = Connection.Devices.GetPropertiesForDevice(Name); 
        if (dict == null) {
            return;
        }   
        dict[property] = value;
    }
    
    /// <summary>
    /// Clear all properties locally. This change will not be sent to the INDI server.
    /// </summary>
    internal void ClearLocalValues() {
        var dict = Connection.Devices.GetPropertiesForDevice(Name); 
        if (dict == null) {
            return;
        }  
        dict.Clear();
    }   

    /// <summary>
    /// Delete property locally. This change will not be sent to the INDI server.
    /// </summary>
    /// <param name="property">property name</param>
    internal void DeleteLocalValue(string property) {
        var dict = Connection.Devices.GetPropertiesForDevice(Name); 
        if (dict == null) {
            return;
        }  
        IndiValue old;
        dict.TryRemove(property, out old);
    }

    /// <summary>
    /// Enumerate over properties
    /// </summary>
    /// <returns>property enumerator</returns>
    public IEnumerator<KeyValuePair<string, IndiValue>> GetEnumerator() {
        var dict = Connection.Devices.GetPropertiesForDevice(Name); 
        if (dict == null)
            return Enumerable.Empty<KeyValuePair<string, IndiValue>>().GetEnumerator();
        return dict.GetEnumerator();
    }

    /// <summary>
    /// Enumerate over properties
    /// </summary>
    /// <returns>property enumerator</returns>
    IEnumerator IEnumerable.GetEnumerator() {
        return this.GetEnumerator();
    }
}

}