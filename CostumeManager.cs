using System;
using System.IO;
using System.Threading.Tasks;
using Live2D.Cubism.Core;
using Live2D.Cubism.Rendering;
using UnityEngine;

namespace StulPlugin
{
    public class CostumeManager
    {
        public enum Costume
        {
            Casual,
            Pain,
            Trash,
            Christmas,
        }

        private static HotkeyManager _manager;

        private static int Delay => PluginConfig.CostumeSwapDuration.Value * 1000;

        /// <summary>
        /// Switch to alternate costume, most likely only temporarily
        /// </summary>
        /// <param name="costume">Costume to switch to</param>
        public async void SwitchTo(Costume costume)
        {
            if (_manager == null)
            {
                _manager = GameObject.Find("Hotkeys").GetComponent<HotkeyManager>();
            }

            switch (costume)
            {
                // Hotkey costumes
                case Costume.Casual:
                    FindAndExecuteHotkey("CostumeRedeem/casual");
                    return;
                case Costume.Pain:
                    FindAndExecuteHotkey("CostumeRedeem/pain");
                    return;
                case Costume.Trash:
                    FindAndExecuteHotkey("CostumeRedeem/trash");
                    return;
                // Texture swaps
                case Costume.Christmas:
                    SwapTextures("costumes/xmas.png", 0);
                    // Have to do the timer manually (sad!)
                    await Task.Delay(Delay);
                    RestoreTextures();
                    return;
            }
        }

        private void RestoreTextures()
        {
            // Get active model
            Log.Debug($"Restoring original textures");
            var model = GameObject.Find("Live2DModel").GetComponentInChildren<CubismTextureReplacer>();
            model.Reload();
        }

        /// <summary>
        /// Loads a given texture and replaces it in every drawable with the provided texture slot
        /// </summary>
        /// <param name="name">Texture filename relative to model folder</param>
        /// <param name="slot">Texture slot to apply to</param>
        private static void SwapTextures(string name, int slot)
        {
            // Get active model
            var cubismModel = GameObject.Find("Live2DModel").GetComponentInChildren<CubismModel>();
            var vtsModel = cubismModel.gameObject.GetComponent<VTubeStudioModel>();

            // Load texture
            var modelPath = Path.GetDirectoryName(vtsModel.Live2DModelJson.AssetPath);
            var texturePath = $"{modelPath}/{name}";
            var replacementTexture =
                IOHelper.GetCubismAssetLoadingHandler()(typeof(Texture2D), texturePath) as Texture2D;
            Log.Debug($"Loaded replacement texture: {replacementTexture}");

            foreach (CubismDrawable drawable in cubismModel.Drawables)
            {
                if (drawable.TextureIndex == slot)
                {
                    drawable.gameObject.GetComponent<CubismRenderer>().MainTexture = replacementTexture;
                    Log.Debug($"Replaced texture for {drawable.name}");
                }
            }
        }

        /// <summary>
        /// Find an hotkey by name and execute it
        /// </summary>
        /// <param name="hotkeyName">Hotkey name</param>
        /// <exception cref="Exception">An hotkey with the provided name could not be found</exception>
        private static void FindAndExecuteHotkey(string hotkeyName)
        {
            var hotkey = _manager.hotkeys.Find(entry => entry.Name == hotkeyName);
            if (hotkey == null)
            {
                throw new ArgumentException("hotkey not found");
            }

            _manager.ExecuteHotkey(hotkey);
        }
    }
}