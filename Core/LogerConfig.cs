using NLog;
using Voxel.Core.Util;

namespace Voxel.Core;

public static class LoggerConfig {
    public const string LogFile = "log.txt";

    public static void Init() {
        string[] layout = [
            "[${date:format=HH\\:mm\\:ss}]",
            "[${logger}/${level:uppercase=true}]",
            "${message:withexception=true}"
        ];
        
        if (File.Exists(LogFile))
            File.Delete(LogFile);

        LogManager.Setup().LoadConfiguration(builder => {
            builder.ForLogger()
                .FilterLevel(LogLevel.Debug)
                .WriteToColoredConsole(
                    layout: Join(layout, AnsiCode.Blue, AnsiCode.Green),
                    enableAnsiOutput: true,
                    highlightWordLevel: true
                );
            builder.ForLogger()
                .FilterLevel(LogLevel.Info)
                .WriteToColoredConsole(
                    layout: Join(layout, AnsiCode.Blue, AnsiCode.Green),
                    enableAnsiOutput: true,
                    highlightWordLevel: true
                );
            builder.ForLogger()
                .FilterLevel(LogLevel.Warn)
                .WriteToColoredConsole(
                    layout: Join(layout, AnsiCode.Blue, AnsiCode.Yellow),
                    enableAnsiOutput: true,
                    highlightWordLevel: true
                );
            builder.ForLogger()
                .FilterLevel(LogLevel.Error)
                .WriteToColoredConsole(
                    layout: Join(layout, AnsiCode.Blue, AnsiCode.Red),
                    enableAnsiOutput: true,
                    highlightWordLevel: true
                );
            builder.ForLogger()
                .FilterLevel(LogLevel.Fatal)
                .WriteToColoredConsole(
                    layout: Join(layout, AnsiCode.Blue, AnsiCode.Red, AnsiCode.Red),
                    enableAnsiOutput: true,
                    highlightWordLevel: true
                );
            builder.ForLogger()
                .FilterMinLevel(LogLevel.Debug)
                .WriteToFile(
                    fileName: LogFile,
                    layout: Join(layout)
                );
        });
    }

    private static string Join(string[] input, params AnsiCode[] colors) {
        var output = "";

        for (int i = 0; i < input.Length; i++) {
            if (colors.Length <= i)
                output += input[i] + " ";
            else
                output += input[i].Ansi(colors[i]) + " ";
        }

        return output;
    }
    
}