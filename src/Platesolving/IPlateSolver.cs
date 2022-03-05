namespace Qkmaxware.Astro.Control.Platesolving {

/// <summary>
/// Interface for platesolving applications
/// </summary>
public interface IPlateSolver {
    /// <summary>
    /// Attempt to platesolve an image using and initial guess 
    /// </summary>
    /// <param name="pathToImage">file path to the image to be platesolved</param>
    /// <param name="approxRa">initial approximation of the right ascension at the center of the image</param>
    /// <param name="approxDec">initial approximation of the right ascension at the center of the image</param>
    /// <param name="searchRadiusDegrees">radius around the approximate location to search for the true location</param>
    /// <param name="trueRa">true value of the right ascension at the center of the image</param>
    /// <param name="trueDec">true value of the right ascension at the center of the image</param>
    /// <returns>true if platesolving was successful, false otherwise</returns>
    bool TrySolve(string pathToImage, double approxRa, double approxDec, double searchRadiusDegrees, out double trueRa, out double trueDec);
    /// <summary>
    /// Attempt to platesolve an image without having an initial idea of the image's location
    /// </summary>
    /// <param name="pathToImage">file path to the image to be platesolved</param>
    /// <param name="trueRa">true value of the right ascension at the center of the image</param>
    /// <param name="trueDec">true value of the right ascension at the center of the image</param>
    /// <returns>true if platesolving was successful, false otherwise</returns>
    bool TryBlindSolve(string pathToImage, out double trueRa, out double trueDec);
}

}