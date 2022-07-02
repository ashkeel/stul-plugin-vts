using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StulPlugin
{
    public class AnimatedTintGroup: TintGroup
    {
        public bool Active { set; get; }
        public float Speed;
        public float Delay;
        
        public AnimatedTintGroup(List<StringStringTuple> artMeshes, float speed = 0.5f, float delay = 0f) : base(artMeshes)
        {
            Speed = speed;
            Delay = delay;
        }
        
        public AnimatedTintGroup(Dictionary<string, Tuple<Color, Color>> artMeshes, float speed = 0.5f, float delay = 0f) : base(artMeshes)
        {
            Speed = speed;
            Delay = delay;
        }

        public override void Apply()
        {
            // Calculate shift amount (abs)
            var shift = Time.time * Speed;
            
            // Shift hue on both depending on speed and current frame
            var index = 0;
            foreach (var item in ArtMeshes.Keys.ToList())
            {
                ArtMeshes[item] =
                    new Tuple<Color, Color>(shiftHSV(shift + index*Delay), ArtMeshes[item].Item2);
                index += 1;
            }

            base.Apply();
        }

        private Color shiftHSV(float amt)
        {
            return Color.HSVToRGB(amt-Mathf.Floor(amt), 1, 1);
        }
    }
}