using NLog;
using Voxel.Client;
using Voxel.Common;
using Voxel.Test;

namespace Voxel;

public class Init {
	#if CLIENT
	public static readonly bool IsClient = true;
	#else
	public static readonly bool IsClient = false;
	#endif

	public static void Main(string[] args) {
		ConfigurateLogging();
		LogUtil.PlatformLogger.Info("Starting up..");

        TestRegistry.RunTests();

		if (IsClient) {
			new VoxelClient();
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
