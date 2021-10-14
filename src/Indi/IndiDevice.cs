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

namespace Qkmaxware.Astro.Control {

/// <summary>
/// Device properties container
/// </summary>
public class IndiPropertiesContainer : IEnumerable<KeyValuePair<string, IndiValue>>{
    private ConcurrentDictionary<string, IndiValue> properties = new ConcurrentDictionary<string, IndiValue>();

    public IndiPropertiesContainer() {}

    /// <summary>
    /// Check if the given property exists
    /// </summary>
    /// <param name="name">property name</param>
    /// <returns>true if property exists</returns>
    public bool HasProperty(string name) {
        return properties.ContainsKey(name);
    }

    /// <summary>
    /// Clear all properties
    /// </summary>
    public void Clear() {
        this.properties.Clear();
    }   

    /// <summary>
    /// Delete property
    /// </summary>
    /// <param name="property">property name</param>
    public void Delete(string property) {
        IndiValue old;
        this.properties.TryRemove(property, out old);
    }

    /// <summary>
    /// Enumerate over properties
    /// </summary>
    /// <returns>property enumerator</returns>
    public IEnumerator<KeyValuePair<string, IndiValue>> GetEnumerator() {
        return this.properties.GetEnumerator();
    }

    /// <summary>
    /// Enumerate over properties
    /// </summary>
    /// <returns>property enumerator</returns>
    IEnumerator IEnumerable.GetEnumerator() {
        return this.GetEnumerator();
    }

    /// <summary>
    /// Get or set a property by name
    /// </summary>
    /// <value>property value</value>
    public IndiValue this[string key] {
        get => properties[key];
        internal set => properties[key] = value;
    }
}

/// <summary>
/// Abstraction for a device accessible over an INDI connection
/// </summary>
public class IndiDevice {
    private IndiConnection conn;
    /// <summary>
    /// Name of the device
    /// </summary>
    /// <value>name</value>
    public string Name {get; private set;}
    /// <summary>
    /// Properties associated with this device
    /// </summary>
    /// <returns>property container</returns>
    public IndiPropertiesContainer Properties {get; private set;} = new IndiPropertiesContainer();

    /// <summary>
    /// Create a new device on the given connection with the given name
    /// </summary>
    /// <param name="name">device name</param>
    /// <param name="connection">connection the device is accessible over</param>
    public IndiDevice(string name, IndiConnection connection) {
        this.Name = name;
        this.conn = connection;
    }

    /// <summary>
    /// Send a request to update a partiular device property
    /// </summary>
    /// <param name="property">property name</param>
    public void RefreshProperty(string property) {
        // Send a request to get the properties for this device
        this.conn.Send(new IndiGetPropertiesMessage(this.Name, property));
    }

    /// <summary>
    /// Send a request to update all device properties
    /// </summary>
    public void RefreshProperties() {
        // Send a request to get the properties for this device
        this.conn.Send(new IndiGetPropertiesMessage(this.Name));
    }

    /// <summary>
    /// Send a request to the INDI server to change the value of a property
    /// </summary>
    /// <param name="name">property name</param>
    /// <param name="value">new property value</param>
    public void ChangeRemoteProperty(string name, IndiValue value) {
        // Create NewProperty client message
        // Populate device name, property name, and value
        var message = new IndiNewPropertyMessage(this.Name, name, value);
        // Update local copy
        // this.Properties[name] = value; 
        // Encode and send
        conn.Send(message);
        //RefreshProperty(name);
    }
}   

}