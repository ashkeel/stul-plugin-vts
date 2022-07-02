using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace StulPlugin
{
    public class HairDye
    {
        public enum HairColor
        {
            Watermelon,
            Teal,
            Tart,
            Grape,
            Grass,
            Mint,
            Gamer // This is special!!
        }

        public struct ModelHairData
        {
            public string[] ArtMeshes;
            public Dictionary<HairColor, TintGroup> Colors;
            public AnimatedTintGroup GamerDye;

            public ModelHairData(string[] artMeshes, Dictionary<HairColor, string> colors)
            {
                ArtMeshes = artMeshes;
                Colors = new Dictionary<HairColor, TintGroup>();
                foreach (var color in colors)
                {
                    var tint = ArtMeshes.Select(mesh => new StringStringTuple(mesh, color.Value)).ToList();
                    Colors.Add(color.Key, new TintGroup(tint));
                }

                GamerDye = new AnimatedTintGroup(Colors.First().Value.ArtMeshes, PluginConfig.RGBHairSpeed.Value,
                    -PluginConfig.RGBHairDelay.Value);
            }
        }

        public readonly Dictionary<string, ModelHairData> Models;

        public HairDye(Dictionary<string, ModelHairData> models)
        {
            Models = models;
        }

        public void SetHairDye(HairColor color)
        {
            // Get current model name
            var name = GameObject.Find("Live2DModel").GetComponentInChildren<VTubeStudioModel>().name;

            // Check that model is compatible
            if (!Models.ContainsKey(name))
            {
                throw new ArgumentException("This model doesn't support hair dying");
            }

            var hairTints = Models[name];

            // Check for special case (gamer)
            if (color == HairColor.Gamer)
            {
                hairTints.GamerDye.Active = true;
                return;
            }

            if (!hairTints.Colors.ContainsKey(color))
            {
                throw new ArgumentException("This color is not available for this model");
            }

            // Disable gamer dye if active
            hairTints.GamerDye.Active = false;
            hairTints.Colors[color].Apply();
        }

        public void ClearHairDye()
        {
            // Get current model name
            var name = GameObject.Find("Live2DModel").GetComponentInChildren<VTubeStudioModel>().name;

            // Check that model is compatible
            if (!Models.ContainsKey(name))
            {
                throw new ArgumentException("This model doesn't support hair dying");
            }

            Models[name].Colors.First().Value.Clear();
            Models[name].GamerDye.Active = false;
        }
    }
}