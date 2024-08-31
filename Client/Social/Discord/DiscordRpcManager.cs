using DiscordRPC;

namespace Foxel.Client.Social.Discord; 

public static class DiscordRpcManager {
    private static readonly DiscordRpcClient Client = new("1077824162195853353");

    public static void Initialize() {
        Client.Initialize();
    }

    public static void UpdateStatus(string state, string details)
        => Client.SetPresence(new() {
            Details = details,
            State = state,
        });
}
