using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Dynamic;
using System.Collections.Generic;
using System.Collections;

namespace Qkmaxware.Astro.Control {

/// <summary>
/// Base class for all INDI values
/// </summary>
public abstract class IndiValue {
    /// <summary>
    /// Value name
    /// </summary>
    public string Name;
    /// <summary>
    /// Value label
    /// </summary>
    public string Label;
    /// <summary>
    /// Name of the type according to INDI protocol design specifications
    /// </summary>
    /// <value></value>
    public abstract string IndiTypeName {get;}
    internal abstract XElement CreateElement(string prefix, string subPrefix);
    /// <summary>
    /// Create XML element to send a New Message
    /// </summary>
    /// <returns>xml</returns>
    public XElement CreateNewElement() {
        return CreateElement("new", "one");
    }
    
    /// <summary>
    /// Create XML element to send a Set Message
    /// </summary>
    /// <returns>xml</returns>
    public XElement CreateSetElement() {
        return CreateElement("set", "one");
    }

    /// <summary>
    /// Create XML element to send a Def Message
    /// </summary>
    /// <returns>xml</returns>
    public XElement CreateDefinitionElement() {
        return CreateElement("def", "def");
    }
}

/// <summary>
/// Base class for INDI values whose values can be updated from other values
/// </summary>
public abstract class UpdatableIndiValue : IndiValue {
    /// <summary>
    /// Try to update this value from another value
    /// </summary>
    /// <param name="value">value to copy</param>
    /// <returns>true if update was successful</returns>
    public abstract bool TryUpdateValue(IndiValue value);
}

/// <summary>
/// Base class for INDI values that encapsulate a primitive type
/// </summary>
/// <typeparam name="T">primitive type</typeparam>
public abstract class IndiValue<T> : UpdatableIndiValue {
    /// <summary>
    /// Stored primitive type
    /// </summary>
    /// <value>value</value>
    public virtual T Value { get; set; }

    public override bool TryUpdateValue(IndiValue from) {
        if (from != null && from is IndiValue<T> valueType) {
            this.UpdateValue(valueType);
            return true;
        } else {
            return false;
        }
    }
    public virtual void UpdateValue(IndiValue<T> from) {
        this.Value = from.Value;
    }
}

/// <summary>
/// INDI text value
/// </summary>
public class IndiTextValue : IndiValue<string> {
    public override string IndiTypeName => "Text";
    internal override XElement CreateElement(string prefix, string subPrefix) {
        var node = new XElement(
            prefix + IndiTypeName, 
            new XText(this.Value ?? string.Empty)
        );
        if (this.Name != null) {
            node.Add(new XAttribute("name", this.Name));
        }
        if (this.Label != null) {
            node.Add(new XAttribute("label", this.Label));
        }
        return node;
    }

    public override string ToString() {
        return Value.ToString();
    }
    
}

/// <summary>
/// INDI numeric value
/// </summary>
public class IndiNumberValue : IndiValue<double> {
    public double Min;
    public double Max;
    public double Step;
    public override string IndiTypeName => "Number";
    internal override XElement CreateElement(string prefix, string subPrefix) {
        var node = new XElement(
            prefix + IndiTypeName, 
            new XAttribute("format", "%f"),
            new XAttribute("min", this.Min),
            new XAttribute("max", this.Max),
            new XAttribute("step", this.Step),
            new XText(this.Value.ToString())
        );
        if (this.Name != null) {
            node.Add(new XAttribute("name", this.Name));
        }
        if (this.Label != null) {
            node.Add(new XAttribute("label", this.Label));
        }
        return node;
    }

    public override string ToString() {
        return Value.ToString();
    }
}

/// <summary>
/// INDI switch 
/// </summary>
public class IndiSwitchValue : IndiValue<bool> {
    public string Switch;
    public bool IsOn => Value == true;
    public override string IndiTypeName => "Switch";
    internal override XElement CreateElement(string prefix, string subPrefix) {
        var node = new XElement(
            prefix + IndiTypeName, 
            new XText(this.IsOn ? "On": "Off")
        );
        if (this.Name != null) {
            node.Add(new XAttribute("name", this.Name));
        }
        if (this.Label != null) {
            node.Add(new XAttribute("label", this.Label));
        }
        return node;
    }

    public override string ToString() {
        return this.IsOn ? "On": "Off";
    }
}

/// <summary>
/// INDI light
/// </summary>
public class IndiLightValue : IndiValue {
    public override string IndiTypeName => "Light";
    internal override XElement CreateElement(string prefix, string subPrefix) {
        throw new NotImplementedException();
    }
}

/// <summary>
/// INDI BLOB value stored as a base64 string
/// </summary>
public class IndiBlobValue : IndiValue<string> {
    public byte[] Blob => Convert.FromBase64String(this.Value);
    public override string IndiTypeName => "BLOB";
    public IndiBlobValue() {}
    public IndiBlobValue(FileStream fs) {
        using (BinaryReader reader = new BinaryReader(fs)) {
            byte[] blob = reader.ReadBytes((int)fs.Length);
            this.Value = Convert.ToBase64String(blob);
        }
    }

