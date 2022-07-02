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
            var hairdye = new HairDye(new Dictionary<string, HairDye.ModelHairData>()
            {
                ["akeel"] = new HairDye.ModelHairData(
                    new[]
                    {
                        "eyebrow_l", "eyebrow_r", "bang_l", "bang_r", "bang_center", "front_hair_l", "front_hair_r",
                        "hair_cover", "hair_back"
                    },
                    new Dictionary<HairDye.HairColor, string>()
                    {
                        [HairDye.HairColor.Watermelon] = "F0ACFF|000000",
                        [HairDye.HairColor.Teal] = "33B2FF|04050E",
                        [HairDye.HairColor.Tart] = "E45F7F|000000",
                        [HairDye.HairColor.Grape] = "8B55D6|000000",
                        [HairDye.HairColor.Grass] = "6CDC3B|131C0F",
                        [HairDye.HairColor.Mint] = "71FFE3|000000",
                    }
                ),
                ["sonic-ash"] = new HairDye.ModelHairData(
                    new[]
                    {
                        "BrowL", "BrowR", "HairFront", "HairFrontShine", "HairClipSideL", "HairSideR", "HairBack"
                    },
                    new Dictionary<HairDye.HairColor, string>()
                    {
                        [HairDye.HairColor.Watermelon] = "FFADFF|351617",
                        [HairDye.HairColor.Teal] = "25ADFF|132F40",
                        [HairDye.HairColor.Tart] = "E45F7F|371415",
                        [HairDye.HairColor.Grape] = "9651F5|571D65",
                        [HairDye.HairColor.Grass] = "6FE939|47651D",
                        [HairDye.HairColor.Mint] = "5BF1D4|295955",
                    }
                )
            });

            // Add gamer dyes to animation controller
            foreach (var model in hairdye.Models)
            {
                _animated.tintGroups.Add(model.Value.GamerDye);
            }

            _twitch.OnRedeem += redeem =>
            {
                Log.Info($"REDEEMED {redeem.eventData.reward.id} by {redeem.eventData.user_login}");
                Log.Info(
                    $"Current model: {GameObject.Find("Live2DModel").GetComponentInChildren<VTubeStudioModel>().name}");
                if (redeem.eventData.reward.id == PluginConfig.RGBHairRedeemID.Value)
                {
                    try
                    {
                        hairdye.SetHairDye(HairDye.HairColor.Gamer);
                        dyeEndTime = Time.time + PluginConfig.RGBHairDuration.Value;
                        TimerUtils.RunAfter(PluginConfig.RGBHairDuration.Value * 1000, hairdye.ClearHairDye);
                    }
                    catch (Exception e)
                    {
                        // TODO REFUND
                        // For now just write it in chat
                        _kv.Set("twitch/@send-chat-message", $"{redeem.eventData.user_name}: {e.Message}");
                    }
                }

                if (redeem.eventData.reward.id == PluginConfig.DyeHairRedeemID.Value)
                {
                    try
                    {
                        var color = redeem.eventData.user_input.ToLower().Trim();
                        HairDye.HairColor dyeColor;
                        switch (color)
                        {
                            case "watermelon":
                            case "pink":
                                dyeColor = HairDye.HairColor.Watermelon;
                                break;
                            case "grape":
                            case "purple":
                                dyeColor = HairDye.HairColor.Grape;
                                break;
                            case "grass":
                            case "green":
                                dyeColor = HairDye.HairColor.Grass;
                                break;
                            case "mint":
                            case "aqua":
                            case "cyan":
                                dyeColor = HairDye.HairColor.Mint;
                                break;
                            case "teal":
                            case "blue":
                                dyeColor = HairDye.HairColor.Teal;
                                break;
                            case "red":
                            case "tart":
                                dyeColor = HairDye.HairColor.Tart;
                                break;
                            default:
                                throw new Exception(
                                    "Invalid hair color, valid choices are: watermelon / grape / grass / mint / teal / tart");
                        }

                        hairdye.SetHairDye(dyeColor);
                        dyeEndTime = Time.time + PluginConfig.DyeHairDuration.Value;
                        TimerUtils.RunAfter(PluginConfig.DyeHairDuration.Value * 1000, hairdye.ClearHairDye);
                    }
                    catch (Exception e)
                    {
                        // TODO Refund
                        // For now just write it in chat
                        _kv.Set("twitch/@send-chat-message", $"{redeem.eventData.user_name}: {e.Message}");
                    }
                }
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