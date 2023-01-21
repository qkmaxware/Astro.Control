using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Platesolving {

/// <summary>
/// Interface for platesolving applications
/// </summary>
public interface IPlateSolver {
    /// <summary>
    /// Attempt to plate-solve an image using and initial guess 
    /// </summary>
    /// <param name="pathToImage">file path to the image to be platesolved</param>
    /// <param name="approxRa">initial approximation of the right ascension at the center of the image</param>
    /// <param name="approxDec">initial approximation of the right ascension at the center of the image</param>
    /// <param name="searchRadius">radius around the approximate location to search for the true location</param>
    /// <param name="results">The results of plate solving</param>
    /// <returns>true if plate-solving was successful, false otherwise</returns>
    bool TrySolve(string pathToImage, Angle approxRa, Angle approxDec, Angle searchRadius, out IPlateSolvingResult results);
    /// <summary>
    /// Attempt to plate-solve an image without having an initial idea of the image's location
    /// </summary>
    /// <param name="pathToImage">file path to the image to be platesolved</param>
    /// <param name="results">The results of plate solving</param>
    /// <returns>true if plate-solving was successful, false otherwise</returns>
    bool TryBlindSolve(string pathToImage, out IPlateSolvingResult results);
}

}