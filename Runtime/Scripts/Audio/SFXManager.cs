using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicFPS.Audio
{
    //Helper for managing & playing all the sounds in the game
    public class SFXManager : MonoBehaviour
    {
        [Header("Sound Manager Scriptable Object")]
        public SoundManager soundManager;

        [Header("General Sound Groupings")]
        private Dictionary<string, SoundGroup> soundGroupings = new Dictionary<string, SoundGroup>();

        public static SFXManager instance;

        private List<SoundGroup> soundGroups
        {
            get
            {
                return soundManager.soundGroups;
            }
        }

        private List<GroundSound> groundSounds
        {
            get
            {
                return soundManager.groundSounds;
            }
        }


        private Dictionary<string, GroundSound> mappedTerrainSounds = new Dictionary<string, GroundSound>();
        private Dictionary<string, GroundSound> mappedGroundSounds = new Dictionary<string, GroundSound>();


        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            soundGroupings.Clear();
            mappedGroundSounds.Clear();
            mappedTerrainSounds.Clear();

            for (int i = 0; i < soundGroups.Count; i++)
            {
                soundGroupings.Add(soundGroups[i].group, soundGroups[i]);
                soundGroups[i].Init();
            }

            for (int i = 0; i < groundSounds.Count; i++)
            {
                if (groundSounds[i].TerrainLayerNameOrTag != "" && groundSounds[i].isTerrainLayer)
                    mappedTerrainSounds.Add(groundSounds[i].TerrainLayerNameOrTag, groundSounds[i]);
                else if (groundSounds[i].TerrainLayerNameOrTag != "" && !groundSounds[i].isTerrainLayer)
                    mappedGroundSounds.Add(groundSounds[i].TerrainLayerNameOrTag, groundSounds[i]);
            }

        }

        public static List<AudioClip> GetSoundFromTerrainLayer(string terrainLayer)
        {
            if (instance.mappedTerrainSounds.ContainsKey(terrainLayer))
            {
                return instance.mappedTerrainSounds[terrainLayer].footsteps;
            }

            return null;
        }

        public static List<AudioClip> GetSoundFromGroundLayer(string tag)
        {
            if (instance.mappedGroundSounds.ContainsKey(tag))
            {
                return instance.mappedGroundSounds[tag].footsteps;
            }

            return null;
        }

        public static SoundGroup SoundGroup(string group)
        {
            if (instance.soundGroupings.ContainsKey(group))
            {
                return instance.soundGroupings[group];
            }
            else
            {
                return new SoundGroup();
            }
        }

        public static AudioClip GetClip(Sound sound)
        {
            if (sound.group != "" && sound.clipName != "")
            {
                string group = sound.group;
                string clipName = sound.clipName;

                return SoundGroup(group).Clip(clipName);
            }
            else
            {
                return null;
            }
        }

        public List<SoundGroup> _SoundGroups()
        {
            return soundGroups;
        }

        public SoundGroup _SoundGroup(string group)
        {
            for (int i = 0; i < soundGroups.Count; i++)
            {
                if (soundGroups[i].group == group) return soundGroups[i];
            }

            return null;
        }

        public static List<SoundGroup> SoundGroups()
        {
            return instance.soundGroups;
        }

        public IEnumerator Wait(float delay, Sound sound, Vector3 point, float volume)
        {
            yield return new WaitForSeconds(delay);
            var clip = GetClip(sound);

            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, point, volume);
        }

        public IEnumerator WaitSource(float delay, Sound sound, AudioSource source)
        {
            yield return new WaitForSeconds(delay);
            AudioClip clip = GetClip(sound);
            if (clip != null)
            {
                source.clip = clip;
                source.Play();
            }
        }


        public static void PlayClipAt(Sound sound, Vector3 point, float volume = 1f, float delay = 0f)
        {
            if (sound.group != "" && sound.clipName != "")
            {
                if (delay == 0f)
                {
                    var clip = GetClip(sound);
                    if (clip != null)
                        AudioSource.PlayClipAtPoint(clip, point, volume);
                }
                else
                    instance.StartCoroutine(instance.Wait(delay, sound, point, volume));
            }
        }

        public AudioSource FindAudioSource(Transform t)
        {
            if (t.GetComponent<AudioSource>() != null) return t.GetComponent<AudioSource>();
            if (t.GetComponentInChildren<AudioSource>() != null) return t.GetComponentInChildren<AudioSource>();

            return null;
        }

        public static void StopClip(AudioSource source)
        {
            source.Stop();
        }

        public static void PlayClipFromSource(Sound sound, AudioSource source, float delay = 0f)
        {
            if (sound.group != "" && sound.clipName != "")
            {
                if (delay == 0f)
                {
                    AudioClip clip = GetClip(sound);
                    if (source != null)
                    {
                        source.PlayOneShot(clip);
                    }
                }
                else
                {
                    instance.StartCoroutine(instance.WaitSource(delay, sound, source));
                }
            }
        }


    }

    [System.Serializable]
    public class ListSound {
        public Sound sound;
    }

    [System.Serializable]
    public class Sound
    {
        [SerializeField]
        public string group;
        [SerializeField]
        public string clipName;

        public Sound(string group, string clipName)
        {
            this.group = group;
            this.clipName = clipName;
        }
    }

    [System.Serializable]
    public class GroundSound
    {

        [Header("Options (Terrain / Non-Terrain)")]
        public string TerrainLayerNameOrTag; //Name of the Terrain Layer you are standing on
        public bool isTerrainLayer;

        [Header("Randomly Pick from Sounds")]
        public List<AudioClip> footsteps; //The sound to play during this time

    }

    [System.Serializable]
    public class SoundGroup
    {
        public string group;
        public List<AudioClip> clips;


        private Dictionary<string, AudioClip> clipNameDictionary = new Dictionary<string, AudioClip>();

        public AudioClip Clip(string name)
        {
            if (clipNameDictionary.ContainsKey(name))
            {
                return clipNameDictionary[name];
            }
            else
            {
                return null;
            }
        }

        public void Init()
        {
            clipNameDictionary.Clear();
            for (int i = 0; i < clips.Count; i++)
            {
                if (clipNameDictionary.ContainsKey(clips[i].name))
                    continue;

                clipNameDictionary.Add(clips[i].name, clips[i]);
            }
        }

    }

}