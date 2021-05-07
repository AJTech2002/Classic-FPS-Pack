using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicFPS.Audio
{
    [CreateAssetMenu(fileName = "Sound Manager", menuName = "Classic FPS/Sound Manager")]
    public class SoundManager : ScriptableObject
    {
        [Header("General Sound Groups")]
        public List<SoundGroup> soundGroups = new List<SoundGroup>();

        [Space(20)]
        [Header("Ground/Terrain Sound Groupings")]
        public List<GroundSound> groundSounds = new List<GroundSound>();

        public SoundGroup _SoundGroup(string group)
        {
            for (int i = 0; i < soundGroups.Count; i++)
            {
                if (soundGroups[i].group == group) return soundGroups[i];
            }

            return null;
        }
    }
}