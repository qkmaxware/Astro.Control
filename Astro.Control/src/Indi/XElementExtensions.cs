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

namespace Qkmaxware.Astro.Control {

/// <summary>
/// Extensions to make dealing with XElements easier
/// </summary>
public static class XElementExtensions {
    /// <summary>
    /// Add a attribute to an xml element, or update it if it exists
    /// </summary>
    /// <param name="element">element to modify</param>
    /// <param name="attribute">attribute name</param>
    /// <param name="value">new attribute value</param>
    public static void AddOrUpdateAttribute(this XElement element, string attribute, string value) {
        var attr = element.Attribute(attribute);
        if (attr == null) {
            element.Add(new XAttribute(attribute, value ?? string.Empty));
        } else {
            attr.Value = value;
        }
    }
}

}