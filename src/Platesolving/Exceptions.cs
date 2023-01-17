using System;
using System.Text.RegularExpressions;
using System.Drawing;
using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Platesolving {

public class PlateSolvingException : System.Exception {}
public class NoSolutionException : PlateSolvingException {}
public class NotEnoughStarsException : PlateSolvingException {}
public class ImageReadingException : PlateSolvingException {}
public class NoStarDatabaseException : PlateSolvingException {}
public class StarDatabaseReadingException : PlateSolvingException {}


}