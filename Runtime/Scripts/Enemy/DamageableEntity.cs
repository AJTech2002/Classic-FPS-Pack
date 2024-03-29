using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ClassicFPS.Saving_and_Loading.States;
using ClassicFPS.Managers;
using ClassicFPS.Audio;
using ClassicFPS.Saving_and_Loading;

namespace ClassicFPS.Enemy
{
    //Any item/enemy that can be damaged by guns/throwable objects (not the player)
    public class DamageableEntity : State
    {
        [Header("Health")]
        public float health;
        public bool respawnOnLoad = false; //Whether or not to respawn the object after save

        [System.Serializable]
        public struct HealthState
        {
            public float health;
        }

        [Header("Drop Options")]
        [Space(20)]
        public bool alwaysDrop = false;
        [Range(0f, 1f)]
        public float chanceOfDrop;
        public List<Transform> droppablePrefabs; //All the possible items that the object can drop, if more than 1 it will be selected randomly
        public Vector3 spawnOffset; //Where the object will spawn relative to the object center

        [Header("Sound Effects")]
        public Sound onTakeDamage;
        public Sound onDeath;

        private HealthState savedState = new HealthState();

        public override string SaveState()
        {
            savedState.health = this.health;

            return SaveUtils.ReturnJSONFromObject<HealthState>(savedState);
        }

        public override void LoadState(string loadedJSON)
        {
            //Getting back the struct through JSON
            savedState = SaveUtils.ReturnStateFromJSON<HealthState>(loadedJSON);

            if (!respawnOnLoad) this.health = savedState.health;

            if (this.health <= 0)
            {
                Die();
            }


        }


        public virtual void TakeDamage(float damage, float delay = 0.2f)
        {
            health -= damage;

            //Play with small delay so gun shoot sound doesn't overlap with damage sound
            SFXManager.PlayClipAt(onTakeDamage, transform.position, 1, delay);

            if (health <= 0)
            {
                SFXManager.PlayClipAt(onDeath, transform.position, 1);
                Die();
            }
        }

        public virtual void Die()
        {
            SpawnDrops();
            gameObject.SetActive(false);
        }

        private void SpawnDrops()
        {
            if (droppablePrefabs.Count > 0)
            {
                if (alwaysDrop || (Random.Range(0, 101) < chanceOfDrop * 100))
                {
                    int random = Random.Range(0, droppablePrefabs.Count);
                    Instantiate(droppablePrefabs[random], transform.position + spawnOffset, Quaternion.identity);
                }
            }
        }

    }
}