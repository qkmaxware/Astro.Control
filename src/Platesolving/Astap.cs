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
        string stdout, stderr;
        return this.TryExecuteCommand(".", this.AstapPath, null, out stdout, out stderr);
    }

    private bool parseAstapOutput(string output, out double ra, out double dec) {
        throw new System.NotImplementedException();
    }

    public bool TryBlindSolve(string pathToImage, out double trueRa, out double trueDec) {
        string stdout, stderr;
        trueRa = 0;
        trueDec = 0;

        bool success = this.TryExecuteCommand(
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

        return success && parseAstapOutput(stdout, out trueRa, out trueDec);
    }

    public bool TrySolve(string pathToImage, double approxRa, double approxDec, double searchRadiusDegrees, out double trueRa, out double trueDec) {
        string stdout, stderr;
        trueRa = 0;
        trueDec = 0;

        bool success = this.TryExecuteCommand(
            ".", 
            this.AstapPath, 
            new string[]{
                // -f	file_name                   File to solve astrometric
                "-f",
                pathToImage,
                // -r	radius_search_field	degrees	The program will search in a square spiral around the start position up to this radius *
                "-r",
                searchRadiusDegrees.ToString(),

            }, 
            out stdout, 
            out stderr
        );

        return success && parseAstapOutput(stdout, out trueRa, out trueDec);
    }
}

}