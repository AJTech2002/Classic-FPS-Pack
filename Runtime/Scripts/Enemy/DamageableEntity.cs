using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ClassicFPS.Saving_and_Loading.States;
using ClassicFPS.Managers;
using ClassicFPS.Audio;
using ClassicFPS.Saving_and_Loading;
using ClassicFPS.Controller.Movement;

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

        [Header("Graphics")]
        [SerializeField] ParticleSystem hitParticles;


        [Header("Drop Options")]
        [Space(20)]
        public bool alwaysDrop = false;
        public bool dropAllItems = false;
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


        public virtual void TakeDamage(float damage, float delay = 0.05f)
        {
            health -= damage;
            //if(hitParticles) hitParticles.Emit(5);
            //Play with small delay so gun shoot sound doesn't overlap with damage sound
            SFXManager.PlayClipAt(onTakeDamage, (transform.position + GameManager.PlayerController.gameObject.transform.position)/2, 1f, delay);

            StartCoroutine(FreezeFrameEffect(.03f));

            if (health <= 0)
            {
                SFXManager.PlayClipAt(onDeath, GameManager.PlayerController.transform.position, 1.5f, delay);
                Die();
            }
        }
     
        public virtual void Die()
        {
            SpawnDrops();
        }

        protected void SpawnDrops()
        {
            if (droppablePrefabs.Count > 0 && !dropAllItems)
            {
                if (alwaysDrop || (Random.Range(0, 101) < chanceOfDrop * 100))
                {
                    int random = Random.Range(0, droppablePrefabs.Count);
                    Instantiate(droppablePrefabs[random], transform.position + spawnOffset, Quaternion.identity);
                    Debug.Log("Spawned" + droppablePrefabs[0]);

                }
            }
            else if (droppablePrefabs.Count > 0 && dropAllItems)
            {
                if (alwaysDrop || (Random.Range(0, 101) < chanceOfDrop * 100))
                {
                    for (int i = 0; i < droppablePrefabs.Count; i++)
                    {
                        Instantiate(droppablePrefabs[i], transform.position + spawnOffset, Quaternion.identity);
                    }
                }
            }
        }

        IEnumerator FreezeFrameEffect(float waitTime)
        {
            Time.timeScale = .6f;
            yield return new WaitForSeconds(waitTime);
            Time.timeScale = 1f;
        }

    }
}