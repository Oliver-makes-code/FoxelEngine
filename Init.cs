using System;
using System.Threading;
using NLog;
using Voxel.Client;
using Voxel.Common;
using Voxel.Common.World;
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

        switch (platform) {
            case Platform.Client:
                _ = new VoxelClient();
                break;
            case Platform.Test:
                TestRegistry.RunTests();
                break;
            default:
                Console.WriteLine("TODO!");
                break;
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
