using BepInEx.Configuration;

namespace StulPlugin
{
    public static class PluginConfig
    {
        public static ConfigEntry<string> StrimertulAddress;
        public static ConfigEntry<string> AuthKey;

        public static ConfigEntry<int> RaidChaosDuration;
        public static ConfigEntry<int> RaidChaosItemsPerMessage;

        public static ConfigEntry<string> CostumeSwapRedeemID;
        public static ConfigEntry<int> CostumeSwapDuration;

        public static ConfigEntry<string> DyeHairRedeemID;
        public static ConfigEntry<int> DyeHairDuration;

        public static ConfigEntry<string> RGBHairRedeemID;
        public static ConfigEntry<int> RGBHairDuration;
        public static ConfigEntry<float> RGBHairSpeed;
        public static ConfigEntry<float> RGBHairDelay;

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

            CostumeSwapRedeemID = config.Bind(
                "Redeems",
                "CostumeSwapRedeemID",
                "b7fd0393-7618-4413-b87c-b4565a596911",
                "Redeem ID for the costume swap");
            CostumeSwapDuration = config.Bind(
                "Redeems",
                "CostumeSwapDuration",
                900, // 15 minutes
                "Duration for costume swap redeems (doesn't apply to hotkeys)");

            DyeHairRedeemID = config.Bind(
                "Redeems",
                "DyeHairRedeemID",
                "7cf56b5d-6467-43f4-b855-9f1a973b3231",
                "Redeem ID for hair dye");
            DyeHairDuration = config.Bind(
                "Redeems",
                "DyeHairDuration",
                600, // 10 minutes
                "Hair dye redeem duration in seconds");

            RGBHairRedeemID = config.Bind(
                "Redeems",
                "RGBHairRedeemID",
                "010bc23e-3590-45c3-84e7-a6c4ebbea5df",
                "Redeem ID for the gamer hair");
            RGBHairDuration = config.Bind(
                "Redeems",
                "RGBHairDuration",
                300, // 5 minutes
                "Gamer hair redeem duration in seconds");
            RGBHairSpeed = config.Bind(
                "Redeems",
                "RGBHairSpeed",
                0.4f,
                "How many full RGB cycles per second");
            RGBHairDelay = config.Bind(
                "Redeems",
                "RGBHairDelay",
                0.007f,
                "How much hue difference per artmesh step (higher = bigger gradient)");
        }
    }
}