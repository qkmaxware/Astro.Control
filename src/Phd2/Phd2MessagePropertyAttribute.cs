using System;
using System.Linq;
using System.Numerics;
using System.Text.Json;

namespace Qkmaxware.Astro.Control {

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class Phd2MessagePropertyAttribute : System.Attribute {
    public string PropertyName {get; set;}
    public Phd2MessagePropertyAttribute(string propertyName) {
        this.PropertyName = propertyName;
    }   
}

}
