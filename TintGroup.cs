using System;
using System.Collections.Generic;
using UnityEngine;

namespace StulPlugin
{
    public class TintGroup
    {
        public Dictionary<string, Tuple<Color, Color>> ArtMeshes { get; }
        protected static VTubeStudioModel Model => GameObject.Find("Live2DModel").GetComponentInChildren<VTubeStudioModel>();
        
        public TintGroup(List<StringStringTuple> artMeshes)
        {
            ArtMeshes = Colors.ColorMultiplyScreenOverridesFromDict(artMeshes);
        }
        
        public TintGroup(Dictionary<string, Tuple<Color, Color>> artMeshes)
        {
            ArtMeshes = artMeshes;
        }
        
        public virtual void Apply()
        {
            var model = Model;
            
            // Apply overrides
            foreach (var item in ArtMeshes)
            {
                model.ColorMultiplyScreenOverrides[item.Key] = item.Value;
            }
            ArtMeshColorTint.SetMultiplyAndScreenColors(model, 0.2f);
        }

        public void Clear()
        {
            var model = Model;
           
            // Clear overrides
            foreach (var item in ArtMeshes)
            {
                model.ColorMultiplyScreenOverrides.Remove(item.Key);
            }
            ArtMeshColorTint.SetMultiplyAndScreenColors(model, 0.2f);
        }
    }
}