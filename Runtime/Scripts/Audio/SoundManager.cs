using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* The SoundManager acts as a drawer to place all your sounds into an organized place, they can then be found by creating a public Sound
    asset within your script which can directly reference sounds in the SoundManager */

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