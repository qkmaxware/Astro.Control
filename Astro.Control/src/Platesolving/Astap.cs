using System;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;
using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Platesolving {

/// <summary>
/// Wrapper for the ASTAP (Astrometric Stacking Program) platesolving functions
/// http://www.hnsky.org/astap.htm#command_line
/// </summary>
public class Astap : BaseProgramWrapper, IPlateSolver {
    private string AstapPath;

    /// <summary>
    /// Create a reference to the ASTAP program if it is accessible on the current PATH
    /// </summary>
    public Astap() : this ("astap") {}
    /// <summary>
    /// Create a reference to the ASTAP program installed at the given location
    /// </summary>
    public Astap(string pathToExe) {
        this.AstapPath = pathToExe;
    }

    /// <summary>
    /// Check if ASTAP is installed an accessible on the host machine
    /// </summary>
    /// <returns>true if the ASTAP can be invoked</returns>
    public override bool IsInstalled() {
        // Check if we are using the full path or are in the dir with the app already
        if (File.Exists(this.AstapPath))
            return true;

        // Check the path by combining the exe name with all path elements
        var values = Environment.GetEnvironmentVariable("PATH");
        foreach (var path in values.Split(Path.PathSeparator)) {
            var fullPath = Path.Combine(path, this.AstapPath);
            if (File.Exists(fullPath))
                return true;
        }
        return false;
    }

    public bool TryBlindSolve(string pathToImage, out IPlateSolvingResult results) {
        string stdout, stderr;

        var code = this.TryExecuteCommand(
            ".", 
            this.AstapPath, 
            new string[]{
                // -f	file_name                   File to solve astrometric
                "-f",
                pathToImage,
                // -r	radius_search_field	degrees	The program will search in a square spiral around the start position up to this radius *
                "-r",
                180.ToString(),
            }, 
            out stdout, 
            out stderr
        );

        results = AstapResults.FromAstapOutput(code, pathToImage);
        return results.WasPlateSolvingSuccessful;
    }

    public bool TrySolve(string pathToImage, Angle approxRa, Angle approxDec, Angle searchRadius, out IPlateSolvingResult results) {
        string stdout, stderr;

        var code = this.TryExecuteCommand(
            ".", 
            this.AstapPath, 
            new string[]{
                // -f	file_name                   File to solve astrometric
                "-f",
                pathToImage,
                // -r	radius_search_field	degrees	The program will search in a square spiral around the start position up to this radius *
                "-r",
                searchRadius.TotalDegrees().ToString(),

            }, 
            out stdout, 
            out stderr
        );

        results = AstapResults.FromAstapOutput(code, pathToImage);
        return results.WasPlateSolvingSuccessful;
    }
}

/// <summary>
/// Plate solving results from ASTAP
/// </summary>
public class AstapResults : IPlateSolvingResult {

    private static Regex fitsHeaderRegex = new Regex(@"\s*(?<var>\w+)\s*=\s*(?<value>[^\n\/]+)\s*(?<comment>\/[^\n]+)?");

    public bool WasPlateSolvingSuccessful {get; private set;}
    public Exception PlateSolvingError {get; private set;}
    public Point ReferencePixelCoordinates {get; private set;}
    public Angle RightAscension {get; private set;}
    public Angle Declination {get; private set;}
    public Angle TwistX {get; private set;}
    public Angle TwistY {get; private set;}

    private static double doubleOrDefault(string s) {
        s = s.Trim();
        double d = 0;
        double.TryParse(s, out d);
        return d;
    }

    private static int intOrDefault(string s) {
        s = s.Trim();
        int d = 0;
        int.TryParse(s, out d);
        return d;
    }

    public static AstapResults FromAstapOutput(int outputCode, string pathToImage) {
        if (outputCode == 0) {
            // No error
            return ParseFromIni(Path.ChangeExtension(pathToImage, ".ini"));
        } else {
            // Had error, decode it
            return FromAstapErrorCode(outputCode);
        }
    }

    protected static AstapResults FromAstapErrorCode(int code) {
        return new AstapResults {
            WasPlateSolvingSuccessful = false,
            PlateSolvingError = code switch {
                1  => new NoSolutionException(), // No solution
                2  => new NotEnoughStarsException(), // Not enough stars
                16 => new ImageReadingException(), // Error reading image file
                32 => new NoStarDatabaseException(), // No star database found
                33 => new StarDatabaseReadingException(), // Error reading star database
                _ => new Exception()
            }
        };
    }

    protected static AstapResults ParseFromIni(string pathToIni) {
        AstapResults results = new AstapResults {
            WasPlateSolvingSuccessful = false,
            PlateSolvingError = new FileNotFoundException()
        };

        if (!File.Exists(pathToIni) || Path.GetExtension(pathToIni) != ".ini")
            return results;

        using var reader = new StreamReader(pathToIni);

        string line;
        while ((line = reader.ReadLine()) != null) {
            var data = fitsHeaderRegex.Match(line);
            if (!data.Success)
                continue;
            
            switch(data.Groups["var"].Value) {
                // ASTAP internal solver success?
                case "PLTSOLVD": {
                    if (data.Groups["value"].Value.Trim().ToLower() == "t") {
                        results.WasPlateSolvingSuccessful = true;
                    }
                } break;
                // RA (J2000) of the reference pixel [degrees]
                case "CRVAL1": {
                    var degrees = doubleOrDefault(data.Groups["value"].Value);
                    results.RightAscension = Angle.Degrees(degrees);
                } break;
                // DEC (J2000) of the reference pixel [in degrees]
                case "CRVAL2": {
                    var degrees = doubleOrDefault(data.Groups["value"].Value);
                    results.Declination = Angle.Degrees(degrees);
                } break;
                // X of the reference & centre pixel
                case "CRPIX1": {
                    var pix = intOrDefault(data.Groups["value"].Value);
                    results.ReferencePixelCoordinates = new Point(pix, results.ReferencePixelCoordinates.Y);
                } break;
                // Y of the reference & centre pixel
                case "CRPIX2": {
                    var pix = intOrDefault(data.Groups["value"].Value);
                    results.ReferencePixelCoordinates = new Point(results.ReferencePixelCoordinates.X, pix);
                } break;
                // Image twist of X axis [degrees]
                case "CROTA1": {
                    var degrees = doubleOrDefault(data.Groups["value"].Value);
                    results.TwistX = Angle.Degrees(degrees);
                } break;
                // Image twist of Y axis [degrees]
                case "CROTA2": {
                    var degrees = doubleOrDefault(data.Groups["value"].Value);
                    results.TwistY = Angle.Degrees(degrees);
                } break;
            };
        }

        return results;
    }

}



}