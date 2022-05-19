using BepInEx.Configuration;

namespace StulPlugin
{
    public static class PluginConfig
    {
        public static ConfigEntry<string> StrimertulAddress;
        public static ConfigEntry<string> AuthKey;
        
        public static ConfigEntry<int> RaidChaosDuration;
        public static ConfigEntry<int> RaidChaosItemsPerMessage;

        public static void Initialize(ConfigFile config)
        {
            config.SaveOnConfigSet = true;

            StrimertulAddress = config.Bind(
                "Strimertul",
                "Address",
                "ws://localhost:4337/ws",
                "Strimertul websocket server address"
            );
            AuthKey = config.Bind(
                "Strimertul",
                "AuthKey",
                "",
                "Strimertul websocket server auth key (empty if no auth is present)"
            );
            
            RaidChaosDuration = config.Bind(
                "Gimmicks",
                "RaidChaosDuration",
                60,
                "Duration of the raid chaos events in seconds"
            );
            RaidChaosItemsPerMessage = config.Bind(
                "Gimmicks",
                "RaidChaosItemsPerMessage",
                3,
                "How many items to throw for each chat message"
            );
                
        }
    }
}