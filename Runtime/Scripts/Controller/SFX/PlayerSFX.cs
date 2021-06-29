using ClassicFPS.Audio;
using ClassicFPS.Controller.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicFPS.Controller.SFX
{
    public class PlayerSFX : MonoBehaviour
    {
        [Header("References")]
        public AudioSource movementAudioSource;
        public AudioSource jumpLandAudioSource;
        public PlayerController controller;

        [Header("Sounds")]
        public Sound jumpSound;
        public Sound landSound;

        [Header("Walking")]
        public float walkingPitch;
        public float runningPitch;
        private float defaultPitch;

        public static PlayerSFX instance;

        private void Awake()
        {
            instance = this;

        }

        private void Start()
        {
            defaultPitch = movementAudioSource.pitch;
        }

        //State Switchers

        public void PlayFootstepSound()
        {
            if (controller.isApproximatelyGrounded())
            {
                string textureName = TerrainSurface.GetMainTexture(transform.position);

                AudioClip playingSound = null;

                //If it is a Terrain then use the terrain sounds
                if (controller.groundHit.transform.CompareTag("Terrain") || controller.groundHit.transform.GetComponent<Terrain>() != null)
                {
                    List<AudioClip> a = SFXManager.GetSoundFromTerrainLayer(textureName);
                    if (a != null && a.Count > 0)
                        playingSound = a[Random.Range(0, a.Count)];
                }
                else
                {
                    List<AudioClip> a = SFXManager.GetSoundFromGroundLayer(controller.groundHit.transform.tag);
                    if (a != null && a.Count > 0)
                        playingSound = a[Random.Range(0, a.Count)];
                }

                if (playingSound != null)
                {
                    movementAudioSource.PlayOneShot(playingSound, movementAudioSource.volume * Random.Range(.8f, 1.1f));
                }
            }
        }

        private bool CurrentlyOnTerrain()
        {
            return (controller.groundHit.transform.CompareTag("Terrain") || controller.groundHit.transform.GetComponent<Terrain>() != null);
        }

    }

}