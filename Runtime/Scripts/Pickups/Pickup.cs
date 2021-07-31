using ClassicFPS.Managers;
using ClassicFPS.Audio;
using ClassicFPS.Saving_and_Loading;
using ClassicFPS.Saving_and_Loading.States;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClassicFPS.Guns;

namespace ClassicFPS.Pickups
{
    public class Pickup : State
    {
    
        [Header("Save Options")]
        public bool requiresSaving = true;
        public bool beenUsed = false;

        [Header("Auto Respawn")]
        public bool allowRespawn;

        [Header("SFX")]
        public Sound collectSFX;
        [Range(0f,2f)]
        public float volume = 0.5f;
        [SerializeField] bool playFlashEffect;

        [System.Serializable]
        public struct EnabledState
        {
            public bool enabled;
        }

        [Header("Saved State")] 
        public EnabledState savedState = new EnabledState();

        //Used State is tracked
        public override string SaveState()
        {
            savedState.enabled = !beenUsed;

            return SaveUtils.ReturnJSONFromObject<EnabledState>(savedState);
        }

        //Used State is loaded
        public override void LoadState(string loadedJSON)
        {
            savedState = SaveUtils.ReturnStateFromJSON<EnabledState>(loadedJSON);

            beenUsed = !savedState.enabled;

            if (allowRespawn) {
                beenUsed = false;
                EnableObject(true);
            }
            else {
                //If it is used then the GameObject will unload
                EnableObject(savedState.enabled);
            }
        }

        public void RunSFX()
        {
            collectSFX.PlayAt(transform.position, volume);
            if (playFlashEffect)
            {
                GameManager.PlayerStatistics.whiteFlashAnimator.ResetTrigger("Flash");
                GameManager.PlayerStatistics.whiteFlashAnimator.SetTrigger("Flash");
            }
        }

        public override bool ShouldSave()
        {
            return requiresSaving;
        }

        public override bool ShouldLoad()
        {
            return requiresSaving;
        }


        public void EnableObject (bool render)
        {
            if (this.GetComponent<MeshRenderer>() != null)
            {
                this.GetComponent<MeshRenderer>().enabled = render;
            }

            foreach (MeshRenderer r in GetComponentsInChildren<MeshRenderer>())
            {
                r.enabled = render;
            }

            foreach (ParticleSystem p in GetComponentsInChildren<ParticleSystem>())
            {
                p.transform.gameObject.SetActive(false);
            }

            if (this.GetComponent<Collider>() != null)
            {
                this.GetComponent<Collider>().enabled = render;
            }

            foreach (Collider c in GetComponentsInChildren<Collider>()) {
                c.enabled = render;
            }

        }

        public void WeaponPickupAnimation ()
        {
            GameObject.FindObjectOfType<Weapon>().HandlePlayerPickup();

        }
    
    }
}