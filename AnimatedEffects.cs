using System;
using System.Collections.Generic;
using UnityEngine;

namespace StulPlugin
{
    public class AnimatedEffects: MonoBehaviour
    {
        public List<AnimatedTintGroup> tintGroups = new List<AnimatedTintGroup>();

        private void Update()
        {
            foreach (var tintGroup in tintGroups)
            {
                if (tintGroup.Active)
                {
                    tintGroup.Apply();
                }
            }
        }
    }
}