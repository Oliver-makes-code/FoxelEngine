namespace Foxel.Core.Util;

public static class AnsiExtensions {
    public static string Ansi(this string text, int code)
        => $"\u001b[{code}m{text}\u001b[0m";
    
    public static string Ansi(this string text, AnsiCode code)
        => Ansi(text, (int)code);

    public static string Black(this string text)
        => text.Ansi(AnsiCode.Black);

    public static string Red(this string text)
        => text.Ansi(AnsiCode.Red);

    public static string Green(this string text)
        => text.Ansi(AnsiCode.Green);

    public static string Yellow(this string text)
        => text.Ansi(AnsiCode.Yellow);

    public static string Blue(this string text)
        => text.Ansi(AnsiCode.Blue);

    public static string Magenta(this string text)
        => text.Ansi(AnsiCode.Magenta);

    public static string Cyan(this string text)
        => text.Ansi(AnsiCode.Cyan);

    public static string White(this string text)
        => text.Ansi(AnsiCode.White);

    public static string BackgroundBlack(this string text)
        => text.Ansi(AnsiCode.BackgroundBlack);

    public static string BackgroundRed(this string text)
        => text.Ansi(AnsiCode.BackgroundRed);

    public static string BackgroundGreen(this string text)
        => text.Ansi(AnsiCode.BackgroundGreen);

    public static string BackgroundYellow(this string text)
        => text.Ansi(AnsiCode.BackgroundYellow);

    public static string BackgroundBlue(this string text)
        => text.Ansi(AnsiCode.BackgroundBlue);

    public static string BackgroundMagenta(this string text)
        => text.Ansi(AnsiCode.BackgroundMagenta);

    public static string BackgroundCyan(this string text)
        => text.Ansi(AnsiCode.BackgroundCyan);

    public static string BackgroundWhite(this string text)
        => text.Ansi(AnsiCode.BackgroundWhite);
}

public enum AnsiCode {
    Reset = 0,

    Bold = 1,
    Dim = 2,
    Italic = 3,
    Underline = 4,
    Blink = 5,

    Inverse = 7,
    Hidden = 8,
    Strikethrough = 9,

    Black = 30,
    Red = 31,
    Green = 32,
    Yellow = 33,
    Blue = 34,
    Magenta = 35,
    Cyan = 36,
    White = 37,
    Default = 39,

    BackgroundBlack = 40,
    BackgroundRed = 41,
    BackgroundGreen = 42,
    BackgroundYellow = 43,
    BackgroundBlue = 44,
    BackgroundMagenta = 45,
    BackgroundCyan = 46,
    BackgroundWhite = 47,
    BackgroundDefault = 49,
}