    /// <summary>
    /// Create a file from this blob data's binary data
    /// </summary>
    /// <param name="path">path of file</param>
    public void WriteBlobToFile(string path) {
        if (!string.IsNullOrEmpty(this.Value)) {
            using (var filestream = File.Open(path, FileMode.Create))
            using (var writer = new BinaryWriter(filestream)) {
                writer.Write(this.Blob);
            }
        }
    }

    internal override XElement CreateElement(string prefix, string subPrefix) {
        var node = new XElement(
            prefix + IndiTypeName, 
            new XText(Value ?? string.Empty)
        );
        if (this.Name != null) {
            node.Add(new XAttribute("name", this.Name));
        }
        if (this.Label != null) {
            node.Add(new XAttribute("label", this.Label));
        }
        return node;
    }

    public override string ToString() {
        return "[BLOB]";
    }
}

/// <summary>
/// Vector containing other INDI values
/// </summary>
/// <typeparam name="T">INDI value type</typeparam>
public class IndiVector<T> : UpdatableIndiValue, IList<T> where T:IndiValue {
    /// <summary>
    /// Group associated with the vector
    /// </summary>
    public string Group;
    /// <summary>
    /// State of the vector
    /// </summary>
    public string State;
    /// <summary>
    /// Permissions for this vector
    /// </summary>
    public string Permissions = "rw";
    /// <summary>
    /// Check if this vector is read only
    /// </summary>
    public bool IsReadOnly => Permissions == "r";
    /// <summary>
    /// Check if this vector is readable and writeable
    /// </summary>
    public bool IsReadWrite => Permissions == "rw";
    /// <summary>
    /// Check if this vector is writable
    /// </summary>
    public bool IsWritable => Permissions.Contains("w");
    /// <summary>
    /// Rule associated with this vector
    /// </summary>
    public string Rule;
    /// <summary>
    /// Timeout on this vector's data
    /// </summary>
    public string Timeout;
    /// <summary>
    /// Last timestamp
    /// </summary>
    public string Timestamp;
    /// <summary>
    /// Vector comments
    /// </summary>
    public string Comment;

    private List<T> vector = new List<T>();

    /// <summary>
    /// Get a value from the vector
    /// </summary>
    /// <value>INDI value</value>
    public T this[int index] { 
        get => vector[index]; 
        set { if(IsWritable) { vector[index] = value; } }
    }   

    /// <summary>
    /// Number of elements in the vector
    /// </summary>
    public int Count => vector.Count;

    /// <summary>
    /// Create an empty vector
    /// </summary>
    public IndiVector () {}
    /// <summary>
    /// Create an empty vector with the given property name
    /// </summary>
    /// <param name="name">property name</param>
    public IndiVector (string name) {
        this.Name = name;
    }

    /// <summary>
    /// Get value in vector with the given identifier
    /// </summary>
    /// <param name="name">name of element</param>
    /// <returns>element or null</returns>
    public T GetItemWithName(string name) {
        return this.vector.Where(value => value.Name == name).FirstOrDefault();
    }

    /// <summary>
    /// add a value to the vector
    /// </summary>
    /// <param name="item">value to add</param>
    public void Add(T item) {
        vector.Add(item);
    }

    /// <summary>
    /// Clear all values
    /// </summary>
    public void Clear() {
        vector.Clear();
    }

    /// <summary>
    /// Check if vector contains the given value
    /// </summary>
    /// <param name="item">item to check</param>
    /// <returns>true if vector contains the given value</returns>
    public bool Contains(T item) {
        return vector.Contains(item);
    }

