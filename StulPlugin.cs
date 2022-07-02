using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BepInEx;
using Trashy;
using UnityEngine;

namespace StulPlugin
{
    [BepInPlugin("StulPlugin", "Strimertul integration for VTS", Version)]
    public class StulPlugin : BaseUnityPlugin
    {
        public const string Version = "0.1.0";

        private KilovoltClient _kv;
        private AnimatedEffects _animated;
        private readonly TwitchEventSource _twitch = new TwitchEventSource();
        private ItemSpawner _trashySpawner;
        private readonly CostumeManager _costumeManager = new CostumeManager();

        private float dyeEndTime;

        public StulPlugin()
        {
            // Setup configuration
            PluginConfig.Initialize(Config);

            _kv = gameObject.AddComponent<KilovoltClient>();
            _animated = gameObject.AddComponent<AnimatedEffects>();
        }

        private void Start()
        {
            StartCoroutine(ConnectKV());

            SetupHairColors();
            SetupRaidChaos();
            SetupCostumeSwap();
        }

        private void SetupHairColors()
        {
            var hairmeshes = new[]
            {
                "eyebrow_l", "eyebrow_r", "bang_l", "bang_r", "bang_center", "front_hair_l", "front_hair_r",
                "hair_cover", "hair_back"
            };
            var colors = new Dictionary<string, string>()
            {
                ["watermelon"] = "F0ACFF|000000",
                ["teal"] = "33B2FF|04050E",
                ["tart"] = "E45F7F|000000",
                ["grape"] = "8B55D6|000000",
                ["grass"] = "6CDC3B|131C0F",
                ["mint"] = "71FFE3|000000",
                ["_DONTUSE"] = "FF0000|000000"
            };
            var hairTints = new Dictionary<string, TintGroup>();
            foreach (var color in colors)
            {
                var artMeshes = hairmeshes.Select(mesh => new StringStringTuple(mesh, color.Value)).ToList();
                hairTints.Add(color.Key, new TintGroup(artMeshes));
            }

            var gamerTint = new AnimatedTintGroup(hairTints["_DONTUSE"].ArtMeshes, PluginConfig.RGBHairSpeed.Value,
                -PluginConfig.RGBHairDelay.Value);
            _animated.tintGroups.Add(gamerTint);


            Action checkClearHair = () =>
            {
                if (Time.time >= dyeEndTime)
                {
                    hairTints["_DONTUSE"].Clear();
                    gamerTint.Active = false;
                }
            };

            _twitch.OnRedeem += redeem =>
            {
                Log.Info($"REDEEMED {redeem.eventData.reward.id}");
                if (redeem.eventData.reward.id == PluginConfig.RGBHairRedeemID.Value)
                {
                    gamerTint.Active = true;
                    dyeEndTime = Time.time + PluginConfig.RGBHairDuration.Value;
                    TimerUtils.RunAfter(PluginConfig.RGBHairDuration.Value * 1000, checkClearHair);
                }

                if (redeem.eventData.reward.id == PluginConfig.DyeHairRedeemID.Value)
                {
                    var color = redeem.eventData.user_input.ToLower().Trim();
                    if (hairTints.ContainsKey(color))
                    {
                        hairTints[color].Apply();
                    }
                    else
                    {
                        //TODO Refund!!
                    }

                    dyeEndTime = Time.time + PluginConfig.DyeHairDuration.Value;
                    TimerUtils.RunAfter(PluginConfig.DyeHairDuration.Value * 1000, checkClearHair);
                }
            };
            _twitch.OnChatMessage += message =>
            {
                if (hairTints.ContainsKey(message.Message))
                {
                    hairTints[message.Message].Apply();
                }
                else
                {
                    // Any is fine
                    hairTints["mint"].Clear();
                }

                gamerTint.Active = message.Message == "gamer";
            };
        }

        private void SetupCostumeSwap()
        {
            _twitch.OnRedeem += (redeem) =>
            {
                if (redeem.eventData.reward.id == PluginConfig.CostumeSwapRedeemID.Value)
                {
                    CostumeManager.Costume costume;
                    switch (redeem.eventData.user_input.ToLower().Trim())
                    {
                        case "casual":
                            costume = CostumeManager.Costume.Casual;
                            break;
                        case "pain":
                            costume = CostumeManager.Costume.Pain;
                            break;
                        case "trash":
                            costume = CostumeManager.Costume.Trash;
                            break;
                        case "xmas":
                        case "christmas":
                            costume = CostumeManager.Costume.Christmas;
                            break;
                        default:
                            // TODO Refund points!!
                            return;
                    }

                    _costumeManager.SwitchTo(costume);
                }
            };
        }

        private void SetupRaidChaos()
        {
            var raidChaosDuration = TimeSpan.FromSeconds(PluginConfig.RaidChaosDuration.Value);
            _trashySpawner = FindObjectOfType<ItemSpawner>();
            var raidChaosActive = false;
            var raidChaosLast = DateTime.Now;
            var fakeTrigger = new TriggerConfig();
            fakeTrigger.ItemCount = PluginConfig.RaidChaosItemsPerMessage.Value;
            _twitch.OnChatMessage += message =>
            {
                if (raidChaosActive)
                {
                    var now = DateTime.Now;
                    if (now - raidChaosLast > raidChaosDuration)
                    {
                        raidChaosActive = false;
                        Log.Debug("Raid chaos disabled");
                        return;
                    }

                    _trashySpawner.SpawnTrash(fakeTrigger);
                }
            };
            _twitch.OnRaid += raid =>
            {
                raidChaosActive = true;
                raidChaosLast = DateTime.Now;
                Log.Debug("Raid chaos enabled");
            };
        }

        private async void OnDestroy()
        {
            await _kv.Close();
        }

        private IEnumerator ConnectKV()
        {
            var setupTask = SetupKV();
            yield return new WaitUntil(() => setupTask.IsCompleted);
        }

        private async Task SetupKV()
        {
            // Connect to default address
            await _kv.Connect(PluginConfig.StrimertulAddress.Value, PluginConfig.AuthKey.Value);

            // Give client to Twitch event source
            await _twitch.SetClient(_kv);
        }
    }
}