using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicFPS.Audio
{
    // Helper for managing & playing all the sounds in the game
    public class SFXManager : MonoBehaviour
    {

        public static SFXManager instance;

  
        // List of all the gorund sounds in the SoundManager
        [Space(20)]
        [Header("Ground/Terrain Sound Groupings")]
        public List<GroundSound> groundSounds = new List<GroundSound>();


        // Mapping the sounds to strings so that they can be called based on tags
        private Dictionary<string, GroundSound> mappedTerrainSounds = new Dictionary<string, GroundSound>();
        private Dictionary<string, GroundSound> mappedGroundSounds = new Dictionary<string, GroundSound>();


        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            
            mappedGroundSounds.Clear();
            mappedTerrainSounds.Clear();

            for (int i = 0; i < groundSounds.Count; i++)
            {
                // Depending on whether or not it is a terrain layer put into the correct dictionary
                if (groundSounds[i].TerrainLayerNameOrTag != "" && groundSounds[i].isTerrainLayer)
                    mappedTerrainSounds.Add(groundSounds[i].TerrainLayerNameOrTag, groundSounds[i]);
                else if (groundSounds[i].TerrainLayerNameOrTag != "" && !groundSounds[i].isTerrainLayer)
                    mappedGroundSounds.Add(groundSounds[i].TerrainLayerNameOrTag, groundSounds[i]);
            }

        }

        // Based on the current 'Terrain Layer' return the SFX - https://docs.unity3d.com/Manual/class-TerrainLayer.html
        public static List<AudioClip> GetSoundFromTerrainLayer(string terrainLayer)
        {
            if (instance.mappedTerrainSounds.ContainsKey(terrainLayer))
            {
                return instance.mappedTerrainSounds[terrainLayer].footsteps;
            }

            return null;
        }

        // Based on the tag of the current ground object return the SFX
        public static List<AudioClip> GetSoundFromGroundLayer(string tag)
        {
            if (instance.mappedGroundSounds.ContainsKey(tag))
            {
                return instance.mappedGroundSounds[tag].footsteps;
            }

            return null;
        }

        // Get the sound g


    }

    //An Asset to Store the Ground Sounds (Footsteps)
    [System.Serializable]
    public class GroundSound
    {

        [Header("Options (Terrain / Non-Terrain)")]
        public string TerrainLayerNameOrTag; //Name of the Terrain Layer / Tag you are standing on
        public bool isTerrainLayer; //Whether or not these sound effects are for terrain or not

        [Header("Randomly Pick from Sounds")]
        public List<AudioClip> footsteps; //The sounds to play during this time

    }
    

    //Stores an AudioClip and some functions to use them
    [System.Serializable]
    public class Sound {
        public AudioClip sound;
        [Range(0f, 2f)]
        public float defaultVolume = 1f;

        IEnumerator RunIn (float delay, Vector3 position, float volume)
        {
            yield return new WaitForSeconds(delay);
            PlayAt(position, volume, 0f);
        }

        IEnumerator RunSourceIn (float delay, AudioSource source, float volume)
        {
            yield return new WaitForSeconds(delay);
            PlayFromSource(source, 0f, volume);
        }

        //Play from a position
        public void PlayAt (Vector3 position, float volume = -1f, float delay = 0f)
        {
            if (volume < 0) volume = defaultVolume;
            if (sound != null)
            {
                if (delay == 0f)
                    AudioSource.PlayClipAtPoint(sound, position, volume);
                else
                    SFXManager.instance.StartCoroutine(RunIn(delay, position, volume));
            }

        }
        
        //Play from a source this clip
        public void PlayFromSource (AudioSource source, float delay=0f, float volume = -1f)
        {
            if (volume < 0) volume = defaultVolume;
            if (sound != null)
            {
                if (delay == 0f)
                    source.PlayOneShot(sound, volume);
                else
                    SFXManager.instance.StartCoroutine(RunSourceIn(delay, source, volume));
            }

        }

    }


}