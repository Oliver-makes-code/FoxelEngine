namespace Foxel.Common.Util;

/// Represents an easing function.
/// The input is the progress of the animation, 0 is started, 1 is finishes
/// The output is the actual distance moved, 0 is the starting position, 1 is the ending position
public delegate double EasingFunction(double time);

public static class EasingFunctions {
    const double Back1 = 1.70158;
    const double Back2 = Back1 * 1.525;
    const double Back3 = Back1 + 1;

    public static double Linear(double time)
        => time;
    
    public static double InBack(double time)
        => Back3 * Math.Pow(time, 3) - Back1 * Math.Pow(time, 2);

    public static double OutBack(double time)
        => 1 + Back3 * Math.Pow(time - 1, 3) + Back1 * Math.Pow(time - 1, 2);

    public static double InOutBack(double time)
        => time < 0.5
        ? (Math.Pow(2 * time, 2) * ((Back2 + 1) * 2 * time - Back2)) / 2
        : (Math.Pow(2 * time - 2, 2) * ((Back2 + 1) * (time * 2 - 2) + Back2) + 2) / 2;
}
