namespace Foxel.Common.Util;

public static class Constants {
    public const int TicksPerSecond = 20;
    public const double SecondsPerTick = 1d / TicksPerSecond;


    /// <summary>
    /// The number of tiles per second per second that entities accelerate downwards at.
    ///
    /// f.e. gain 16 tps velocity per second
    /// </summary>
    public const double Gravity = 32;
    public const double GravityPerTick = Gravity * SecondsPerTick;
}