    /// <summary>
    /// Copy vector values to array
    /// </summary>
    /// <param name="array">array to copy to</param>
    /// <param name="arrayIndex">index to copy at</param>
    public void CopyTo(T[] array, int arrayIndex) {
        vector.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Get enumerator for this vector
    /// </summary>
    /// <returns>vector enumerator</returns>
    public IEnumerator<T> GetEnumerator() {
        return vector.GetEnumerator();
    }

    /// <summary>
    /// Get the index of an item in this vector
    /// </summary>
    /// <param name="item">item</param>
    /// <returns>index of item if it exists in the vector -1 otherwise</returns>
    public int IndexOf(T item) {
        return vector.IndexOf(item);
    }

    /// <summary>
    /// Insert an item into this vector at the given index
    /// </summary>
    /// <param name="index">index to insert at</param>
    /// <param name="item">item to insert</param>
    public void Insert(int index, T item) {
        vector.Insert(index, item);
    }

    /// <summary>
    /// Remove an item from the vector
    /// </summary>
    /// <param name="item">item to remove</param>
    /// <returns>true if item was removed successfully</returns>
    public bool Remove(T item) {
        return vector.Remove(item);
    }

    /// <summary>
    /// Remove an item at the given position
    /// </summary>
    /// <param name="index">index to remove</param>
    public void RemoveAt(int index) {
        vector.RemoveAt(index);
    }

    /// <summary>
    /// Get enumerator for this vector
    /// </summary>
    /// <returns>vector enumerator</returns>
    IEnumerator IEnumerable.GetEnumerator() {
        return vector.GetEnumerator();
    }

    public override string IndiTypeName => this.vector[0].IndiTypeName + "Vector"; // Only works if there is at least 1 element
    internal override XElement CreateElement(string prefix, string subPrefix) {
        var parent = new XElement(
            prefix + IndiTypeName
        );
        if (!string.IsNullOrEmpty(this.Name))
            parent.Add(new XAttribute("name", this.Name));
        if (!string.IsNullOrEmpty(this.Label))
            parent.Add(new XAttribute("label", this.Label));
        if (!string.IsNullOrEmpty(this.Group))
            parent.Add(new XAttribute("group", this.Group));
        // Don't need these for setting
        if (!string.IsNullOrEmpty(this.State))
            parent.Add(new XAttribute("state", this.State));
        if (!string.IsNullOrEmpty(this.Permissions))
            parent.Add(new XAttribute("perm", this.Permissions));
        if (!string.IsNullOrEmpty(this.Rule))
            parent.Add(new XAttribute("rule", this.Rule));

        foreach (var child in this.vector) {
            parent.Add(child.CreateElement(subPrefix, subPrefix));
        }
        return parent;
    }

    /// <summary>
    /// Try to update the values of this vector from another vector
    /// </summary>
    /// <param name="value">indi values to draw from</param>
    /// <returns>true if indi value is a compatible type vector</returns>
    public override bool TryUpdateValue(IndiValue value) {
        if (value != null && value is IndiVector<T> vec) {
            var updates = vec.vector;
            List<T> newVector = new List<T>(vector.Count);
            foreach (var update in updates) {
                // Access existing property
                var existingProperty = vector.Where(prop => prop.Name == update.Name).FirstOrDefault();
                if (existingProperty != null && existingProperty is UpdatableIndiValue updatableProperty) {
                    if (updatableProperty.TryUpdateValue(update)) {
                        // Updated existing property, add back to the list
                        newVector.Add(existingProperty);
                    } else {
                        // Failed to update existing property, add the new value as raw
                        newVector.Add(update);
                    }
                } else {
                    // No previous property, add new value as raw
                    newVector.Add(update);
                }
            }
            this.vector = newVector;
            return true;
        } else {
            return false;
        }
    }
}

/// <summary>
/// Extention methods for different vector types
/// </summary>
public static class IndiVectorExtentions {
    /// <summary>
    /// Get the first enabled value in this switch vector
    /// </summary>
    /// <param name="options">vector of options</param>
    /// <returns>switch value</returns>
    public static IndiSwitchValue GetFirstEnabledSwitch(this IndiVector<IndiSwitchValue> options) {
        foreach (var option in options) {
            if (option.Value == true) {
                return option;
            }
        } 
        return null;
    }
    /// <summary>
    /// Enable a specific switch
    /// </summary>
    /// <param name="options">list of possible switch values</param>
    /// <param name="name">name of the option to enable</param>
    public static void SwitchTo(this IndiVector<IndiSwitchValue> options, string name) {
        foreach (var option in options) {
            option.Value = option.Name == name;
        }
    }
    /// <summary>
    /// Enable a specific switch
    /// </summary>
    /// <param name="options">list of possible switch values</param>
    /// <param name="option">index of the option to enable</param>
    public static void SwitchTo(this IndiVector<IndiSwitchValue> options, int option) {
        for(var i = 0; i < options.Count; i++) {
            options[i].Value = i == option;
        }
    }

    /// <summary>
    /// Enable a specific switch
    /// </summary>
    /// <param name="options">list of possible switch values</param>
    /// <param name="selector">function to select switch</param>
    public static void SwitchTo(this IndiVector<IndiSwitchValue> options, Func<IndiSwitchValue, bool> selector) {
        foreach (var toggle in options) {
            toggle.Value = selector(toggle);
        }
    }

    /// <summary>
    /// Get the first switch with the given name from the vector
    /// </summary>
    /// <param name="options">list of possible switch values</param>
    /// <param name="name">name of the switch</param>
    /// <returns>switch</returns>
    public static IndiSwitchValue GetSwitch (this IndiVector<IndiSwitchValue> options, string name) {
        return options.Where(opt => opt.Name == name).First();
    }
}

}