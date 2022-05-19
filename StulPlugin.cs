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

        private KilovoltClient kv;
        private TwitchEventSource twitch = new TwitchEventSource();
        private ItemSpawner trashySpawner;

        public StulPlugin()
        {
            // Setup configuration
            PluginConfig.Initialize(Config);
            
            kv = gameObject.AddComponent<KilovoltClient>();
        }

        private void Start()
        {
            StartCoroutine(ConnectKV());

            SetupRaidChaos();
        }

        private void SetupRaidChaos()
        {
            var raidChaosDuration = TimeSpan.FromSeconds(PluginConfig.RaidChaosDuration.Value);
            trashySpawner = FindObjectOfType<ItemSpawner>();
            var raidChaosActive = false;
            var raidChaosLast = DateTime.Now;
            var fakeTrigger = new TriggerConfig();
            fakeTrigger.ItemCount =PluginConfig.RaidChaosItemsPerMessage.Value;
            twitch.OnChatMessage += message =>
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
                    trashySpawner.SpawnTrash(fakeTrigger);
                }
            };
            twitch.OnRaid += raid =>
            {
                raidChaosActive = true;
                raidChaosLast = DateTime.Now;
                Log.Debug("Raid chaos enabled");
            };
        }

        private async void OnDestroy()
        {
            await kv.Close();
        }

        private IEnumerator ConnectKV()
        {
            var setupTask = SetupKV();
            yield return new WaitUntil(() => setupTask.IsCompleted);
        }

        private async Task SetupKV()
        {
            // Connect to default address
            await kv.Connect(PluginConfig.StrimertulAddress.Value, PluginConfig.AuthKey.Value);

            // Give client to Twitch event source
            await twitch.SetClient(kv);
        }
    }
}