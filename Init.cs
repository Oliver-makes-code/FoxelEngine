using NLog;
using Voxel.Client;
using Voxel.Common;
using Voxel.Test;

namespace Voxel;

public class Init {
    #if TEST
    public static readonly Platform platform = Platform.Test;
    #elif CLIENT
    public static readonly Platform platform = Platform.Client;
    #else
    public static readonly Platform platform = Platform.Server;
    #endif

    public static void Main(string[] args) {
        ConfigurateLogging();
        LogUtil.PlatformLogger.Info("Starting up..");

        if (platform == Platform.Client) {
            new VoxelClient();
        } else if (platform == Platform.Test) {
            TestRegistry.RunTests();
        } else {
            Console.WriteLine("TODO!");
        }
    }

    public static void ConfigurateLogging() {
        LogManager.Setup().LoadConfiguration(builder => {
            var layout = "(${date:format=yyyy.MM.dd hh\\:mmt:lowercase=true}) [${logger}] ${level} - ${message}";
            builder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteToConsole(layout: layout);
            builder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteToFile(fileName: "latest.log", layout: layout);
        });
    }
}

public enum Platform {
    Server,
    Client,
    Test
}
