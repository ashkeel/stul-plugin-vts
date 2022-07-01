using System;
using System.Collections;
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
        private TwitchEventSource _twitch = new TwitchEventSource();
        private ItemSpawner _trashySpawner;
        private CostumeManager _costumeManager = new CostumeManager();

        public StulPlugin()
        {
            // Setup configuration
            PluginConfig.Initialize(Config);
            
            _kv = gameObject.AddComponent<KilovoltClient>();
        }

        private void Start()
        {
            StartCoroutine(ConnectKV());

            SetupRaidChaos();
            SetupCostumeSwap();
        }

        private void SetupCostumeSwap()
        {
            _twitch.OnRedeem += (redeem) =>
            {
                if (redeem.eventData.reward.id == PluginConfig.CostumeSwapRedeemID.Value)
                {
                    var costume = redeem.eventData.user_input;
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
            fakeTrigger.ItemCount =PluginConfig.RaidChaosItemsPerMessage.Value;
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