using System;
using System.Text.RegularExpressions;
using System.Drawing;
using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Platesolving {

/// <summary>
/// The results of running plate solving against an input image
/// </summary>
public interface IPlateSolvingResult {
    bool WasPlateSolvingSuccessful {get;}
    public Exception PlateSolvingError {get;}

    public Angle RightAscension {get;}
    public Angle Declination {get;}
}   

}
