using NLog;
using Voxel.Client;
using Voxel.Common;

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
		if (IsClient) {
			new VoxelClient();
		} else {
			Console.WriteLine("TODO!");
		}
	}

	public static void ConfigurateLogging() {
		LogManager.Setup().LoadConfiguration(builder => {
			builder.ForLogger().FilterMinLevel(LogLevel.Info).WriteToConsole(
				layout: "(${date:format=yyyy.MM.dd hh\\:mmt:lowercase=true}) [${logger}] ${level} - ${message}"
			);
			builder.ForLogger().FilterMinLevel(LogLevel.Info).WriteToFile(fileName: "latest.log");
		});
	}
}
